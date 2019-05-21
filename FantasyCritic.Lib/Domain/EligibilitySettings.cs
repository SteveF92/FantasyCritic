using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Domain
{
    public class EligibilitySettings
    {
        public EligibilitySettings(EligibilityLevel eligibilityLevel, bool yearlyInstallment, bool earlyAccess, 
            bool freeToPlay, bool releasedInternationally, bool expansionPack)
        {
            EligibilityLevel = eligibilityLevel;
            YearlyInstallment = yearlyInstallment;
            EarlyAccess = earlyAccess;
            FreeToPlay = freeToPlay;
            ReleasedInternationally = releasedInternationally;
            ExpansionPack = expansionPack;
        }

        public EligibilityLevel EligibilityLevel { get; }
        public bool YearlyInstallment { get; }
        public bool EarlyAccess { get; }
        public bool FreeToPlay { get; }
        public bool ReleasedInternationally { get; }
        public bool ExpansionPack { get; }
    }
}
