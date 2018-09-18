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
            WaiverGames = leagueYear.Options.WaiverGames;
            CounterPicks = leagueYear.Options.CounterPicks;
            EstimatedGameScore = leagueYear.Options.EstimatedGameScore;
            EligibilitySystem = leagueYear.Options.EligibilitySystem.Value;
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            WaiverSystem = leagueYear.Options.WaiverSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Value;
            Publishers = publishers.OrderBy(x => x.DraftPosition).Select(x => new PublisherViewModel(x, clock)).ToList();
            Standings = publishers.OrderByDescending(x => x.TotalFantasyScore).Select(x => new StandingViewModel(x)).ToList();
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedGameScore { get; }
        public string EligibilitySystem { get; }
        public string DraftSystem { get; }
        public string WaiverSystem { get; }
        public string ScoringSystem { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public IReadOnlyList<StandingViewModel> Standings { get; }
    }
}
