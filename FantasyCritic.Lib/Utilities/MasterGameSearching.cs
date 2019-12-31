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
            var subsequenceMatches = masterGames
                .Select(x => new Tuple<MasterGame, double>(x, GetDistance(gameName, x.GameName)));

            var substringMatches = masterGames
                .Select(x => new Tuple<MasterGame, double>(x, GetPercentInCommon(gameName, x.GameName)));

            var perfectMatches = substringMatches.Where(x => Math.Abs(x.Item2 - 1.0) < .01);
            var filteredSubsequenceMatches = subsequenceMatches
                .OrderByDescending(x => x.Item2);
            var combinedSequences = perfectMatches
                .Concat(filteredSubsequenceMatches)
                .Select(x => x.Item1);

            return combinedSequences.Distinct().ToList();
        }

        public static IReadOnlyList<MasterGameYear> SearchMasterGameYears(string gameName, IEnumerable<MasterGameYear> masterGames)
        {
            var subsequenceMatches = masterGames
                .Select(x => new Tuple<MasterGameYear, double>(x, GetDistance(gameName, x.MasterGame.GameName)));

            var substringMatches = masterGames
                .Select(x => new Tuple<MasterGameYear, double>(x, GetPercentInCommon(gameName, x.MasterGame.GameName)));

            var perfectMatches = substringMatches.Where(x => Math.Abs(x.Item2 - 1.0) < .01);
            var filteredSubsequenceMatches = subsequenceMatches
                .OrderByDescending(x => x.Item2);
            var combinedSequences = perfectMatches
                .Concat(filteredSubsequenceMatches)
                .Select(x => x.Item1);

            return combinedSequences.Distinct().ToList();
        }

        private static double GetDistance(string source, string target)
        {
            var longestCommon = source.ToLowerInvariant().LongestCommonSubsequence(target.ToLowerInvariant());
            return longestCommon.Length;
        }

        private static double GetPercentInCommon(string source, string target)
        {
            var longestCommon = source.ToLowerInvariant().LongestCommonSubstring(target.ToLowerInvariant());
            double percent = (double)longestCommon.Length / target.Length;
            return percent;
        }
    }
}
