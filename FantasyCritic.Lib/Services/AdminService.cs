﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using FantasyCritic.Lib.Statistics;
using FantasyCritic.Stats;
using Microsoft.Extensions.Logging;
using NodaTime;
using RDotNet;

namespace FantasyCritic.Lib.Services
{
    public class AdminService
    {
        private readonly FantasyCriticService _fantasyCriticService;
        private readonly IFantasyCriticRepo _fantasyCriticRepo;
        private readonly IMasterGameRepo _masterGameRepo;
        private readonly InterLeagueService _interLeagueService;
        private readonly IOpenCriticService _openCriticService;
        private readonly IClock _clock;
        private readonly ILogger<OpenCriticService> _logger;

        public AdminService(FantasyCriticService fantasyCriticService, IFantasyCriticRepo fantasyCriticRepo, IMasterGameRepo masterGameRepo,
            InterLeagueService interLeagueService, IOpenCriticService openCriticService, IClock clock, ILogger<OpenCriticService> logger)
        {
            _fantasyCriticService = fantasyCriticService;
            _fantasyCriticRepo = fantasyCriticRepo;
            _masterGameRepo = masterGameRepo;
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

                await _fantasyCriticService.UpdateFantasyPoints(supportedYear.Year);
            }

            _logger.LogInformation("Done updating fantasy points");

            await Task.Delay(1000);
            await RefreshCaches();
        }

        public async Task RefreshCaches()
        {
            _logger.LogInformation("Refreshing caches");

            LocalDate tomorrow = _clock.GetToday().PlusDays(1);
            await _masterGameRepo.UpdateReleaseDateEstimates(tomorrow);

            await _fantasyCriticRepo.UpdateSystemWideValues();
            var hypeConstants = await GetHypeConstants();
            await UpdateHypeFactor(hypeConstants);
            _logger.LogInformation("Done refreshing caches");
        }

        private async Task<HypeConstants> GetHypeConstants()
        {
            REngine.SetEnvironmentVariables();
            var engine = REngine.GetInstance();
            string rscript = Encoding.UTF8.GetString(Resource.MasterGameStatisticsScript);

            var masterGames = await _masterGameRepo.GetMasterGameYears(2019, true);

            var gamesToOutput = masterGames
                .Where(x => x.Year == 2019)
                .Where(x => x.DateAdjustedHypeFactor > 0)
                .Where(x => !x.WillRelease() || x.MasterGame.CriticScore.HasValue);

            var outputModels = gamesToOutput.Select(x => new MasterGameYearRInput(x));

            string fileName = Guid.NewGuid() + ".csv";
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(outputModels);
            }

            var args_r = new string[1] { fileName };
            engine.SetCommandLineArguments(args_r);

            CharacterVector vector = engine.Evaluate(rscript).AsCharacter();
            string result = vector[0];
            var splitString = result.Split(' ');

            double baseScore = double.Parse(splitString[2]);
            double counterPickConstant = double.Parse(splitString[4]);
            double bidPercentileConstant = double.Parse(splitString[8]);
            double hypeFactorConstant = double.Parse(splitString[12]);

            HypeConstants hypeConstants = new HypeConstants(baseScore, counterPickConstant, bidPercentileConstant, hypeFactorConstant);

            return hypeConstants;
        }

        private async Task UpdateHypeFactor(HypeConstants hypeConstants)
        {
            var supportedYears = await _interLeagueService.GetSupportedYears();
            foreach (var supportedYear in supportedYears)
            {
                List<MasterGameHypeScores> hypeScores = new List<MasterGameHypeScores>();
                var masterGames = await _masterGameRepo.GetMasterGameYears(supportedYear.Year, false);
                foreach (var masterGame in masterGames)
                {
                    double notNullAverageDraftPosition = masterGame.AverageDraftPosition ?? 0;
                    double notNullAverageWinningBid = masterGame.AverageWinningBid ?? 0;

                    double hypeFactor = (101 - notNullAverageDraftPosition) * masterGame.PercentStandardGame;
                    double dateAdjustedHypeFactor = (101 - notNullAverageDraftPosition) * masterGame.EligiblePercentStandardGame;

                    double counterPickCalulation = masterGame.EligiblePercentCounterPick * hypeConstants.CounterPickConstant;
                    double bidPercentileCalculation = masterGame.BidPercentile * hypeConstants.BidPercentileConstant;
                    double hypeFactorCalculation = dateAdjustedHypeFactor * hypeConstants.HypeFactorConstant;

                    double linearRegressionHypeFactor = hypeConstants.BaseScore - counterPickCalulation + bidPercentileCalculation + hypeFactorCalculation;

                    hypeScores.Add(new MasterGameHypeScores(masterGame, hypeFactor, dateAdjustedHypeFactor, linearRegressionHypeFactor));
                }

                await _masterGameRepo.UpdateHypeFactors(hypeScores, supportedYear.Year);
            }
        }
    }
}
