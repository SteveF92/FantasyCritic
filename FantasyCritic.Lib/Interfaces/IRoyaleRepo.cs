﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Interfaces
{
    public interface IRoyaleRepo
    {
        Task CreatePublisher(RoyalePublisher publisher);
        Task<Maybe<RoyalePublisher>> GetPublisher(RoyaleYearQuarter yearQuarter, FantasyCriticUser user);
        Task<IReadOnlyList<RoyaleYearQuarter>> GetYearQuarters();
        Task<Maybe<RoyalePublisher>> GetPublisher(Guid publisherID);
        Task PurchaseGame(RoyalePublisherGame game);
        Task SellGame(RoyalePublisherGame publisherGame);
        Task SetAdvertisingMoney(RoyalePublisherGame publisherGame, decimal advertisingMoney);
    }
}
