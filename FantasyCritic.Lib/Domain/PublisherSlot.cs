using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain
{
    public class PublisherSlot
    {
        public PublisherSlot(bool counterPick, Maybe<SpecialGameSlot> specialGameSlot, Maybe<PublisherGame> publisherGame)
        {
            CounterPick = counterPick;
            SpecialGameSlot = specialGameSlot;
            PublisherGame = publisherGame;
        }

        public bool CounterPick { get; }
        public Maybe<SpecialGameSlot> SpecialGameSlot { get; }
        public Maybe<PublisherGame> PublisherGame { get; }
    }
}
