using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class TradeViewModel
    {
        public Guid TradeID { get; }
        public string ProposerPublisherName { get; }
        public string CounterPartyPublisherName { get; }
        public IReadOnlyList<PublisherGameViewModel> ProposerSendGames { get; }
        public IReadOnlyList<PublisherGameViewModel> CounterPartySendGames { get; }
        public uint ProposerBudgetSendAmount { get; }
        public uint CounterPartyBudgetSendAmount { get; }
        public string Message { get; }
        public Instant ProposedTimestamp { get; }
        public Instant? CompletedTimestamp { get; }
        public string Status { get; }
    }
}
