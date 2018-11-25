using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, SupportedYear supportedYear, IEnumerable<Publisher> publishers, FantasyCriticUser currentUser, Publisher userPublisher,
            IClock clock, PlayStatus playStatus, StartDraftResult startDraftResult, IEnumerable<FantasyCriticUser> users, Maybe<Publisher> nextDraftPublisher, DraftPhase draftPhase,
            IEnumerable<PublisherGame> availableCounterPicks, LeagueWideValues leagueWideValues)
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
            Publishers = publishers.OrderBy(x => x.DraftPosition).Select(x => new PublisherViewModel(x, clock, nextDraftPublisher)).ToList();
            Standings = publishers.OrderByDescending(x => x.TotalFantasyPoints).Select(x => new StandingViewModel(x, leagueYear.Options.ScoringSystem, leagueWideValues)).ToList();

            if (!(userPublisher is null))
            {
                UserPublisher = new PublisherViewModel(userPublisher, clock);
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
                    playerVMs.Add(new PlayerWithPublisherViewModel(leagueYear, user, publisher, clock));
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
            AvailableCounterPicks = availableCounterPicks.Select(x => new PublisherGameViewModel(x, clock)).ToList();
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public SupportedYearViewModel SupportedYear { get; }
        public int StandardGames { get; }
        public int GamesToDraft { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public EligibilityLevelViewModel MaximumEligibilityLevel { get; }
        public string DraftSystem { get; }
        public string PickupSystem { get; }
        public string ScoringSystem { get; }
        public bool UnlinkedGameExists { get; }
        public IReadOnlyList<PlayerWithPublisherViewModel> Players { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public IReadOnlyList<StandingViewModel> Standings { get; }
        public PublisherViewModel UserPublisher { get; }
        public PlayStatusViewModel PlayStatus { get; }
        public IReadOnlyList<PublisherGameViewModel> AvailableCounterPicks { get; }
    }
}
