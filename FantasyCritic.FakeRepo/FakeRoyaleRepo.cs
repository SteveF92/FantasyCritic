using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.FakeRepo
{
    public class FakeRoyaleRepo : IRoyaleRepo
    {
        private readonly FakeFantasyCriticUserStore _userStore;
        private readonly FakeMasterGameRepo _fakeMasterGameRepo;

        public FakeRoyaleRepo(FakeFantasyCriticUserStore userStore, FakeMasterGameRepo fakeMasterGameRepo)
        {
            _userStore = userStore;
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

        public Task SellGame(RoyalePublisherGame publisherGame)
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
    }
}
