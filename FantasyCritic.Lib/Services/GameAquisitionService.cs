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
    public class GameAquisitionService
    {
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly LeagueMemberService _leagueMemberService;
        private readonly IClock _clock;

        public GameAquisitionService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo, LeagueMemberService leagueMemberService, IClock clock)
        {
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
            _leagueMemberService = leagueMemberService;
            _clock = clock;
        }

        public async Task<ClaimResult> CanClaimGame(ClaimGameDomainRequest request, IEnumerable<SupportedYear> supportedYears, LeagueYear leagueYear)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            var basicErrors = await GetBasicErrors(request.Publisher.League, request.Publisher, supportedYears);
            claimErrors.AddRange(basicErrors);

            LeagueOptions yearOptions = leagueYear.Options;
            if (request.MasterGame.HasValue && !request.CounterPick)
            {
                var masterGameErrors = GetMasterGameErrors(leagueYear.Options, request.MasterGame.Value, leagueYear.Year, request.CounterPick);
                claimErrors.AddRange(masterGameErrors);
            }

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<Publisher> otherPublishers = allPublishers.Where(x => x.User.UserID != request.Publisher.User.UserID).ToList();

            IReadOnlyList<PublisherGame> gamesForYear = allPublishers.SelectMany(x => x.PublisherGames).ToList();
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

        public async Task<ClaimResult> CanAssociateGame(AssociateGameDomainRequest request)
        {
            List<ClaimError> associationErrors = new List<ClaimError>();
            var supportedYears = await _fantasyCriticRepo.GetSupportedYears();

            var basicErrors = await GetBasicErrors(request.Publisher.League, request.Publisher, supportedYears);
            associationErrors.AddRange(basicErrors);

            var leagueYear = await _fantasyCriticRepo.GetLeagueYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<ClaimError> masterGameErrors = GetMasterGameErrors(leagueYear.Value.Options, request.MasterGame, leagueYear.Value.Year, request.PublisherGame.CounterPick);
            associationErrors.AddRange(masterGameErrors);

            IReadOnlyList<Publisher> allPublishers = await _fantasyCriticRepo.GetPublishersInLeagueForYear(request.Publisher.League, request.Publisher.Year);
            IReadOnlyList<Publisher> publishersForYear = allPublishers.Where(x => x.Year == leagueYear.Value.Year).ToList();
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

        private async Task<IReadOnlyList<ClaimError>> GetBasicErrors(League league, Publisher publisher, IEnumerable<SupportedYear> supportedYears)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool isInLeague = await _leagueMemberService.UserIsInLeague(league, publisher.User);
            if (!isInLeague)
            {
                claimErrors.Add(new ClaimError("User is not in that league.", false));
            }

            if (!league.Years.Contains(publisher.Year))
            {
                claimErrors.Add(new ClaimError("League is not active for that year.", false));
            }

            var openYears = supportedYears.Where(x => x.OpenForPlay).Select(x => x.Year);
            if (!openYears.Contains(publisher.Year))
            {
                claimErrors.Add(new ClaimError("That year is not open for play", false));
            }

            return claimErrors;
        }

        private IReadOnlyList<ClaimError> GetMasterGameErrors(LeagueOptions yearOptions, MasterGame masterGame, int year, bool counterPick)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool eligible = masterGame.IsEligible(yearOptions.MaximumEligibilityLevel);
            if (!eligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's settings.", true));
            }

            bool earlyAccessEligible = (!masterGame.EarlyAccess || yearOptions.AllowEarlyAccess);
            if (!earlyAccessEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's early access settings.", true));
            }

            bool yearlyInstallmentEligible = (!masterGame.YearlyInstallment || yearOptions.AllowYearlyInstallments);
            if (!yearlyInstallmentEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's yearly installment settings.", true));
            }

            bool released = masterGame.IsReleased(_clock);
            if (released)
            {
                claimErrors.Add(new ClaimError("That game has already been released.", true));
            }

            if (masterGame.ReleaseDate.HasValue)
            {
                if (released && masterGame.ReleaseDate.Value.Year < year)
                {
                    claimErrors.Add(new ClaimError($"That game was released prior to the start of {year}.", false));
                }
                else if (!released && masterGame.ReleaseDate.Value.Year > year && !counterPick)
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
    }
}
