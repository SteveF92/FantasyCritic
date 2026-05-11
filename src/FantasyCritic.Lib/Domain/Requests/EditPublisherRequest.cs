namespace FantasyCritic.Lib.Domain.Requests;

public class EditPublisherRequest
{
    public EditPublisherRequest(LeagueYear leagueYear, Publisher publisher, string newPublisherName, int budget, int unrestrictedReleaseStatusGamesDropped,
        int willNotReleaseGamesDropped, int willReleaseGamesDropped, int superDropsAvailable)
    {
        LeagueYear = leagueYear;
        Publisher = publisher;
        if (publisher.PublisherName != newPublisherName)
        {
            NewPublisherName = newPublisherName;
        }
        if (publisher.Budget != budget)
        {
            Budget = budget;
        }
        if (publisher.UnrestrictedReleaseStatusGamesDropped != unrestrictedReleaseStatusGamesDropped)
        {
            UnrestrictedReleaseStatusGamesDropped = unrestrictedReleaseStatusGamesDropped;
        }
        if (publisher.WillNotReleaseGamesDropped != willNotReleaseGamesDropped)
        {
            WillNotReleaseGamesDropped = willNotReleaseGamesDropped;
        }
        if (publisher.WillReleaseGamesDropped != willReleaseGamesDropped)
        {
            WillReleaseGamesDropped = willReleaseGamesDropped;
        }
        if (publisher.SuperDropsAvailable != superDropsAvailable)
        {
            SuperDropsAvailable = superDropsAvailable;
        }
    }

    public LeagueYear LeagueYear { get; }
    public Publisher Publisher { get; }
    public string? NewPublisherName { get; }
    public int? Budget { get; }
    public int? UnrestrictedReleaseStatusGamesDropped { get; }
    public int? WillNotReleaseGamesDropped { get; }
    public int? WillReleaseGamesDropped { get; }
    public int? SuperDropsAvailable { get; }

    public bool SomethingChanged()
    {
        return NewPublisherName is not null ||
               Budget.HasValue ||
               UnrestrictedReleaseStatusGamesDropped.HasValue ||
               WillNotReleaseGamesDropped.HasValue ||
               WillReleaseGamesDropped.HasValue ||
               SuperDropsAvailable.HasValue;
    }

    public string GetActionString()
    {
        List<string> changes = new List<string>();
        if (NewPublisherName is not null)
        {
            changes.Add($"Changed publisher name to {NewPublisherName}");
        }
        if (Budget.HasValue)
        {
            changes.Add($"Changed budget to {Budget.Value}");
        }
        if (UnrestrictedReleaseStatusGamesDropped.HasValue)
        {
            changes.Add($"Changed 'unrestricted games dropped' to {UnrestrictedReleaseStatusGamesDropped.Value}");
        }
        if (WillNotReleaseGamesDropped.HasValue)
        {
            changes.Add($"Changed 'will not release games dropped' to {WillNotReleaseGamesDropped.Value}");
        }
        if (WillReleaseGamesDropped.HasValue)
        {
            changes.Add($"Changed 'will release games dropped' to {WillReleaseGamesDropped.Value}");
        }
        if (SuperDropsAvailable.HasValue)
        {
            changes.Add($"Changed 'super drops available' to {SuperDropsAvailable.Value}");
        }

        if (changes.Count == 0)
        {
            throw new Exception($"Nothing changed for publisher: {Publisher.PublisherID}");
        }
        if (changes.Count == 1)
        {
            return changes.Single() + ".";
        }

        string finalString = string.Join("\n", changes.Select(x => $"• {x}"));
        return finalString;
    }
}
