using System.Collections.Generic;
using FantasyCritic.Lib.Discord.Handlers;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Test.TestUtilities;

namespace FantasyCritic.Test.Discord;

internal class GameNewsChannelTests : BaseGameNewsTests
{
    // No skipped tags
    public static readonly BaseGameNewsRelevanceHandler Setting_All_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), null, new List<MasterGameTag>()));

    public static readonly BaseGameNewsRelevanceHandler Setting_WillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), null, new List<MasterGameTag>()));

    public static readonly BaseGameNewsRelevanceHandler Setting_MightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), null, new List<MasterGameTag>()));

    public static readonly BaseGameNewsRelevanceHandler Setting_Off_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, null, new List<MasterGameTag>()));

    // Skipped tag: UNA (Unannounced game)
    public static readonly BaseGameNewsRelevanceHandler Setting_All_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), null, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly BaseGameNewsRelevanceHandler Setting_WillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), null, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly BaseGameNewsRelevanceHandler Setting_MightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), null, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));

    public static readonly BaseGameNewsRelevanceHandler Setting_Off_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, null, new List<MasterGameTag> { MasterGameTagDictionary.TagDictionary["UNA"] }));
}
