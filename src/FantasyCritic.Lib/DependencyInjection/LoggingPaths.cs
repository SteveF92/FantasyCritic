using System.Runtime.InteropServices;

namespace FantasyCritic.Lib.DependencyInjection;

public class LoggingPaths
{
    private const string _windowsBase = @"C:\FantasyCritic\Logs\";
    private const string _linuxBase = @"/var/log/fantasy-critic/";

    public string AllLogPath => $"{GetBase()}log-all.txt";
    public string MyLogPath => $"{GetBase()}log-my.txt";
    public string WarnLogPath => $"{GetBase()}log-warning.txt";

    private static string GetBase() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? _windowsBase : _linuxBase;
}
