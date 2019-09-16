﻿using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Royale;
using MoreLinq;
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

        public async Task<RoyaleYearQuarter> GetActiveYearQuarter()
        {
            IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await GetYearQuarters();
            var activeQuarter = supportedQuarters.Where(x => x.OpenForPlay).MaxBy(x => x.YearQuarter).Single();
            return activeQuarter;
        }

        public async Task<Maybe<RoyaleYearQuarter>> GetYearQuarter(int year, int quarter)
        {
            IReadOnlyList<RoyaleYearQuarter> supportedQuarters = await GetYearQuarters();
            var requestedQuarter = supportedQuarters.SingleOrDefault(x => x.YearQuarter.Year == year && x.YearQuarter.Quarter == quarter);
            return requestedQuarter;
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

        public async Task<Result> SellGame(RoyalePublisher publisher, RoyalePublisherGame publisherGame)
        {
            if (publisherGame.MasterGame.MasterGame.IsReleased(_clock))
            {
                return Result.Fail("Game has been released.");
            }
            if (publisherGame.MasterGame.MasterGame.CriticScore.HasValue)
            {
                return Result.Fail("Game has a score.");
            }

            if (!publisher.PublisherGames.Contains(publisherGame))
            {
                return Result.Fail("Publisher doesn't have that game.");
            }

            await _royaleRepo.SellGame(publisherGame);
            return Result.Ok();
        }

        public async Task<Result> SetAdvertisingMoney(RoyalePublisher publisher, RoyalePublisherGame publisherGame, decimal advertisingMoney)
        {
            if (publisherGame.MasterGame.MasterGame.IsReleased(_clock))
            {
                return Result.Fail("Game has been released.");
            }
            if (publisherGame.MasterGame.MasterGame.CriticScore.HasValue)
            {
                return Result.Fail("Game has a score.");
            }

            if (!publisher.PublisherGames.Contains(publisherGame))
            {
                return Result.Fail("Publisher doesn't have that game.");
            }

            if (advertisingMoney > 10m)
            {
                return Result.Fail("Can't allocate more than 10 dollars in advertising money.");
            }

            await _royaleRepo.SetAdvertisingMoney(publisherGame, advertisingMoney);
            return Result.Ok();
        }
    }
}
