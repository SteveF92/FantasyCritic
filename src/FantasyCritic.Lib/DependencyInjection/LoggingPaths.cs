using System.Runtime.InteropServices;

namespace FantasyCritic.Lib.DependencyInjection;

public class LoggingPaths
{
    private readonly string _windowsBase = @"C:\FantasyCritic\Logs\";
    private readonly string _linuxBase = @"~/FantasyCritic/Logs/";

    public string AllLogPath => $"{GetBase()}log-all.txt";
    public string MyLogPath => $"{GetBase()}log-my.txt";
    public string WarnLogPath => $"{GetBase()}log-warning.txt";

    private string GetBase()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return _windowsBase;
        }

        return _linuxBase;
    }
}
