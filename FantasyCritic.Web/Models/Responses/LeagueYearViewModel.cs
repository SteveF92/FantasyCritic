using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, SupportedYear supportedYear, IEnumerable<Publisher> publishers, Maybe<Publisher> userPublisher,
            LocalDate currentDate, PlayStatus playStatus, StartDraftResult startDraftResult, IEnumerable<FantasyCriticUser> activeUsers, Maybe<Publisher> nextDraftPublisher,
            DraftPhase draftPhase, IEnumerable<PublisherGame> availableCounterPicks, LeagueOptions options, SystemWideValues systemWideValues,
            IEnumerable<LeagueInvite> invitedPlayers, bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager,
            Maybe<FantasyCriticUser> accessingUser, IEnumerable<ManagerMessage> managerMessages, Maybe<FantasyCriticUser> previousYearWinner)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            SupportedYear = new SupportedYearViewModel(supportedYear);
            StandardGames = leagueYear.Options.StandardGames;
            GamesToDraft = leagueYear.Options.GamesToDraft;
            CounterPicks = leagueYear.Options.CounterPicks;
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            PickupSystem = leagueYear.Options.PickupSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
            UnlinkedGameExists = publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame.HasNoValue);
            UserIsActive = activeUsers.Any(x => x.UserID == accessingUser.Unwrap(y => y.UserID));
            Publishers = publishers
                .OrderBy(x => x.DraftPosition)
                .Select(x => new PublisherViewModel(x, currentDate, nextDraftPublisher, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished))
                .ToList();

            if (userPublisher.HasValue)
            {
                UserPublisher = new PublisherViewModel(userPublisher.Value, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished);
            }

            List<PlayerWithPublisherViewModel> playerVMs = new List<PlayerWithPublisherViewModel>();
            bool allPublishersMade = true;
            foreach (var user in activeUsers)
            {
                var publisher = publishers.SingleOrDefault(x => x.User.UserID == user.UserID);
                if (publisher is null)
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, false));
                    allPublishersMade = false;
                }
                else
                {
                    bool isPreviousYearWinner = previousYearWinner.HasValue && previousYearWinner.Value.UserID == user.UserID;
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, currentDate, options, systemWideValues,
                        userIsInLeague, userIsInvitedToLeague, supportedYear, false, isPreviousYearWinner));
                }
            }

            foreach (var invitedPlayer in invitedPlayers)
            {
                allPublishersMade = false;

                if (invitedPlayer.User.HasValue)
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, invitedPlayer.User.Value.UserName));
                }
                else
                {
                    if (accessingUser.HasValue)
                    {
                        if (userIsManager || string.Equals(invitedPlayer.EmailAddress, accessingUser.Value.Email, StringComparison.OrdinalIgnoreCase))
                        {
                            playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, invitedPlayer.EmailAddress));
                        }
                        else
                        {
                            playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, "<Email Address Hidden>"));
                        }
                    }
                    else
                    {
                        playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.InviteID, "<Email Address Hidden>"));
                    }
                }
            }

            bool readyToSetDraftOrder = false;
            if (allPublishersMade)
            {
                Players = playerVMs.OrderBy(x => x.Publisher.DraftPosition).ToList();
                readyToSetDraftOrder = true;
            }
            else
            {
                Players = playerVMs;
            }

            PlayStatus = new PlayStatusViewModel(playStatus, readyToSetDraftOrder, startDraftResult.Ready, startDraftResult.Errors, draftPhase);
            AvailableCounterPicks = availableCounterPicks
                .Select(x => new PublisherGameViewModel(x, currentDate, leagueYear.Options.ScoringSystem, systemWideValues))
                .OrderBy(x => x.GameName).ToList();
            EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();

            ManagerMessages = managerMessages.Select(x => new ManagerMessageViewModel(x, x.IsDismissed(accessingUser))).OrderBy(x => x.Timestamp).ToList();
            if (!userIsInLeague)
            {
                ManagerMessages = ManagerMessages.Where(x => x.IsPublic).ToList();
            }
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public SupportedYearViewModel SupportedYear { get; }
        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public string DraftSystem { get; }
        public string PickupSystem { get; }
        public string ScoringSystem { get; }
        public bool UnlinkedGameExists { get; }
        public bool UserIsActive { get; }
        public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public IReadOnlyList<EligibilityOverrideViewModel> EligibilityOverrides { get; }
        public PublisherViewModel UserPublisher { get; }
        public PlayStatusViewModel PlayStatus { get; }
        public IReadOnlyList<PublisherGameViewModel> AvailableCounterPicks { get; }
        public IReadOnlyList<ManagerMessageViewModel> ManagerMessages { get; }
    }
}
