namespace FantasyCritic.Lib.Enums;

public record AutoDraftSettings
{
    public AutoDraftSettings(AutoDraftMode mode, bool onlyDraftFromWatchlist)
    {
        Mode = mode;
        OnlyDraftFromWatchlist = mode.Equals(AutoDraftMode.Off) ? false : onlyDraftFromWatchlist;
    }

    public AutoDraftMode Mode { get; init; }
    public bool OnlyDraftFromWatchlist { get; init; }
}
