using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;

public interface IReadOnlyFantasyCriticUserStore
{
    void ClearUserCache();
    Task<FantasyCriticUser?> FindByIdAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers();
    Task<IReadOnlyList<FantasyCriticUser>> GetUsers(IEnumerable<Guid> userIDs);
    Task<int> GetOpenDisplayNumber(string displayName);
    Task<FantasyCriticUser?> FindByDisplayName(string displayName, int displayNumber);

    Task<FantasyCriticUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken);
    Task<SupportTicket?> GetSupportTicket(Guid supportTicketID);
    Task<SupportTicket?> GetActiveSupportTicket(Guid userID);
    Task<IReadOnlyList<SupportTicket>> GetAllActiveSupportTickets();
    Task<IReadOnlyList<FantasyCriticUser>> SearchUsersForSupport(SupportUserSearchKind searchKind, string searchValue);
}
