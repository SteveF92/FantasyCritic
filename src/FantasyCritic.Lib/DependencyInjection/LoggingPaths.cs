using System.Runtime.InteropServices;

namespace FantasyCritic.Lib.DependencyInjection;

public class LoggingPaths
{
    private const string _windowsBase = @"C:\FantasyCritic\Logs";
    private const string _linuxBase = @"/var/log/fantasy-critic";

    public static LoggingPaths WebApplication => new LoggingPaths("web");
    public static LoggingPaths DatabaseUpdater => new LoggingPaths("databaseupdater");
    public static LoggingPaths UnitTests => new LoggingPaths("tests");

    private LoggingPaths(string appName)
    {
        AppName = appName;
    }

    public string AppName { get; }


    public string AllLogPath => $"{GetBase()}/{AppName}/log-all.txt";
    public string MyLogPath => $"{GetBase()}/{AppName}/log-my.txt";
    public string WarnLogPath => $"{GetBase()}/{AppName}/log-warning.txt";

    private static string GetBase() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? _windowsBase : _linuxBase;
}
