using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;

namespace FantasyCritic.MySQL;

//Runs the MySQL client tools, which must be on PATH: mysqldump to write a gzipped dump, and mysql to load one.
public sealed partial class MysqldumpRunner
{
    private static readonly ILogger _logger = Log.ForContext<MysqldumpRunner>();

    private const long ProgressReportIntervalBytes = 10 * 1024 * 1024;

    //MySQL 8.4 refuses foreign keys on a non-unique key, which schemas from before the upgrade still have.
    private const string ImportPreamble = "/*!80400 SET SESSION restrict_fk_on_non_standard_key = OFF */;";
    private const string HostHeaderPrefix = "-- Host: ";
    private const string DatabaseHeaderMarker = "Database: ";

    [GeneratedRegex("DEFINER=`[^`]*`@`[^`]*`")]
    private static partial Regex DefinerPattern();

    public Task<Result<string>> DumpToGzipFile(string connectionString, string outputFilePath, CancellationToken cancellationToken) =>
        DumpToGzipFile(connectionString, outputFilePath, [], cancellationToken);

    public async Task<Result<string>> DumpToGzipFile(string connectionString, string outputFilePath,
        IReadOnlyList<string> additionalArguments, CancellationToken cancellationToken)
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
        //Tablespaces need the PROCESS privilege, which the read-only backup user doesn't have.
        startInfo.ArgumentList.Add("--no-tablespaces");
        startInfo.ArgumentList.Add("--verbose");
        foreach (var argument in additionalArguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

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
        Task copyTask = CopyDumpForImportAsync(gzipStream, process.StandardInput.BaseStream, cancellationToken);

        IOException? writeFailure = null;
        try
        {
            await copyTask;
            process.StandardInput.Close();
        }
        //mysql stops reading at the first failed statement; its error is on stderr.
        catch (IOException ex)
        {
            writeFailure = ex;
        }

        await stderrTask;
        await process.WaitForExitAsync(cancellationToken);

        string stderr = string.Join(Environment.NewLine, stderrLines);
        if (process.ExitCode != 0)
        {
            return Result.Failure(BuildProcessFailureMessage("mysql", process.ExitCode, stderr));
        }

        return writeFailure is null
            ? Result.Success()
            : Result.Failure($"Writing the dump to mysql failed: {writeFailure.Message}");
    }

    public static string? GetSourceSchemaIfHeader(string line)
    {
        if (!line.StartsWith(HostHeaderPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var markerIndex = line.IndexOf(DatabaseHeaderMarker, StringComparison.Ordinal);
        return markerIndex < 0 ? null : line[(markerIndex + DatabaseHeaderMarker.Length)..].Trim();
    }

    //Creating an object for another definer takes SET_ANY_DEFINER, and old snapshots' objects belong to RDS's root user.
    //Old snapshots' views also name their own schema, which would point them at another database once imported.
    public static string RewriteStatementForImport(string line, string? sourceSchema)
    {
        if (line.StartsWith("INSERT INTO", StringComparison.Ordinal))
        {
            return line;
        }

        if (line.Contains("DEFINER=", StringComparison.Ordinal))
        {
            line = DefinerPattern().Replace(line, "DEFINER=CURRENT_USER");
        }

        return sourceSchema is null ? line : line.Replace($"`{sourceSchema}`.", string.Empty, StringComparison.Ordinal);
    }

    //Latin-1 maps every byte to one char and back, so data in any encoding passes through unchanged.
    private static async Task CopyDumpForImportAsync(Stream source, Stream destination, CancellationToken cancellationToken)
    {
        using StreamReader reader = new StreamReader(source, Encoding.Latin1);
        await using StreamWriter writer = new StreamWriter(destination, Encoding.Latin1, bufferSize: 81920, leaveOpen: true) { NewLine = "\n" };
        long totalBytes = 0;
        long nextReportAt = ProgressReportIntervalBytes;
        string? sourceSchema = null;

        await writer.WriteLineAsync(ImportPreamble.AsMemory(), cancellationToken);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            sourceSchema ??= GetSourceSchemaIfHeader(line);
            await writer.WriteLineAsync(RewriteStatementForImport(line, sourceSchema).AsMemory(), cancellationToken);
            totalBytes += line.Length + 1;
            if (totalBytes >= nextReportAt)
            {
                _logger.Debug("[mysql import] {Megabytes} processed...", FormatMegabytes(totalBytes));
                nextReportAt += ProgressReportIntervalBytes;
            }
        }

        await writer.FlushAsync(cancellationToken);
        _logger.Information("[mysql import] Finished: {Megabytes} processed.", FormatMegabytes(totalBytes));
    }

    //mysqldump writes this line last, so a dump cut off part way through lacks it.
    public static async Task<bool> GzipDumpIsComplete(string inputFilePath, CancellationToken cancellationToken)
    {
        await using FileStream inputFile = File.OpenRead(inputFilePath);
        await using GZipStream gzipStream = new GZipStream(inputFile, CompressionMode.Decompress);
        using StreamReader reader = new StreamReader(gzipStream, Encoding.UTF8);

        string? lastNonBlankLine = null;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lastNonBlankLine = line;
            }
        }

        return lastNonBlankLine is not null && lastNonBlankLine.StartsWith("-- Dump completed", StringComparison.Ordinal);
    }

    //Progress is Debug: the snapshot manager's console shows it, the worker's logs keep only the total.
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

    //A command line is readable by any process on the machine.
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

    //Quoting keeps a # from starting a comment, but option files have no escape for the quote itself.
    public static string QuoteOptionValue(string value)
    {
        var escaped = value.Replace(@"\", @"\\");
        if (!escaped.Contains('"'))
        {
            return $"\"{escaped}\"";
        }

        if (!escaped.Contains('\''))
        {
            return $"'{escaped}'";
        }

        throw new ArgumentException("A MySQL option file can't hold a password containing both kinds of quotation mark.", nameof(value));
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
