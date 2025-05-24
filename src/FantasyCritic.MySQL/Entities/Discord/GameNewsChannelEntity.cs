using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities.Discord;
internal class GameNewsChannelEntity
{
    public GameNewsChannelEntity()
    {

    }

    public GameNewsChannelEntity(ulong guildID, ulong channelID, GameNewsSetting gameNewsSetting)
    {
        GuildID = guildID;
        ChannelID = channelID;

        ShowAlreadyReleasedNews = gameNewsSetting.ShowAlreadyReleasedNews;
        ShowWillReleaseInYearNews = gameNewsSetting.ShowWillReleaseInYearNews;
        ShowMightReleaseInYearNews = gameNewsSetting.ShowMightReleaseInYearNews;
        ShowWillNotReleaseInYearNews = gameNewsSetting.ShowWillNotReleaseInYearNews;
        ShowScoreGameNews = gameNewsSetting.ShowScoreGameNews;
        ShowJustReleasedAnnouncements = gameNewsSetting.ShowJustReleasedAnnouncements;
        ShowNewGameAnnouncements = gameNewsSetting.ShowNewGameAnnouncements;
        ShowEditedGameNews = gameNewsSetting.ShowEditedGameNews;
    }

    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }

    public bool ShowAlreadyReleasedNews { get; set; }
    public bool ShowWillReleaseInYearNews { get; set; }
    public bool ShowMightReleaseInYearNews { get; set; }
    public bool ShowWillNotReleaseInYearNews { get; set; }
    public bool ShowScoreGameNews { get; set; }
    public bool ShowJustReleasedAnnouncements { get; set; }
    public bool ShowNewGameAnnouncements { get; set; }
    public bool ShowEditedGameNews { get; set; }

    public GameNewsChannel ToDomain(IEnumerable<MasterGameTag> skippedTags)
    {
        var settings = new GameNewsSetting()
        {
            ShowAlreadyReleasedNews = ShowAlreadyReleasedNews,
            ShowWillReleaseInYearNews = ShowWillReleaseInYearNews,
            ShowMightReleaseInYearNews = ShowMightReleaseInYearNews,
            ShowWillNotReleaseInYearNews = ShowWillNotReleaseInYearNews,
            ShowScoreGameNews = ShowScoreGameNews,
            ShowJustReleasedAnnouncements = ShowJustReleasedAnnouncements,
            ShowNewGameAnnouncements = ShowNewGameAnnouncements,
            ShowEditedGameNews = ShowEditedGameNews,
        };

        return new GameNewsChannel(GuildID, ChannelID, settings, skippedTags.ToList());
    }
}
