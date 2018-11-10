using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.OpenCritic;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IFantasyCriticRepo
    {
        Task<Maybe<League>> GetLeagueByID(Guid id);
        Task<Maybe<LeagueYear>> GetLeagueYear(League requestLeague, int requestYear);
        Task CreateLeague(League league, int initialYear, LeagueOptions options);
        Task AddNewLeagueYear(League league, int year, LeagueOptions options);
        Task EditLeagueYear(LeagueYear leagueYear);

        Task<IReadOnlyList<FantasyCriticUser>> GetUsersInLeague(League league);
        Task<IReadOnlyList<League>> GetLeaguesForUser(FantasyCriticUser currentUser);
        Task<IReadOnlyList<League>> GetLeaguesInvitedTo(FantasyCriticUser currentUser);
        Task SaveInvite(League league, FantasyCriticUser user);
        Task<IReadOnlyList<FantasyCriticUser>> GetOutstandingInvitees(League league);
        Task AcceptInvite(League league, FantasyCriticUser inviteUser);
        Task DeclineInvite(League league, FantasyCriticUser inviteUser);

        Task RemovePublisher(Publisher publisherValue);
        Task RemovePlayerFromLeague(League league, FantasyCriticUser removeUser);

        Task<Maybe<Publisher>> GetPublisher(Guid publisherID);
        Task<Maybe<Publisher>> GetPublisher(League league, int year, FantasyCriticUser user);
        Task<Maybe<PublisherGame>> GetPublisherGame(Guid publisherGameID);
        Task CreatePublisher(Publisher publisher);
        Task<IReadOnlyList<Publisher>> GetPublishersInLeagueForYear(League league, int year);
        Task AddPublisherGame(Publisher publisher, PublisherGame publisherGame);
        Task AssociatePublisherGame(Publisher publisher, PublisherGame publisherGame, MasterGame masterGame);

        Task<IReadOnlyList<SupportedYear>> GetSupportedYears();

        Task<IReadOnlyList<MasterGame>> GetMasterGames();
        Task<Maybe<MasterGame>> GetMasterGame(Guid masterGameID);
        Task UpdateCriticStats(MasterGame masterGame, OpenCriticGame openCriticGame);
        Task UpdateCriticStats(MasterSubGame masterSubGame, OpenCriticGame openCriticGame);

        Task<IReadOnlyList<LeagueYear>> GetLeagueYears(int year);
        Task UpdateFantasyPoints(Dictionary<Guid, decimal?> publisherGameScores);
        Task CreateMasterGame(MasterGame masterGame);
        Task<IReadOnlyList<EligibilityLevel>> GetEligibilityLevels();
        Task<EligibilityLevel> GetEligibilityLevel(int eligibilityLevel);
        Task<Result> RemovePublisherGame(Guid publisherGameID);
        Task ManuallyScoreGame(PublisherGame publisherGame, decimal? manualCriticScore);

        Task CreatePickupBid(PickupBid currentBid);
        Task RemovePickupBid(PickupBid bid);
        Task<IReadOnlyList<PickupBid>> GetActivePickupBids(Publisher publisher);
        Task<Maybe<PickupBid>> GetPickupBid(Guid bidID);
        Task MarkBidStatus(PickupBid bid, bool success);
        Task SpendBudget(Publisher successBidPublisher, uint successBidBidAmount);
        Task AddLeagueAction(LeagueAction action);
        Task<IReadOnlyList<LeagueAction>> GetLeagueActions(LeagueYear leagueYear);
        Task ChangePublisherName(Publisher publisher, string publisherName);
        Task ChangeLeagueName(League league, string leagueName);
        Task StartPlay(LeagueYear leagueYear);
        Task SetDraftOrder(IEnumerable<KeyValuePair<Publisher, int>> draftPositions);
    }
}
