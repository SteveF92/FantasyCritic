using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.FakeRepo.TestUtilities;
public static class MasterGameTagDictionary
{
    private static readonly MasterGameTagType RemakeLevelType = new MasterGameTagType("RemakeLevel");
    private static readonly MasterGameTagType OtherType = new MasterGameTagType("Other");

    public static readonly Dictionary<string, MasterGameTag> TagDictionary = new List<MasterGameTag>()
    {
        new MasterGameTag("Cancelled", "Cancelled", "CNCL", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("CurrentlyInEarlyAccess", "Currently in Early Access", "C-EA", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("DirectorsCut", "Director's Cut", "DC", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("ExpansionPack", "Expansion Pack", "EXP", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("FreeToPlay", "Free to Play", "FTP", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("NewGame", "New Game", "NG", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("NewGamingFranchise", "New Gaming Franchise", "NGF", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("PartialRemake", "Partial Remake", "P-RMKE", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("PlannedForEarlyAccess", "Planned for Early Access", "P-EA", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("Port", "Port", "PRT", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("Reimagining", "Reimagining", "RIMG", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("ReleasedInternationally", "Released Internationally", "R-INT", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("Remake", "Remake", "RMKE", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("Remaster", "Remaster", "RMSTR", RemakeLevelType, false, false, "", new List<string>(), ""),
        new MasterGameTag("UnannouncedGame", "Unannounced Game", "UNA", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("VirtualReality", "Virtual Reality", "VR", OtherType, false, false, "", new List<string>(), ""),
        new MasterGameTag("WillReleaseInternationallyFirst", "Will Release Internationally First", "W-INT", OtherType, true, false, "", new List<string>(), ""),
        new MasterGameTag("YearlyInstallment", "Yearly Installment", "YI", OtherType, false, false, "", new List<string>(), ""),
    }.ToDictionary(x => x.ShortName);
}
