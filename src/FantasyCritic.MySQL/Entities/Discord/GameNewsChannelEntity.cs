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
        GameNewsSetting = gameNewsSetting.Value;
    }

    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public string GameNewsSetting { get; set; } = null!;

    public GameNewsChannel ToDomain(IEnumerable<MasterGameTag> skippedTags)
    {
        return new GameNewsChannel(GuildID, ChannelID, Lib.Discord.Models.GameNewsSetting.FromValue(GameNewsSetting), skippedTags.ToList());
    }
}
