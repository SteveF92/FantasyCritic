using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueOptionsViewModel
    {
        public LeagueOptionsViewModel(IEnumerable<EligibilitySystem> eligibilitySystems,
            IEnumerable<DraftSystem> draftSystems, IEnumerable<WaiverSystem> waiverSystems,
            IEnumerable<ScoringSystem> scoringSystems)
        {
            EligibilitySystems = eligibilitySystems.Select(x => x.Value).ToList();
            DraftSystems = draftSystems.Select(x => x.Value).ToList();
            WaiverSystems = waiverSystems.Select(x => x.Value).ToList();
            ScoringSystems = scoringSystems.Select(x => x.Value).ToList();
        }

        public IReadOnlyList<string> EligibilitySystems { get; }
        public IReadOnlyList<string> DraftSystems { get; }
        public IReadOnlyList<string> WaiverSystems { get; }
        public IReadOnlyList<string> ScoringSystems { get; }
    }
}
