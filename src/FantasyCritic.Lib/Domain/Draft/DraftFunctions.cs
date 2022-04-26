using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Draft;
public static class DraftFunctions
{
    public static bool LeagueIsReadyToSetDraftOrder(IEnumerable<Publisher> publishersInLeague, IEnumerable<FantasyCriticUser> activeUsers)
    {
        if (publishersInLeague.Count() != activeUsers.Count())
        {
            return false;
        }

        if (publishersInLeague.Count() < 2 || publishersInLeague.Count() > 20)
        {
            return false;
        }

        return true;
    }

    public static IReadOnlyList<string> GetStartDraftResult(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers)
    {
        if (leagueYear.PlayStatus.PlayStarted)
        {
            return new List<string>();
        }

        var supportedYear = leagueYear.SupportedYear;
        List<string> errors = new List<string>();

        if (activeUsers.Count() < 2)
        {
            errors.Add("You need to have at least two players in the league.");
        }

        if (activeUsers.Count() > 20)
        {
            errors.Add("You cannot have more than 20 players in the league.");
        }

        if (leagueYear.Publishers.Count() != activeUsers.Count())
        {
            errors.Add("Not every player has created a publisher.");
        }

        if (!supportedYear.OpenForPlay)
        {
            errors.Add($"This year is not yet open for play. It will become available on {supportedYear.StartDate}.");
        }

        if (!leagueYear.DraftOrderSet)
        {
            errors.Add("You must set the draft order.");
        }

        return errors;
    }

    public static DraftStatus? GetDraftStatus(LeagueYear leagueYear)
    {
        if (!leagueYear.PlayStatus.DraftIsActive)
        {
            return null;
        }

        var draftPhase = GetDraftPhase(leagueYear);
        if (draftPhase.Equals(DraftPhase.Complete))
        {
            return null;
        }

        var nextDraftPublisher = GetNextDraftPublisher(leagueYear);
        var draftPositionStatus = GetDraftPositionStatus(leagueYear, draftPhase, nextDraftPublisher);

        DraftStatus draftStatus = new DraftStatus(draftPhase, nextDraftPublisher, draftPositionStatus.DraftPosition, draftPositionStatus.OverallDraftPosition);
        return draftStatus;
    }

    private static DraftPhase GetDraftPhase(LeagueYear leagueYear)
    {
        int numberOfStandardGamesToDraft = leagueYear.Options.GamesToDraft * leagueYear.Publishers.Count;
        var allPublisherGames = leagueYear.Publishers.SelectMany(x => x.PublisherGames).ToList();
        int standardGamesDrafted = allPublisherGames.Count(x => !x.CounterPick);
        if (standardGamesDrafted < numberOfStandardGamesToDraft)
        {
            return DraftPhase.StandardGames;
        }

        int numberOfCounterPicksToDraft = leagueYear.Options.CounterPicksToDraft * leagueYear.Publishers.Count;
        int counterPicksDrafted = allPublisherGames.Count(x => x.CounterPick);
        if (counterPicksDrafted < numberOfCounterPicksToDraft)
        {
            return DraftPhase.CounterPicks;
        }

        return DraftPhase.Complete;
    }

    private static Publisher GetNextDraftPublisher(LeagueYear leagueYear)
    {
        var phase = GetDraftPhase(leagueYear);
        if (phase.Equals(DraftPhase.StandardGames))
        {
            var publishersWithLowestNumberOfGames = leagueYear.Publishers.WhereMin(x => x.PublisherGames.Count(y => !y.CounterPick));
            var allPlayersHaveSameNumberOfGames = leagueYear.Publishers.Select(x => x.PublisherGames.Count(y => !y.CounterPick)).Distinct().Count() == 1;
            var maxNumberOfGames = leagueYear.Publishers.Max(x => x.PublisherGames.Count(y => !y.CounterPick));
            var roundNumber = maxNumberOfGames;
            if (allPlayersHaveSameNumberOfGames)
            {
                roundNumber++;
            }

            bool roundNumberIsOdd = (roundNumber % 2 != 0);
            if (roundNumberIsOdd)
            {
                var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderBy(x => x.DraftPosition);
                var firstPublisherOdd = sortedPublishersOdd.First();
                return firstPublisherOdd;
            }
            //Else round is even
            var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderByDescending(x => x.DraftPosition);
            var firstPublisherEven = sortedPublishersEven.First();
            return firstPublisherEven;
        }
        if (phase.Equals(DraftPhase.CounterPicks))
        {
            var publishersWithLowestNumberOfGames = leagueYear.Publishers.WhereMin(x => x.PublisherGames.Count(y => y.CounterPick));
            var allPlayersHaveSameNumberOfGames = leagueYear.Publishers.Select(x => x.PublisherGames.Count(y => y.CounterPick)).Distinct().Count() == 1;
            var maxNumberOfGames = leagueYear.Publishers.Max(x => x.PublisherGames.Count(y => y.CounterPick));

            var roundNumber = maxNumberOfGames;
            if (allPlayersHaveSameNumberOfGames)
            {
                roundNumber++;
            }

            bool roundNumberIsOdd = (roundNumber % 2 != 0);
            if (roundNumberIsOdd)
            {
                var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderByDescending(x => x.DraftPosition);
                var firstPublisherOdd = sortedPublishersOdd.First();
                return firstPublisherOdd;
            }
            //Else round is even
            var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderBy(x => x.DraftPosition);
            var firstPublisherEven = sortedPublishersEven.First();
            return firstPublisherEven;
        }

        throw new Exception($"Invalid draft state: {leagueYear.League.LeagueID}");
    }

    private static DraftPositionStatus GetDraftPositionStatus(LeagueYear leagueYear, DraftPhase draftPhase, Publisher nextDraftPublisher)
    {
        if (draftPhase.Equals(DraftPhase.StandardGames))
        {
            var publisherPosition = nextDraftPublisher.PublisherGames.Count(x => !x.CounterPick) + 1;
            var overallPosition = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick) + 1;
            return new DraftPositionStatus(publisherPosition, overallPosition);
        }
        if (draftPhase.Equals(DraftPhase.CounterPicks))
        {
            var publisherPosition = nextDraftPublisher.PublisherGames.Count(x => x.CounterPick) + 1;
            var overallPosition = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick) + 1;
            return new DraftPositionStatus(publisherPosition, overallPosition);
        }

        throw new Exception($"Invalid draft state: {leagueYear.League.LeagueID}");
    }

    private record DraftPositionStatus(int DraftPosition, int OverallDraftPosition);
}
