using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueOptionsViewModel
    {
        public LeagueOptionsViewModel(IEnumerable<int> openYears, IEnumerable<DraftSystem> draftSystems,
            IEnumerable<PickupSystem> pickupSystems, IEnumerable<ScoringSystem> scoringSystems)
        {
            OpenYears = openYears.ToList();
            DraftSystems = draftSystems.Select(x => x.Value).ToList();
            PickupSystems = pickupSystems.Select(x => new SelectOptionViewModel(x.Value, x.ReadableName)).ToList();
            ScoringSystems = scoringSystems.Select(x => x.Name).ToList();
        }

        public IReadOnlyList<int> OpenYears { get; }
        public IReadOnlyList<string> DraftSystems { get; }
        public IReadOnlyList<SelectOptionViewModel> PickupSystems { get; }
        public IReadOnlyList<string> ScoringSystems { get; }
    }
}
