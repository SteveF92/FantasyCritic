using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.Draft;

/// <summary>
/// What draft tests share: who a fixture publisher's user is, whose turn it is, and a game the server will let them
/// draft. The local database's top available games aren't all draftable, so tests pick the one the API's simulated
/// players would.
/// </summary>
public abstract class DraftTestBase : UiTestBase
{
    protected static async Task<Guid> UserIDOfAsync(TestPublisher publisher) => (await publisher.Session.Account.CurrentUserAsync()).UserID;

    protected static async Task<TestPublisher> NextUpAsync(LeagueFixture league)
    {
        var next = (await league.GetLeagueYearAsync()).Publishers.Single(publisher => publisher.NextToDraft);
        return league.Publishers.Single(publisher => publisher.PublisherID == next.PublisherID);
    }

    protected static TestPublisher Other(LeagueFixture league, TestPublisher publisher) => league.Publishers.Single(other => other.PublisherID != publisher.PublisherID);

    protected static async Task<string> DraftableGameNameAsync(LeagueFixture league, TestPublisher publisher)
    {
        var available = await publisher.Session.League.TopAvailableGamesAsync(league.Year, league.LeagueID, publisher.PublisherID, null);
        return available.First(game => game.IsAvailable && !game.Taken).MasterGame.GameName;
    }

    /// <summary>
    /// Clicks Select on the row of a game table whose name cell is exactly the game's name.
    /// </summary>
    protected static async Task SelectGameRowAsync(ILocator scope, string gameName)
    {
        var row = scope.GetByRole(AriaRole.Row).Filter(new() { Has = scope.Page.GetByRole(AriaRole.Cell, new() { Name = gameName, Exact = true }) }).First;
        await row.GetByRole(AriaRole.Button, new() { Name = "Select" }).ClickAsync();
        await Expect(scope.GetByRole(AriaRole.Heading, new() { Name = "Selected Game:" })).ToBeVisibleAsync();
    }

    protected static ILocator WarningDialog(IPage page) => Ui.Dialog(page, "Warning!");
}
