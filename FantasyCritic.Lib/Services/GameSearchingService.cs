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

            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, currentPublisher.LeagueYear, eligibilityFactors);
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
            foreach (var masterGame in matchingMasterGames)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, currentPublisher.LeagueYear, eligibilityFactors);

                if (!possibleMasterGame.IsAvailable)
                {
                    continue;
                }

                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public IReadOnlyList<PossibleMasterGameYear> GetPossibleCounterPicks(Publisher currentPublisher, IReadOnlyList<Publisher> publishersInLeagueForYear, int year)
        {
            HashSet<MasterGame> publisherMasterGames = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value.MasterGame)
                .ToHashSet();

            HashSet<MasterGame> myPublisherMasterGames = currentPublisher.MyMasterGames;

            List<MasterGameYear> existingCounterPicks = publishersInLeagueForYear
                .SelectMany(x => x.PublisherGames)
                .Where(x => x.CounterPick && x.MasterGame.HasValue)
                .Select(x => x.MasterGame.Value)
                .ToList();
            IEnumerable<Publisher> otherPublishersInLeague = publishersInLeagueForYear.Except(new List<Publisher>() { currentPublisher });
            List<MasterGameYear> existingStandardGamesForOtherPlayers = otherPublishersInLeague
                .SelectMany(x => x.PublisherGames)
                .Where(x => !x.CounterPick)
                .Select(x => x.MasterGame.Value)
                .ToList();

            var availableCounterPicks = existingStandardGamesForOtherPlayers
                .Except(existingCounterPicks);

            List<PossibleMasterGameYear> possibleMasterGames = new List<PossibleMasterGameYear>();
            foreach (var masterGame in availableCounterPicks)
            {
                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, currentPublisher.LeagueYear, eligibilityFactors);

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
            foreach (var queuedGame in queuedGames)
            {
                var masterGame = masterGameYearDictionary[queuedGame.MasterGame.MasterGameID];

                var eligibilityFactors = currentPublisher.LeagueYear.GetEligibilityFactorsForMasterGame(masterGame.MasterGame);
                PossibleMasterGameYear possibleMasterGame = GetPossibleMasterGameYear(masterGame, publisherMasterGames, myPublisherMasterGames, currentPublisher.LeagueYear, eligibilityFactors);
                possibleMasterGames.Add(possibleMasterGame);
            }

            return possibleMasterGames;
        }

        public PossibleMasterGameYear GetPossibleMasterGameYear(MasterGameYear masterGame, HashSet<MasterGame> publisherStandardMasterGames, 
            HashSet<MasterGame> myPublisherMasterGames, LeagueYear leagueYear, MasterGameWithEligibilityFactors eligibilityFactors)
        {
            bool isEligible = SlotEligibilityService.GameIsEligibleInLeagueYear(leagueYear, eligibilityFactors);
            bool taken = publisherStandardMasterGames.Contains(masterGame.MasterGame);
            bool alreadyOwned = myPublisherMasterGames.Contains(masterGame.MasterGame);
            var currentDate = _clock.GetToday();
            bool isReleased = masterGame.MasterGame.IsReleased(currentDate);
            bool willRelease = masterGame.WillRelease();
            bool hasScore = masterGame.MasterGame.CriticScore.HasValue;

            PossibleMasterGameYear possibleMasterGame = new PossibleMasterGameYear(masterGame, taken, alreadyOwned, isEligible, isReleased, willRelease, hasScore);
            return possibleMasterGame;
        }
    }
}
