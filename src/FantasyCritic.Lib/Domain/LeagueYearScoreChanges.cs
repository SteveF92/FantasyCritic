namespace FantasyCritic.Lib.Domain;
public class LeagueYearScoreChanges
{
    private readonly IReadOnlyList<Publisher> _oldPublishers;
    private readonly IReadOnlyList<Publisher> _newPublishers;
    
    public LeagueYearScoreChanges(LeagueYear oldLeagueYear, LeagueYear newLeagueYear)
    {
        LeagueYear = newLeagueYear;
        _oldPublishers = oldLeagueYear.Publishers;
        _newPublishers = newLeagueYear.Publishers;
    }
    
    public LeagueYear LeagueYear { get; }

    public IReadOnlyList<PublisherScoreChange> GetScoreChanges()
    {
        var publishersOrderedByNewScore = _newPublishers.OrderByDescending(x => x.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options)).ToList();
        var publishersOrderedByOldScore = _oldPublishers.OrderByDescending(x => x.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options)).ToList();
        var oldPublisherDictionary = _oldPublishers.ToDictionary(x => x.PublisherID);

        List<PublisherScoreChange> scoreChanges = new List<PublisherScoreChange>();
        for (int newIndex = 0; newIndex < publishersOrderedByNewScore.Count; newIndex++)
        {
            var newPublisher = publishersOrderedByNewScore[newIndex];
            var oldPublisher = oldPublisherDictionary[newPublisher.PublisherID];

            var oldIndex = publishersOrderedByOldScore.IndexOf(oldPublisher);

            var newScore = newPublisher.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options);
            var oldScore = oldPublisher.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options);

            scoreChanges.Add(new PublisherScoreChange(newPublisher, oldScore, newScore, oldIndex + 1, newIndex + 1));
        }

        return scoreChanges;
    }
}

public record PublisherScoreChange(Publisher publisher, decimal oldScore, decimal newScore, int oldRank, int newRank)
{
    public string Direction
    {
        get
        {
            if (newScore > oldScore)
            {
                return "UP";
            }

            return "DOWN";
        }
    }
}
