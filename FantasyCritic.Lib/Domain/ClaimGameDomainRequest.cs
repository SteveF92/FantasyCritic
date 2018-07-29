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
        public ClaimGameDomainRequest(League league, FantasyCriticUser user, int year, string gameName, bool waiver, bool antiPick, Maybe<MasterGame> masterGame)
        {
            League = league;
            User = user;
            Year = year;
            GameName = gameName;
            Waiver = waiver;
            AntiPick = antiPick;
            MasterGame = masterGame;
        }

        public League League { get; }
        public FantasyCriticUser User { get; }
        public int Year { get; }
        public string GameName { get; }
        public bool Waiver { get; }
        public bool AntiPick { get; }
        public Maybe<MasterGame> MasterGame { get; }
    }
}
