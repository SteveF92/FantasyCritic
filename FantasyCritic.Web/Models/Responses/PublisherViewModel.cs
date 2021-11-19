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
        public PublisherViewModel(Publisher publisher, LocalDate currentDate, bool userIsInLeague,
            bool outstandingInvite, SystemWideValues systemWideValues, bool yearFinished)
        : this(publisher, currentDate, Maybe<Publisher>.None, userIsInLeague, outstandingInvite, systemWideValues, yearFinished)
        {

        }

        public PublisherViewModel(Publisher publisher, LocalDate currentDate, Maybe<Publisher> nextDraftPublisher,
            bool userIsInLeague, bool outstandingInvite, SystemWideValues systemWideValues, bool yearFinished)
        {
            PublisherID = publisher.PublisherID;
            LeagueID = publisher.LeagueYear.League.LeagueID;
            PublisherName = publisher.PublisherName;
            LeagueName = publisher.LeagueYear.League.LeagueName;
            PlayerName = publisher.User.UserName;
            Year = publisher.LeagueYear.Year;
            DraftPosition = publisher.DraftPosition;
            AutoDraft = publisher.AutoDraft;
            Games = publisher.PublisherGames
                .OrderBy(x => x.Timestamp)
                .Select(x => new PublisherGameViewModel(x, currentDate, publisher.LeagueYear.Options.ScoringSystem, systemWideValues))
                .ToList();

            AverageCriticScore = publisher.AverageCriticScore;
            TotalFantasyPoints = publisher.TotalFantasyPoints;
            TotalProjectedPoints = publisher.GetProjectedFantasyPoints(publisher.LeagueYear.Options, systemWideValues, false, currentDate);
            Budget = publisher.Budget;

            if (nextDraftPublisher.HasValue && nextDraftPublisher.Value.PublisherID == publisher.PublisherID)
            {
                NextToDraft = true;
            }

            UserIsInLeague = userIsInLeague;
            PublicLeague = publisher.LeagueYear.Options.PublicLeague;
            OutstandingInvite = outstandingInvite;

            var dateToCheck = currentDate;
            if (yearFinished)
            {
                dateToCheck = new LocalDate(Year, 12, 31);
            }

            GamesReleased = publisher.PublisherGames
                .Where(x => !x.CounterPick)
                .Where(x => x.MasterGame.HasValue)
                .Count(x => x.MasterGame.Value.MasterGame.IsReleased(dateToCheck));
            var allWillRelease = publisher.PublisherGames
                .Where(x => !x.CounterPick)
                .Where(x => x.MasterGame.HasValue)
                .Count(x => x.WillRelease());
            GamesWillRelease = allWillRelease - GamesReleased;

            FreeGamesDropped = publisher.FreeGamesDropped;
            WillNotReleaseGamesDropped = publisher.WillNotReleaseGamesDropped;
            WillReleaseGamesDropped = publisher.WillReleaseGamesDropped;
            FreeDroppableGames = publisher.LeagueYear.Options.FreeDroppableGames;
            WillNotReleaseDroppableGames = publisher.LeagueYear.Options.WillNotReleaseDroppableGames;
            WillReleaseDroppableGames = publisher.LeagueYear.Options.WillReleaseDroppableGames;
        }

        public Guid PublisherID { get; }
        public Guid LeagueID { get; }
        public string PublisherName { get; }
        public string LeagueName { get; }
        public string PlayerName { get; }
        public int Year { get; }
        public int DraftPosition { get; }
        public bool AutoDraft { get; }
        public IReadOnlyList<PublisherGameViewModel> Games { get; }
        public decimal? AverageCriticScore { get; }
        public decimal TotalFantasyPoints { get; }
        public decimal TotalProjectedPoints { get; }
        public uint Budget { get; }
        public bool NextToDraft { get; }
        public bool UserIsInLeague { get; }
        public bool PublicLeague { get; }
        public bool OutstandingInvite { get; }

        public int GamesReleased { get; }
        public int GamesWillRelease { get; }
        public int FreeGamesDropped { get; }
        public int WillNotReleaseGamesDropped { get; }
        public int WillReleaseGamesDropped { get; }
        public int FreeDroppableGames { get; }
        public int WillNotReleaseDroppableGames { get; }
        public int WillReleaseDroppableGames { get; }
    }
}
