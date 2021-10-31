﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    public class PickupBidEntity
    {
        public PickupBidEntity()
        {
            
        }

        public PickupBidEntity(PickupBid domain)
        {
            BidID = domain.BidID;
            PublisherID = domain.Publisher.PublisherID;
            MasterGameID = domain.MasterGame.MasterGameID;
            ConditionalDropPublisherGameID = domain.ConditionalDropPublisherGame.Unwrap(x => (Guid?) x.PublisherGameID);
            Timestamp = domain.Timestamp;
            Priority = domain.Priority;
            BidAmount = domain.BidAmount;
            Successful = domain.Successful;
        }

        public PickupBidEntity(PickupBid domain, bool successful)
        {
            BidID = domain.BidID;
            PublisherID = domain.Publisher.PublisherID;
            MasterGameID = domain.MasterGame.MasterGameID;
            ConditionalDropPublisherGameID = domain.ConditionalDropPublisherGame.Unwrap(x => (Guid?) x.PublisherGameID);
            Timestamp = domain.Timestamp;
            Priority = domain.Priority;
            BidAmount = domain.BidAmount;
            Successful = successful;
        }

        public Guid BidID { get; set; }
        public Guid PublisherID { get; set; }
        public Guid MasterGameID { get; set; }
        public Guid? ConditionalDropPublisherGameID { get; set; }
        public Instant Timestamp { get; set; }
        public int Priority { get; set; }
        public uint BidAmount { get; set; }
        public bool? Successful { get; set; }

        public PickupBid ToDomain(Publisher publisher, MasterGame masterGame, Maybe<PublisherGame> conditionalDropPublisherGame, LeagueYear leagueYear)
        {
            return new PickupBid(BidID, publisher, leagueYear, masterGame, conditionalDropPublisherGame, BidAmount, Priority, Timestamp, Successful);
        }
    }
}
