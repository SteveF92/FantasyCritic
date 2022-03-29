using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.OpenCritic;

namespace FantasyCritic.Lib.Interfaces;

public interface IMasterGameRepo
{
    Task<IReadOnlyList<MasterGame>> GetMasterGames();
    Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year);
    Task<MasterGame?> GetMasterGame(Guid masterGameID);
    Task<MasterGameYear?> GetMasterGameYear(Guid masterGameID, int year);
    Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
    Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame);

    Task CreateMasterGame(MasterGame masterGame);
    Task EditMasterGame(MasterGame masterGame);

    Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year);

    Task CreateMasterGameRequest(MasterGameRequest domainRequest);
    Task CreateMasterGameChangeRequest(MasterGameChangeRequest domainRequest);
    Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user);
    Task<IReadOnlyList<MasterGameChangeRequest>> GetMasterGameChangeRequestsForUser(FantasyCriticUser user);
    Task<MasterGameRequest?> GetMasterGameRequest(Guid requestID);
    Task<MasterGameChangeRequest?> GetMasterGameChangeRequest(Guid requestID);
    Task DeleteMasterGameRequest(MasterGameRequest request);
    Task DeleteMasterGameChangeRequest(MasterGameChangeRequest request);
    Task DismissMasterGameRequest(MasterGameRequest masterGameRequest);
    Task DismissMasterGameChangeRequest(MasterGameChangeRequest request);
    Task<IReadOnlyList<MasterGameRequest>> GetAllMasterGameRequests();
    Task<IReadOnlyList<MasterGameChangeRequest>> GetAllMasterGameChangeRequests();
    Task<int> GetNumberOutstandingCorrections(MasterGame masterGame);
    Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote, MasterGame? masterGame);
    Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime, string responseNote);
    Task LinkToOpenCritic(MasterGame masterGame, int openCriticID);
    Task LinkToGG(MasterGame masterGame, string ggToken);
    Task UpdateReleaseDateEstimates(LocalDate tomorrow);
    Task UpdateCalculatedStats(IEnumerable<MasterGameCalculatedStats> calculatedStats, int year);

    Task<IReadOnlyList<MasterGameTag>> GetMasterGameTags();
    Task<IReadOnlyDictionary<string, MasterGameTag>> GetMasterGameTagDictionary();
    Task UpdateCodeBasedTags(IReadOnlyDictionary<MasterGame, IReadOnlyList<MasterGameTag>> tagsToAdd);
    Task UpdateGGStats(MasterGame masterGame, GGGame ggGame);

    async Task<MasterGame> GetMasterGameOrThrow(Guid masterGameID)
    {
        var masterGameResult = await GetMasterGame(masterGameID);
        if (masterGameResult is null)
        {
            throw new Exception($"Master Game not found: {masterGameID}");
        }

        return masterGameResult;
    }

    async Task<MasterGameYear> GetMasterGameYearOrThrow(Guid masterGameID, int year)
    {
        var masterGameResult = await GetMasterGameYear(masterGameID, year);
        if (masterGameResult is null)
        {
            throw new Exception($"Master Game Year not found: {masterGameID}|{year}");
        }

        return masterGameResult;
    }
}
