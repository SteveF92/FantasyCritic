using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace FantasyCritic.UiTests;

/// <summary>
/// What every UI test in the run shares: one browser, the client build it is served, and the console warnings report.
/// </summary>
[SetUpFixture]
public sealed class UiRun
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    public static IBrowser Browser => _browser ?? throw new InvalidOperationException("The browser has not been launched.");
    public static string Dist { get => field ?? throw new InvalidOperationException("The client build has not been found."); private set; }
    public static ConsoleWarningsReport ConsoleWarnings { get; } = new();

    public static string ResultsDirectory { get; } = Path.Combine(RepositoryRoot.Find(), "src", "FantasyCritic.UiTests", "TestResults", "ui");

    [OneTimeSetUp]
    public async Task LaunchBrowser()
    {
        Dist = UiWebApplicationFactory.FindCurrentDist();
        if (Directory.Exists(ResultsDirectory))
        {
            Directory.Delete(ResultsDirectory, recursive: true);
        }

        Directory.CreateDirectory(ResultsDirectory);

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Channel = "chrome",
            Headless = Environment.GetEnvironmentVariable("UI_TESTS_HEADED") != "1",
        });
    }

    [OneTimeTearDown]
    public async Task CloseBrowser()
    {
        await File.WriteAllTextAsync(Path.Combine(ResultsDirectory, "console-warnings.md"), ConsoleWarnings.ToMarkdown());

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
        }

        _playwright?.Dispose();
    }
}
