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
    }
}
