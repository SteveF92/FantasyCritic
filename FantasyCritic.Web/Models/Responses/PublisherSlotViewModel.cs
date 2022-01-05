using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Services;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherSlotViewModel
    {
        public PublisherSlotViewModel(int year, PublisherSlot slot, LocalDate currentDate, LeagueYear leagueYear,
            SystemWideValues systemWideValues, IReadOnlySet<Guid> dropBlockedPublisherGameIDs)
        {
            SlotNumber = slot.SlotNumber;
            OverallSlotNumber = slot.OverallSlotNumber;
            CounterPick = slot.CounterPick;
            SpecialSlot = slot.SpecialGameSlot.GetValueOrDefault(x => new SpecialGameSlotViewModel(x));
            PublisherGame = slot.PublisherGame.GetValueOrDefault(x => new PublisherGameViewModel(x, currentDate, dropBlockedPublisherGameIDs.Contains(x.PublisherGameID)));

            EligibilityErrors = slot.GetClaimErrorsForSlot(leagueYear).Select(x => x.Error).ToList();
            GameMeetsSlotCriteria = !EligibilityErrors.Any();

            var ineligiblePointsShouldCount = !SupportedYear.Year2022FeatureSupported(year);
            SimpleProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(GameMeetsSlotCriteria || ineligiblePointsShouldCount, leagueYear.Options.ScoringSystem, systemWideValues, true, currentDate);
            AdvancedProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(GameMeetsSlotCriteria || ineligiblePointsShouldCount, leagueYear.Options.ScoringSystem, systemWideValues, false, currentDate);
        }

        public int SlotNumber { get; }
        public int OverallSlotNumber { get; }
        public bool CounterPick { get; }
        public SpecialGameSlotViewModel SpecialSlot { get; }
        public PublisherGameViewModel PublisherGame { get; }
        public IReadOnlyList<string> EligibilityErrors { get; }
        public bool GameMeetsSlotCriteria { get; }

        public decimal? SimpleProjectedFantasyPoints { get; }
        public decimal? AdvancedProjectedFantasyPoints { get; }
    }
}
