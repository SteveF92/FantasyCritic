using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Utilities;
public static class FireAndForgetUtilities
{
    public static void FireAndForget(ILogger logger, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fire and forget action failed.");
        }
    }

    public static async Task FireAndForgetAsync(ILogger logger, Func<Task> asyncAction)
    {
        try
        {
            await asyncAction();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fire and forget action failed.");
        }
    }
}
