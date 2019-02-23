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

namespace FantasyCritic.Lib.Services
{
    public class InterLeagueService
    {
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IMasterGameRepo _masterGameRepo;

        public InterLeagueService(IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo)
        {
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
        }

        public Task<SystemWideSettings> GetSystemWideSettings()
        {
            return _fantasyCriticRepo.GetSystemWideSettings();
        }

        public Task<SystemWideValues> GetSystemWideValues()
        {
            return _fantasyCriticRepo.GetSystemWideValues();
        }

        public Task<SiteCounts> GetSiteCounts()
        {
            return _fantasyCriticRepo.GetSiteCounts();
        }

        public Task CreateMasterGame(MasterGame masterGame)
        {
            return _masterGameRepo.CreateMasterGame(masterGame);
        }

        public Task<IReadOnlyList<SupportedYear>> GetSupportedYears()
        {
            return _fantasyCriticRepo.GetSupportedYears();
        }

        public Task<IReadOnlyList<MasterGame>> GetMasterGames()
        {
            return _masterGameRepo.GetMasterGames();
        }

        public Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
        {
            return _masterGameRepo.GetMasterGameYears(year);
        }

        public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
        {
            return _masterGameRepo.GetMasterGame(masterGameID);
        }

        public Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year)
        {
            return _masterGameRepo.GetMasterGameYear(masterGameID, year);
        }

        public Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
        {
            return _masterGameRepo.GetAllSelectedMasterGameIDsForYear(year);
        }

        public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
        {
            return _masterGameRepo.UpdateCriticStats(masterGame, openCriticGame);
        }

        public Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
        {
            return _masterGameRepo.UpdateCriticStats(masterSubGame, openCriticGame);
        }

        public Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel)
        {
            return _masterGameRepo.GetEligibilityLevel(eligibilityLevel);
        }

        public Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels()
        {
            return _masterGameRepo.GetEligibilityLevels();
        }
    }
}
