using System;
using System.Collections.Generic;
using System.Text;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Domain
{
    public class PlayerGame
    {
        public PlayerGame(string gameName, int rosterSlot, bool waiver, bool antiPick, decimal fantasyScore)
        {
            GameName = gameName;
            RosterSlot = rosterSlot;
            Waiver = waiver;
            AntiPick = antiPick;
            FantasyScore = fantasyScore;
            MasterGame = Maybe<MasterGame>.None;
        }

        public PlayerGame(string gameName, int rosterSlot, bool waiver, bool antiPick, decimal fantasyScore,
            MasterGame masterGame)
        {
            GameName = gameName;
            RosterSlot = rosterSlot;
            Waiver = waiver;
            AntiPick = antiPick;
            FantasyScore = fantasyScore;
            MasterGame = masterGame;
        }

        public string GameName { get; }
        public int RosterSlot { get; }
        public bool Waiver { get; }
        public bool AntiPick { get; }
        public decimal FantasyScore { get; }
        public Maybe<MasterGame> MasterGame { get; }
    }
}
