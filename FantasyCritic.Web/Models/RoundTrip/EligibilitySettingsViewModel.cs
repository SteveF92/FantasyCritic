using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Web.Models.Responses;

namespace FantasyCritic.Web.Models.RoundTrip
{
    public class EligibilitySettingsViewModel
    {
        public EligibilitySettingsViewModel()
        {

        }

        public EligibilitySettingsViewModel(EligibilitySettings eligibilitySettings, bool includeExamples)
        {
            EligibilityLevel = new EligibilityLevelViewModel(eligibilitySettings.EligibilityLevel, includeExamples);
            YearlyInstallment = eligibilitySettings.YearlyInstallment;
            EarlyAccess = eligibilitySettings.EarlyAccess;
            FreeToPlay = eligibilitySettings.FreeToPlay;
            ReleasedInternationally = eligibilitySettings.ReleasedInternationally;
            ExpansionPack = eligibilitySettings.ExpansionPack;
        }

        public EligibilityLevelViewModel EligibilityLevel { get; set; }
        public bool YearlyInstallment { get; set;  }
        public bool EarlyAccess { get; set; }
        public bool FreeToPlay { get; set; }
        public bool ReleasedInternationally { get; set; }
        public bool ExpansionPack { get; set; }

        public EligibilitySettings ToDomain(EligibilityLevel eligibilityLevel)
        {
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack);
            return eligibilitySettings;
        }
    }
}
