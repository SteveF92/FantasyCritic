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

        ProcessStartInfo startInfo = BuildMySqlToolStartInfo("mysqldump");
        AddCommonConnectionArguments(startInfo, builder);
        startInfo.ArgumentList.Add("--single-transaction");
        startInfo.ArgumentList.Add("--routines");
        startInfo.ArgumentList.Add("--triggers");
        startInfo.ArgumentList.Add("--set-gtid-purged=OFF");
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

        ProcessStartInfo startInfo = BuildMySqlToolStartInfo("mysql");
        AddCommonConnectionArguments(startInfo, builder);
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

    private static void AddCommonConnectionArguments(ProcessStartInfo startInfo, MySqlConnectionStringBuilder builder)
    {
        startInfo.ArgumentList.Add($"-h{builder.Server}");
        startInfo.ArgumentList.Add($"-P{builder.Port}");
        startInfo.ArgumentList.Add($"-u{builder.UserID}");
        startInfo.ArgumentList.Add($"--password={builder.Password}");
    }

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
