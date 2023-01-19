using FantasyCritic.Lib.Discord.Models;

namespace FantasyCritic.MySQL.Entities;
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

    public GameNewsChannel ToDomain()
    {
        return new GameNewsChannel(GuildID, ChannelID, FantasyCritic.Lib.Discord.Models.GameNewsSetting.FromValue(GameNewsSetting));
    }
}