using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using NodaTime;

namespace FantasyCritic.FakeRepo
{
    public class FakeMasterGameRepo : IMasterGameRepo
    {
        private readonly FakeFantasyCriticUserStore _userStore;

        public FakeMasterGameRepo(FakeFantasyCriticUserStore userStore)
        {
            _userStore = userStore;
        }

        public Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote, Maybe<MasterGame> masterGame)
        {
            throw new NotImplementedException();
        }

        public Task CreateMasterGame(MasterGame masterGame)
        {
            throw new NotImplementedException();
        }

        public Task CreateMasterGameRequest(MasterGameRequest domainRequest)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMasterGameRequest(MasterGameRequest request)
        {
            throw new NotImplementedException();
        }

        public Task DismissMasterGameRequest(MasterGameRequest masterGameRequest)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<MasterGameRequest>> GetAllMasterGameRequests()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
        {
            throw new NotImplementedException();
        }

        public Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<MasterGameRequest>> GetMasterGameRequest(Guid requestID)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            throw new NotImplementedException();
        }

        public Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
        {
            throw new NotImplementedException();
        }
    }
}
