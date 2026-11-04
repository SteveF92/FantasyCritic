using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

    protected async Task<TestUser> NewUserAsync()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        var email = $"u-{id}@uitest.local";
        const string password = "UiTestPassword";
        var displayName = $"UI-{id}";

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
    protected async Task<IPage> NewSignedInPageAsync(TestUser user, string path = "/home", ViewportSize? viewport = null)
    {
        var page = await NewPageAsync(viewport);
        await page.GotoAsync($"/dev/sign-in-as?userId={user.UserID}&returnUrl={Uri.EscapeDataString(path)}");
        return page;
    }

    private void WatchForProblems(IBrowserContext context)
    {
        var test = $"{GetType().Name}.{TestContext.CurrentContext.Test.Name}";

        context.WebError += (_, webError) => AddProblem($"Page exception on {PathOf(webError.Page)}: {webError.Error}");
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
                    AddProblem($"Console error on {PathOf(message.Page)}: {message.Text}");
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
                AddProblem($"{response.Status} from {response.Request.Method} {url.PathAndQuery}");
            }
        };
    }

    private void AddProblem(string problem)
    {
        lock (_problems)
        {
            _problems.Add(problem);
        }
    }

    private static string PathOf(IPage? page) => page is null ? "(no page)" : new Uri(page.Url).PathAndQuery;

    private static string SafeFileName(string name) => string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
}
