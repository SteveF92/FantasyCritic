using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Test;
public static class MasterGameTestExtensions
{
    public static MasterGame WithScore(this MasterGame game, decimal? score)
    {
        return new MasterGame(game.MasterGameID, game.GameName, game.EstimatedReleaseDate, game.MinimumReleaseDate,
            game.MaximumReleaseDate, game.EarlyAccessReleaseDate,
            game.InternationalReleaseDate, game.AnnouncementDate, game.ReleaseDate, game.OpenCriticID, game.GGToken,
            game.GGSlug, score, game.HasAnyReviews, game.OpenCriticSlug,
            game.Notes, game.BoxartFileName, game.GGCoverArtFileName, game.FirstCriticScoreTimestamp,
            game.DoNotRefreshDate, game.DoNotRefreshAnything, game.UseSimpleEligibility,
            game.DelayContention, game.ShowNote, game.AddedTimestamp, game.AddedByUser, game.SubGames, game.Tags);
    }
}
