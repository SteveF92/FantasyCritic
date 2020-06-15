using System.Collections.Generic;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.FakeRepo.Factories
{
    internal class EligibilityLevelFactory
    {
        public static IReadOnlyList<EligibilityLevel> GetEligibilityLevels()
        {
            return new List<EligibilityLevel>()
            {
                new EligibilityLevel(0, "New Game", "A definitively new game.", new List<string>()),
                new EligibilityLevel(1, "Complete Remake", "A remake that radically overhauls the original gameplay.", new List<string>()),
                new EligibilityLevel(2, "Remake", "A remake that modernizes gameplay without fundamentally changing it.", new List<string>()),
                new EligibilityLevel(3, "Partial Remake", "A game that adds some new features while largely playing the same as the original.", new List<string>()),
                new EligibilityLevel(4, "Remaster", "A re-release that updates graphics while changing little else.", new List<string>()),
                new EligibilityLevel(5, "Port", "A game that was not originally released on a particular platform that is ported to it without many changes.", new List<string>())
            };
        }
    }
}