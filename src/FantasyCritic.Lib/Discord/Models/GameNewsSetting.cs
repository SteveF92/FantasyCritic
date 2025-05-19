namespace FantasyCritic.Lib.Discord.Models;
public record GameNewsSetting
{
    public required bool ShowWillReleaseInYearNews { get; init; }
    public required bool ShowMightReleaseInYearNews { get; init; }
    public required bool ShowWillNotReleaseInYearNews { get; init; }
    public required bool ShowScoreGameNews { get; init; }
    public required bool ShowReleasedGameNews { get; init; }
    public required bool ShowNewGameNews { get; init; }
    public required bool ShowEditedGameNews { get; init; }

    public bool IsOff()
    {
        return !(ShowWillReleaseInYearNews &&
                 ShowMightReleaseInYearNews &&
                 ShowWillNotReleaseInYearNews &&
                 ShowScoreGameNews &&
                 ShowReleasedGameNews &&
                 ShowNewGameNews &&
                 ShowEditedGameNews);
    }

    public bool IsAllOn()
    {
               return ShowWillReleaseInYearNews &&
               ShowMightReleaseInYearNews &&
               ShowWillNotReleaseInYearNews &&
               ShowScoreGameNews &&
               ShowReleasedGameNews &&
               ShowNewGameNews &&
               ShowEditedGameNews;
    }

    public static GameNewsSetting GetOffSetting()
    {
        return new GameNewsSetting()
        {
            ShowWillReleaseInYearNews = false,
            ShowMightReleaseInYearNews = false,
            ShowWillNotReleaseInYearNews = false,
            ShowScoreGameNews = false,
            ShowReleasedGameNews = false,
            ShowNewGameNews = false,
            ShowEditedGameNews = false,
        };
    }
}
