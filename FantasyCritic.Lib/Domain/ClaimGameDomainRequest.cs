using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain
{
    public class ClaimGameDomainRequest
    {
        public ClaimGameDomainRequest(Publisher publisher, string gameName, bool waiver, bool counterPick, Maybe<MasterGame> masterGame)
        {
            Publisher = publisher;
            GameName = gameName;
            Waiver = waiver;
            CounterPick = counterPick;
            MasterGame = masterGame;
        }

        public Publisher Publisher { get; }
        public string GameName { get; }
        public bool Waiver { get; }
        public bool CounterPick { get; }
        public Maybe<MasterGame> MasterGame { get; }
    }
}
