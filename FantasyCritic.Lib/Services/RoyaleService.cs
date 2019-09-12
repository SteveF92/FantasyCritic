using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Royale;
using NLog.Targets.Wrappers;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class RoyaleService
    {
        private readonly IRoyaleRepo _royaleRepo;
        private readonly IClock _clock;

        public const int MAX_GAMES = 25;

        public RoyaleService(IRoyaleRepo royaleRepo, IClock clock)
        {
            _royaleRepo = royaleRepo;
            _clock = clock;
        }

        public Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters()
        {
            return _royaleRepo.GetYearQuarters();
        }

        public async Task<RoyalePublisher> CreatePublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user, string publisherName)
        {
            RoyalePublisher publisher = new RoyalePublisher(Guid.NewGuid(), yearQuarter, user, publisherName, new List<RoyalePublisherGame>(), 200m);
            await _royaleRepo.CreatePublisher(publisher);
            return publisher;
        }

        public Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
        {
            return _royaleRepo.GetPublisher(yearQuarter, user);
        }

        public Task<Maybe<RoyalePublisher>> GetPublisher(Guid publisherID)
        {
            return _royaleRepo.GetPublisher(publisherID);
        }

        public async Task<Result<RoyalePublisherGame>> PurchaseGame(RoyalePublisher publisher, MasterGameYear masterGame)
        {
            if (publisher.PublisherGames.Count >= MAX_GAMES)
            {
                return Result.Fail<RoyalePublisherGame>("Roster is full.");
            }
            if (publisher.PublisherGames.Select(x => x.MasterGame).Contains(masterGame))
            {
                return Result.Fail<RoyalePublisherGame>("Publisher already has that game.");
            }
            if (!masterGame.WillReleaseInQuarter(publisher.YearQuarter.YearQuarter))
            {
                return Result.Fail<RoyalePublisherGame>("Game will not release this quarter.");
            }
            if (masterGame.MasterGame.IsReleased(_clock))
            {
                return Result.Fail<RoyalePublisherGame>("Game has been released.");
            }
            if (masterGame.MasterGame.CriticScore.HasValue)
            {
                return Result.Fail<RoyalePublisherGame>("Game has a score.");
            }

            var eligibilityErrors = EligibilitySettings.GetRoyaleEligibilitySettings().GameIsEligible(masterGame.MasterGame);
            if (eligibilityErrors.Any())
            {
                return Result.Fail<RoyalePublisherGame>("Game is not eligible under Royale rules.");
            }

            var currentBudget = publisher.Budget;
            var gameCost = masterGame.GetRoyaleGameCost();
            if (currentBudget < gameCost)
            {
                return Result.Fail<RoyalePublisherGame>("Not enough budget.");
            }

            RoyalePublisherGame game = new RoyalePublisherGame(publisher.PublisherID, publisher.YearQuarter, masterGame, _clock.GetCurrentInstant(), gameCost, 0m, 0m);
            await _royaleRepo.PurchaseGame(game);
            return Result.Ok(game);
        }

        public Task SellGame(RoyalePublisher publisher, RoyalePublisherGame game)
        {
            throw new NotImplementedException();
        }

        public Task SetAdvertising(RoyalePublisher publisher, RoyalePublisherGame game, decimal advertisingMoney)
        {
            throw new NotImplementedException();
        }
    }
}
