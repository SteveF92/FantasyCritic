namespace FantasyCritic.Lib.Discord.Models;

public class PublisherSearchResults
{
    public PublisherSearchResults()
    {
        FoundByPlayerName = new List<Publisher>();
        FoundByPublisherName = new List<Publisher>();
    }

    public IReadOnlyList<Publisher> FoundByPlayerName { get; set; }
    public IReadOnlyList<Publisher> FoundByPublisherName { get; set; }
    public Publisher? PublisherFoundForDiscordUser { get; set; }

    public bool HasAnyResults()
    {
        return FoundByPlayerName.Any() || FoundByPublisherName.Any() || PublisherFoundForDiscordUser != null;
    }
}
