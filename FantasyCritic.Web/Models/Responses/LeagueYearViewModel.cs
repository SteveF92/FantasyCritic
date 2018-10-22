using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class LeagueYearViewModel
    {
        public LeagueYearViewModel(LeagueYear leagueYear, SupportedYear supportedYear, IEnumerable<Publisher> publishers, FantasyCriticUser currentUser, IClock clock)
        {
            LeagueID = leagueYear.League.LeagueID;
            Year = leagueYear.Year;
            SupportedYear = new SupportedYearViewModel(supportedYear);
            DraftGames = leagueYear.Options.DraftGames;
            AcquisitionGames = leagueYear.Options.AcquisitionGames;
            CounterPicks = leagueYear.Options.CounterPicks;
            EstimatedCriticScore = leagueYear.Options.EstimatedCriticScore;
            MaximumEligibilityLevel = new EligibilityLevelViewModel(leagueYear.Options.MaximumEligibilityLevel, false);
            DraftSystem = leagueYear.Options.DraftSystem.Value;
            AcquisitionSystem = leagueYear.Options.AcquisitionSystem.Value;
            ScoringSystem = leagueYear.Options.ScoringSystem.Name;
            UnlinkedGameExists = publishers.SelectMany(x => x.PublisherGames).Any(x => x.MasterGame.HasNoValue);
            Publishers = publishers.OrderBy(x => x.DraftPosition).Select(x => new PublisherViewModel(x, clock)).ToList();
            Standings = publishers.OrderByDescending(x => x.TotalFantasyPoints).Select(x => new StandingViewModel(x, leagueYear.Options.ScoringSystem, leagueYear.Options.EstimatedCriticScore)).ToList();

            var userPublisher = publishers.SingleOrDefault(x => x.User.UserID == currentUser.UserID);
            if (!(userPublisher is null))
            {
                UserPublisher = new PublisherViewModel(userPublisher, clock);
            }
        }

        public Guid LeagueID { get; }
        public int Year { get; }
        public SupportedYearViewModel SupportedYear { get; }
        public int DraftGames { get; }
        public int AcquisitionGames { get; }
        public int CounterPicks { get; }
        public decimal EstimatedCriticScore { get; }
        public EligibilityLevelViewModel MaximumEligibilityLevel { get; }
        public string DraftSystem { get; }
        public string AcquisitionSystem { get; }
        public string ScoringSystem { get; }
        public bool UnlinkedGameExists { get; }
        public IReadOnlyList<PublisherViewModel> Publishers { get; }
        public IReadOnlyList<StandingViewModel> Standings { get; }
        public PublisherViewModel UserPublisher { get; }
    }
}
