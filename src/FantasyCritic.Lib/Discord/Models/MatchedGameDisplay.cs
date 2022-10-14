using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Lib.Discord.Models;

public class MatchedGameDisplay
{
    public MatchedGameDisplay(MasterGameYear gameFound)
    {
        GameFound = gameFound;
    }

    public Publisher? PublisherWhoPicked { get; init; }
    public Publisher? PublisherWhoCounterPicked { get; init; }
    public MasterGameYear GameFound { get; }
}
