using System;
using System.Linq;
using System.Threading.Tasks;
using FantasyCritic.ApiClient;
using FantasyCritic.IntegrationTests.Helpers;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Royale;

[TestFixture]
public class RoyaleGroupTests : IntegrationTestBase
{
    [Test]
    public async Task CreateManualRoyaleGroup_ValidName_Succeeds()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        Assert.That(created, Is.Not.Null);
        Assert.That(created.GroupID, Is.Not.EqualTo(Guid.Empty));

        var group = await session.RoyaleGroup.GetRoyaleGroupAsync(created.GroupID);
        Assert.That(group, Is.Not.Null);
        Assert.That(group.GroupName, Is.EqualTo(groupName));
        Assert.That(group.MemberCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetRoyaleGroupData_AfterCreation_ReturnsGroupWithCreator()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var data = await session.RoyaleGroup.GetRoyaleGroupDataAsync(created.GroupID);

        Assert.That(data, Is.Not.Null);
        Assert.That(data.Group.GroupName, Is.EqualTo(groupName));
        Assert.That(data.Members, Is.Not.Null);
        Assert.That(data.Members.Count, Is.EqualTo(1));
        Assert.That(data.ActiveQuarter, Is.Not.Null);
    }

    [Test]
    public async Task CreateGroupInviteLink_AsManager_ReturnsActiveLink()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var link = await session.RoyaleGroup.CreateGroupInviteLinkAsync(created.GroupID);

        Assert.That(link, Is.Not.Null);
        Assert.That(link.InviteID, Is.Not.EqualTo(Guid.Empty));
        Assert.That(link.InviteCode, Is.Not.EqualTo(Guid.Empty));
        Assert.That(link.Active, Is.True);
    }

    [Test]
    public async Task GetGroupInviteLinks_AsManager_ReturnsLinks()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        await session.RoyaleGroup.CreateGroupInviteLinkAsync(created.GroupID);

        var links = await session.RoyaleGroup.GetGroupInviteLinksAsync(created.GroupID);

        Assert.That(links, Is.Not.Null);
        Assert.That(links.Count, Is.EqualTo(1));
        Assert.That(links.First().Active, Is.True);
    }

    [Test]
    public async Task DeactivateGroupInviteLink_AsManager_LinkBecomesInactive()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var link = await session.RoyaleGroup.CreateGroupInviteLinkAsync(created.GroupID);
        Assert.That(link.Active, Is.True);

        await session.RoyaleGroup.DeactivateGroupInviteLinkAsync(link.InviteID);

        var links = await session.RoyaleGroup.GetGroupInviteLinksAsync(created.GroupID);
        Assert.That(links, Is.Not.Null);
        Assert.That(links.Count, Is.EqualTo(1));
        Assert.That(links.First().Active, Is.False);
    }

    [Test]
    public async Task SearchRoyaleGroups_WithMatchingName_ReturnsGroup()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        // Use a unique enough prefix to avoid collision with other test groups
        var uniqueToken = Guid.NewGuid().ToString("N")[..8];
        var groupName = $"SrchGrp-{uniqueToken}";
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var results = await session.RoyaleGroup.SearchRoyaleGroupsAsync(uniqueToken);

        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(g => g.GroupID == created.GroupID), Is.True,
            "Search by the unique token in the group name should return the newly created group.");
    }

    [Test]
    public async Task GetGroupsForUser_AfterCreating_ReturnsGroup()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var groups = await session.RoyaleGroup.GetGroupsForUserAsync();

        Assert.That(groups, Is.Not.Null);
        Assert.That(groups.Any(g => g.GroupID == created.GroupID), Is.True,
            "Creator should be a member of their own group.");
    }

    [Test]
    public async Task JoinWithInviteLink_SecondUser_JoinsSuccessfully()
    {
        // ── Manager creates group and invite link ──────────────────────────────
        var (email1, password1, displayName1) = NewUser();
        using var managerSession = new ApiSession(Factory);
        await managerSession.RegisterAsync(email1, password1, displayName1);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await managerSession.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var link = await managerSession.RoyaleGroup.CreateGroupInviteLinkAsync(created.GroupID);

        // ── Second user joins via invite code ──────────────────────────────────
        var (email2, password2, displayName2) = NewUser();
        using var joinerSession = new ApiSession(Factory);
        await joinerSession.RegisterAsync(email2, password2, displayName2);

        await joinerSession.RoyaleGroup.JoinWithInviteLinkAsync(
            new JoinRoyaleGroupWithInviteLinkRequest { InviteCode = link.InviteCode });

        // ── Verify group now has two members ──────────────────────────────────
        var group = await joinerSession.RoyaleGroup.GetRoyaleGroupAsync(created.GroupID);
        Assert.That(group, Is.Not.Null);
        Assert.That(group.MemberCount, Is.EqualTo(2),
            "Group should have two members after the second user joins.");

        // ── Verify group appears in the joiner's list ─────────────────────────
        var joinerGroups = await joinerSession.RoyaleGroup.GetGroupsForUserAsync();
        Assert.That(joinerGroups.Any(g => g.GroupID == created.GroupID), Is.True,
            "The joined group should appear in the joiner's group list.");
    }

    [Test]
    public async Task LeaveGroup_AfterJoining_RemovesSelfFromGroup()
    {
        // ── Manager creates group and invite link ──────────────────────────────
        var (email1, password1, displayName1) = NewUser();
        using var managerSession = new ApiSession(Factory);
        await managerSession.RegisterAsync(email1, password1, displayName1);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await managerSession.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var link = await managerSession.RoyaleGroup.CreateGroupInviteLinkAsync(created.GroupID);

        // ── Second user joins ──────────────────────────────────────────────────
        var (email2, password2, displayName2) = NewUser();
        using var joinerSession = new ApiSession(Factory);
        await joinerSession.RegisterAsync(email2, password2, displayName2);

        await joinerSession.RoyaleGroup.JoinWithInviteLinkAsync(
            new JoinRoyaleGroupWithInviteLinkRequest { InviteCode = link.InviteCode });

        var groupAfterJoin = await joinerSession.RoyaleGroup.GetRoyaleGroupAsync(created.GroupID);
        Assert.That(groupAfterJoin.MemberCount, Is.EqualTo(2));

        // ── Second user leaves ─────────────────────────────────────────────────
        await joinerSession.RoyaleGroup.LeaveGroupAsync(created.GroupID);

        var groupAfterLeave = await managerSession.RoyaleGroup.GetRoyaleGroupAsync(created.GroupID);
        Assert.That(groupAfterLeave.MemberCount, Is.EqualTo(1),
            "Group should be back to one member after the joiner leaves.");

        var joinerGroups = await joinerSession.RoyaleGroup.GetGroupsForUserAsync();
        Assert.That(joinerGroups.Any(g => g.GroupID == created.GroupID), Is.False,
            "Left group must no longer appear in the leaver's group list.");
    }

    [Test]
    public async Task RemoveMember_AsManager_RemovesMemberFromGroup()
    {
        // ── Manager creates group and invite link ──────────────────────────────
        var (email1, password1, displayName1) = NewUser();
        using var managerSession = new ApiSession(Factory);
        await managerSession.RegisterAsync(email1, password1, displayName1);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await managerSession.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var link = await managerSession.RoyaleGroup.CreateGroupInviteLinkAsync(created.GroupID);

        // ── Second user joins ──────────────────────────────────────────────────
        var (email2, password2, displayName2) = NewUser();
        using var joinerSession = new ApiSession(Factory);
        await joinerSession.RegisterAsync(email2, password2, displayName2);

        await joinerSession.RoyaleGroup.JoinWithInviteLinkAsync(
            new JoinRoyaleGroupWithInviteLinkRequest { InviteCode = link.InviteCode });

        // ── Get the joiner's UserID from the group data ────────────────────────
        var data = await managerSession.RoyaleGroup.GetRoyaleGroupDataAsync(created.GroupID);
        Assert.That(data.Members.Count, Is.EqualTo(2));

        // The manager's display name differs — find the joiner's entry
        var joinerMember = data.Members.FirstOrDefault(m => m.DisplayName == displayName2);
        Assert.That(joinerMember, Is.Not.Null, "Should find the joiner in the group members.");

        // ── Manager removes the joiner ─────────────────────────────────────────
        await managerSession.RoyaleGroup.RemoveMemberAsync(
            new RemoveRoyaleGroupMemberRequest
            {
                GroupID = created.GroupID,
                UserID = joinerMember!.UserID,
            });

        var groupAfterRemove = await managerSession.RoyaleGroup.GetRoyaleGroupAsync(created.GroupID);
        Assert.That(groupAfterRemove.MemberCount, Is.EqualTo(1),
            "Group should be back to one member after the manager removes the joiner.");
    }

    [Test]
    public async Task GetRoyaleGroupQuarter_ForActiveQuarter_ReturnsData()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);

        var groupName = $"Group-{Guid.NewGuid():N}"[..20];
        var created = await session.RoyaleGroup.CreateManualRoyaleGroupAsync(
            new CreateManualRoyaleGroupRequest { GroupName = groupName });

        var activeQuarter = await session.Royale.ActiveRoyaleQuarterAsync();
        Assert.That(activeQuarter, Is.Not.Null);

        var quarterData = await session.RoyaleGroup.GetRoyaleGroupQuarterAsync(
            created.GroupID, activeQuarter.Year, activeQuarter.Quarter);

        Assert.That(quarterData, Is.Not.Null);
        Assert.That(quarterData.Year, Is.EqualTo(activeQuarter.Year));
        Assert.That(quarterData.Quarter, Is.EqualTo(activeQuarter.Quarter));
    }
}
