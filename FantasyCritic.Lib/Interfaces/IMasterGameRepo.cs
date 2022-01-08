using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.OpenCritic;
using NodaTime;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IMasterGameRepo
    {
        Task<IReadOnlyList<MasterGame>> GetMasterGames();
        Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year);
        Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID);
        Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year);
        Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
        Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame);

        Task CreateMasterGame(MasterGame masterGame);
        Task EditMasterGame(MasterGame masterGame);

        Task<IReadOnlyList<Guid>> GetAllSelectedMasterGameIDsForYear(int year);

        Task CreateMasterGameRequest(MasterGameRequest domainRequest);
        Task CreateMasterGameChangeRequest(MasterGameChangeRequest domainRequest);
        Task<IReadOnlyList<MasterGameRequest>> GetMasterGameRequestsForUser(FantasyCriticUser user);
        Task<IReadOnlyList<MasterGameChangeRequest>> GetMasterGameChangeRequestsForUser(FantasyCriticUser user);
        Task<Maybe<MasterGameRequest>> GetMasterGameRequest(Guid requestID);
        Task<Maybe<MasterGameChangeRequest>> GetMasterGameChangeRequest(Guid requestID);
        Task DeleteMasterGameRequest(MasterGameRequest request);
        Task DeleteMasterGameChangeRequest(MasterGameChangeRequest request);
        Task DismissMasterGameRequest(MasterGameRequest masterGameRequest);
        Task DismissMasterGameChangeRequest(MasterGameChangeRequest request);
        Task<IReadOnlyList<MasterGameRequest>> GetAllMasterGameRequests();
        Task<IReadOnlyList<MasterGameChangeRequest>> GetAllMasterGameChangeRequests();
        Task<int> GetNumberOutstandingCorrections(MasterGame masterGame);
        Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote, Maybe<MasterGame> masterGame);
        Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime, string responseNote);
        Task LinkToOpenCritic(MasterGame masterGame, int openCriticID);
        Task LinkToGG(MasterGame masterGame, string ggToken);
        Task UpdateReleaseDateEstimates(LocalDate tomorrow);
        Task UpdateCalculatedStats(IEnumerable<MasterGameCalculatedStats> calculatedStats, int year);

        Task<IReadOnlyList<MasterGameTag>> GetMasterGameTags();
        Task<IReadOnlyDictionary<string, MasterGameTag>> GetMasterGameTagDictionary();
        Task UpdateCodeBasedTags(IReadOnlyDictionary<MasterGame, IReadOnlyList<MasterGameTag>> tagsToAdd);
    }
}
