namespace FantasyCritic.MySQL.Entities.Discord;
internal class GameNewsChannelSkippedTagEntity
{
    public GameNewsChannelSkippedTagEntity()
    {

    }

    public GameNewsChannelSkippedTagEntity(ulong guildID, ulong channelID, MasterGameTag tag)
    {
        GuildID = guildID;
        ChannelID = channelID;
        TagName = tag.Name;
    }

    public ulong GuildID { get; set; }
    public ulong ChannelID { get; set; }
    public string TagName { get; set; } = null!;
}
