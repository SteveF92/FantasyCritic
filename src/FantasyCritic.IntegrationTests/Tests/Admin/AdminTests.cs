using System;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Admin;

[TestFixture]
public class AdminTests : IntegrationTestBase
{
    [Test]
    public async Task GrantRole_FactChecker_RoleAppearsInGetUserInfo()
    {
        // Register a fresh user and get their ID.
        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);
        var me = await userSession.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        // Verify FactChecker is NOT in roles yet.
        var before = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(before.Roles, Does.Not.Contain("FactChecker"),
            "New user should not have FactChecker role before it is granted.");

        // Grant the role.
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "FactChecker",
        });

        // Verify it appears.
        var after = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(after.Roles, Contains.Item("FactChecker"),
            "FactChecker role should be present after GrantRole.");
    }

    [Test]
    public async Task RemoveRole_FactChecker_RoleDisappearsFromGetUserInfo()
    {
        var (email, password, displayName) = NewUser();
        using var userSession = new ApiSession(Factory);
        await userSession.RegisterAsync(email, password, displayName);
        var me = await userSession.Account.CurrentUserAsync();

        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        // Grant the role first.
        await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "FactChecker",
        });
        var withRole = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(withRole.Roles, Contains.Item("FactChecker"),
            "Role should be present after GrantRole.");

        // Remove the role.
        await adminSession.Admin.RemoveRoleAsync(new UserRoleRequest
        {
            UserID = me.UserID,
            RoleName = "FactChecker",
        });
        var withoutRole = await adminSession.Admin.GetUserInfoAsync(me.UserID);
        Assert.That(withoutRole.Roles, Does.Not.Contain("FactChecker"),
            "FactChecker role should be gone after RemoveRole.");
    }

    [Test]
    public async Task GrantRole_UnknownUser_Returns404()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        ApiException? ex = null;
        try
        {
            await adminSession.Admin.GrantRoleAsync(new UserRoleRequest
            {
                UserID = Guid.NewGuid(),
                RoleName = "FactChecker",
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException to be thrown for unknown user.");
        Assert.That(ex!.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GrantRole_AsNonAdmin_Returns403()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        ApiException? ex = null;
        try
        {
            await session.Admin.GrantRoleAsync(new UserRoleRequest
            {
                UserID = Guid.NewGuid(),
                RoleName = "FactChecker",
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException to be thrown for non-admin user.");
        Assert.That(ex!.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task SetInitialTime_AsAdmin_Returns200()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
        });
        // No exception thrown means the endpoint returned 200.
    }

    [Test]
    public async Task SetTime_Forward_Returns200()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 6, 0, 0, 0, TimeSpan.Zero)
        });

        await adminSession.Admin.SetTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 1, 13, 0, 0, 0, TimeSpan.Zero)
        });
        // No exception thrown means the endpoint returned 200.
    }

    [Test]
    public async Task SetTime_Backwards_Returns400()
    {
        using var adminSession = new ApiSession(Factory);
        await LoginAsLocalAdminAsync(adminSession);

        await adminSession.Admin.SetInitialTimeAsync(new SetTimeRequest
        {
            NewTime = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero)
        });

        ApiException? ex = null;
        try
        {
            await adminSession.Admin.SetTimeAsync(new SetTimeRequest
            {
                NewTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
            });
        }
        catch (ApiException caught)
        {
            ex = caught;
        }

        Assert.That(ex, Is.Not.Null, "Expected ApiException for backwards time travel.");
        Assert.That(ex!.StatusCode, Is.EqualTo(400));
    }
}
