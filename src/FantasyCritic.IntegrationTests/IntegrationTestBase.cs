using System;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests;

/// <summary>
/// Base class for all integration test fixtures.
/// Manages the <see cref="FantasyCriticWebApplicationFactory"/> lifecycle so every
/// fixture gets one app instance (fast — factory cold-start happens once per fixture,
/// not once per test) and disposes it cleanly after all tests in the fixture run.
/// </summary>
public abstract class IntegrationTestBase
{
    protected FantasyCriticWebApplicationFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        Factory = new FantasyCriticWebApplicationFactory();
    }

    [OneTimeTearDown]
    public void BaseOneTimeTearDown()
    {
        Factory.Dispose();
    }

    /// <summary>
    /// Returns unique registration credentials that will never collide across
    /// test runs. Display name stays within the 30-char limit; password meets
    /// the minimum requirements (≥10 chars, ≥5 unique chars).
    /// </summary>
    protected static (string email, string password, string displayName) NewUser()
    {
        var id = Guid.NewGuid().ToString("N")[..12];
        return (
            $"u-{id}@integrationtest.local",
            "IntegrationTestPass",
            $"T-{id}"                         // "T-" + 12 hex chars = 14 chars
        );
    }

    /// <summary>
    /// Credentials for the pre-seeded local admin user created by FantasyCritic.LocalDatabaseTool.
    /// UserID = Guid.Empty; has every role.
    /// </summary>
    protected const string LocalAdminEmail = LocalAdmin.Email;
    protected const string LocalAdminPassword = LocalAdmin.Password;

    /// <summary>
    /// Logs an <see cref="ApiSession"/> in as the pre-seeded local admin user.
    /// Throws if the login fails (most likely cause: DB not seeded — run docker compose first).
    /// </summary>
    internal static Task LoginAsLocalAdminAsync(ApiSession session) => LocalAdmin.LoginAsync(session);
}
