using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueOptionsViewModel
    {
        public LeagueOptionsViewModel(IEnumerable<int> openYears, IEnumerable<EligibilitySystem> eligibilitySystems,
            IEnumerable<DraftSystem> draftSystems, IEnumerable<WaiverSystem> waiverSystems,
            IEnumerable<ScoringSystem> scoringSystems)
        {
            OpenYears = openYears.ToList();
            EligibilitySystems = eligibilitySystems.Select(x => x.Value).ToList();
            DraftSystems = draftSystems.Select(x => x.Value).ToList();
            WaiverSystems = waiverSystems.Select(x => x.Value).ToList();
            ScoringSystems = scoringSystems.Select(x => x.Name).ToList();
        }

        public IReadOnlyList<int> OpenYears { get; }
        public IReadOnlyList<string> EligibilitySystems { get; }
        public IReadOnlyList<string> DraftSystems { get; }
        public IReadOnlyList<string> WaiverSystems { get; }
        public IReadOnlyList<string> ScoringSystems { get; }
    }
}
