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

        public Task<RoyalePublisher> CreatePublisher()
        {

        }

        public Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user)
        {

        }

        public Task<RoyalePublisherGame> PurchaseGame(RoyalePublisher publisher, MasterGame masterGame)
        {

        }

        public Task SellGame(RoyalePublisher publisher, RoyalePublisherGame game)
        {

        }

        public Task SetAdvertising(RoyalePublisher publisher, RoyalePublisherGame game, decimal advertisingMoney)
        {

        }
    }
}
