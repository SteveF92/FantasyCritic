using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace FantasyCritic.Lib.Services
{
    public class AdminService
    {
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly InterLeagueService _interLeagueService;
        private readonly IOpenCriticService _openCriticService;
        private readonly IClock _clock;
        private readonly ILogger<OpenCriticService> _logger;

        public AdminService(FantasyCriticService fantasyCriticService, IFantasyCriticRepo fantasyCriticRepo, InterLeagueService interLeagueService, IOpenCriticService openCriticService, IClock clock, ILogger<OpenCriticService> logger)
        {
            _fantasyCriticService = fantasyCriticService;
            _fantasyCriticRepo = fantasyCriticRepo;
            _interLeagueService = interLeagueService;
            _openCriticService = openCriticService;
            _clock = clock;
            _logger = logger;
        }

        public async Task RefreshCriticInfo()
        {
            _logger.LogInformation("Refreshing critic scores");
            var supportedYears = await _interLeagueService.GetSupportedYears();
            var masterGames = await _interLeagueService.GetMasterGames();
            foreach (var masterGame in masterGames)
            {
                if (!masterGame.OpenCriticID.HasValue)
                {
                    continue;
                }

                if (masterGame.DoNotRefreshAnything)
                {
                    continue;
                }

                if (masterGame.IsReleased(_clock) && masterGame.ReleaseDate.HasValue)
                {
                    var year = masterGame.ReleaseDate.Value.Year;
                    var supportedYear = supportedYears.SingleOrDefault(x => x.Year == year);
                    if (supportedYear != null && supportedYear.Finished)
                    {
                        continue;
                    }
                }

                var openCriticGame = await _openCriticService.GetOpenCriticGame(masterGame.OpenCriticID.Value);
                if (openCriticGame.HasValue)
                {
                    await _interLeagueService.UpdateCriticStats(masterGame, openCriticGame.Value);
                }

                foreach (var subGame in masterGame.SubGames)
                {
                    if (!subGame.OpenCriticID.HasValue)
                    {
                        continue;
                    }

                    var subGameOpenCriticGame = await _openCriticService.GetOpenCriticGame(subGame.OpenCriticID.Value);

                    if (subGameOpenCriticGame.HasValue)
                    {
                        await _interLeagueService.UpdateCriticStats(subGame, subGameOpenCriticGame.Value);
                    }
                }
            }

            _logger.LogInformation("Done refreshing critic scores");

            await Task.Delay(1000);
            await RefreshCaches();
            await Task.Delay(1000);
            await UpdateFantasyPoints();
        }

        public async Task UpdateFantasyPoints()
        {
            _logger.LogInformation("Updating fantasy points");

            var systemWideValues = await _interLeagueService.GetSystemWideValues();
            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                if (supportedYear.Finished || !supportedYear.OpenForPlay)
                {
                    continue;
                }

                await _fantasyCriticService.UpdateFantasyPoints(systemWideValues, supportedYear.Year);
            }

            _logger.LogInformation("Done updating fantasy points");

            await Task.Delay(1000);
            await RefreshCaches();
        }

        public async Task RefreshCaches()
        {
            _logger.LogInformation("Refreshing caches");
            await _fantasyCriticRepo.UpdateReleaseDateEstimates();
            await _fantasyCriticRepo.UpdateSystemWideValues();
            await UpdateHypeFactor();
            _logger.LogInformation("Done refreshing caches");
        }

        private async Task UpdateHypeFactor()
        {

        }
    }
}
