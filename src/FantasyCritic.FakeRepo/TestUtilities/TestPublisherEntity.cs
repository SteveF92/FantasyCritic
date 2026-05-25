using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.FakeRepo.TestUtilities;
public class TestPublisherEntity
{
    public Guid PublisherID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int DraftPosition { get; set; }
    public int UnrestrictedReleaseStatusGamesDropped { get; set; }
    public int WillNotReleaseGamesDropped { get; set; }
    public int WillReleaseGamesDropped { get; set; }
    public int SuperDropsAvailable { get; set; }
    public uint Budget { get; set; }

    public Publisher ToDomain(IEnumerable<PublisherGame> publisherGames, IEnumerable<FormerPublisherGame> formerPublisherGames)
    {
        var leagueYearKey = new LeagueYearKey(LeagueID, Year);
        var draftID = TestLeagueDraftIds.For(leagueYearKey);
        var draftInfos = new[] { new PublisherDraftInfo(draftID, 1, PublisherID, DraftPosition) };
        return new Publisher(PublisherID, leagueYearKey, FantasyCriticUser.GetFakeUser(), PublisherID.ToString(), null, null, draftInfos,
            publisherGames, formerPublisherGames, Budget, UnrestrictedReleaseStatusGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped, SuperDropsAvailable, new AutoDraftSettings(AutoDraftMode.Off, false));
    }
}
