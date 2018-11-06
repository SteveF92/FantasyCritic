using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Results;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, SupportedYear supportedYear, IEnumerable<Publisher> publishers, FantasyCriticUser currentUser, IClock clock, PlayStatus playStatus, IEnumerable<FantasyCriticUser> users)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            SupportedYear = new SupportedYearViewModel(supportedYear);
            TotalGames = leagueYear.Options.TotalGames;
            DraftGames = leagueYear.Options.DraftGames;
            CounterPicks = leagueYear.Options.CounterPicks;
            EstimatedCriticScore = leagueYear.Options.EstimatedCriticScore;
            MaximumEligibilityLevel = new EligibilityLevelViewModel(leagueYear.Options.MaximumEligibilityLevel, false);
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            PickupSystem = leagueYear.Options.PickupSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
            UnlinkedGameExists = publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame.HasNoValue);
            Publishers = publishers.OrderBy(x => x.DraftPosition).Select(x => new PublisherViewModel(x, clock)).ToList();
            Standings = publishers.OrderByDescending(x => x.TotalFantasyPoints).Select(x => new StandingViewModel(x, leagueYear.Options.ScoringSystem, leagueYear.Options.EstimatedCriticScore)).ToList();

            var userPublisher = publishers.SingleOrDefault(x => x.User.UserID == currentUser.UserID);
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

            if (allPublishersMade)
            {
                Players = playerVMs.OrderBy(x => x.Publisher.DraftPosition).ToList();
                ReadyToSetDraftOrder = true;
            }
            else
            {
                Players = playerVMs;
            }

            PlayStatus = new PlayStatusViewModel(playStatus);
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public SupportedYearViewModel SupportedYear { get; }
        public int TotalGames { get; }
        public int DraftGames { get; }
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
        public bool ReadyToSetDraftOrder { get; }
    }
}
