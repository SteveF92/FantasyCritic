using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.Lib.Domain.Trades
{
    public record TradeVote(Guid TradeID, FantasyCriticUser User, bool Approve, Maybe<string> Comment, Instant Timestamp);
}
