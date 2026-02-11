namespace DateSantiere.Web.Services;

public class ScriptExecutionService
{
    private readonly ILogger<ScriptExecutionService> _logger;

    public ScriptExecutionService(ILogger<ScriptExecutionService> logger)
    {
        _logger = logger;
    }

    public async Task<ScriptExecutionResult> ExecuteScriptAsync(string fileName)
    {
        var scriptsPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        var scriptFile = Path.Combine(scriptsPath, fileName);

        // Security check: ensure the script is within the Scripts directory
        if (!Path.GetFullPath(scriptFile).StartsWith(Path.GetFullPath(scriptsPath)))
        {
            _logger.LogWarning("Unauthorized access attempt to script: {FileName}", fileName);
            return new ScriptExecutionResult
            {
                Success = false,
                Output = "Acces neautorizat la script.",
                ExitCode = -1,
                ExecutedAt = DateTime.UtcNow
            };
        }

        if (!File.Exists(scriptFile))
        {
            _logger.LogWarning("Script not found: {ScriptFile}", scriptFile);
            return new ScriptExecutionResult
            {
                Success = false,
                Output = "Scriptul nu a fost găsit.",
                ExitCode = -1,
                ExecutedAt = DateTime.UtcNow
            };
        }

        try
        {
            var output = new System.Text.StringBuilder();
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            var extension = Path.GetExtension(scriptFile);

            if (extension == ".ps1")
            {
                startInfo.FileName = "powershell.exe";
                startInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptFile}\"";
            }
            else if (extension == ".bat")
            {
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/c \"{scriptFile}\"";
            }
            else if (extension == ".sh")
            {
                startInfo.FileName = "/bin/bash";
                startInfo.Arguments = $"\"{scriptFile}\"";
            }
            else
            {
                _logger.LogWarning("Unsupported script type: {Extension}", extension);
                return new ScriptExecutionResult
                {
                    Success = false,
                    Output = "Tip de script nesuportat.",
                    ExitCode = -1,
                    ExecutedAt = DateTime.UtcNow
                };
            }

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            _logger.LogInformation("Executing script: {FileName}", fileName);
            var startTime = DateTime.UtcNow;

            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                if (process == null)
                {
                    return new ScriptExecutionResult
                    {
                        Success = false,
                        Output = "Eroare la pornirea procesului.",
                        ExitCode = -1,
                        ExecutedAt = startTime
                    };
                }

                var stdout = await process.StandardOutput.ReadToEndAsync();
                var stderr = await process.StandardError.ReadToEndAsync();

                output.Append(stdout);
                if (!string.IsNullOrEmpty(stderr))
                {
                    output.AppendLine("\n=== ERRORS ===");
                    output.Append(stderr);
                }

                var exitedWithTimeout = !process.WaitForExit(60000); // 60 second timeout
                var exitCode = exitedWithTimeout ? -1 : process.ExitCode;

                if (exitedWithTimeout)
                {
                    process.Kill();
                    _logger.LogWarning("Script execution timeout: {FileName}", fileName);
                    output.AppendLine("\n\n⏱️ TIMEOUT: Scriptul a depășit limita de 60 de secunde și a fost oprit.");
                }
                else
                {
                    _logger.LogInformation("Script execution completed: {FileName}, Exit Code: {ExitCode}", fileName, exitCode);
                }

                return new ScriptExecutionResult
                {
                    Success = exitCode == 0 && !exitedWithTimeout,
                    Output = output.ToString(),
                    ExitCode = exitCode,
                    ExecutedAt = startTime,
                    Duration = DateTime.UtcNow - startTime
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing script: {FileName}", fileName);
            return new ScriptExecutionResult
            {
                Success = false,
                Output = $"Eroare la executarea scriptului: {ex.Message}\n\n{ex.StackTrace}",
                ExitCode = -1,
                ExecutedAt = DateTime.UtcNow
            };
        }
    }
}

public class ScriptExecutionResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public DateTime ExecutedAt { get; set; }
    public TimeSpan Duration { get; set; }
}
