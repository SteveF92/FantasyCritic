using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace FantasyCritic.UiTests;

public sealed record TestUser(Guid UserID, string Email, string Password, string DisplayName, ApiSession Session);

/// <summary>
/// Base class for UI test fixtures. Each fixture gets its own app on a real port; each page gets its own browser
/// context, so two pages are two users. A page exception, a console error or a 5xx from the API fails the test, and a
/// failed test leaves a trace and screenshots under TestResults/ui.
/// </summary>
public abstract class UiTestBase
{
    private readonly List<IBrowserContext> _contexts = [];
    private readonly List<ApiSession> _sessions = [];
    private readonly List<LeagueFixture> _leagues = [];
    private readonly List<string> _problems = [];

    protected UiWebApplicationFactory Factory { get; private set; } = null!;

    /// <summary>
    /// The page exceptions, console errors and API failures seen so far in this test.
    /// </summary>
    protected IReadOnlyList<string> Problems
    {
        get
        {
            lock (_problems)
            {
                return _problems.ToList();
            }
        }
    }

    [OneTimeSetUp]
    public void StartApp()
    {
        Factory = new UiWebApplicationFactory(UiRun.Dist);
        Factory.Start();
    }

    [OneTimeTearDown]
    public void StopApp()
    {
        Factory.Dispose();
    }

    [TearDown]
    public async Task FinishTest()
    {
        if (UiRun.PauseAtEnd && _contexts.LastOrDefault()?.Pages.LastOrDefault() is { } lastPage)
        {
            await lastPage.PauseAsync();
        }

        var problems = Problems;
        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed || problems.Count > 0;
        var resultsDirectory = Path.Combine(UiRun.ResultsDirectory, SafeFileName($"{GetType().Name}.{TestContext.CurrentContext.Test.Name}"));

        for (var contextIndex = 0; contextIndex < _contexts.Count; contextIndex++)
        {
            var context = _contexts[contextIndex];
            if (failed)
            {
                Directory.CreateDirectory(resultsDirectory);
                for (var pageIndex = 0; pageIndex < context.Pages.Count; pageIndex++)
                {
                    await context.Pages[pageIndex].ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = Path.Combine(resultsDirectory, $"context{contextIndex}-page{pageIndex}.png"),
                        FullPage = true,
                    });
                }

                await context.Tracing.StopAsync(new TracingStopOptions { Path = Path.Combine(resultsDirectory, $"context{contextIndex}-trace.zip") });
            }
            else
            {
                await context.Tracing.StopAsync();
            }

            await context.CloseAsync();
        }

        _contexts.Clear();
        foreach (var session in _sessions)
        {
            session.Dispose();
        }

        _sessions.Clear();
        foreach (var league in _leagues)
        {
            await league.DisposeAsync();
        }

        _leagues.Clear();
        lock (_problems)
        {
            _problems.Clear();
        }

        if (problems.Count > 0)
        {
            Assert.Fail($"The browser reported problems:\n{string.Join("\n", problems)}");
        }
    }

    protected void ForgetProblems()
    {
        lock (_problems)
        {
            _problems.Clear();
        }
    }

    /// <summary>
    /// Unique registration details, in the shape <see cref="LeagueFixtureBuilder"/> takes.
    /// </summary>
    protected static (string Email, string Password, string DisplayName) NewCredentials()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        return ($"u-{id}@uitest.local", "UiTestPassword", $"UI-{id}");
    }

    /// <summary>
    /// A league with its members and publishers, made through the API, and its manager's user ID for signing in.
    /// Disposed after the test.
    /// </summary>
    protected async Task<(LeagueFixture League, Guid ManagerUserID)> NewLeagueAsync(LeagueScenario scenario, bool startDraft = false)
    {
        var league = startDraft
            ? await LeagueFixtureBuilder.CreateAndStartDraftAsync(Factory, scenario, NewCredentials)
            : await LeagueFixtureBuilder.CreateLeagueWithMembersAsync(Factory, scenario, NewCredentials);
        _leagues.Add(league);
        return (league, (await league.Manager.Account.CurrentUserAsync()).UserID);
    }

    protected async Task<TestUser> NewUserAsync()
    {
        var (email, password, displayName) = NewCredentials();
        var session = new ApiSession(Factory);
        _sessions.Add(session);
        await session.RegisterAsync(email, password, displayName);
        if (!await session.LoginAsync(email, password))
        {
            throw new InvalidOperationException($"{email} registered but could not log in.");
        }

        var user = await session.Account.CurrentUserAsync();
        return new TestUser(user.UserID, email, password, displayName, session);
    }

    /// <summary>
    /// A user with the PlusUser role, granted by the local admin. Sign them in after this, so the cookie carries the role.
    /// </summary>
    protected async Task<TestUser> NewPlusUserAsync()
    {
        var user = await NewUserAsync();
        using var admin = new ApiSession(Factory);
        await LocalAdmin.LoginAsync(admin);
        await admin.Admin.GrantRoleAsync(new UserRoleRequest { UserID = user.UserID, RoleName = "PlusUser" });
        await user.Session.LoginAsync(user.Email, user.Password);
        return user;
    }

    protected async Task<IPage> NewPageAsync(ViewportSize? viewport = null)
    {
        var context = await UiRun.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = Factory.BaseAddress.ToString(),
            ViewportSize = viewport ?? new ViewportSize { Width = 1280, Height = 900 },
        });
        _contexts.Add(context);
        WatchForProblems(context);
        await context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true });
        return await context.NewPageAsync();
    }

    /// <summary>
    /// A page signed in as the user through /dev/sign-in-as, opened at the path.
    /// </summary>
    protected Task<IPage> NewSignedInPageAsync(TestUser user, string path = "/home", ViewportSize? viewport = null) =>
        NewSignedInPageAsync(user.UserID, path, viewport);

    protected async Task<IPage> NewSignedInPageAsync(Guid userID, string path = "/home", ViewportSize? viewport = null)
    {
        var page = await NewPageAsync(viewport);
        await page.GotoAsync($"/dev/sign-in-as?userId={userID}&returnUrl={Uri.EscapeDataString(path)}");
        return page;
    }

    private void WatchForProblems(IBrowserContext context)
    {
        var test = $"{GetType().Name}.{TestContext.CurrentContext.Test.Name}";

        context.WebError += (_, webError) => AddProblem($"Page exception on {PathOf(webError.Page)}: {webError.Error}", test, PathOf(webError.Page));
        context.Console += (_, message) =>
        {
            // Chrome logs every failed response this way, and the API answers 401 and 400 by design. Responses are judged below instead.
            if (message.Text.StartsWith("Failed to load resource:", StringComparison.Ordinal))
            {
                return;
            }

            switch (message.Type)
            {
                case "error":
                    AddProblem($"Console error on {PathOf(message.Page)}: {message.Text}", test, PathOf(message.Page));
                    break;
                case "warning":
                    UiRun.ConsoleWarnings.Add(message.Text, test, PathOf(message.Page));
                    break;
            }
        };
        context.Response += (_, response) =>
        {
            var url = new Uri(response.Url);
            if (url.Authority != Factory.BaseAddress.Authority)
            {
                return;
            }

            var isApi = url.AbsolutePath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
            if (response.Status >= 500 || (!isApi && response.Status >= 400))
            {
                AddProblem($"{response.Status} from {response.Request.Method} {url.PathAndQuery}", test, PathOf(response.Frame.Page));
            }
        };
    }

    private void AddProblem(string problem, string test, string page)
    {
        var knownProblem = KnownProblems.Explain(problem);
        if (knownProblem is not null)
        {
            UiRun.ConsoleWarnings.Add($"Known problem ({knownProblem}): {problem.Split('\n')[0]}", test, page);
            return;
        }

        lock (_problems)
        {
            _problems.Add(problem);
        }
    }

    private static string PathOf(IPage? page) => page is null ? "(no page)" : new Uri(page.Url).PathAndQuery;

    private static string SafeFileName(string name) => string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
}
