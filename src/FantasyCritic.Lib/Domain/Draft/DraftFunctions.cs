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

        var previousDraftPicks = GetPastDraftPicks(leagueYear, leagueYear.ActiveDraft);
        var processedPicks = GetFutureDraftPicks(leagueYear, leagueYear.ActiveDraft, previousDraftPicks);
        if (processedPicks.NextPick is null)
        {
            return null;
        }

        var previousDraftPick = previousDraftPicks.LastOrDefault();
        var previousNonSkippedPick = previousDraftPicks.LastOrDefault(x => !x.Skipped);

        var draftStatus = new DraftStatus(leagueYear.ActiveDraft, processedPicks.NextPick, previousDraftPick, previousNonSkippedPick, processedPicks.PicksToSkip);
        return draftStatus;
    }

    private static PickProcessingResult GetFutureDraftPicks(LeagueYear leagueYear, LeagueDraft activeDraft, IReadOnlyList<PastDraftPick> pastDraftPicks)
    {
        FutureDraftPick? nextPick = null;
        List<FutureDraftPick> picksToSkip = new List<FutureDraftPick>();

        //TODO Go through the turns, actually do the skipping logic based on slots, and determine what is to be skipped, and what is next.

        return new PickProcessingResult(nextPick, picksToSkip);
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

    private static IReadOnlyList<PastDraftPick> GetPastDraftPicks(LeagueYear leagueYear, LeagueDraft draft)
    {
        if (draft.GamesToDraft == 0 && draft.CounterPicksToDraft == 0)
        {
            return new List<PastDraftPick>();
        }

        var gameDictionary = leagueYear.Publishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.DraftID == draft.DraftID)
            .ToDictionary(x => (x.CounterPick, x.OverallDraftPosition), y => y);

        var draftPicks = new List<PastDraftPick>();

        //TODO this needs to be rewritten

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

        return draftPicks;
    }
}
