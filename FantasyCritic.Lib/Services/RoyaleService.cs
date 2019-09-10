using FantasyCritic.Lib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Services
{
    public class RoyaleService
    {
        private readonly IRoyaleRepo _royaleRepo;

        public RoyaleService(IRoyaleRepo royaleRepo)
        {
            _royaleRepo = royaleRepo;
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

        public Task<RoyalePublisherGame> PurchaseGame(RoyalePublisher publisher, MasterGame masterGame)
        {
            throw new NotImplementedException();
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
