using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Web.Models.RoundTrip;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueOptionsViewModel
    {
        public LeagueOptionsViewModel(IEnumerable<int> openYears, IEnumerable<DraftSystem> draftSystems,
            IEnumerable<PickupSystem> pickupSystems, IEnumerable<TiebreakSystem> tiebreakSystems,
            IEnumerable<ScoringSystem> scoringSystems, IEnumerable<TradingSystem> tradingSystems)
        {
            OpenYears = openYears.ToList();
            DraftSystems = draftSystems.Select(x => x.Value).ToList();
            PickupSystems = pickupSystems.Select(x => new SelectOptionViewModel(x.Value, x.ReadableName)).ToList();
            TiebreakSystems = tiebreakSystems.Select(x => new SelectOptionViewModel(x.Value, x.Value.CamelCaseToSpaces())).ToList();
            ScoringSystems = scoringSystems.Select(x => x.Name).ToList();
            TradingSystems = tradingSystems.Select(x => new SelectOptionViewModel(x.Value, x.ReadableName)).ToList();
        }

        public IReadOnlyList<int> OpenYears { get; }
        public IReadOnlyList<string> DraftSystems { get; }
        public IReadOnlyList<SelectOptionViewModel> PickupSystems { get; }
        public IReadOnlyList<SelectOptionViewModel> TiebreakSystems { get; }
        public IReadOnlyList<string> ScoringSystems { get; }
        public IReadOnlyList<SelectOptionViewModel> TradingSystems { get; }
    }
}
