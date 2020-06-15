using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
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

        public async Task<IReadOnlyList<PossibleMasterGameYear>> SearchGames(string searchName, Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year, true);
            IReadOnlyList<MasterGameYear> matchingMasterGames = MasterGameSearching.SearchMasterGameYears(searchName, masterGames);
            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();

            foreach (var masterGame in matchingMasterGames)
            {
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames,
                    myPublisherMasterGames, currentPublisher.LeagueYear.Options.AllowedEligibilitySettings);
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

            IReadOnlyList<MasterGameYear> masterGames = await _interLeagueService.GetMasterGameYears(year, true);
            IReadOnlyList<MasterGameYear> matchingMasterGames = masterGames.OrderByDescending(x => x.DateAdjustedHypeFactor).ToList();

            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
            foreach (var masterGame in matchingMasterGames)
            {
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames,
                    myPublisherMasterGames, currentPublisher.LeagueYear.Options.AllowedEligibilitySettings);

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

            IReadOnlyList<MasterGameYear> masterGameYears = await _interLeagueService.GetMasterGameYears(currentPublisher.LeagueYear.Year, true);
            var masterGamesForThisYear = masterGameYears.Where(x => x.Year == currentPublisher.LeagueYear.Year);
            var masterGameYearDictionary = masterGamesForThisYear.ToDictionary(x => x.MasterGame.MasterGameID, y => y);

            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
            foreach (var queuedGame in queuedGames)
            {
                var masterGame = masterGameYearDictionary[queuedGame.MasterGame.MasterGameID];

                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, 
                    myPublisherMasterGames, currentPublisher.LeagueYear.Options.AllowedEligibilitySettings);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public PossibleMasterGameYear GetPossibleMasterGameYear(MasterGameYear masterGame, HashSet<MasterGame> publisherStandardMasterGames, HashSet<MasterGame> myPublisherMasterGames,
            EligibilitySettings eligibilitySettings)
        {
            var eligibilityErrors = eligibilitySettings.GameIsEligible(masterGame.MasterGame);
            bool isEligible = !eligibilityErrors.Any();
            bool taken = publisherStandardMasterGames.Contains(masterGame.MasterGame);
            bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
            bool isReleased = masterGame.MasterGame.IsReleased(_clock.GetCurrentInstant());
            bool willRelease = masterGame.WillRelease();
            bool hasScore = masterGame.MasterGame.CriticScore.HasValue;

            PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
            return possibleMasterGame;
        }
    }
}
