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
}
