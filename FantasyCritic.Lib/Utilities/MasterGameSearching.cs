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
        private static readonly int MaxDistanceGames = 5;

        public static IReadOnlyList<MasterGame> SearchMasterGames(string gameName, IEnumerable<MasterGame> masterGames) 
        {
            var subsequenceMatches = masterGames
                .Select(x => new Tuple<MasterGame, double>(x, GetDistance(gameName, x.GameName)));

            var substringMatches = masterGames
                .Select(x => new Tuple<MasterGame, double>(x, GetSubstringCount(gameName, x.GameName)));

            var combinedSequences = substringMatches.Select(x => Tuple.Create(x.Item1, x.Item2 * 2))
                .Concat(subsequenceMatches)
                .OrderByDescending(x => x.Item2)
                .Select(x => x.Item1)
                .Take(MaxDistanceGames * 2);

            return combinedSequences.ToList();
        }

        public static IReadOnlyList<MasterGameYear> SearchMasterGameYears(string gameName, IEnumerable<MasterGameYear> masterGames)
        {
            var subsequenceMatches = masterGames
                .Select(x => new Tuple<MasterGameYear, double>(x, GetDistance(gameName, x.MasterGame.GameName)));

            var substringMatches = masterGames
                .Select(x => new Tuple<MasterGameYear, double>(x, GetSubstringCount(gameName, x.MasterGame.GameName)));

            var combinedSequences = substringMatches.Select(x => Tuple.Create(x.Item1, x.Item2 * 2))
                .Concat(subsequenceMatches)
                .OrderByDescending(x => x.Item2)
                .Select(x => x.Item1)
                .Take(MaxDistanceGames * 2);

            return combinedSequences.ToList();
        }

        private static double GetDistance(string source, string target)
        {
            var longestCommon = source.ToLowerInvariant().LongestCommonSubsequence(target.ToLowerInvariant());
            return longestCommon.Length;
        }

        private static double GetSubstringCount(string source, string target)
        {
            var longestCommon = source.ToLowerInvariant().LongestCommonSubstring(target.ToLowerInvariant());
            return longestCommon.Length;
        }

        //https://stackoverflow.com/a/9453762
        private static int CalculateLevenshteinDistance(this string a, string b)
        {
            if (string.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
            for (int j = 1; j <= lengthB; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                distances[i, j] = Math.Min
                (
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost
                );
            }
            return distances[lengthA, lengthB];
        }
    }
}
