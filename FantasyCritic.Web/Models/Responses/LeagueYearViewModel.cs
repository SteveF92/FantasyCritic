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
            LocalDate currentDate, StartDraftResult startDraftResult, IEnumerable<FantasyCriticUser> activeUsers, Maybe<Publisher> nextDraftPublisher,
            DraftPhase draftPhase, SystemWideValues systemWideValues,
            IEnumerable<LeagueInvite> invitedPlayers, bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager,
            Maybe<FantasyCriticUser> accessingUser, IEnumerable<ManagerMessage> managerMessages, Maybe<FantasyCriticUser> previousYearWinner,
            Maybe<IReadOnlyList<PublicBiddingMasterGame>> publicBiddingGames, IReadOnlySet<Guid> dropBlockedPublisherGameIDs)
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
            UserIsActive = activeUsers.Any(x => x.Id == accessingUser.GetValueOrDefault(y => y.Id));
            HasSpecialSlots = leagueYear.Options.HasSpecialSlots();
            Publishers = publishers
                .OrderBy(x => x.DraftPosition)
                .Select(x => new PublisherViewModel(x, currentDate, nextDraftPublisher, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished, dropBlockedPublisherGameIDs))
                .ToList();

            if (userPublisher.HasValue)
            {
                UserPublisher = new PublisherViewModel(userPublisher.Value, currentDate, userIsInLeague, userIsInvitedToLeague, systemWideValues, supportedYear.Finished, dropBlockedPublisherGameIDs);
            }

            List<PlayerWithPublisherViewModel> playerVMs = new List<PlayerWithPublisherViewModel>();
            bool allPublishersMade = true;
            foreach (var user in activeUsers)
            {
                var publisher = publishers.SingleOrDefault(x => x.User.Id == user.Id);
                if (publisher is null)
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, false));
                    allPublishersMade = false;
                }
                else
                {
                    bool isPreviousYearWinner = previousYearWinner.HasValue && previousYearWinner.Value.Id == user.Id;
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, currentDate, systemWideValues,
                        userIsInLeague, userIsInvitedToLeague, supportedYear, false, isPreviousYearWinner, dropBlockedPublisherGameIDs));
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

            PlayStatus = new PlayStatusViewModel(leagueYear.PlayStatus, readyToSetDraftOrder, startDraftResult.Ready, startDraftResult.Errors, draftPhase);
            EligibilityOverrides = leagueYear.EligibilityOverrides.Select(x => new EligibilityOverrideViewModel(x, currentDate)).ToList();
            TagOverrides = leagueYear.TagOverrides.Select(x => new TagOverrideViewModel(x, currentDate)).ToList();
            SlotInfo = new PublisherSlotRequirementsViewModel(leagueYear.Options);

            ManagerMessages = managerMessages.Select(x => new ManagerMessageViewModel(x, x.IsDismissed(accessingUser))).OrderBy(x => x.Timestamp).ToList();
            if (!userIsInLeague)
            {
                ManagerMessages = ManagerMessages.Where(x => x.IsPublic).ToList();
            }

            if (publicBiddingGames.HasValue)
            {
                PublicBiddingGames = publicBiddingGames.Value.Select(x => new PublicBiddingMasterGameViewModel(x, currentDate)).ToList();
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
        public bool HasSpecialSlots { get; }
        public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public IReadOnlyList<EligibilityOverrideViewModel> EligibilityOverrides { get; }
        public IReadOnlyList<TagOverrideViewModel> TagOverrides { get; }
        public PublisherSlotRequirementsViewModel SlotInfo { get; }
        public PublisherViewModel UserPublisher { get; }
        public PlayStatusViewModel PlayStatus { get; }
        public IReadOnlyList<ManagerMessageViewModel> ManagerMessages { get; }
        public IReadOnlyList<PublicBiddingMasterGameViewModel> PublicBiddingGames { get; }
    }
}
