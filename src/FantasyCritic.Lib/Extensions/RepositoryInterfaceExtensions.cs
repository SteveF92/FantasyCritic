using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.Lib.Extensions;
public static class RepositoryInterfaceExtensions
{
    public static async Task<League> GetLeagueOrThrow(this IFantasyCriticRepo repo, Guid id)
    {
        var result = await repo.GetLeague(id);
        if (result is null)
        {
            throw new Exception($"League not found: {id}");
        }

        return result;
    }

    public static async Task<LeagueYear> GetLeagueYearOrThrow(this IFantasyCriticRepo repo, League league, int year)
    {
        var leagueYear = await repo.GetLeagueYear(league, year);
        if (leagueYear is null)
        {
            throw new Exception($"League year not found: {league.LeagueID} | {year}");
        }

        return leagueYear;
    }

    public static async Task<MasterGame> GetMasterGameOrThrow(this IMasterGameRepo repo, Guid masterGameID)
    {
        var masterGameResult = await repo.GetMasterGame(masterGameID);
        if (masterGameResult is null)
        {
            throw new Exception($"Master Game not found: {masterGameID}");
        }

        return masterGameResult;
    }

    public static async Task<MasterGameYear> GetMasterGameYearOrThrow(this IMasterGameRepo repo, Guid masterGameID, int year)
    {
        var masterGameResult = await repo.GetMasterGameYear(masterGameID, year);
        if (masterGameResult is null)
        {
            throw new Exception($"Master Game Year not found: {masterGameID}|{year}");
        }

        return masterGameResult;
    }

    public static Task<FantasyCriticUser?> FindByIdAsync(this IReadOnlyFantasyCriticUserStore repo, Guid id, CancellationToken cancellationToken)
    {
        return repo.FindByIdAsync(id.ToString(), cancellationToken);
    }

    public static async Task<FantasyCriticUser> FindByIdOrThrowAsync(this IReadOnlyFantasyCriticUserStore repo, Guid id, CancellationToken cancellationToken)
    {
        var userResult = await repo.FindByIdAsync(id.ToString(), cancellationToken);
        if (userResult is null)
        {
            throw new Exception($"User not found: {id}");
        }

        return userResult;
    }

    public static Task<FantasyCriticUser?> GetUserThatMightExist(this IReadOnlyFantasyCriticUserStore repo, Guid? id, CancellationToken cancellationToken)
    {
        if (!id.HasValue)
        {
            return Task.FromResult<FantasyCriticUser?>(null);
        }

        return repo.FindByIdAsync(id.Value.ToString(), cancellationToken);
    }

    public static Task<FantasyCriticUser?> GetUserThatMightExist(this IReadOnlyFantasyCriticUserStore repo, Guid? id)
    {
        return GetUserThatMightExist(repo, id, CancellationToken.None);
    }
}
