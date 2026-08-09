namespace FantasyCritic.Lib.Domain;

/// <summary>
/// Precomputes cross-draft positions for every publisher game across a set of league years.
/// Build once from a batch of league years, then call <see cref="GetPosition"/> per game for O(1) lookup.
/// </summary>
public sealed class CrossDraftPositionCache
{
    private readonly IReadOnlyDictionary<Guid, int?> _positionsByPublisherGameID;

    private CrossDraftPositionCache(IReadOnlyDictionary<Guid, int?> positionsByPublisherGameID)
    {
        _positionsByPublisherGameID = positionsByPublisherGameID;
    }

    public static CrossDraftPositionCache Build(IEnumerable<LeagueYear> leagueYears)
    {
        var leagueYearList = leagueYears.ToList();

        var draftStartingPositions = new Dictionary<Guid, (int StandardGameStartingPoint, int CounterPickStartingPoint)>();
        foreach (var leagueYear in leagueYearList)
        {
            foreach (var draft in leagueYear.Drafts)
            {
                var result = draft.GetStartingOverallDraftPosition(leagueYear);
                if (result.IsSuccess)
                {
                    draftStartingPositions[draft.DraftID] = result.Value;
                }
            }
        }

        var positions = new Dictionary<Guid, int?>();
        foreach (var leagueYear in leagueYearList)
        {
            foreach (var publisher in leagueYear.Publishers)
            {
                foreach (var game in publisher.PublisherGames)
                {
                    positions[game.PublisherGameID] = ComputePosition(game, draftStartingPositions);
                }
            }
        }

        return new CrossDraftPositionCache(positions);
    }

    public int? GetPosition(PublisherGame game)
    {
        if (_positionsByPublisherGameID.TryGetValue(game.PublisherGameID, out var position))
        {
            return position;
        }

        throw new InvalidOperationException(
            $"PublisherGame {game.PublisherGameID} (publisher {game.PublisherID}) is not present in the cross-draft position cache.");
    }

    private static int? ComputePosition(PublisherGame game,
        IReadOnlyDictionary<Guid, (int StandardGameStartingPoint, int CounterPickStartingPoint)> draftStartingPositions)
    {
        if (!game.DraftID.HasValue)
        {
            return null;
        }

        if (!game.OverallPickNumber.HasValue)
        {
            throw new InvalidOperationException(
                $"PublisherGame {game.PublisherGameID} (publisher {game.PublisherID}) has DraftID {game.DraftID} but no OverallDraftPosition.");
        }

        if (!draftStartingPositions.TryGetValue(game.DraftID.Value, out var startingPos))
        {
            throw new InvalidOperationException(
                $"PublisherGame {game.PublisherGameID} (publisher {game.PublisherID}) is assigned to draft {game.DraftID}, " +
                $"but that draft has not started.");
        }

        return game.CounterPick
            ? startingPos.CounterPickStartingPoint + game.OverallPickNumber.Value
            : startingPos.StandardGameStartingPoint + game.OverallPickNumber.Value;
    }
}
