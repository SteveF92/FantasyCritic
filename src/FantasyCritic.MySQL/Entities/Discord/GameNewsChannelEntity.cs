using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities.Discord;
internal class GameNewsChannelEntity
{
    public GameNewsChannelEntity()
    {

    }

    public GameNewsChannelEntity(ulong guildID, ulong channelID, GameNewsSettingOld gameNewsSettingOld)
    {
        GuildID = guildID;
        ChannelID = channelID;
        GameNewsSetting = gameNewsSettingOld.Value;
    }

    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public string GameNewsSetting { get; set; } = null!;

    public GameNewsChannel ToDomain(IEnumerable<MasterGameTag> skippedTags)
    {
        return new GameNewsChannel(GuildID, ChannelID, Lib.Discord.Models.GameNewsSettingOld.FromValue(GameNewsSetting), skippedTags.ToList());
    }
}
