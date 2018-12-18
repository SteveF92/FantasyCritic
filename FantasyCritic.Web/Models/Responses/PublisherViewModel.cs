using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublisherViewModel
    {
        public PublisherViewModel(Publisher publisher, IClock clock)
        : this(publisher, clock, Maybe<Publisher>.None)
        {

        }

        public PublisherViewModel(Publisher publisher, IClock clock, Maybe<Publisher> nextDraftPublisher)
        {
            PublisherID = publisher.PublisherID;
            LeagueID = publisher.League.LeagueID;
            PublisherName = publisher.PublisherName;
            LeagueName = publisher.League.LeagueName;
            PlayerName = publisher.User.DisplayName;
            Year = publisher.Year;
            DraftPosition = publisher.DraftPosition;
            Games = publisher.PublisherGames.OrderBy(x => x.Timestamp).Select(x => new PublisherGameViewModel(x, clock)).ToList();
            AverageCriticScore = publisher.AverageCriticScore;
            TotalFantasyPoints = publisher.TotalFantasyPoints;
            Budget = publisher.Budget;

            if (nextDraftPublisher.HasValue && nextDraftPublisher.Value.PublisherID == publisher.PublisherID)
            {
                NextToDraft = true;
            }
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
        public int Budget { get; }
        public bool NextToDraft { get; }
    }
}
