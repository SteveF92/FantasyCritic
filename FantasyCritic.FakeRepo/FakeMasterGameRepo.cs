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
using NodaTime.Text;

namespace FantasyCritic.FakeRepo
{
    public class FakeMasterGameRepo : IMasterGameRepo
    {
        private readonly FakeFantasyCriticUserStore _userStore;
        private readonly List<MasterGame> _masterGames;

        public FakeMasterGameRepo(FakeFantasyCriticUserStore userStore)
        {
            _userStore = userStore;
            _masterGames = PopulateMasterGames();
        }

        private List<MasterGame> PopulateMasterGames()
        {
            List<MasterGame> games = new List<MasterGame>();

            var sekiro = new MasterGame(Guid.Parse("96f5e8e3-672b-4626-b47e-4bff3a6c4430"), "Sekiro: Shadows Die Twice",
                "2019-03-22", new LocalDate(2019, 3, 22), 6630,
                89.9200m, 2019, new EligibilityLevel(1, "New Game", "A new Game", new List<string>()), false, false,
                null, InstantPattern.ExtendedIso.Parse("2019-03-20T15:30:00Z").GetValueOrThrow(), false,
                InstantPattern.ExtendedIso.Parse("2019-01-22T15:30:00Z").GetValueOrThrow());
            games.Add(sekiro);

            return games;
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
            var masterGameYears = _masterGames.Select(x => new MasterGameYear(x, year)).ToList();
            return Task.FromResult<IReadOnlyList<MasterGameYear>>(masterGameYears);
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
