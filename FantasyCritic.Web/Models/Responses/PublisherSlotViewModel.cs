using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherSlotViewModel
    {
        public PublisherSlotViewModel(PublisherSlot slot, LocalDate currentDate, LeagueYear leagueYear, SystemWideValues systemWideValues)
        {
            SlotNumber = slot.SlotNumber;
            OverallSlotNumber = slot.OverallSlotNumber;
            CounterPick = slot.CounterPick;
            SpecialSlot = slot.SpecialGameSlot.GetValueOrDefault(x => new SpecialGameSlotViewModel(x));
            PublisherGame = slot.PublisherGame.GetValueOrDefault(x => new PublisherGameViewModel(x, currentDate));

            GameMeetsSlotCriteria = slot.SlotHasEligibleGame(leagueYear);

            SimpleProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(leagueYear, systemWideValues, true, currentDate);
            AdvancedProjectedFantasyPoints = slot.GetProjectedOrRealFantasyPoints(leagueYear, systemWideValues, false, currentDate);
        }

        public int SlotNumber { get; }
        public int OverallSlotNumber { get; }
        public bool CounterPick { get; }
        public SpecialGameSlotViewModel SpecialSlot { get; }
        public PublisherGameViewModel PublisherGame { get; }
        public bool GameMeetsSlotCriteria { get; }

        public decimal? SimpleProjectedFantasyPoints { get; }
        public decimal? AdvancedProjectedFantasyPoints { get; }
    }
}
