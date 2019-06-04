﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.FakeRepo.Factories;
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
            _masterGames = MasterGameFactory.GetMasterGames();
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

        public Task CreateMasterGameChangeRequest(MasterGameChangeRequest domainRequest)
        {
            throw new NotImplementedException();
        }

        public Task<Maybe<MasterGameChangeRequest>> GetMasterGameChangeRequest(Guid requestID)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMasterGameRequest(MasterGameRequest request)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMasterGameChangeRequest(MasterGameChangeRequest request)
        {
            throw new NotImplementedException();
        }

        public Task DismissMasterGameRequest(MasterGameRequest masterGameRequest)
        {
            throw new NotImplementedException();
        }

        public Task DismissMasterGameChangeRequest(MasterGameChangeRequest request)
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
            return Task.FromResult(EligibilityLevelFactory.GetEligibilityLevels().Single(x => x.Level == eligibilityLevel));
        }

        public Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            return Task.FromResult(EligibilityLevelFactory.GetEligibilityLevels());
        }

        public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            Maybe<MasterGame> masterGame = _masterGames.SingleOrDefault(x => x.MasterGameID == masterGameID);
            return Task.FromResult(masterGame);
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
            return Task.FromResult<IReadOnlyList<MasterGame>>(_masterGames);
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
