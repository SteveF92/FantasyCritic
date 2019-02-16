using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, SupportedYear supportedYear, IEnumerable<Publisher> publishers, Publisher userPublisher,
            IClock clock, PlayStatus playStatus, StartDraftResult startDraftResult, IEnumerable<FantasyCriticUser> users, Maybe<Publisher> nextDraftPublisher, DraftPhase draftPhase,
            IEnumerable<PublisherGame> availableCounterPicks, LeagueOptions options, SystemWideValues systemWideValues, IEnumerable<LeagueInvite> invitedPlayers,
            bool userIsInLeague, bool userIsInvitedToLeague, bool userIsManager)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            SupportedYear = new SupportedYearViewModel(supportedYear);
            StandardGames = leagueYear.Options.StandardGames;
            GamesToDraft = leagueYear.Options.GamesToDraft;
            CounterPicks = leagueYear.Options.CounterPicks;
            MaximumEligibilityLevel = new EligibilityLevelViewModel(leagueYear.Options.MaximumEligibilityLevel, false);
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            PickupSystem = leagueYear.Options.PickupSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
            UnlinkedGameExists = publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame.HasNoValue);
            Publishers = publishers.OrderBy(x => x.DraftPosition).Select(x =>
                new PublisherViewModel(x, clock, nextDraftPublisher, userIsInLeague, leagueYear.League.PublicLeague, userIsInvitedToLeague)).ToList();

            if (!(userPublisher is null))
            {
                UserPublisher = new PublisherViewModel(userPublisher, clock, userIsInLeague, leagueYear.League.PublicLeague, userIsInvitedToLeague);
            }

            List<PlayerWithPublisherViewModel> playerVMs = new List<PlayerWithPublisherViewModel>();
            bool allPublishersMade = true;
            foreach (var user in users)
            {
                var publisher = publishers.SingleOrDefault(x => x.User.UserID == user.UserID);
                if (publisher is null)
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user));
                    allPublishersMade = false;
                }
                else
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, clock, options, systemWideValues, userIsInLeague, userIsInvitedToLeague, supportedYear));
                }
            }

            foreach (var invitedPlayer in invitedPlayers)
            {
                allPublishersMade = false;

                if (invitedPlayer.User.HasValue)
                {
                    playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.User.Value.DisplayName));
                }
                else
                {
                    if (userIsManager)
                    {
                        playerVMs.Add(new PlayerWithPublisherViewModel(invitedPlayer.EmailAddress));
                    }
                    else
                    {
                        playerVMs.Add(new PlayerWithPublisherViewModel("<Email Address Hidden>"));
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
            AvailableCounterPicks = availableCounterPicks.Select(x => new PublisherGameViewModel(x, clock)).OrderBy(x => x.GameName).ToList();
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public SupportedYearViewModel SupportedYear { get; }
        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public EligibilityLevelViewModel MaximumEligibilityLevel { get; }
        public string DraftSystem { get; }
        public string PickupSystem { get; }
        public string ScoringSystem { get; }
        public bool UnlinkedGameExists { get; }
        public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public PublisherViewModel UserPublisher { get; }
        public PlayStatusViewModel PlayStatus { get; }
        public IReadOnlyList<PublisherGameViewModel> AvailableCounterPicks { get; }
    }
}
