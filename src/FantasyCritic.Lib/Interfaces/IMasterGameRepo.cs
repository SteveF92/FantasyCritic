using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.OpenCritic;

namespace FantasyCritic.Lib.Interfaces;

public interface IMasterGameRepo
{
    void ClearMasterGameCache();
    void ClearMasterGameYearCache();
    Task<IReadOnlyList<MasterGame>> GetMasterGames();
    Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year);
    Task<MasterGame?> GetMasterGame(Guid masterGameID);
    Task<MasterGameYear?> GetMasterGameYear(Guid masterGameID, int year);
    Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
    Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame);

    Task CreateMasterGame(MasterGame masterGame);
    Task EditMasterGame(MasterGame masterGame, IEnumerable<MasterGameChangeLogEntry> changeLogEntries);

    Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year);

    Task<IReadOnlyList<MasterGameChangeLogEntry>> GetMasterGameChangeLog(MasterGame masterGame);
    Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges();
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
    Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote,
        FantasyCriticUser responseUser, MasterGame? masterGame);
    Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime,
        FantasyCriticUser responseUser, string responseNote);
    Task LinkToOpenCritic(MasterGame masterGame, int openCriticID);
    Task LinkToGG(MasterGame masterGame, string ggToken);
    Task UpdateReleaseDateEstimates(LocalDate tomorrow);
    Task UpdateCalculatedStats(IEnumerable<MasterGameCalculatedStats> calculatedStats, int year);

    Task<IReadOnlyList<MasterGameTag>> GetMasterGameTags();
    Task<IReadOnlyDictionary<string, MasterGameTag>> GetMasterGameTagDictionary();
    Task UpdateCodeBasedTags(IReadOnlyDictionary<MasterGame, IReadOnlyList<MasterGameTag>> tagsToAdd);
    Task UpdateGGStats(MasterGame masterGame, GGGame ggGame);

    Task<IReadOnlyList<LocalDate>> GetProcessingDatesForTopBidsAndDrops();
    Task<IReadOnlyList<TopBidsAndDropsGame>> GetTopBidsAndDrops(LocalDate processingDate);
    Task<IReadOnlyList<LeagueYearWithMasterGame>> GetLeagueYearsWithMasterGameForUser(Guid userID, Guid masterGameID);

    Task<MasterGame> GetTestMasterGame(int year);
    Task<MasterGameYear> GetTestMasterGameYear(int year);

}
