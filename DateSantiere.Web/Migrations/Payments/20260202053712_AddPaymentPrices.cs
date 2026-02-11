using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DateSantiere.Web.Migrations.Payments
{
    /// <inheritdoc />
    public partial class AddPaymentPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceId",
                table: "PaymentRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AmountCents = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    BillingInterval = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsForSantier = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentPrices", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PaymentPrices",
                columns: new[] { "Id", "AmountCents", "BillingInterval", "Currency", "IsActive", "IsForSantier", "Name" },
                values: new object[,]
                {
                    { 1, 1499, "one-time", "eur", true, true, "Deblocare Șantier" },
                    { 2, 999, "monthly", "eur", true, false, "Pro Monthly" },
                    { 3, 9999, "annual", "eur", true, false, "Pro Annual" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentPrices");

            migrationBuilder.DropColumn(
                name: "PriceId",
                table: "PaymentRecords");
        }
    }
}
