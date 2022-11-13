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

    public PublisherScoreChangeList GetScoreChanges()
    {
        var publishersOrderedByNewScore = _newPublishers.OrderByDescending(x => x.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options)).ToList();
        var publishersOrderedByOldScore = _oldPublishers.OrderByDescending(x => x.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options)).ToList();
        var oldPublisherDictionary = _oldPublishers.ToDictionary(x => x.PublisherID);

        bool anyChanges = false;
        List<PublisherScoreChange> scoreChanges = new List<PublisherScoreChange>();
        for (int newIndex = 0; newIndex < publishersOrderedByNewScore.Count; newIndex++)
        {
            var newPublisher = publishersOrderedByNewScore[newIndex];
            var oldPublisher = oldPublisherDictionary[newPublisher.PublisherID];

            var oldIndex = publishersOrderedByOldScore.IndexOf(oldPublisher);

            var newScore = newPublisher.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options);
            var oldScore = oldPublisher.GetTotalFantasyPoints(LeagueYear.SupportedYear, LeagueYear.Options);
            if (oldScore != newScore)
            {
                anyChanges = true;
            }

            scoreChanges.Add(new PublisherScoreChange(newPublisher, oldScore, newScore, oldIndex + 1, newIndex + 1));
        }

        return new PublisherScoreChangeList(scoreChanges, anyChanges);
    }
}

public record PublisherScoreChange(Publisher Publisher, decimal OldScore, decimal NewScore, int OldRank, int NewRank)
{
    public string Direction => NewScore > OldScore ? "UP" : "DOWN";
    public string FormattedOldRank => FormatRank(OldRank);
    public string FormattedNewRank => FormatRank(NewRank);
    public decimal RoundedOldScore => RoundScore(OldScore);
    public decimal RoundedNewScore => RoundScore(NewScore);
    public bool ScoreChanged => NewScore != OldScore;
    public bool RankChanged => NewRank != OldRank;
    private static decimal RoundScore(decimal score) => Math.Round(score, 1);
    private static string FormatRank(int rankNumber)
    {
        var numberToFormat = rankNumber.ToString();
        var suffix = "th";
        if (!numberToFormat.EndsWith("11") && !numberToFormat.EndsWith("12") && !numberToFormat.EndsWith("13"))
        {
            if (numberToFormat.EndsWith("1"))
            {
                suffix = "st";
            }
            else if (numberToFormat.EndsWith("2"))
            {
                suffix = "nd";
            }
            else if (numberToFormat.EndsWith("3"))
            {
                suffix = "rd";
            }
        }
        return $"{numberToFormat}{suffix}";
    }
}

public record PublisherScoreChangeList(IReadOnlyList<PublisherScoreChange> Changes, bool AnyChanges);
