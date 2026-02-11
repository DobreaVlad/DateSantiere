using System.Data;
using Microsoft.Data.Sqlite;
using MySqlConnector;

static int GetIntArg(string[] args, string name, int defaultValue)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            if (!int.TryParse(args[i + 1], out var v)) return defaultValue;
            return v;
        }
    }
    return defaultValue;
}

static string? GetArg(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            return args[i + 1];
    }
    return null;
}

static bool HasFlag(string[] args, string name) =>
    args.Any(a => a.Equals(name, StringComparison.OrdinalIgnoreCase));

var sqlitePath = GetArg(args, "--sqlite");
var mysqlConn = GetArg(args, "--mysql");
var batchSize = GetIntArg(args, "--batchSize", 500);
var truncate = HasFlag(args, "--truncate");

if (string.IsNullOrWhiteSpace(sqlitePath) || string.IsNullOrWhiteSpace(mysqlConn))
{
    Console.Error.WriteLine("Usage: dotnet run --project DateSantiere.DbImport -- --sqlite /path/to/db.sqlite --mysql \"Server=...;Database=...;User=...;Password=...;\" [--truncate] [--batchSize 500]");
    return 2;
}

if (!File.Exists(sqlitePath))
{
    Console.Error.WriteLine($"SQLite file not found: {sqlitePath}");
    return 2;
}

var sqliteCsb = new SqliteConnectionStringBuilder { DataSource = sqlitePath, Mode = SqliteOpenMode.ReadOnly };

await using var sqlite = new SqliteConnection(sqliteCsb.ToString());
await sqlite.OpenAsync();

await using var mysql = new MySqlConnection(mysqlConn);
await mysql.OpenAsync();

static async Task<List<string>> GetSqliteTables(SqliteConnection c)
{
    var tables = new List<string>();
    await using var cmd = c.CreateCommand();
    cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name;";
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
        tables.Add(r.GetString(0));
    return tables;
}

static async Task<List<(string Name, bool IsGenerated)>> GetSqliteColumns(SqliteConnection c, string table)
{
    // `pragma_table_xinfo` includes hidden/generated columns (rare); we skip generated.
    var cols = new List<(string, bool)>();
    await using var cmd = c.CreateCommand();
    cmd.CommandText = $"SELECT name, hidden FROM pragma_table_xinfo('{table.Replace("'", "''")}') ORDER BY cid;";
    await using var r = await cmd.ExecuteReaderAsync();
    while (await r.ReadAsync())
    {
        var name = r.GetString(0);
        var hidden = r.GetInt64(1);
        var isGenerated = hidden != 0; // 2/3 for generated/hidden columns.
        cols.Add((name, isGenerated));
    }
    return cols;
}

static string QMy(string ident) => $"`{ident.Replace("`", "``")}`";
static string QSq(string ident) => $"\"{ident.Replace("\"", "\"\"")}\"";

static async Task<bool> MySqlTableExists(MySqlConnection c, string table)
{
    await using var cmd = c.CreateCommand();
    cmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @t;";
    cmd.Parameters.AddWithValue("@t", table);
    var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());
    return count > 0;
}

static async Task<long> MySqlCount(MySqlConnection c, string table)
{
    await using var cmd = c.CreateCommand();
    cmd.CommandText = $"SELECT COUNT(*) FROM {QMy(table)};";
    return Convert.ToInt64(await cmd.ExecuteScalarAsync());
}

var tables = await GetSqliteTables(sqlite);
tables.Remove("__EFMigrationsHistory"); // MySQL has its own migration history.

// Only copy tables that exist in MySQL.
var copyTables = new List<string>();
foreach (var t in tables)
{
    if (await MySqlTableExists(mysql, t))
        copyTables.Add(t);
}

Console.WriteLine($"SQLite tables: {tables.Count}. MySQL matching tables: {copyTables.Count}.");

await using (var cmd = mysql.CreateCommand())
{
    cmd.CommandText = "SET FOREIGN_KEY_CHECKS=0; SET UNIQUE_CHECKS=0;";
    await cmd.ExecuteNonQueryAsync();
}

try
{
    if (truncate)
    {
        Console.WriteLine("Truncating MySQL tables...");
        foreach (var t in copyTables)
        {
            await using var cmd = mysql.CreateCommand();
            cmd.CommandText = $"TRUNCATE TABLE {QMy(t)};";
            await cmd.ExecuteNonQueryAsync();
        }
    }
    else
    {
        // Guard: if any target table has rows, abort to avoid duplicates.
        foreach (var t in copyTables)
        {
            var cnt = await MySqlCount(mysql, t);
            if (cnt > 0)
            {
                Console.Error.WriteLine($"Refusing to import: MySQL table {t} already has {cnt} rows. Re-run with --truncate if this is a fresh migration.");
                return 3;
            }
        }
    }

    foreach (var t in copyTables)
    {
        var cols = (await GetSqliteColumns(sqlite, t))
            .Where(c => !c.IsGenerated)
            .Select(c => c.Name)
            .ToList();

        if (cols.Count == 0)
            continue;

        var selectSql = $"SELECT {string.Join(", ", cols.Select(QSq))} FROM {QSq(t)};";
        var insertSql = $"INSERT INTO {QMy(t)} ({string.Join(", ", cols.Select(QMy))}) VALUES ({string.Join(", ", cols.Select((_, i) => $"@p{i}"))});";

        Console.WriteLine($"Import {t} ({cols.Count} cols)...");

        await using var selectCmd = sqlite.CreateCommand();
        selectCmd.CommandText = selectSql;
        await using var reader = await selectCmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

        await using var insertCmd = mysql.CreateCommand();
        insertCmd.CommandText = insertSql;
        insertCmd.Parameters.Clear();
        for (var i = 0; i < cols.Count; i++)
            insertCmd.Parameters.Add(new MySqlParameter($"@p{i}", DBNull.Value));
        await insertCmd.PrepareAsync();

        var row = 0;
        var sinceCommit = 0;
        MySqlTransaction? tx = await mysql.BeginTransactionAsync();
        insertCmd.Transaction = tx;
        while (await reader.ReadAsync())
        {
            for (var i = 0; i < cols.Count; i++)
            {
                var v = reader.GetValue(i);
                insertCmd.Parameters[i].Value = v is null ? DBNull.Value : v;
            }

            await insertCmd.ExecuteNonQueryAsync();
            row++;
            sinceCommit++;

            if (sinceCommit >= batchSize)
            {
                if (tx is not null)
                {
                    await tx.CommitAsync();
                    await tx.DisposeAsync();
                }
                tx = await mysql.BeginTransactionAsync();
                insertCmd.Transaction = tx;
                sinceCommit = 0;
            }
        }

        if (tx is not null)
        {
            await tx.CommitAsync();
            await tx.DisposeAsync();
        }

        Console.WriteLine($"Imported {t}: {row} rows.");
    }
}
finally
{
    await using var cmd = mysql.CreateCommand();
    cmd.CommandText = "SET FOREIGN_KEY_CHECKS=1; SET UNIQUE_CHECKS=1;";
    await cmd.ExecuteNonQueryAsync();
}

Console.WriteLine("Done.");
return 0;
