using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, IEnumerable<Publisher> publishers)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            DraftGames = leagueYear.Options.DraftGames;
            WaiverGames = leagueYear.Options.WaiverGames;
            AntiPicks = leagueYear.Options.AntiPicks;
            EstimatedGameScore = leagueYear.Options.EstimatedGameScore;
            EligibilitySystem = leagueYear.Options.EligibilitySystem.Value;
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            WaiverSystem = leagueYear.Options.WaiverSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Value;
            Publishers = publishers.Select(x => new PublisherViewModel(x)).ToList();
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public int DraftGames { get; }
        public int WaiverGames { get; }
        public int AntiPicks { get; }
        public decimal EstimatedGameScore { get; }
        public string EligibilitySystem { get; }
        public string DraftSystem { get; }
        public string WaiverSystem { get; }
        public string ScoringSystem { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
    }
}
