namespace FantasyCritic.Lib.Discord.Models;
public record GameNewsSetting
{
    public required bool ShowJustReleasedAnnouncements { get; init; }
    public required bool ShowNewGameAnnouncements { get; init; }
    public required bool ShowAlreadyReleasedNews { get; init; }
    public required bool ShowWillReleaseInYearNews { get; init; }
    public required bool ShowMightReleaseInYearNews { get; init; }
    public required bool ShowWillNotReleaseInYearNews { get; init; }
    public required bool ShowScoreGameNews { get; init; }
    public required bool ShowEditedGameNews { get; init; }

    public bool IsOff()
    {
        return Equals(GetOffSetting());
    }

    public bool IsAllOn()
    {
        return Equals(GetAllOnSetting());
    }

    public bool IsRecommended()
    {
        return Equals(GetRecommendedSetting());
    }

    public static GameNewsSetting GetRecommendedSetting()
    {
        return new GameNewsSetting()
        {
            ShowAlreadyReleasedNews = false,
            ShowWillReleaseInYearNews = true,
            ShowMightReleaseInYearNews = true,
            ShowWillNotReleaseInYearNews = false,
            ShowScoreGameNews = true,
            ShowJustReleasedAnnouncements = true,
            ShowNewGameAnnouncements = true,
            ShowEditedGameNews = true,
        };
    }

    public static GameNewsSetting GetAllOnSetting()
    {
        return new GameNewsSetting()
        {
            ShowAlreadyReleasedNews = true,
            ShowWillReleaseInYearNews = true,
            ShowMightReleaseInYearNews = true,
            ShowWillNotReleaseInYearNews = true,
            ShowScoreGameNews = true,
            ShowJustReleasedAnnouncements = true,
            ShowNewGameAnnouncements = true,
            ShowEditedGameNews = true,
        };
    }

    public static GameNewsSetting GetOffSetting()
    {
        return new GameNewsSetting()
        {
            ShowAlreadyReleasedNews = false,
            ShowWillReleaseInYearNews = false,
            ShowMightReleaseInYearNews = false,
            ShowWillNotReleaseInYearNews = false,
            ShowScoreGameNews = false,
            ShowJustReleasedAnnouncements = false,
            ShowNewGameAnnouncements = false,
            ShowEditedGameNews = false,
        };
    }
}
