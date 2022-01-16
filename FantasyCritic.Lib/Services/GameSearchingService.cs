using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class GameSearchingService
    {
        private readonly InterLeagueService _interLeagueService;
        private readonly IClock _clock;

        public GameSearchingService(InterLeagueService interLeagueService, IClock clock)
        {
            _interLeagueService = interLeagueService;
            _clock = clock;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> GetAllPossibleMasterGameYearsForLeagueYear(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();

            LocalDate currentDate = _clock.GetToday();
            foreach (var masterGame in masterGames)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames,
                    eligibilityFactors, currentDate);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> SearchGames(string searchName, Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            IReadOnlyList<MasterGameYear> matchingMasterGames = MasterGameSearching.SearchMasterGameYears(searchName, masterGames);
            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();

            LocalDate currentDate = _clock.GetToday();
            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, 
                    eligibilityFactors, currentDate);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> GetTopAvailableGames(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            IReadOnlyList<MasterGameYear> matchingMasterGames = masterGames.OrderByDescending(x => x.DateAdjustedHypeFactor).ToList();
            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();

            LocalDate currentDate = _clock.GetToday();
            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, 
                    eligibilityFactors, currentDate);

                if (!possibleMasterGame.IsAvailable)
                {
                    continue;
                }

                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> GetTopAvailableGamesForSlot(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, 
            int year, IEnumerable<LeagueTagStatus> leagueTagRequirements)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year);
            IReadOnlyList<MasterGameYear> matchingMasterGames = masterGames.OrderByDescending(x => x.DateAdjustedHypeFactor).ToList();
            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();

            LocalDate currentDate = _clock.GetToday();
            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames,
                    eligibilityFactors, leagueTagRequirements, currentDate);

                if (!possibleMasterGame.IsAvailable)
                {
                    continue;
                }

                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public async Task<IReadOnlyList<PossibleMasterGameYear>> GetQueuedPossibleGames(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, 
            IEnumerable<QueuedGame> queuedGames)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            IReadOnlyList<MasterGameYear> masterGameYears = await _interLeagueService.GetMasterGameYears(currentPublisher.LeagueYear.Year);
            var masterGamesForThisYear = masterGameYears.Where(x => x.Year == currentPublisher.LeagueYear.Year);
            var masterGameYearDictionary = masterGamesForThisYear.ToDictionary(x => x.MasterGame.MasterGameID, y => y);

            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
            LocalDate currentDate = _clock.GetToday();
            foreach (var queuedGame in queuedGames)
            {
                var masterGame = masterGameYearDictionary[queuedGame.MasterGame.MasterGameID];

                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame, currentDate);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, 
                    eligibilityFactors, currentDate);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public static PossibleMasterGameYear GetPossibleMasterGameYear(MasterGameYear masterGame, HashSet<MasterGame> publisherStandardMasterGames, 
            HashSet<MasterGame> myPublisherMasterGames, MasterGameWithEligibilityFactors eligibilityFactors, LocalDate currentDate)
        {
            bool isEligible = SlotEligibilityService.GameIsEligibleInLeagueYear(eligibilityFactors);
            bool taken = publisherStandardMasterGames.Contains(masterGame.MasterGame);
            bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
            bool isReleased = masterGame.MasterGame.IsReleased(currentDate);
            bool willRelease = masterGame.WillRelease();
            bool hasScore = masterGame.MasterGame.CriticScore.HasValue;

            PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
            return possibleMasterGame;
        }

        public PossibleMasterGameYear GetPossibleMasterGameYear(MasterGameYear masterGame, HashSet<MasterGame> publisherStandardMasterGames,
            HashSet<MasterGame> myPublisherMasterGames, MasterGameWithEligibilityFactors eligibilityFactors, IEnumerable<LeagueTagStatus> tagsForSlot, LocalDate currentDate)
        {
            var tagsToUse = eligibilityFactors.TagOverrides.Any() ? eligibilityFactors.TagOverrides : masterGame.MasterGame.Tags;
            var claimErrors = LeagueTagExtensions.GameHasValidTags(tagsForSlot, new List<LeagueTagStatus>(), masterGame.MasterGame, tagsToUse, currentDate);
            bool isEligible = !claimErrors.Any();
            bool taken = publisherStandardMasterGames.Contains(masterGame.MasterGame);
            bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
            bool isReleased = masterGame.MasterGame.IsReleased(currentDate);
            bool willRelease = masterGame.WillRelease();
            bool hasScore = masterGame.MasterGame.CriticScore.HasValue;

            PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
            return possibleMasterGame;
        }
    }
}
