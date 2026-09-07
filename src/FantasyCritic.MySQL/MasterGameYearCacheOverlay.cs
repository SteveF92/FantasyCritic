namespace FantasyCritic.MySQL;

/// <summary>
/// tbl_caching_mastergameyear is only rebuilt when caches are refreshed, so its copy of the master game
/// (release dates, critic score, tags) can lag tbl_mastergame. Anything read out of that table must replace
/// the inner MasterGame with the live row, or eligibility-sensitive checks will disagree with the code paths
/// that read tbl_mastergame directly.
/// </summary>
internal static class MasterGameYearCacheOverlay
{
    public static MasterGameYear OverlayFreshMasterGame(this MasterGameYear masterGameYear,
        IReadOnlyDictionary<Guid, MasterGame> freshMasterGames)
    {
        var freshMasterGame = freshMasterGames.GetValueOrDefault(masterGameYear.MasterGame.MasterGameID);
        return freshMasterGame is not null ? masterGameYear.WithNewMasterGame(freshMasterGame) : masterGameYear;
    }

    public static IReadOnlyList<MasterGameYear> OverlayFreshMasterGames(this IReadOnlyList<MasterGameYear> masterGameYears,
        IReadOnlyDictionary<Guid, MasterGame> freshMasterGames)
    {
        var overlaid = new List<MasterGameYear>(masterGameYears.Count);
        foreach (var masterGameYear in masterGameYears)
        {
            overlaid.Add(masterGameYear.OverlayFreshMasterGame(freshMasterGames));
        }

        return overlaid;
    }
}
