using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using NodaTime;

namespace FantasyCritic.Test.Mocks;
public class MockedMasterGameRepo : IMasterGameRepo
{
    public MockedMasterGameRepo(IEnumerable<MasterGame> masterGames, IEnumerable<MasterGameYear> masterGameYears)
    {

    }

    public Task<IReadOnlyList<MasterGame>> GetMasterGames()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
    {
        throw new NotImplementedException();
    }

    public Task<MasterGame?> GetMasterGame(Guid masterGameID)
    {
        throw new NotImplementedException();
    }

    public Task<MasterGameYear?> GetMasterGameYear(Guid masterGameID, int year)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
    {
        throw new NotImplementedException();
    }

    public Task CreateMasterGame(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task EditMasterGame(MasterGame masterGame, IEnumerable<MasterGameChangeLogEntry> changeLogEntries)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameChangeLogEntry>> GetMasterGameChangeLog(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameChangeLogEntry>> GetRecentMasterGameChanges()
    {
        throw new NotImplementedException();
    }

    public Task CreateMasterGameRequest(MasterGameRequest domainRequest)
    {
        throw new NotImplementedException();
    }

    public Task CreateMasterGameChangeRequest(MasterGameChangeRequest domainRequest)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameChangeRequest>> GetMasterGameChangeRequestsForUser(FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<MasterGameRequest?> GetMasterGameRequest(Guid requestID)
    {
        throw new NotImplementedException();
    }

    public Task<MasterGameChangeRequest?> GetMasterGameChangeRequest(Guid requestID)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMasterGameRequest(MasterGameRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMasterGameChangeRequest(MasterGameChangeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DismissMasterGameRequest(MasterGameRequest masterGameRequest)
    {
        throw new NotImplementedException();
    }

    public Task DismissMasterGameChangeRequest(MasterGameChangeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameRequest>> GetAllMasterGameRequests()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameChangeRequest>> GetAllMasterGameChangeRequests()
    {
        throw new NotImplementedException();
    }

    public Task<int> GetNumberOutstandingCorrections(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote,
        FantasyCriticUser responseUser, MasterGame? masterGame)
    {
        throw new NotImplementedException();
    }

    public Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime,
        FantasyCriticUser responseUser, string responseNote)
    {
        throw new NotImplementedException();
    }

    public Task LinkToOpenCritic(MasterGame masterGame, int openCriticID)
    {
        throw new NotImplementedException();
    }

    public Task LinkToGG(MasterGame masterGame, string ggToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateReleaseDateEstimates(LocalDate tomorrow)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCalculatedStats(IEnumerable<MasterGameCalculatedStats> calculatedStats, int year)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameTag>> GetMasterGameTags()
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyDictionary<string, MasterGameTag>> GetMasterGameTagDictionary()
    {
        throw new NotImplementedException();
    }

    public Task UpdateCodeBasedTags(IReadOnlyDictionary<MasterGame, IReadOnlyList<MasterGameTag>> tagsToAdd)
    {
        throw new NotImplementedException();
    }

    public Task UpdateGGStats(MasterGame masterGame, GGGame ggGame)
    {
        throw new NotImplementedException();
    }
}
