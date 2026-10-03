using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Serilog;

namespace FantasyCritic.MySQL;

//Runs the MySQL client tools, which must be on PATH: mysqldump to write a gzipped dump, and mysql to load one.
public sealed class MysqldumpRunner
{
    private static readonly ILogger _logger = Log.ForContext<MysqldumpRunner>();

    private const long ProgressReportIntervalBytes = 10 * 1024 * 1024;

    public async Task<Result<string>> DumpToGzipFile(string connectionString, string outputFilePath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
        var builder = new MySqlConnectionStringBuilder(connectionString);
        using var passwordFile = new PasswordOptionFile(builder.Password);

        ProcessStartInfo startInfo = BuildMySqlToolStartInfo("mysqldump");
        AddCommonConnectionArguments(startInfo, builder, passwordFile);
        startInfo.ArgumentList.Add("--single-transaction");
        startInfo.ArgumentList.Add("--routines");
        startInfo.ArgumentList.Add("--triggers");
        startInfo.ArgumentList.Add("--set-gtid-purged=OFF");
        //Tablespace information needs the PROCESS privilege, which a read-only backup user doesn't have, and nothing here uses it.
        startInfo.ArgumentList.Add("--no-tablespaces");
        startInfo.ArgumentList.Add("--verbose");
        startInfo.ArgumentList.Add(builder.Database);

        using Process process = StartProcess(startInfo);
        await using FileStream outputFile = File.Create(outputFilePath);
        await using GZipStream gzipStream = new GZipStream(outputFile, CompressionLevel.Optimal);

        List<string> stderrLines = [];
        Task stderrTask = PumpProcessOutputToLogAsync(process.StandardError, "mysqldump", stderrLines, cancellationToken);
        Task copyTask = CopyWithProgressAsync(process.StandardOutput.BaseStream, gzipStream, "mysqldump", cancellationToken);

        await Task.WhenAll(copyTask, stderrTask);
        await process.WaitForExitAsync(cancellationToken);

        string stderr = string.Join(Environment.NewLine, stderrLines);
        return process.ExitCode == 0
            ? Result.Success(outputFilePath)
            : Result.Failure<string>(BuildProcessFailureMessage("mysqldump", process.ExitCode, stderr));
    }

    public async Task<Result> ImportGzipFile(string connectionString, string inputFilePath, CancellationToken cancellationToken)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        using var passwordFile = new PasswordOptionFile(builder.Password);

        ProcessStartInfo startInfo = BuildMySqlToolStartInfo("mysql");
        AddCommonConnectionArguments(startInfo, builder, passwordFile);
        startInfo.ArgumentList.Add(builder.Database);
        startInfo.RedirectStandardInput = true;

        using Process process = StartProcess(startInfo);
        await using FileStream inputFile = File.OpenRead(inputFilePath);
        await using GZipStream gzipStream = new GZipStream(inputFile, CompressionMode.Decompress);

        List<string> stderrLines = [];
        Task stderrTask = PumpProcessOutputToLogAsync(process.StandardError, "mysql", stderrLines, cancellationToken);
        Task copyTask = CopyWithProgressAsync(gzipStream, process.StandardInput.BaseStream, "mysql import", cancellationToken);

        await copyTask;
        process.StandardInput.Close();
        await stderrTask;
        await process.WaitForExitAsync(cancellationToken);

        string stderr = string.Join(Environment.NewLine, stderrLines);
        return process.ExitCode == 0
            ? Result.Success()
            : Result.Failure(BuildProcessFailureMessage("mysql", process.ExitCode, stderr));
    }

    //Progress is Debug, so a console run can show it while a host's logs keep only the total.
    private static async Task CopyWithProgressAsync(Stream source, Stream destination, string label, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[81920];
        long totalBytes = 0;
        long nextReportAt = ProgressReportIntervalBytes;

        while (true)
        {
            int bytesRead = await source.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                break;
            }

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytes += bytesRead;

            if (totalBytes >= nextReportAt)
            {
                _logger.Debug("[{Label}] {Megabytes} processed...", label, FormatMegabytes(totalBytes));
                nextReportAt += ProgressReportIntervalBytes;
            }
        }

        _logger.Information("[{Label}] Finished: {Megabytes} processed.", label, FormatMegabytes(totalBytes));
    }

    //Captured for the failure message. mysqldump's --verbose writes a line per step here, so these are Debug too.
    private static async Task PumpProcessOutputToLogAsync(
        StreamReader reader,
        string label,
        List<string> capturedLines,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            string? line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            capturedLines.Add(line);
            _logger.Debug("[{Label}] {Line}", label, line);
        }
    }

    private static string FormatMegabytes(long bytes) => $"{bytes / (1024.0 * 1024.0):F1} MB";

    private static ProcessStartInfo BuildMySqlToolStartInfo(string fileName)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    private static void AddCommonConnectionArguments(ProcessStartInfo startInfo, MySqlConnectionStringBuilder builder, PasswordOptionFile passwordFile)
    {
        //The client tools only accept this as their first argument.
        startInfo.ArgumentList.Add($"--defaults-extra-file={passwordFile.Path}");
        startInfo.ArgumentList.Add($"-h{builder.Server}");
        startInfo.ArgumentList.Add($"-P{builder.Port}");
        startInfo.ArgumentList.Add($"-u{builder.UserID}");
        startInfo.ArgumentList.Add($"--ssl-mode={ToClientSslMode(builder.SslMode)}");
    }

    public static string ToClientSslMode(MySqlSslMode sslMode) => sslMode switch
    {
        MySqlSslMode.None => "DISABLED",
        MySqlSslMode.Preferred => "PREFERRED",
        MySqlSslMode.Required => "REQUIRED",
        MySqlSslMode.VerifyCA => "VERIFY_CA",
        MySqlSslMode.VerifyFull => "VERIFY_IDENTITY",
        _ => throw new ArgumentOutOfRangeException(nameof(sslMode), sslMode, "No mysql client equivalent."),
    };

    /// <summary>
    /// An option file holding only the password, so that it isn't on a command line, where any process on the machine can
    /// read it. Deleted when disposed.
    /// </summary>
    private sealed class PasswordOptionFile : IDisposable
    {
        public PasswordOptionFile(string password)
        {
            Path = System.IO.Path.GetTempFileName();
            File.WriteAllText(Path, $"[client]{Environment.NewLine}password={QuoteOptionValue(password)}{Environment.NewLine}");
        }

        public string Path { get; }

        public void Dispose() => File.Delete(Path);
    }

    //An option file reads backslash escapes in a value, and strips one pair of quotes around it, which keeps a # from starting
    //a comment. Quotes inside the value are kept as they are, so only backslashes need escaping.
    public static string QuoteOptionValue(string value) => $"\"{value.Replace(@"\", @"\\")}\"";

    private static Process StartProcess(ProcessStartInfo startInfo)
    {
        return Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start {startInfo.FileName}.");
    }

    private static string BuildProcessFailureMessage(string toolName, int exitCode, string stderr)
    {
        if (string.IsNullOrWhiteSpace(stderr))
        {
            return $"{toolName} failed with exit code {exitCode}.";
        }

        return $"{toolName} failed with exit code {exitCode}: {stderr.Trim()}";
    }
}
