using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.FakeRepo
{
    public class FakeRoyaleRepo : IRoyaleRepo
    {
        private readonly FakeMasterGameRepo _fakeMasterGameRepo;

        public FakeRoyaleRepo(FakeMasterGameRepo fakeMasterGameRepo)
        {
            _fakeMasterGameRepo = fakeMasterGameRepo;
        }

        public Task CreatePublisher(RoyalePublisher publisher)
        {
            throw new NotImplementedException();
        }

        public Task ChangePublisherName(RoyalePublisher publisher, string publisherName)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RoyaleYearQuarter>> GetQuartersWonByUser(FantasyCriticUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyDictionary<FantasyCriticUser, IReadOnlyList<RoyaleYearQuarter>>> GetRoyaleWinners()
        {
            throw new NotImplementedException();
        }

        public Task StartNewQuarter(YearQuarter nextQuarter)
        {
            throw new NotImplementedException();
        }

        public Task FinishQuarter(RoyaleYearQuarter supportedQuarter)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters()
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<RoyalePublisher>> GetPublisher(Guid publisherID)
        {
            throw new NotImplementedException();
        }

        public Task PurchaseGame(RoyalePublisherGame game)
        {
            throw new NotImplementedException();
        }

        public Task SellGame(RoyalePublisherGame publisherGame, bool fullRefund)
        {
            throw new NotImplementedException();
        }

        public Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RoyalePublisher>> GetAllPublishers(int year, int quarter)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFantasyPoints(Dictionary<(Guid, Guid), decimal?> publisherGameScores)
        {
            throw new NotImplementedException();
        }

        public Task ChangePublisherIcon(RoyalePublisher publisher, Maybe<string> publisherIcon)
        {
            throw new NotImplementedException();
        }
    }
}
