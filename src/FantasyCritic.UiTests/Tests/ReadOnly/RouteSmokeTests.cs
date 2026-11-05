using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace FantasyCritic.UiTests.Tests.ReadOnly;

/// <summary>
/// Every page that only reads renders its heading without a browser problem, logged out and in, wide and phone-sized.
/// </summary>
public sealed class RouteSmokeTests : UiTestBase
{
    private static readonly ViewportSize Desktop = new() { Width = 1280, Height = 900 };
    private static readonly ViewportSize Phone = new() { Width = 375, Height = 812 };

    private LeagueFixture _league = null!;
    private Guid _managerUserID;
    private Guid _masterGameID;
    private string _masterGameName = null!;
    private string _publisherName = null!;
    private string _leagueName = null!;

    [OneTimeSetUp]
    public async Task CreateLeague()
    {
        _league = await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, LeagueScenarios.TwoPlayerSmall, NewCredentials);
        await _league.DraftToCompletionAsync();

        var leagueYear = await _league.GetLeagueYearAsync();
        _leagueName = leagueYear.League.LeagueName;
        var publisher = leagueYear.Publishers.Single(p => p.PublisherID == _league.Publishers[0].PublisherID);
        _publisherName = publisher.PublisherName;
        var masterGame = publisher.Games.First(game => game.MasterGame is not null).MasterGame!;
        _masterGameID = masterGame.MasterGameID;
        _masterGameName = masterGame.GameName;
        _managerUserID = (await _league.Manager.Account.CurrentUserAsync()).UserID;
    }

    [OneTimeTearDown]
    public async Task DisposeLeague()
    {
        await _league.DisposeAsync();
    }

    public static IEnumerable<TestCaseData> PublicPages()
    {
        var pages = new (string Path, string Heading)[]
        {
            ("/about", "About"),
            ("/discord-bot", "Discord Bot"),
            ("/howtoplay", "How to Play"),
            ("/faq", "Frequently Asked Questions"),
            ("/site-updates", "Site updates"),
            ("/contact", "Contact"),
            ("/community", "Community"),
            ("/fantasyCriticPlus", "What is Fantasy Critic Plus?"),
            ("/games", "Master Games List"),
            ("/gameChanges", "Recent Master Game Changes"),
            ("/mostDesiredReviews", "Most Desired Reviews"),
            ("/advancedGameStats", "Advanced Game Stats"),
            ("/publicLeagues", "Public Leagues"),
        };

        foreach (var (path, heading) in pages)
        {
            foreach (var signedIn in new[] { false, true })
            {
                foreach (var phone in new[] { false, true })
                {
                    yield return new TestCaseData(path, heading, signedIn, phone)
                        .SetArgDisplayNames(path, signedIn ? "signed in" : "logged out", phone ? "phone" : "desktop");
                }
            }
        }
    }

    [TestCaseSource(nameof(PublicPages))]
    public async Task PublicPageRenders(string path, string heading, bool signedIn, bool phone)
    {
        var viewport = phone ? Phone : Desktop;
        var page = signedIn ? await NewSignedInPageAsync(_managerUserID, path, viewport) : await NewPageAsync(viewport);
        if (!signedIn)
        {
            await page.GotoAsync(path);
        }

        await Expect(Ui.Heading(page, heading)).ToBeVisibleAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public static IEnumerable<TestCaseData> LeaguePages()
    {
        var pages = new[] { "home", "topBidsAndDrops", "league", "leagueHistory", "leagueAllTimeStats", "publisher", "mastergame" };
        foreach (var name in pages)
        {
            foreach (var phone in new[] { false, true })
            {
                yield return new TestCaseData(name, phone).SetArgDisplayNames(name, phone ? "phone" : "desktop");
            }
        }
    }

    [TestCaseSource(nameof(LeaguePages))]
    public async Task SignedInPageRenders(string name, bool phone)
    {
        var (path, heading) = name switch
        {
            "home" => ("/home", $"Welcome {(await _league.Manager.Account.CurrentUserAsync()).DisplayName}!"),
            "topBidsAndDrops" => ("/topBidsAndDrops", "Top Bids and Drops"),
            "league" => ($"/league/{_league.LeagueID}/{_league.Year}", _leagueName),
            "leagueHistory" => ($"/leagueHistory/{_league.LeagueID}/{_league.Year}", $"{_leagueName} History ({_league.Year})"),
            "leagueAllTimeStats" => ($"/leagueAllTimeStats/{_league.LeagueID}", $"{_leagueName} All Time Stats"),
            "publisher" => ($"/publisher/{_league.Publishers[0].PublisherID}", _publisherName),
            "mastergame" => ($"/mastergame/{_masterGameID}", _masterGameName),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null),
        };

        var page = await NewSignedInPageAsync(_managerUserID, path, phone ? Phone : Desktop);

        await Expect(Ui.Heading(page, heading)).ToBeVisibleAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
