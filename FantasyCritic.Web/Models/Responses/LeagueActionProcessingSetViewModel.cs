using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueActionProcessingSetViewModel
    {
        public LeagueActionProcessingSetViewModel(LeagueActionProcessingSet domain, LocalDate currentDate, SystemWideValues systemWideValues)
        {
            LeagueID = domain.LeagueYear.League.LeagueID;
            LeagueName = domain.LeagueYear.League.LeagueName;
            Year = domain.LeagueYear.Year;
            ProcessSetID = domain.ProcessSetID;
            ProcessTime = domain.ProcessTime;
            ProcessName = domain.ProcessName;
            Drops = domain.Drops.Select(x => new DropGameRequestViewModel(x, currentDate)).ToList();
            Bids = domain.Bids.Select(x => new PickupBidViewModel(x, currentDate, domain.LeagueYear.Options.ScoringSystem, systemWideValues)).ToList();
        }

        public Guid LeagueID { get; }
        public string LeagueName { get; }
        public int Year { get; }
        public Guid ProcessSetID { get; }
        public Instant ProcessTime { get; }
        public string ProcessName { get; }
        public IReadOnlyList<DropGameRequestViewModel> Drops { get; }
        public IReadOnlyList<PickupBidViewModel> Bids { get; }
    }
}
