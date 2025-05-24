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

        ShowWillReleaseInYearNews = gameNewsSetting.ShowWillReleaseInYearNews;
        ShowMightReleaseInYearNews = gameNewsSetting.ShowMightReleaseInYearNews;
        ShowWillNotReleaseInYearNews = gameNewsSetting.ShowWillNotReleaseInYearNews;
        ShowScoreGameNews = gameNewsSetting.ShowScoreGameNews;
        ShowReleasedGameNews = gameNewsSetting.ShowJustReleasedAnnouncements;
        ShowNewGameNews = gameNewsSetting.ShowNewGameAnnouncements;
        ShowEditedGameNews = gameNewsSetting.ShowEditedGameNews;
    }

    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }

    public bool ShowWillReleaseInYearNews { get; set; }
    public bool ShowMightReleaseInYearNews { get; set; }
    public bool ShowWillNotReleaseInYearNews { get; set; }
    public bool ShowScoreGameNews { get; set; }
    public bool ShowReleasedGameNews { get; set; }
    public bool ShowNewGameNews { get; set; }
    public bool ShowEditedGameNews { get; set; }

    public GameNewsChannel ToDomain(IEnumerable<MasterGameTag> skippedTags)
    {
        var settings = new GameNewsSetting()
        {
            ShowWillReleaseInYearNews = ShowWillReleaseInYearNews,
            ShowMightReleaseInYearNews = ShowMightReleaseInYearNews,
            ShowWillNotReleaseInYearNews = ShowWillNotReleaseInYearNews,
            ShowScoreGameNews = ShowScoreGameNews,
            ShowJustReleasedAnnouncements = ShowReleasedGameNews,
            ShowNewGameAnnouncements = ShowNewGameNews,
            ShowEditedGameNews = ShowEditedGameNews,
        };

        return new GameNewsChannel(GuildID, ChannelID, settings, skippedTags.ToList());
    }
}
