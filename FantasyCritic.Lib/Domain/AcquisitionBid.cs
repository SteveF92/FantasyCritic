﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class AcquisitionBid
    {
        public AcquisitionBid(Guid bidID, Publisher publisher, MasterGame masterGame, uint bidAmount, int priority, Instant timestamp, bool? successful)
        {
            BidID = bidID;
            Publisher = publisher;
            MasterGame = masterGame;
            BidAmount = bidAmount;
            Priority = priority;
            Timestamp = timestamp;
            Successful = successful;
        }

        public Guid BidID { get; }
        public Publisher Publisher { get; }
        public MasterGame MasterGame { get; }
        public uint BidAmount { get; }
        public int Priority { get; }
        public Instant Timestamp { get; }
        public bool? Successful { get; }
    }
}
