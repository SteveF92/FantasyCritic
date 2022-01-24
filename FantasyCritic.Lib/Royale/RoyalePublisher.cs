﻿using FantasyCritic.Lib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyCritic.Lib.Identity;
using NodaTime;
using CSharpFunctionalExtensions;

namespace FantasyCritic.Lib.Royale
{
    public class RoyalePublisher
    {
        public RoyalePublisher(Guid publisherID, RoyaleYearQuarter yearQuarter, FantasyCriticUser user, 
            string publisherName, Maybe<string> publisherIcon, IEnumerable<RoyalePublisherGame> publisherGames, decimal budget)
        {
            PublisherID = publisherID;
            YearQuarter = yearQuarter;
            User = user;
            PublisherName = publisherName;
            PublisherIcon = publisherIcon;
            PublisherGames = publisherGames.ToList();
            Budget = budget;
        }

        public Guid PublisherID { get; }
        public RoyaleYearQuarter YearQuarter { get; }
        public FantasyCriticUser User { get; }
        public string PublisherName { get; }
        public Maybe<string> PublisherIcon { get; }
        public IReadOnlyList<RoyalePublisherGame> PublisherGames { get; }
        public decimal Budget { get; }

        public decimal GetTotalFantasyPoints()
        {
            decimal? points = PublisherGames
                .Sum(x => x.FantasyPoints);
            return points ?? 0m;
        }
    }
}
