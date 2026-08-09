namespace FantasyCritic.Lib.Domain;

/// <summary>
/// Precomputes cross-draft pick numbers for every publisher game across a set of league years.
/// Build once from a batch of league years, then call <see cref="GetPickNumber"/> per game for O(1) lookup.
/// </summary>
public sealed class CrossDraftPickNumberCache
{
    private readonly IReadOnlyDictionary<Guid, int?> _pickNumbersByPublisherGameID;

    private CrossDraftPickNumberCache(IReadOnlyDictionary<Guid, int?> pickNumbersByPublisherGameID)
    {
        _pickNumbersByPublisherGameID = pickNumbersByPublisherGameID;
    }

    public static CrossDraftPickNumberCache Build(IEnumerable<LeagueYear> leagueYears)
    {
        var leagueYearList = leagueYears.ToList();

        var draftStartingPickNumbers = new Dictionary<Guid, (int StandardGameStartingPoint, int CounterPickStartingPoint)>();
        foreach (var leagueYear in leagueYearList)
        {
            foreach (var draft in leagueYear.Drafts)
            {
                var result = draft.GetStartingOverallPickNumber(leagueYear);
                if (result.IsSuccess)
                {
                    draftStartingPickNumbers[draft.DraftID] = result.Value;
                }
            }
        }

        var pickNumbers = new Dictionary<Guid, int?>();
        foreach (var leagueYear in leagueYearList)
        {
            foreach (var publisher in leagueYear.Publishers)
            {
                foreach (var game in publisher.PublisherGames)
                {
                    pickNumbers[game.PublisherGameID] = ComputePickNumber(game, draftStartingPickNumbers);
                }
            }
        }

        return new CrossDraftPickNumberCache(pickNumbers);
    }

    public int? GetPickNumber(PublisherGame game)
    {
        if (_pickNumbersByPublisherGameID.TryGetValue(game.PublisherGameID, out var pickNumber))
        {
            return pickNumber;
        }

        throw new InvalidOperationException(
            $"PublisherGame {game.PublisherGameID} (publisher {game.PublisherID}) is not present in the cross-draft pick number cache.");
    }

    private static int? ComputePickNumber(PublisherGame game,
        IReadOnlyDictionary<Guid, (int StandardGameStartingPoint, int CounterPickStartingPoint)> draftStartingPickNumbers)
    {
        if (!game.DraftID.HasValue)
        {
            return null;
        }

        if (!game.OverallPickNumber.HasValue)
        {
            throw new InvalidOperationException(
                $"PublisherGame {game.PublisherGameID} (publisher {game.PublisherID}) has DraftID {game.DraftID} but no OverallPickNumber.");
        }

        if (!draftStartingPickNumbers.TryGetValue(game.DraftID.Value, out var startingPickNumber))
        {
            throw new InvalidOperationException(
                $"PublisherGame {game.PublisherGameID} (publisher {game.PublisherID}) is assigned to draft {game.DraftID}, " +
                $"but that draft has not started.");
        }

        return game.CounterPick
            ? startingPickNumber.CounterPickStartingPoint + game.OverallPickNumber.Value
            : startingPickNumber.StandardGameStartingPoint + game.OverallPickNumber.Value;
    }
}
