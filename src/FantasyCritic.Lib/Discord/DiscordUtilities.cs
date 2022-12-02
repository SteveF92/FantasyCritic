using FantasyCritic.Lib.Discord.UrlBuilders;

namespace FantasyCritic.Lib.Discord;
public static class DiscordUtilities
{
    public static string? BuildGameMessage(Publisher? standardPublisher, Publisher? counterPickPublisher, MasterGame masterGame, string baseAddress)
    {
        var gameUrl = new GameUrlBuilder(baseAddress, masterGame.MasterGameID).BuildUrl(masterGame.GameName);

        if (standardPublisher is not null)
        {
            if (counterPickPublisher is not null)
            {
                return $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > {standardPublisher.GetPublisherAndUserDisplayName()}\n > Counter Picked By {counterPickPublisher.GetPublisherAndUserDisplayName()}";
            }

            return $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > {standardPublisher.GetPublisherAndUserDisplayName()}";
        }

        if (counterPickPublisher is not null)
        {
            return $"> **{masterGame.EstimatedReleaseDate}** - {gameUrl}\n > Counter Pick For {counterPickPublisher!.GetPublisherAndUserDisplayName()}";
        }

        return null;
    }
}
