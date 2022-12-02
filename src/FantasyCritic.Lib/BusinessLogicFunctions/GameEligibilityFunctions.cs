using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.BusinessLogicFunctions;
public static class GameEligibilityFunctions
{
    public static ClaimResult CanClaimGame(ClaimGameDomainRequest request, Instant? nextBidTime, int? validDropSlot, bool acquiringNow, bool drafting, bool partOfSpecialAuction, bool counterPickWillBeConditionallyDropped, LocalDate currentDate)
    {
        var dateOfPotentialAcquisition = currentDate;
        if (nextBidTime.HasValue)
        {
            dateOfPotentialAcquisition = nextBidTime.Value.ToEasternDate();
        }

        var leagueYear = request.LeagueYear;

        List<ClaimError> claimErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, request.Publisher);
        claimErrors.AddRange(basicErrors);

        if (request.MasterGame is not null)
        {
            var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, false, currentDate,
                dateOfPotentialAcquisition, request.CounterPick, request.CounterPickedGameIsManualWillNotRelease, drafting, partOfSpecialAuction);
            claimErrors.AddRange(masterGameErrors);
        }

        if (counterPickWillBeConditionallyDropped)
        {
            claimErrors.Add(new ClaimError("Game has been dropped by the other player.", false));
        }

        LeaguePublisherGameSet gameSet = new LeaguePublisherGameSet(request.Publisher.PublisherID, leagueYear.Publishers);
        bool thisPlayerAlreadyHas = gameSet.ThisPlayerStandardGames.ContainsGame(request);
        bool gameAlreadyClaimed = gameSet.OtherPlayerStandardGames.ContainsGame(request);
        if (!request.CounterPick)
        {
            if (gameAlreadyClaimed)
            {
                claimErrors.Add(new ClaimError("Cannot claim a game that someone already has.", false));
            }

            if (thisPlayerAlreadyHas)
            {
                claimErrors.Add(new ClaimError("Cannot claim a game that you already have.", false));
            }
        }

        if (request.CounterPick)
        {
            bool otherPlayerHasCounterPick = gameSet.OtherPlayerCounterPicks.ContainsGame(request);
            if (otherPlayerHasCounterPick)
            {
                claimErrors.Add(new ClaimError("Cannot counter-pick a game that someone else has already counter picked.", false));
            }
            bool thisPlayerHasCounterPick = gameSet.ThisPlayerCounterPicks.ContainsGame(request);
            if (thisPlayerHasCounterPick)
            {
                claimErrors.Add(new ClaimError("You already have that counter pick.", false));
            }

            bool otherPlayerHasDraftGame = gameSet.OtherPlayerStandardGames.ContainsGame(request);
            if (!otherPlayerHasDraftGame)
            {
                claimErrors.Add(new ClaimError("Cannot counter pick a game that no other player is publishing.", false));
            }
        }

        MasterGameWithEligibilityFactors? eligibilityFactors = null;
        if (request.MasterGame is not null)
        {
            eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(request.MasterGame, dateOfPotentialAcquisition);
        }

        var slotResult = SlotEligibilityFunctions.GetPublisherSlotAcquisitionResult(request.Publisher, leagueYear.Options, eligibilityFactors,
            request.CounterPick, validDropSlot, acquiringNow, request.ManagerOverride);
        if (!slotResult.SlotNumber.HasValue)
        {
            claimErrors.AddRange(slotResult.ClaimErrors);
            return new ClaimResult(claimErrors, null);
        }

        var result = new ClaimResult(claimErrors, slotResult.SlotNumber.Value);
        if (result.Overridable && request.ManagerOverride)
        {
            return new ClaimResult(slotResult.SlotNumber.Value);
        }

        return result;
    }

    public static DropResult CanDropGame(DropRequest request, LeagueYear leagueYear, Publisher publisher, LocalDate currentDate)
    {
        List<ClaimError> dropErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, publisher);
        dropErrors.AddRange(basicErrors);

        var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, true, currentDate, currentDate, false, false, false, false);
        dropErrors.AddRange(masterGameErrors);

        //Drop limits
        var publisherGame = publisher.GetPublisherGame(request.MasterGame);
        if (publisherGame is null)
        {
            return new DropResult(Result.Failure("Cannot drop a game that you do not have"));
        }
        if (dropErrors.Any())
        {
            return new DropResult(Result.Failure("Game is no longer eligible for dropping."));
        }
        bool gameWasDrafted = publisherGame.OverallDraftPosition.HasValue;
        if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
        {
            return new DropResult(Result.Failure("You can only drop games that you drafted due to your league settings."));
        }

        var otherPublishers = leagueYear.GetAllPublishersExcept(publisher);
        bool gameWasCounterPicked = otherPublishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.CounterPick)
            .ContainsGame(request.MasterGame);
        if (gameWasCounterPicked && leagueYear.Options.CounterPicksBlockDrops)
        {
            return new DropResult(Result.Failure("You cannot drop that game because it was counter picked."));
        }

        bool gameCouldRelease = publisherGame.CouldRelease();
        var dropResult = publisher.CanDropGame(gameCouldRelease, leagueYear.Options, false);
        return new DropResult(dropResult);
    }

    public static DropResult CanConditionallyDropGame(PickupBid request, LeagueYear leagueYear, Publisher publisher, Instant? nextBidTime, LocalDate currentDate)
    {
        List<ClaimError> dropErrors = new List<ClaimError>();

        var basicErrors = GetBasicErrors(leagueYear.League, publisher);
        dropErrors.AddRange(basicErrors);

        var dateOfPotentialAcquisition = currentDate;
        if (nextBidTime.HasValue)
        {
            dateOfPotentialAcquisition = nextBidTime.Value.ToEasternDate();
        }

        if (request.ConditionalDropPublisherGame?.MasterGame is null)
        {
            throw new Exception($"Invalid conditional drop for bid: {request.BidID}");
        }

        var masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.ConditionalDropPublisherGame.MasterGame.MasterGame, leagueYear.Year,
            true, currentDate, dateOfPotentialAcquisition, false, false, false, false);
        dropErrors.AddRange(masterGameErrors);

        //Drop limits
        var publisherGame = publisher.GetPublisherGameByPublisherGameID(request.ConditionalDropPublisherGame.PublisherGameID);
        if (publisherGame is null)
        {
            return new DropResult(Result.Failure("Cannot drop a game that you do not have"));
        }
        if (dropErrors.Any())
        {
            return new DropResult(Result.Failure("Game is no longer eligible for dropping."));
        }
        bool gameWasDrafted = publisherGame.OverallDraftPosition.HasValue;
        if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
        {
            return new DropResult(Result.Failure("You can only drop games that you drafted due to your league settings."));
        }

        var otherPublishers = leagueYear.GetAllPublishersExcept(publisher);
        bool gameWasCounterPicked = otherPublishers
            .SelectMany(x => x.PublisherGames)
            .Where(x => x.CounterPick)
            .ContainsGame(request.ConditionalDropPublisherGame.MasterGame.MasterGame);
        if (gameWasCounterPicked && leagueYear.Options.CounterPicksBlockDrops)
        {
            return new DropResult(Result.Failure("You cannot drop that game because it was counter picked."));
        }

        bool gameCouldRelease = publisherGame.CouldRelease();
        var dropResult = publisher.CanDropGame(gameCouldRelease, leagueYear.Options, false);
        return new DropResult(dropResult);
    }

    public static ClaimResult CanAssociateGame(AssociateGameDomainRequest request, LocalDate currentDate)
    {
        List<ClaimError> associationErrors = new List<ClaimError>();
        var basicErrors = GetBasicErrors(request.LeagueYear.League, request.Publisher);
        associationErrors.AddRange(basicErrors);
        var leagueYear = request.LeagueYear;

        var dateOfPotentialAcquisition = currentDate;

        IReadOnlyList<ClaimError> masterGameErrors = GetGenericSlotMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, false, currentDate,
            dateOfPotentialAcquisition, request.PublisherGame.CounterPick, false, false, false);
        associationErrors.AddRange(masterGameErrors);

        LeaguePublisherGameSet gameSet = new LeaguePublisherGameSet(request.Publisher.PublisherID, request.LeagueYear.Publishers);

        bool thisPlayerAlreadyHas = gameSet.ThisPlayerStandardGames.ContainsGame(request.MasterGame);
        bool gameAlreadyClaimed = gameSet.OtherPlayerStandardGames.ContainsGame(request.MasterGame);
        if (!request.PublisherGame.CounterPick)
        {
            if (gameAlreadyClaimed)
            {
                associationErrors.Add(new ClaimError("Cannot claim a game that someone already has.", false));
            }

            if (thisPlayerAlreadyHas)
            {
                associationErrors.Add(new ClaimError("Cannot claim a game that you already have.", false));
            }
        }

        if (request.PublisherGame.CounterPick)
        {
            bool otherPlayerHasCounterPick = gameSet.OtherPlayerCounterPicks.ContainsGame(request.MasterGame);
            if (otherPlayerHasCounterPick)
            {
                associationErrors.Add(new ClaimError("Cannot counter-pick a game that someone else has already counter picked.", false));
            }
            bool thisPlayerHasCounterPick = gameSet.ThisPlayerCounterPicks.ContainsGame(request.MasterGame);
            if (thisPlayerHasCounterPick)
            {
                associationErrors.Add(new ClaimError("You already have that counter pick.", false));
            }

            bool otherPlayerHasDraftGame = gameSet.OtherPlayerStandardGames.ContainsGame(request.MasterGame);
            if (!otherPlayerHasDraftGame)
            {
                associationErrors.Add(new ClaimError("Cannot counter pick a game that no other player is publishing.", false));
            }
        }

        var result = new ClaimResult(associationErrors, request.PublisherGame.SlotNumber);
        if (result.Overridable && request.ManagerOverride)
        {
            return new ClaimResult(request.PublisherGame.SlotNumber);
        }

        return result;
    }

    private static IReadOnlyList<ClaimError> GetBasicErrors(League league, Publisher publisher)
    {
        List<ClaimError> claimErrors = new List<ClaimError>();

        bool isInLeague = (publisher.LeagueYearKey.LeagueID == league.LeagueID);
        if (!isInLeague)
        {
            claimErrors.Add(new ClaimError("User is not in that league.", false));
        }

        if (!league.Years.Contains(publisher.LeagueYearKey.Year))
        {
            claimErrors.Add(new ClaimError("League is not active for that year.", false));
        }

        return claimErrors;
    }

    public static IReadOnlyList<ClaimError> GetGenericSlotMasterGameErrors(LeagueYear leagueYear, MasterGame masterGame, int year, bool dropping,
        LocalDate currentDate, LocalDate dateOfPotentialAcquisition, bool counterPick, bool counterPickedGameIsManualWillNotRelease,
        bool drafting, bool partOfSpecialAuction)
    {
        MasterGameWithEligibilityFactors eligibilityFactors = leagueYear.GetEligibilityFactorsForMasterGame(masterGame, dateOfPotentialAcquisition);
        List<ClaimError> claimErrors = new List<ClaimError>();

        bool manuallyEligible = eligibilityFactors.OverridenEligibility.HasValue && eligibilityFactors.OverridenEligibility.Value;
        bool released = masterGame.IsReleased(currentDate);
        if (released && !partOfSpecialAuction)
        {
            claimErrors.Add(new ClaimError("That game has already been released.", true));
        }

        if (currentDate != dateOfPotentialAcquisition && !partOfSpecialAuction)
        {
            bool releaseBeforeNextBids = masterGame.IsReleased(dateOfPotentialAcquisition);
            if (releaseBeforeNextBids)
            {
                if (!dropping)
                {
                    claimErrors.Add(new ClaimError("That game will release before bids are processed.", true));
                }
                else
                {
                    claimErrors.Add(new ClaimError("That game will release before drops are processed.", true));
                }
            }
        }

        if (released && masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year < year)
        {
            claimErrors.Add(new ClaimError($"That game was released prior to the start of {year}.", false));
        }

        bool willRelease = masterGame.MinimumReleaseDate.Year == year && !counterPickedGameIsManualWillNotRelease;
        if (!dropping && !released && !willRelease && !manuallyEligible)
        {
            claimErrors.Add(new ClaimError($"That game is not scheduled to be released in {year}.", true));
        }

        if (counterPick && !drafting)
        {
            if (masterGame.DelayContention)
            {
                claimErrors.Add(new ClaimError("That game is in 'delay contention', and therefore cannot be counter picked.", false));
            }

            bool confirmedWillRelease = masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year == year;
            bool acquiringAfterDeadline = dateOfPotentialAcquisition >= leagueYear.CounterPickDeadline;
            if (!confirmedWillRelease && acquiringAfterDeadline && willRelease)
            {
                claimErrors.Add(new ClaimError($"That game does not have a confirmed release date in {year}, and the 'counter pick deadline' has already passed (or will have by the time bids process).", false));
            }
        }

        bool hasScore = masterGame.CriticScore.HasValue;
        if (hasScore && !manuallyEligible && !partOfSpecialAuction)
        {
            claimErrors.Add(new ClaimError("That game already has a score.", true));
        }

        if (!hasScore && masterGame.HasAnyReviews && !manuallyEligible && !partOfSpecialAuction)
        {
            claimErrors.Add(new ClaimError("That game already has reviews.", true));
        }

        return claimErrors;
    }
}
