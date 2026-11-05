using System;
using System.Threading.Tasks;

namespace FantasyCritic.IntegrationTests.Helpers;

/// <summary>
/// The pre-seeded local admin user created by FantasyCritic.LocalDatabaseTool. UserID = Guid.Empty; has every role.
/// </summary>
public static class LocalAdmin
{
    public const string Email = "localadmin@example.com";
    public const string Password = "localadminpassword";

    /// <summary>
    /// Logs the session in as the local admin. Throws if the login fails (most likely cause: DB not seeded).
    /// </summary>
    public static async Task LoginAsync(ApiSession session)
    {
        var success = await session.LoginAsync(Email, Password);
        if (!success)
        {
            throw new InvalidOperationException(
                "Local admin login failed. Is the Docker DB running and seeded? " +
                "Run: docker compose -f infrastructure/docker-compose-mysql.yaml up -d");
        }
    }
}
