using FantasyCritic.Lib.Discord.Models;
using Microsoft.Extensions.Logging;

namespace FantasyCritic.Lib.Jobs.Utilities;

internal static class FinalYearStandingsJobUtilities
{
    public static void LogResult(ILogger logger, int year, FinalYearStandingsSendResult result)
    {
        if (!result.BotAvailable)
        {
            logger.LogWarning("Final standings for {Year} not sent: the Discord bot is unavailable.", year);
            return;
        }

        logger.LogInformation("Sent final standings for {Year}: {MessageCount} messages to {LeagueCount} leagues, {FailedMessageCount} failed.",
            year, result.Messages, result.Leagues, result.FailedMessages);
    }

    public static string Describe(int year, FinalYearStandingsSendResult result)
    {
        if (!result.BotAvailable)
        {
            return $"Final standings for {year}: bot unavailable, nothing sent.";
        }

        var description = $"Final standings for {year}: {result.Messages} {(result.Messages == 1 ? "message" : "messages")} to " +
                          $"{result.Leagues} {(result.Leagues == 1 ? "league" : "leagues")}";
        return result.FailedMessages == 0 ? $"{description}." : $"{description}, {result.FailedMessages} failed.";
    }
}
