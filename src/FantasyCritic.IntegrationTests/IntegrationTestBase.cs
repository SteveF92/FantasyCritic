using System;
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
}
