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

    public static IReadOnlyList<string> GetStartDraftResult(LeagueYear leagueYear, IEnumerable<FantasyCriticUser> activeUsers, bool isManager)
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
            errors.Add(isManager ? "You must set the draft order." : "Your league manager must set the draft order.");
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

    public static Result<IReadOnlyList<KeyValuePair<Publisher, int>>> GetDraftPositions(LeagueYear leagueYear, DraftOrderType draftOrderType, IReadOnlyList<Guid>? manualPublisherDraftPositions, LeagueYear? previousLeagueYear)
    {
        if (draftOrderType.Equals(DraftOrderType.Manual))
        {
            if (manualPublisherDraftPositions is null)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("Draft Order Setting failed.");
            }
            return GetDraftPositionsInternal(leagueYear, manualPublisherDraftPositions);
        }

        Random rng = new Random();
        if (draftOrderType.Equals(DraftOrderType.Random))
        {
            return GetDraftPositionsInternal(leagueYear, leagueYear.Publishers.Select(x => x.PublisherID).OrderBy(_ => rng.Next()).ToList());
        }

        if (draftOrderType.Equals(DraftOrderType.InverseStandings))
        {
            if (previousLeagueYear is null)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("There is no previous year to use for standings.");
            }

            if (!previousLeagueYear.PlayStatus.DraftFinished)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("The previous league year was not completed.");
            }

            var previousYearPublishers = previousLeagueYear.Publishers
                .OrderBy(x => x.GetTotalFantasyPoints(previousLeagueYear.SupportedYear, previousLeagueYear.Options));

            List<Publisher> currentYearPublishersInOrder = new List<Publisher>();
            foreach (var previousYearPublisher in previousYearPublishers)
            {
                var currentYearPublisher = leagueYear.GetUserPublisher(previousYearPublisher.User);
                if (currentYearPublisher is null)
                {
                    continue;
                }

                currentYearPublishersInOrder.Add(currentYearPublisher);
            }

            var notInLastYearPublishers = leagueYear.Publishers.Except(currentYearPublishersInOrder).OrderBy(_ => rng.Next()).ToList();
            var midpoint = currentYearPublishersInOrder.Count / 2;
            currentYearPublishersInOrder.InsertRange(midpoint, notInLastYearPublishers);

            return GetDraftPositionsInternal(leagueYear, currentYearPublishersInOrder.Select(x => x.PublisherID).ToList());
        }

        return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("Draft Order Setting failed.");
    }

    private static Result<IReadOnlyList<KeyValuePair<Publisher, int>>> GetDraftPositionsInternal(LeagueYear leagueYear, IReadOnlyList<Guid> publisherIDsInRequestedDraftOrder)
    {
        List<KeyValuePair<Publisher, int>> draftPositions = new List<KeyValuePair<Publisher, int>>();
        for (var index = 0; index < publisherIDsInRequestedDraftOrder.Count; index++)
        {
            var requestPublisher = publisherIDsInRequestedDraftOrder[index];
            var publisher = leagueYear.GetPublisherByID(requestPublisher);
            if (publisher is null)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("Draft Order Setting failed.");
            }

            draftPositions.Add(new KeyValuePair<Publisher, int>(publisher, index + 1));
        }
        return draftPositions;
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
