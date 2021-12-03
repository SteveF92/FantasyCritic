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
        public PublisherSlotViewModel(PublisherSlot domain, LocalDate currentDate, ScoringSystem scoringSystem, SystemWideValues systemWideValues)
        {
            CounterPick = domain.CounterPick;
            SpecialSlot = domain.SpecialGameSlot.GetValueOrDefault(x => new SpecialGameSlotViewModel(x));
            PublisherGame = domain.PublisherGame.GetValueOrDefault(x => new PublisherGameViewModel(x, currentDate, scoringSystem, systemWideValues));
        }

        public bool CounterPick { get; }
        public SpecialGameSlotViewModel SpecialSlot { get; }
        public PublisherGameViewModel PublisherGame { get; }
    }
}
