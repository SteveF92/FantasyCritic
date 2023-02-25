using FantasyCritic.Lib.Identity;

namespace FantasyCritic.Lib.Interfaces;

public interface IReadOnlyFantasyCriticUserStore
{
    Task<FantasyCriticUser?> FindByIdAsync(string userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<FantasyCriticUser>> GetAllUsers();
    Task<IReadOnlyList<FantasyCriticUser>> GetUsers(IEnumerable<Guid> userIDs);
    Task<int> GetOpenDisplayNumber(string displayName);
    Task<FantasyCriticUser?> FindByDisplayName(string displayName, int displayNumber);

    Task<FantasyCriticUser?> FindByLoginAsync(string loginProvider, string providerKey,
        CancellationToken cancellationToken);
}
