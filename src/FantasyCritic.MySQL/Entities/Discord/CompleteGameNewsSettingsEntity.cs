using FantasyCritic.Lib.Discord.Enums;
using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities.Discord;
internal class CompleteGameNewsSettingsEntity
{
    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public bool EnableGameNews { get; set; }
    public bool? ShowPickedGameNews { get; set; } 
    public bool? ShowEligibleGameNews { get; set; } 
    public NotableMissSetting? NotableMissSetting { get; set; }
    public bool ShowWillReleaseInYearNews { get; set; }
    public bool ShowMightReleaseInYearNews { get; set; }
    public bool ShowWillNotReleaseInYearNews { get; set; }
    public bool ShowScoreGameNews { get; set; }
    public bool ShowReleasedGameNews { get; set; }
    public bool ShowNewGameNews { get; set; }
    public bool ShowEditedGameNews { get; set; }

    public CompleteGameNewsSettings ToDomain(List<MasterGameTag> skippedTags)
    {
        return new CompleteGameNewsSettings()
        {
            EnableGameNews = EnableGameNews,
            ShowPickedGameNews = ShowPickedGameNews,
            ShowEligibleGameNews = ShowEligibleGameNews,
            NotableMissSetting = NotableMissSetting,
            ShowWillReleaseInYearNews = ShowWillReleaseInYearNews,
            ShowMightReleaseInYearNews = ShowMightReleaseInYearNews,
            ShowWillNotReleaseInYearNews = ShowWillNotReleaseInYearNews,
            ShowScoreGameNews = ShowScoreGameNews,
            ShowReleasedGameNews = ShowReleasedGameNews,
            ShowNewGameNews = ShowNewGameNews,
            ShowEditedGameNews = ShowEditedGameNews,
            SkippedTags = skippedTags,
        };
    }
}
