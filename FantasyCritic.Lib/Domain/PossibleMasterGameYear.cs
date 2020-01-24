﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class PossibleMasterGameYear
    {
        public PossibleMasterGameYear(MasterGameYear masterGame, bool taken, bool alreadyOwned, bool isEligible, bool isReleased, bool willRelease, bool hasScore)
        {
            MasterGame = masterGame;
            Taken = taken;
            AlreadyOwned = alreadyOwned;
            IsEligible = isEligible;
            IsReleased = isReleased;
            WillRelease = willRelease;
            HasScore = hasScore;
        }

        public MasterGameYear MasterGame { get; }
        public bool Taken { get; }
        public bool AlreadyOwned { get; }
        public bool IsEligible { get; }
        public bool IsReleased { get; }
        public bool WillRelease { get; }
        public bool HasScore { get; }

        public bool IsAvailable => !Taken && !AlreadyOwned && IsEligible && !IsReleased && WillRelease && !HasScore;
    }
}
