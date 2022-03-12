using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.FakeRepo.Factories;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.GG;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.OpenCritic;
using NodaTime;

namespace FantasyCritic.FakeRepo;

public class FakeMasterGameRepo : IMasterGameRepo
{
    private readonly List<MasterGame> _masterGames;

    public FakeMasterGameRepo()
    {
        _masterGames = MasterGameFactory.GetMasterGames();
    }

    public Task<IReadOnlyList<MasterGameChangeRequest>> GetAllMasterGameChangeRequests()
    {
        throw new NotImplementedException();
    }

    public Task<int> GetNumberOutstandingCorrections(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote, Maybe<MasterGame> masterGame)
    {
        throw new NotImplementedException();
    }

    public Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime, string responseNote)
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

    public Task CreateMasterGame(MasterGame masterGame)
    {
        throw new NotImplementedException();
    }

    public Task EditMasterGame(MasterGame masterGame)
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

    public Task<Maybe<MasterGameChangeRequest>> GetMasterGameChangeRequest(Guid requestID)
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

    public Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year)
    {
        throw new NotImplementedException();
    }

    public Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID)
    {
        Maybe<MasterGame> masterGame = _masterGames.SingleOrDefault(x => x.MasterGameID == masterGameID);
        return Task.FromResult(masterGame);
    }

    public Task<IReadOnlyList<MasterGameChangeRequest>> GetMasterGameChangeRequestsForUser(FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<Maybe<MasterGameRequest>> GetMasterGameRequest(Guid requestID)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGame>> GetMasterGames()
    {
        return Task.FromResult<IReadOnlyList<MasterGame>>(_masterGames);
    }

    public Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year, bool useCache)
    {
        var masterGameYears = _masterGames.Select(x => new MasterGameYear(x, year)).ToList();
        return Task.FromResult<IReadOnlyList<MasterGameYear>>(masterGameYears);
    }

    public Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame)
    {
        throw new NotImplementedException();
    }

    public Task UpdateReleaseDateEstimates(LocalDate tomorrow)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year)
    {
        throw new NotImplementedException();
    }
}