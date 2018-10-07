using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, IEnumerable<Publisher> publishers, IClock clock)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            DraftGames = leagueYear.Options.DraftGames;
            AcquisitionGames = leagueYear.Options.AcquisitionGames;
            CounterPicks = leagueYear.Options.CounterPicks;
            EstimatedCriticScore = leagueYear.Options.EstimatedCriticScore;
            MaximumEligibilityLevel = new EligibilityLevelViewModel(leagueYear.Options.MaximumEligibilityLevel, false);
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            AcquisitionSystem = leagueYear.Options.AcquisitionSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
            UnlinkedGameExists = publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame.HasNoValue);
            Publishers = publishers.OrderBy(x => x.DraftPosition).Select(x => new PublisherViewModel(x, clock)).ToList();
            Standings = publishers.OrderByDescending(x => x.TotalFantasyPoints).Select(x => new StandingViewModel(x, leagueYear.Options.ScoringSystem, leagueYear.Options.EstimatedCriticScore)).ToList();
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public int DraftGames { get; }
        public int AcquisitionGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public EligibilityLevelViewModel MaximumEligibilityLevel { get; }
        public string DraftSystem { get; }
        public string AcquisitionSystem { get; }
        public string ScoringSystem { get; }
        public bool UnlinkedGameExists { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public IReadOnlyList<StandingViewModel> Standings { get; }
    }
}
