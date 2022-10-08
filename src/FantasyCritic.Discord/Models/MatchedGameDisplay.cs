using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Discord.Models;

public class MatchedGameDisplay
{
    public MatchedGameDisplay(MasterGameYear gameFound)
    {
        GameFound = gameFound;
    }

    public Publisher? PublisherWhoPicked { get; set; }
    public Publisher? PublisherWhoCounterPicked { get; set; }
    public MasterGameYear GameFound { get; }
}
