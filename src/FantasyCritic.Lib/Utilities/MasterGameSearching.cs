using FuzzySharp;

namespace FantasyCritic.Lib.Utilities;

public static class MasterGameSearching
{
    public static IReadOnlyList<MasterGame> SearchMasterGames(string gameName, IEnumerable<MasterGame> masterGames)
    {
        var substringMatches = masterGames
            .Select(x => new Tuple<MasterGame, int>(x, GetSimilarity(gameName, x.GameName)));

        var perfectMatches = substringMatches.Where(x => string.Equals(gameName, x.Item1.GameName, StringComparison.InvariantCultureIgnoreCase));
        var filteredSubstringMatches = substringMatches.OrderByDescending(x => x.Item2);

        var combinedSequences = perfectMatches
            .Concat(filteredSubstringMatches)
            .Select(x => x.Item1);

        return combinedSequences.Distinct().ToList();
    }

    public static IReadOnlyList<MasterGameYear> SearchMasterGameYears(string gameName, IEnumerable<MasterGameYear> masterGames, bool onlyBestMatches)
    {
        var substringMatches = masterGames
            .Select(x => new Tuple<MasterGameYear, int>(x, GetSimilarity(gameName, x.MasterGame.GameName)));

        var perfectMatches = substringMatches.Where(x => string.Equals(gameName, x.Item1.MasterGame.GameName, StringComparison.InvariantCultureIgnoreCase));
        var filteredSubstringMatches = substringMatches.OrderByDescending(x => x.Item2).ThenByDescending(x => x.Item1.DateAdjustedHypeFactor).ToList();

        if (onlyBestMatches)
        {
            var bestMatchCount = filteredSubstringMatches.FirstOrDefault()?.Item2;
            if (bestMatchCount.HasValue)
            {
                filteredSubstringMatches = filteredSubstringMatches.Where(x => x.Item2 == bestMatchCount.Value).ToList();
            }
        }

        var combinedSequences = perfectMatches
            .Concat(filteredSubstringMatches)
            .Select(x => x.Item1);

        return combinedSequences.Distinct().ToList();
    }

    private static int GetSimilarity(string source, string target)
    {
        return Fuzz.PartialRatio(source.ToLowerInvariant(), target.ToLowerInvariant());
    }
}
