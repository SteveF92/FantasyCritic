using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueOptions
    {
        public LeagueOptions(LeagueCreationParameters parameters)
        {
            RosterSize = parameters.RosterSize;
            EstimatedGameScore = parameters.EstimatedGameScore;
            EligibilitySystem = parameters.EligibilitySystem;
            DraftSystem = parameters.DraftSystem;
            WaiverSystem = parameters.WaiverSystem;
            ScoringSystem = parameters.ScoringSystem;
        }

        public int RosterSize { get; }
        public decimal EstimatedGameScore { get; }
        public EligibilitySystem EligibilitySystem { get; }
        public DraftSystem DraftSystem { get; }
        public WaiverSystem WaiverSystem { get; }
        public ScoringSystem ScoringSystem { get; }
    }
}
