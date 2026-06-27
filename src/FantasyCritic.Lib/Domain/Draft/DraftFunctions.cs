using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Domain.Draft;
public static class DraftFunctions
{
    public static bool LeagueIsReadyToSetDraftOrder(IEnumerable<Publisher> publishersInLeague, IEnumerable<IMinimalFantasyCriticUser> activeUsers)
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

    public static IReadOnlyList<string> GetStartDraftResult(LeagueYear leagueYear, LeagueDraft leagueDraft, IEnumerable<IMinimalFantasyCriticUser> activeUsers, bool isManager, bool conferenceDraftsNotEnabled)
    {
        if (leagueDraft.PlayStatus.PlayStarted)
        {
            return ["Draft is already started."];
        }

        var supportedYear = leagueYear.SupportedYear;
        List<string> errors = new List<string>();

        if (activeUsers.Count() < 2)
        {
            errors.Add("You need to have at least two players in the league.");
        }

        if (activeUsers.Count() > 20)
        {
            errors.Add("You cannot have more than 20 players in the league. You should consider a conference.");
        }

        if (leagueYear.Publishers.Count != activeUsers.Count())
        {
            errors.Add("Not every player has created a publisher.");
        }

        if (!supportedYear.OpenForPlay)
        {
            errors.Add($"This year is not yet open for play. It will become available on {supportedYear.StartDate}.");
        }

        if (!leagueDraft.DraftOrderSet)
        {
            errors.Add(isManager ? "You must set the draft order." : "Your league manager must set the draft order.");
        }

        if (conferenceDraftsNotEnabled)
        {
            errors.Add("The conference manager has not enabled drafting yet.");
        }

        return errors;
    }

    public static DraftStatus? GetDraftStatus(LeagueYear leagueYear)
    {
        if (leagueYear.ActiveDraft is null)
        {
            return null;
        }

        var draftTurns = GetDraftTurns(leagueYear, leagueYear.ActiveDraft);
        var draftPhase = GetDraftPhase(leagueYear);
        if (draftPhase.Equals(DraftPhase.Complete))
        {
            return null;
        }

        var nextDraftPublisher = GetNextDraftPublisher(leagueYear);
        Publisher? previousDraftPublisher = null;
        var gamesInActiveDraft = leagueYear.Publishers.SelectMany(x => x.PublisherGames)
            .Where(g => g.DraftID == leagueYear.ActiveDraft.DraftID)
            .ToList();
        if (gamesInActiveDraft.Any())
        {
            var mostRecentGame = gamesInActiveDraft.MaxBy(x => x.OverallDraftPosition);
            if (mostRecentGame is not null)
            {
                previousDraftPublisher = leagueYear.Publishers.Single(x => x.PublisherID == mostRecentGame.PublisherID);
            }
        }

        var draftPositionStatus = GetDraftPositionStatus(leagueYear, draftPhase, nextDraftPublisher);

        DraftStatus draftStatus = new DraftStatus(leagueYear.ActiveDraft, draftPhase, nextDraftPublisher, previousDraftPublisher, draftPositionStatus.DraftPosition, draftPositionStatus.OverallDraftPosition, draftTurns);
        return draftStatus;
    }

    public static Result<IReadOnlyList<KeyValuePair<Publisher, int>>> GetDraftPositions(LeagueYear leagueYear, DraftOrderType draftOrderType,
        IReadOnlyList<Guid>? manualPublisherDraftPositions, LeagueYear? previousLeagueYear, SystemWideValues? systemWideValues, int targetDraftNumber)
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

            if (!previousLeagueYear.IsFirstDraftFinished)
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

        if (draftOrderType.Equals(DraftOrderType.InverseProjectedPoints))
        {
            if (targetDraftNumber <= 1)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("Inverse projected points is only available for drafts after the first.");
            }

            if (systemWideValues is null)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("Draft Order Setting failed.");
            }

            var previousDraft = leagueYear.Drafts.SingleOrDefault(x => x.DraftNumber == targetDraftNumber - 1);
            if (previousDraft is null)
            {
                return Result.Failure<IReadOnlyList<KeyValuePair<Publisher, int>>>("Draft Order Setting failed.");
            }

            var previousDraftID = previousDraft.DraftID;
            var orderedPublishers = leagueYear.Publishers
                .OrderBy(x => x.GetProjectedFantasyPoints(leagueYear, systemWideValues))
                .ThenBy(x => x.GetTotalFantasyPoints(leagueYear.SupportedYear, leagueYear.Options))
                .ThenByDescending(x => x.GetDraftPosition(previousDraftID))
                .ThenBy(x => x.PublisherID)
                .ToList();

            return GetDraftPositionsInternal(leagueYear, orderedPublishers.Select(x => x.PublisherID).ToList());
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
        if (leagueYear.ActiveDraft is null)
        {
            throw new Exception($"Draft is not active for league: {leagueYear.Key}");
        }

        var activeDraft = leagueYear.ActiveDraft;
        int numberOfStandardGamesToDraft = activeDraft.GamesToDraft * leagueYear.Publishers.Count;
        var gamesInActiveDraft = leagueYear.Publishers.SelectMany(x => x.PublisherGames)
            .Where(x => x.DraftID == activeDraft.DraftID)
            .ToList();
        int standardGamesDrafted = gamesInActiveDraft.Count(x => !x.CounterPick);
        if (standardGamesDrafted < numberOfStandardGamesToDraft)
        {
            return DraftPhase.StandardGames;
        }

        int numberOfCounterPicksToDraft = activeDraft.CounterPicksToDraft * leagueYear.Publishers.Count;
        int counterPicksDrafted = gamesInActiveDraft.Count(x => x.CounterPick);
        if (counterPicksDrafted < numberOfCounterPicksToDraft)
        {
            return DraftPhase.CounterPicks;
        }

        return DraftPhase.Complete;
    }

    private static Publisher GetNextDraftPublisher(LeagueYear leagueYear)
    {
        if (leagueYear.ActiveDraft is null)
        {
            throw new Exception($"Draft is not active for league: {leagueYear.Key}");
        }

        var activeDraft = leagueYear.ActiveDraft;
        var phase = GetDraftPhase(leagueYear);
        if (phase.Equals(DraftPhase.StandardGames))
        {
            var publishersWithLowestNumberOfGames = leagueYear.Publishers.WhereMin(x => x.PublisherGames.Count(y => !y.CounterPick && y.DraftID == activeDraft.DraftID));
            var allPlayersHaveSameNumberOfGames = leagueYear.Publishers.Select(x => x.PublisherGames.Count(y => !y.CounterPick && y.DraftID == activeDraft.DraftID)).Distinct().Count() == 1;
            var maxNumberOfGames = leagueYear.Publishers.Max(x => x.PublisherGames.Count(y => !y.CounterPick && y.DraftID == activeDraft.DraftID));
            var roundNumber = maxNumberOfGames;
            if (allPlayersHaveSameNumberOfGames)
            {
                roundNumber++;
            }

            bool roundNumberIsOdd = (roundNumber % 2 != 0);
            if (roundNumberIsOdd)
            {
                var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderBy(x => x.GetDraftPosition(activeDraft.DraftID));
                return sortedPublishersOdd.First();
            }
            //Else round is even
            var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderByDescending(x => x.GetDraftPosition(activeDraft.DraftID));
            return sortedPublishersEven.First();
        }
        if (phase.Equals(DraftPhase.CounterPicks))
        {
            var publishersWithLowestNumberOfGames = leagueYear.Publishers.WhereMin(x => x.PublisherGames.Count(y => y.CounterPick && y.DraftID == activeDraft.DraftID));
            var allPlayersHaveSameNumberOfGames = leagueYear.Publishers.Select(x => x.PublisherGames.Count(y => y.CounterPick && y.DraftID == activeDraft.DraftID)).Distinct().Count() == 1;
            var maxNumberOfGames = leagueYear.Publishers.Max(x => x.PublisherGames.Count(y => y.CounterPick && y.DraftID == activeDraft.DraftID));
            var roundNumber = maxNumberOfGames;
            if (allPlayersHaveSameNumberOfGames)
            {
                roundNumber++;
            }

            bool roundNumberIsOdd = (roundNumber % 2 != 0);
            if (roundNumberIsOdd)
            {
                var sortedPublishersOdd = publishersWithLowestNumberOfGames.OrderByDescending(x => x.GetDraftPosition(activeDraft.DraftID));
                return sortedPublishersOdd.First();
            }
            //Else round is even
            var sortedPublishersEven = publishersWithLowestNumberOfGames.OrderBy(x => x.GetDraftPosition(activeDraft.DraftID));
            return sortedPublishersEven.First();
        }

        throw new Exception($"Invalid draft state: {leagueYear.League.LeagueID}");
    }

    private static IReadOnlyList<DraftTurn> GetDraftTurns(LeagueYear leagueYear, LeagueDraft draft)
    {
        if (draft.GamesToDraft == 0 && draft.CounterPicksToDraft == 0)
        {
            return new List<DraftTurn>();
        }

        var gameDictionary = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.DraftID == draft.DraftID)
            .ToDictionary(x => (x.CounterPick, x.OverallDraftPosition), y => y);

        var draftTurns = new List<DraftTurn>();

        var draftPhase = DraftPhase.StandardGames;
        if (draft.GamesToDraft > 0)
        {
            int overallDraftPosition = 1;
            for (int roundNumber = 1; roundNumber <= draft.GamesToDraft; roundNumber++)
            {
                bool roundNumberIsOdd = (roundNumber % 2 != 0);
                var sortedPublishers = roundNumberIsOdd ?
                    leagueYear.Publishers.OrderBy(x => x.GetDraftPosition(draft.DraftID)).ToList() :
                    leagueYear.Publishers.OrderByDescending(x => x.GetDraftPosition(draft.DraftID)).ToList();

                foreach (var publisher in sortedPublishers)
                {
                    var gameSelectedAtOverallDraftPosition = gameDictionary.GetValueOrDefault((false, overallDraftPosition));
                    PublisherGame? gameSelectedOnThisPublisherTurn = null;
                    bool? skipped;
                    if (gameSelectedAtOverallDraftPosition is null)
                    {
                        skipped = null;
                    }
                    else
                    {
                        var thisPublisherDraftedThisGame = publisher.PublisherID == gameSelectedAtOverallDraftPosition.PublisherID;
                        skipped = !thisPublisherDraftedThisGame;
                        if (thisPublisherDraftedThisGame)
                        {
                            gameSelectedOnThisPublisherTurn = gameSelectedAtOverallDraftPosition;
                        }
                    }

                    var draftTurn = new DraftTurn(draftPhase, publisher, roundNumber, overallDraftPosition, gameSelectedOnThisPublisherTurn, skipped);
                    draftTurns.Add(draftTurn);

                    if (!skipped.HasValue || !skipped.Value)
                    {
                        overallDraftPosition++;
                    }
                }
            }
        }


        if (draft.CounterPicksToDraft > 0)
        {
            draftPhase = DraftPhase.CounterPicks;
            int overallDraftPosition = 1;
            for (int roundNumber = 1; roundNumber <= draft.CounterPicksToDraft; roundNumber++)
            {
                bool roundNumberIsOdd = (roundNumber % 2 != 0);
                var sortedPublishers = roundNumberIsOdd ?
                    leagueYear.Publishers.OrderByDescending(x => x.GetDraftPosition(draft.DraftID)).ToList() :
                    leagueYear.Publishers.OrderBy(x => x.GetDraftPosition(draft.DraftID)).ToList();

                foreach (var publisher in sortedPublishers)
                {
                    var gameSelectedAtOverallDraftPosition = gameDictionary.GetValueOrDefault((true, overallDraftPosition));
                    PublisherGame? gameSelectedOnThisPublisherTurn = null;
                    bool? skipped;
                    if (gameSelectedAtOverallDraftPosition is null)
                    {
                        skipped = null;
                    }
                    else
                    {
                        var thisPublisherDraftedThisGame = publisher.PublisherID == gameSelectedAtOverallDraftPosition.PublisherID;
                        skipped = !thisPublisherDraftedThisGame;
                        if (thisPublisherDraftedThisGame)
                        {
                            gameSelectedOnThisPublisherTurn = gameSelectedAtOverallDraftPosition;
                        }
                    }

                    var draftTurn = new DraftTurn(draftPhase, publisher, roundNumber, overallDraftPosition, gameSelectedOnThisPublisherTurn, skipped);
                    draftTurns.Add(draftTurn);

                    if (!skipped.HasValue || !skipped.Value)
                    {
                        overallDraftPosition++;
                    }
                }
            }
        }

        return draftTurns;
    }

    private static DraftPositionStatus GetDraftPositionStatus(LeagueYear leagueYear, DraftPhase draftPhase, Publisher nextDraftPublisher)
    {
        if (leagueYear.ActiveDraft is null)
        {
            throw new Exception($"Draft is not active for league: {leagueYear.Key}");
        }

        var activeDraft = leagueYear.ActiveDraft;
        if (draftPhase.Equals(DraftPhase.StandardGames))
        {
            var publisherPosition = nextDraftPublisher.PublisherGames.Count(x => !x.CounterPick && x.DraftID == activeDraft.DraftID) + 1;
            var overallPosition = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick && x.DraftID == activeDraft.DraftID) + 1;
            return new DraftPositionStatus(publisherPosition, overallPosition);
        }
        if (draftPhase.Equals(DraftPhase.CounterPicks))
        {
            var publisherPosition = nextDraftPublisher.PublisherGames.Count(x => x.CounterPick && x.DraftID == activeDraft.DraftID) + 1;
            var overallPosition = leagueYear.Publishers.SelectMany(x => x.PublisherGames).Count(x => x.CounterPick && x.DraftID == activeDraft.DraftID) + 1;
            return new DraftPositionStatus(publisherPosition, overallPosition);
        }

        throw new Exception($"Invalid draft state: {leagueYear.League.LeagueID}");
    }

    private record DraftPositionStatus(int DraftPosition, int OverallDraftPosition);
}
