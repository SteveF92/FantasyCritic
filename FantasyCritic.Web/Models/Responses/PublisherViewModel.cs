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
        public PublisherViewModel(Publisher publisher, IClock clock, bool userIsInLeague,
            bool outstandingInvite, SystemWideValues systemWideValues)
        : this(publisher, clock, Maybe<Publisher>.None, userIsInLeague, outstandingInvite, systemWideValues)
        {

        }

        public PublisherViewModel(Publisher publisher, IClock clock, Maybe<Publisher> nextDraftPublisher,
            bool userIsInLeague, bool outstandingInvite, SystemWideValues systemWideValues)
        {
            PublisherID = publisher.PublisherID;
            LeagueID = publisher.LeagueYear.League.LeagueID;
            PublisherName = publisher.PublisherName;
            LeagueName = publisher.LeagueYear.League.LeagueName;
            PlayerName = publisher.User.DisplayName;
            Year = publisher.LeagueYear.Year;
            DraftPosition = publisher.DraftPosition;
            Games = publisher.PublisherGames
                .OrderBy(x => x.Timestamp)
                .Select(x => new PublisherGameViewModel(x, clock, publisher.LeagueYear.Options.ScoringSystem, systemWideValues))
                .ToList();

            AverageCriticScore = publisher.AverageCriticScore;
            TotalFantasyPoints = publisher.TotalFantasyPoints;
            Budget = publisher.Budget;

            if (nextDraftPublisher.HasValue && nextDraftPublisher.Value.PublisherID == publisher.PublisherID)
            {
                NextToDraft = true;
            }

            UserIsInLeague = userIsInLeague;
            PublicLeague = publisher.LeagueYear.Options.PublicLeague;
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
