using System;
using System.Collections.Generic;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Test.TestUtilities;
public class TestPublisherEntity
{
    public Guid PublisherID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public int DraftPosition { get; set; }
    public int FreeGamesDropped { get; set; }
    public int WillNotReleaseGamesDropped { get; set; }
    public int WillReleaseGamesDropped { get; set; }
    public int SuperDropsAvailable { get; set; }
    public uint Budget { get; set; }

    public Publisher ToDomain(IEnumerable<PublisherGame> publisherGames, IEnumerable<FormerPublisherGame> formerPublisherGames)
    {
        return new Publisher(PublisherID, new LeagueYearKey(LeagueID, Year), FantasyCriticUser.GetFakeUser(), PublisherID.ToString(), null, null, DraftPosition,
            publisherGames, formerPublisherGames, Budget, FreeGamesDropped, WillNotReleaseGamesDropped, WillReleaseGamesDropped, SuperDropsAvailable, AutoDraftMode.Off);
    }
}
