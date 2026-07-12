using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.Discord.Handlers;

namespace FantasyCritic.Test.Discord;

internal class GameNewsChannelTests : BaseGameNewsTests
{
    // No skipped tags
    public static readonly BaseGameNewsRelevanceHandler Setting_All_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), null, []));

    public static readonly BaseGameNewsRelevanceHandler Setting_WillReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), null, []));

    public static readonly BaseGameNewsRelevanceHandler Setting_MightReleaseInYear_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), null, []));

    public static readonly BaseGameNewsRelevanceHandler Setting_Off_NoSkippedTags =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, null, []));

    // Skipped tag: UNA (Unannounced game)
    public static readonly BaseGameNewsRelevanceHandler Setting_All_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("All"), null, [MasterGameTagDictionary.TagDictionary["UNA"]]));

    public static readonly BaseGameNewsRelevanceHandler Setting_WillReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("WillReleaseInYear"), null, [MasterGameTagDictionary.TagDictionary["UNA"]]));

    public static readonly BaseGameNewsRelevanceHandler Setting_MightReleaseInYear_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            new OriginalGameChannel("MightReleaseInYear"), null, [MasterGameTagDictionary.TagDictionary["UNA"]]));

    public static readonly BaseGameNewsRelevanceHandler Setting_Off_SkipUNA =
        DatabaseDeserializer.GetCombinedChannelGameSetting(new OriginalDatabaseStructure(
            null, null, [MasterGameTagDictionary.TagDictionary["UNA"]]));
}
