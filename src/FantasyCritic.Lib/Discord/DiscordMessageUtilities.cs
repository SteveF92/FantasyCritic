using FantasyCritic.Lib.Discord.UrlBuilders;

namespace FantasyCritic.Lib.Discord;
public static class DiscordMessageUtilities
{
    public static string? BuildGameMessage(Publisher? standardPublisher, Publisher? counterPickPublisher, MasterGame masterGame, string baseAddress)
    {
        var gameUrl = new GameUrlBuilder(baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);

        if (standardPublisher is not null)
        {
            return counterPickPublisher is not null
                ? $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > {standardPublisher.GetPublisherAndUserDisplayName()}\n > Counter Picked By {counterPickPublisher.GetPublisherAndUserDisplayName()}"
                : $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > {standardPublisher.GetPublisherAndUserDisplayName()}";
        }

        return counterPickPublisher is not null
            ? $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > Counter Pick For {counterPickPublisher!.GetPublisherAndUserDisplayName()}"
            : null;
    }
}
