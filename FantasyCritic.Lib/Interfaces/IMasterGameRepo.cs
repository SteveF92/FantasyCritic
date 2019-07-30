using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.OpenCritic;
using NodaTime;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IMasterGameRepo
    {
        Task<IReadOnlyList<MasterGame>> GetMasterGames();
        Task<IReadOnlyList<MasterGameYear>> GetMasterGameYears(int year, bool useCache);
        Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID);
        Task<Maybe<MasterGameYear>> GetMasterGameYear(Guid masterGameID, int year);
        Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
        Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame);

        Task CreateMasterGame(MasterGame masterGame);
        Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels();
        Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel);

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
        Task CompleteMasterGameRequest(MasterGameRequest masterGameRequest, Instant responseTime, string responseNote, Maybe<MasterGame> masterGame);
        Task CompleteMasterGameChangeRequest(MasterGameChangeRequest masterGameRequest, Instant responseTime, string responseNote);
        Task LinkToOpenCritic(MasterGame masterGame, int openCriticID);
        Task UpdateHypeFactors(IEnumerable<MasterGameHypeScores> hypeScores);
    }
}
