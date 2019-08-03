using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherViewModel
    {
        public PublisherViewModel(Publisher publisher, IClock clock, bool userIsInLeague, bool publicLeague,
            bool outstandingInvite, ScoringSystem scoringSystem, SystemWideValues systemWideValues)
        : this(publisher, clock, Maybe<Publisher>.None, userIsInLeague, publicLeague, outstandingInvite, scoringSystem, systemWideValues)
        {

        }

        public PublisherViewModel(Publisher publisher, IClock clock, Maybe<Publisher> nextDraftPublisher, bool userIsInLeague,
            bool publicLeague, bool outstandingInvite, ScoringSystem scoringSystem, SystemWideValues systemWideValues)
        {
            PublisherID = publisher.PublisherID;
            LeagueID = publisher.League.LeagueID;
            PublisherName = publisher.PublisherName;
            LeagueName = publisher.League.LeagueName;
            PlayerName = publisher.User.DisplayName;
            Year = publisher.Year;
            DraftPosition = publisher.DraftPosition;
            Games = publisher.PublisherGames
                .OrderBy(x => x.Timestamp)
                .Select(x => new PublisherGameViewModel(x, clock, scoringSystem, systemWideValues))
                .ToList();

            AverageCriticScore = publisher.AverageCriticScore;
            TotalFantasyPoints = publisher.TotalFantasyPoints;
            Budget = publisher.Budget;

            if (nextDraftPublisher.HasValue && nextDraftPublisher.Value.PublisherID == publisher.PublisherID)
            {
                NextToDraft = true;
            }

            UserIsInLeague = userIsInLeague;
            PublicLeague = publicLeague;
            OutstandingInvite = outstandingInvite;
        }

        public Guid PublisherID { get; }
        public Guid LeagueID { get; }
        public string PublisherName { get; }
        public string LeagueName { get; }
        public string PlayerName { get; }
        public int Year { get; }
        public int DraftPosition { get; }
        public IReadOnlyList<PublisherGameViewModel> Games { get; }
        public decimal? AverageCriticScore { get; }
        public decimal TotalFantasyPoints { get; }
        public uint Budget { get; }
        public bool NextToDraft { get; }
        public bool UserIsInLeague { get; }
        public bool PublicLeague { get; }
        public bool OutstandingInvite { get; }
    }
}
