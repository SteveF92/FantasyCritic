using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FuzzyString;

namespace FantasyCritic.Lib.Utilities
{
    public static class MasterGameSearching
    {
        public static IReadOnlyList<MasterGame> SearchMasterGames(string gameName, IEnumerable<MasterGame> masterGames) 
        {
            var substringMatches = masterGames
                .Select(x => new Tuple<MasterGame, double>(x, GetAbsoluteSubsequenceInCommon(gameName, x.GameName)));

            var perfectMatches = substringMatches.Where(x => string.Equals(gameName, x.Item1.GameName, StringComparison.InvariantCultureIgnoreCase));
            var filteredSubstringMatches = substringMatches.OrderByDescending(x => x.Item2);

            var combinedSequences = perfectMatches
                .Concat(filteredSubstringMatches)
                .Select(x => x.Item1);

            return combinedSequences.Distinct().ToList();
        }

        public static IReadOnlyList<MasterGameYear> SearchMasterGameYears(string gameName, IEnumerable<MasterGameYear> masterGames)
        {
            var substringMatches = masterGames
                .Select(x => new Tuple<MasterGameYear, double>(x, GetAbsoluteSubsequenceInCommon(gameName, x.MasterGame.GameName)));

            var perfectMatches = substringMatches.Where(x => string.Equals(gameName, x.Item1.MasterGame.GameName, StringComparison.InvariantCultureIgnoreCase));
            var filteredSubstringMatches = substringMatches.OrderByDescending(x => x.Item2).ThenByDescending(x => x.Item1.DateAdjustedHypeFactor);

            var combinedSequences = perfectMatches
                .Concat(filteredSubstringMatches)
                .Select(x => x.Item1);

            return combinedSequences.Distinct().ToList();
        }

        private static double GetAbsoluteSubsequenceInCommon(string source, string target)
        {
            var longestCommon = source.ToLowerInvariant().LongestCommonSubsequence(target.ToLowerInvariant()); 
            double result = longestCommon.Length;
            return result;
        }
    }
}
