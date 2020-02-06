using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Domain
{
    public class EligibilitySettings
    {
        public EligibilitySettings(EligibilityLevel eligibilityLevel, bool yearlyInstallment, bool earlyAccess, 
            bool freeToPlay, bool releasedInternationally, bool expansionPack, bool unannouncedGame)
        {
            EligibilityLevel = eligibilityLevel;
            YearlyInstallment = yearlyInstallment;
            EarlyAccess = earlyAccess;
            FreeToPlay = freeToPlay;
            ReleasedInternationally = releasedInternationally;
            ExpansionPack = expansionPack;
            UnannoucedGame = unannouncedGame;
        }

        public EligibilityLevel EligibilityLevel { get; }
        public bool YearlyInstallment { get; }
        public bool EarlyAccess { get; }
        public bool FreeToPlay { get; }
        public bool ReleasedInternationally { get; }
        public bool ExpansionPack { get; }
        public bool UnannoucedGame { get; }

        public static EligibilitySettings GetRoyaleEligibilitySettings()
        {
            var level = new EligibilityLevel(2, "Remake", "A remake that modernizes gameplay without fundamentally changing it.", new List<string>());
            return new EligibilitySettings(level, false, false, false, false, false, false);
        }

        public IReadOnlyList<ClaimError> GameIsEligible(MasterGame masterGame)
        {
            List<ClaimError> claimErrors = new List<ClaimError>();

            bool remakeEligible = masterGame.EligibilitySettings.EligibilityLevel.Level <= EligibilityLevel.Level;
            if (!remakeEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league 'remake level' settings.", true));
            }

            bool earlyAccessEligible = (!masterGame.EligibilitySettings.EarlyAccess || EarlyAccess);
            if (!earlyAccessEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's early access settings.", true));
            }

            bool yearlyInstallmentEligible = (!masterGame.EligibilitySettings.YearlyInstallment || YearlyInstallment);
            if (!yearlyInstallmentEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's yearly installment settings.", true));
            }

            bool freeToPlayEligible = (!masterGame.EligibilitySettings.FreeToPlay || FreeToPlay);
            if (!freeToPlayEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's free to play settings.", true));
            }

            bool releasedInternationallyEligible = (!masterGame.EligibilitySettings.ReleasedInternationally || ReleasedInternationally);
            if (!releasedInternationallyEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's international release settings.", true));
            }

            bool expansionEligible = (!masterGame.EligibilitySettings.ExpansionPack || ExpansionPack);
            if (!expansionEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's expansion pack settings.", true));
            }

            bool unannouncedEligible = (!masterGame.EligibilitySettings.UnannoucedGame || UnannoucedGame);
            if (!unannouncedEligible)
            {
                claimErrors.Add(new ClaimError("That game is not eligible under this league's unannounced game settings.", true));
            }

            return claimErrors;
        }
    }
}
