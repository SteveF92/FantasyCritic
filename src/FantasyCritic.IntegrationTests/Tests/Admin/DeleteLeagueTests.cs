using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Admin;

[TestFixture]
public class DeleteLeagueTests : IntegrationTestBase
{
    private static readonly LeagueScenario DraftedWithSpecialSlots = new()
    {
        Name = "DraftedWithSpecialSlots",
        PlayerCount = 2,
        StandardGames = 4,
        GamesToDraft = 4,
        CounterPicks = 1,
        CounterPicksToDraft = 1,
        PickupSystem = "SemiPublicBiddingSecretCounterPicks",
        ScoringSystem = "LinearPositive",
        TradingSystem = "Standard",
        TiebreakSystem = "LowestProjectedPoints",
        ReleaseSystem = "OnlyNeedsScore",
        IneligibleGameSystem = "DroppableAsWillNotRelease",
        HasSpecialSlots = true,
        SpecialGameSlots =
        [
            new SpecialGameSlotViewModel { SpecialSlotPosition = 0, RequiredTags = ["NewGame"] },
            new SpecialGameSlotViewModel { SpecialSlotPosition = 1, RequiredTags = ["NewGame"] },
        ],
    };

    [Test]
    public async Task DeleteLeague_DraftedTestLeague_LeagueIsGone()
    {
        await using var league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, DraftedWithSpecialSlots, NewUser);
        await league.DraftToCompletionAsync();

        var drafted = await league.GetLeagueYearAsync();
        Assert.That(drafted.Publishers.SelectMany(p => p.Games).Count(g => g.CounterPick), Is.EqualTo(2));

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);
        await adminSession.Admin.DeleteLeagueAsync(new DeleteLeagueRequest { LeagueID = league.LeagueID });

        var ex = Assert.ThrowsAsync<ApiException>(() => league.Manager.League.GetLeagueAsync(league.LeagueID, null));
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }
}
