using System.Collections.Generic;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Test.Discord;
internal static class DatabaseDeserializer
{
    public static OriginalDatabaseStructure TransformToNewStructure(OriginalDatabaseStructure structure)
    {
        return structure;
    }

    public static CombinedChannelGameSetting GetCombinedChannelGameSetting(OriginalDatabaseStructure structure)
    {
        return new CombinedChannelGameSetting(structure.LeagueChannel?.SendLeagueMasterGameUpdates ?? false, structure.LeagueChannel?.SendNotableMisses ?? false,
            GameNewsSetting.FromValue(structure.GameChannel?.GameNewsSetting ?? "Off"), structure.SkippedTags);
    }
}

public record OriginalDatabaseStructure(OriginalGameChannel? GameChannel, OriginalLeagueChannel? LeagueChannel, List<MasterGameTag> SkippedTags);
public record OriginalGameChannel(string GameNewsSetting);
public record OriginalLeagueChannel(bool SendLeagueMasterGameUpdates, bool SendNotableMisses);
