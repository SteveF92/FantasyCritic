using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Requests;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class GameAcquisitionService
    {
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly LeagueMemberService _leagueMemberService;
        private readonly IClock _clock;

        public GameAcquisitionService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, LeagueMemberService leagueMemberService, IClock clock)
        {
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
            _leagueMemberService = leagueMemberService;
            _clock = clock;
        }

        public ClaimResult CanClaimGame(ClaimGameDomainRequest request, IEnumerable<SupportedYear> supportedYears, LeagueYear leagueYear, IEnumerable<Publisher> publishersInLeague)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            var basicErrors = GetBasicErrors(request.Publisher.LeagueYear.League, request.Publisher, supportedYears);
            claimErrors.AddRange(basicErrors);

            LeagueOptions yearOptions = leagueYear.Options;
            if (request.MasterGame.HasValue && !request.CounterPick)
            {
                var masterGameErrors = GetMasterGameErrors(leagueYear, request.MasterGame.Value, leagueYear.Year, request.CounterPick, false);
                claimErrors.AddRange(masterGameErrors);
            }

            IReadOnlyList<Publisher> otherPublishers = publishersInLeague.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = publishersInLeague.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> thisPlayersGames = request.Publisher.PublisherGames;
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request);

            if (!request.CounterPick)
            {
                if (gameAlreadyClaimed)
                {
                    claimErrors.Add(new ClaimError("Cannot claim a game that someone already has.", false));
                }

                int leagueDraftGames = yearOptions.StandardGames;
                int userDraftGames = thisPlayersGames.Count(x => !x.CounterPick);
                if (userDraftGames == leagueDraftGames)
                {
                    claimErrors.Add(new ClaimError("User's game spaces are filled.", false));
                }
            }

            if (request.CounterPick)
            {
                bool otherPlayerHasCounterPick = otherPlayersGames.Where(x => x.CounterPick).ContainsGame(request);
                if (otherPlayerHasCounterPick)
                {
                    claimErrors.Add(new ClaimError("Cannot counter-pick a game that someone else has already counter-picked.", false));
                }

                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick).ContainsGame(request);

                int leagueCounterPicks = yearOptions.CounterPicks;
                int userCounterPicks = thisPlayersGames.Count(x => x.CounterPick);
                if (userCounterPicks == leagueCounterPicks)
                {
                    claimErrors.Add(new ClaimError("User's counter pick spaces are filled.", false));
                }

                if (!otherPlayerHasDraftGame)
                {
                    claimErrors.Add(new ClaimError("Cannot counterPick a game that no other player is publishing.", false));
                }
            }

            var result = new ClaimResult(claimErrors);
            if (result.Overridable && request.ManagerOverride)
            {
                return new ClaimResult(new List<ClaimError>());
            }

            return result;
        }

        public DropResult CanDropGame(DropRequest request, IEnumerable<SupportedYear> supportedYears, LeagueYear leagueYear, Publisher publisher)
        {
            List<ClaimError> dropErrors = new List<ClaimError>();

            var basicErrors = GetBasicErrors(request.Publisher.LeagueYear.League, publisher, supportedYears);
            dropErrors.AddRange(basicErrors);

            var masterGameErrors = GetMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, false, true);
            dropErrors.AddRange(masterGameErrors);

            //Drop limits
            var publisherGame = publisher.GetPublisherGame(request.MasterGame);
            if (publisherGame.HasNoValue)
            {
                return new DropResult(Result.Fail("Cannot drop a game that you do not have"), false);
            }
            bool gameWillRelease = publisherGame.Value.WillRelease();
            if (dropErrors.Any())
            {
                return new DropResult(Result.Fail("Game is no longer eligible for dropping."), !gameWillRelease);
            }
            bool gameWasDrafted = publisherGame.Value.OverallDraftPosition.HasValue;
            if (!gameWasDrafted && leagueYear.Options.DropOnlyDraftGames)
            {
                return new DropResult(Result.Fail("You can only drop games that you drafted due to your league settings."), false);
            }

            var dropResult = publisher.CanDropGame(gameWillRelease);
            return new DropResult(dropResult, !gameWillRelease);
        }

        public async Task<ClaimResult> CanAssociateGame(AssociateGameDomainRequest request)
        {
            List<ClaimError> associationErrors = new List<ClaimError>();
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var basicErrors = GetBasicErrors(request.Publisher.LeagueYear.League, request.Publisher, supportedYears);
            associationErrors.AddRange(basicErrors);

            var leagueYear = request.Publisher.LeagueYear;
            IReadOnlyList<ClaimError> masterGameErrors = GetMasterGameErrors(leagueYear, request.MasterGame, leagueYear.Year, request.PublisherGame.CounterPick, false);
            associationErrors.AddRange(masterGameErrors);

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.LeagueYear);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.LeagueYear.Year == leagueYear.Year).ToList();
            IReadOnlyList<Publisher> otherPublishers = publishersForYear.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = publishersForYear.SelectMany(x => x.PublisherGames).ToList();
            IReadOnlyList<PublisherGame> otherPlayersGames = otherPublishers.SelectMany(x => x.PublisherGames).ToList();

            bool gameAlreadyClaimed = gamesForYear.ContainsGame(request.MasterGame);

            if (!request.PublisherGame.CounterPick)
            {
                if (gameAlreadyClaimed)
                {
                    associationErrors.Add(new ClaimError("Cannot select a game that someone already has.", false));
                }
            }

            if (request.PublisherGame.CounterPick)
            {
                bool otherPlayerHasDraftGame = otherPlayersGames.Where(x => !x.CounterPick).ContainsGame(request.MasterGame);
                if (!otherPlayerHasDraftGame)
                {
                    associationErrors.Add(new ClaimError("Cannot counter pick a game that no other player is publishing.", false));
                }
            }

            var result = new ClaimResult(associationErrors);
            if (result.Overridable && request.ManagerOverride)
            {
                return new ClaimResult(new List<ClaimError>());
            }

            return result;
        }

        private IReadOnlyList<ClaimError> GetBasicErrors(League league, Publisher publisher, IEnumerable<SupportedYear> supportedYears)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool isInLeague = (publisher.LeagueYear.League.LeagueID == league.LeagueID);
            if (!isInLeague)
            {
                claimErrors.Add(new ClaimError("User is not in that league.", false));
            }

            if (!league.Years.Contains(publisher.LeagueYear.Year))
            {
                claimErrors.Add(new ClaimError("League is not active for that year.", false));
            }

            return claimErrors;
        }

        private IReadOnlyList<ClaimError> GetMasterGameErrors(LeagueYear leagueYear, MasterGame masterGame, int year, bool counterPick, bool dropping)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            if (!dropping)
            {
                var overriddenEligibility = leagueYear.GetOverriddenEligibility(masterGame);
                if (overriddenEligibility.HasValue)
                {
                    if (!overriddenEligibility.Value)
                    {
                        claimErrors.Add(new ClaimError("That game has been specifically banned by your league.", false));
                    }
                }
                else
                {
                    //Normal eligibility (not manually set)
                    var eligibilityErrors = leagueYear.Options.AllowedEligibilitySettings.GameIsEligible(masterGame);
                    claimErrors.AddRange(eligibilityErrors);
                }
            }

            bool released = masterGame.IsReleased(_clock);
            if (released)
            {
                claimErrors.Add(new ClaimError("That game has already been released.", true));
            }

            if (released && masterGame.ReleaseDate.HasValue && masterGame.ReleaseDate.Value.Year < year)
            {
                claimErrors.Add(new ClaimError($"That game was released prior to the start of {year}.", false));
            }

            if (!dropping)
            {
                if (!released && masterGame.MinimumReleaseDate.Year > year && !counterPick)
                {
                    claimErrors.Add(new ClaimError($"That game is not scheduled to be released in {year}.", true));
                }
            }

            bool hasScore = masterGame.CriticScore.HasValue;
            if (hasScore)
            {
                claimErrors.Add(new ClaimError("That game already has a score.", true));
            }

            return claimErrors;
        }

        public async Task<ClaimResult> ClaimGame(ClaimGameDomainRequest request, bool managerAction, bool draft, IReadOnlyList<Publisher> publishersForYear)
        {
            Maybe<MasterGameYear> masterGameYear = Maybe<MasterGameYear>.None;
            if (request.MasterGame.HasValue)
            {
                masterGameYear = new MasterGameYear(request.MasterGame.Value, request.Publisher.LeagueYear.Year);
            }

            PublisherGame playerGame = new PublisherGame(request.Publisher.PublisherID, Guid.NewGuid(), request.GameName, _clock.GetCurrentInstant(), request.CounterPick, null, null,
                masterGameYear, request.DraftPosition, request.OverallDraftPosition);

            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();
            LeagueYear leagueYear = request.Publisher.LeagueYear;

            ClaimResult claimResult = CanClaimGame(request, supportedYears, leagueYear, publishersForYear);

            if (!claimResult.Success)
            {
                return claimResult;
            }

            LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant(), managerAction, draft, request.AutoDraft);
            await _fantasyCriticRepo.AddLeagueAction(leagueAction);

            await _fantasyCriticRepo.AddPublisherGame(playerGame);

            return claimResult;
        }

        public async Task<ClaimResult> AssociateGame(AssociateGameDomainRequest request)
        {
            ClaimResult claimResult = await CanAssociateGame(request);

            if (!claimResult.Success)
            {
                return claimResult;
            }

            LeagueAction leagueAction = new LeagueAction(request, _clock.GetCurrentInstant());
            await _fantasyCriticRepo.AddLeagueAction(leagueAction);

            await _fantasyCriticRepo.AssociatePublisherGame(request.Publisher, request.PublisherGame, request.MasterGame);

            return claimResult;
        }

        public async Task<ClaimResult> MakePickupBid(Publisher publisher, MasterGame masterGame, uint bidAmount)
        {
            if (bidAmount > publisher.Budget)
            {
                return new ClaimResult(new List<ClaimError>() { new ClaimError("You do not have enough budget to make that bid.", false) });
            }

            IReadOnlyList<PickupBid> pickupBids = await _fantasyCriticRepo.GetActivePickupBids(publisher);
            bool alreadyBidFor = pickupBids.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
            if (alreadyBidFor)
            {
                return new ClaimResult(new List<ClaimError>() { new ClaimError("You cannot have two active bids for the same game.", false) });
            }

            var claimRequest = new ClaimGameDomainRequest(publisher, masterGame.GameName, false, false, false, masterGame, null, null);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var leagueYear = publisher.LeagueYear;
            var publishersForYear = await _fantasyCriticRepo.GetPublishersInLeagueForYear(publisher.LeagueYear);

            var claimResult = CanClaimGame(claimRequest, supportedYears, leagueYear, publishersForYear);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            var nextPriority = pickupBids.Count + 1;

            PickupBid currentBid = new PickupBid(Guid.NewGuid(), publisher, leagueYear, masterGame, bidAmount, nextPriority, _clock.GetCurrentInstant(), null);
            await _fantasyCriticRepo.CreatePickupBid(currentBid);

            return claimResult;
        }

        public async Task<ClaimResult> QueueGame(Publisher publisher, MasterGame masterGame)
        {
            IReadOnlyList<QueuedGame> queuedGames = await _fantasyCriticRepo.GetQueuedGames(publisher);
            bool alreadyQueued = queuedGames.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
            if (alreadyQueued)
            {
                return new ClaimResult(new List<ClaimError>() { new ClaimError("You already have that game queued.", false) });
            }

            var claimRequest = new ClaimGameDomainRequest(publisher, masterGame.GameName, false, false, false, masterGame, null, null);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var leagueYear = publisher.LeagueYear;
            var publishersForYear = await _fantasyCriticRepo.GetPublishersInLeagueForYear(publisher.LeagueYear);

            var claimResult = CanClaimGame(claimRequest, supportedYears, leagueYear, publishersForYear);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            var nextRank = queuedGames.Count + 1;
            QueuedGame queuedGame = new QueuedGame(publisher, masterGame, nextRank);
            await _fantasyCriticRepo.QueueGame(queuedGame);

            return claimResult;
        }

        public Task RemoveQueuedGame(QueuedGame queuedGame)
        {
            return _fantasyCriticRepo.RemoveQueuedGame(queuedGame);
        }

        public async Task<DropResult> MakeDropRequest(Publisher publisher, PublisherGame publisherGame)
        {
            if (publisherGame.CounterPick)
            {
                return new DropResult(Result.Fail("You can't drop a counterpick."), false);
            }

            if (publisherGame.MasterGame.HasNoValue)
            {
                return new DropResult(Result.Fail("You can't drop a game that is not linked to a master game."), false);
            }

            MasterGame masterGame = publisherGame.MasterGame.Value.MasterGame;
            IReadOnlyList<DropRequest> dropRequests = await _fantasyCriticRepo.GetActiveDropRequests(publisher);
            bool alreadyDropping = dropRequests.Select(x => x.MasterGame.MasterGameID).Contains(masterGame.MasterGameID);
            if (alreadyDropping)
            {
                return new DropResult(Result.Fail("You cannot have two active drop requests for the same game."), false);
            }

            DropRequest dropRequest = new DropRequest(Guid.NewGuid(), publisher, publisher.LeagueYear, masterGame, _clock.GetCurrentInstant(), null);
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var dropResult = CanDropGame(dropRequest, supportedYears, publisher.LeagueYear, publisher);
            if (dropResult.Result.IsFailure)
            {
                return dropResult;
            }

            await _fantasyCriticRepo.CreateDropRequest(dropRequest);

            return dropResult;
        }

        public Task<IReadOnlyList<PickupBid>> GetActiveAcquistitionBids(Publisher publisher)
        {
            return _fantasyCriticRepo.GetActivePickupBids(publisher);
        }

        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<PickupBid>>> GetActiveAcquistitionBids(SupportedYear supportedYear)
        {
            return _fantasyCriticRepo.GetActivePickupBids(supportedYear.Year);
        }

        public Task<IReadOnlyList<DropRequest>> GetActiveDropRequests(Publisher publisher)
        {
            return _fantasyCriticRepo.GetActiveDropRequests(publisher);
        }

        public Task<IReadOnlyDictionary<LeagueYear, IReadOnlyList<DropRequest>>> GetActiveDropRequests(SupportedYear supportedYear)
        {
            return _fantasyCriticRepo.GetActiveDropRequests(supportedYear.Year);
        }

        public Task<Maybe<PickupBid>> GetPickupBid(Guid bidID)
        {
            return _fantasyCriticRepo.GetPickupBid(bidID);
        }

        public Task<Maybe<DropRequest>> GetDropRequest(Guid dropRequest)
        {
            return _fantasyCriticRepo.GetDropRequest(dropRequest);
        }

        public async Task<Result> RemovePickupBid(PickupBid bid)
        {
            if (bid.Successful != null)
            {
                return Result.Fail("Bid has already been processed");
            }

            await _fantasyCriticRepo.RemovePickupBid(bid);
            return Result.Ok();
        }

        public async Task<Result> RemoveDropRequest(DropRequest dropRequest)
        {
            if (dropRequest.Successful != null)
            {
                return Result.Fail("Drop request has already been processed");
            }

            await _fantasyCriticRepo.RemoveDropRequest(dropRequest);
            return Result.Ok();
        }
    }
}
