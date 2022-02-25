using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain.Trades;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class TradeViewModel
    {
        public TradeViewModel(Trade domain, LocalDate currentDate)
        {
            TradeID = domain.TradeID;
            ProposerPublisherName = domain.Proposer.PublisherName;
            ProposerDisplayName = domain.Proposer.User.UserName;
            CounterPartyPublisherName = domain.CounterParty.PublisherName;
            CounterPartyDisplayName = domain.CounterParty.User.UserName;

            ProposerSendGames = domain.ProposerMasterGames.Select(x => new MasterGameYearWithCounterPickViewModel(x.MasterGameYear, x.CounterPick, currentDate)).ToList();
            CounterPartySendGames = domain.CounterPartyMasterGames.Select(x => new MasterGameYearWithCounterPickViewModel(x.MasterGameYear, x.CounterPick, currentDate)).ToList();

            ProposerBudgetSendAmount = domain.ProposerBudgetSendAmount;
            CounterPartyBudgetSendAmount = domain.CounterPartyBudgetSendAmount;
            Message = domain.Message;
            ProposedTimestamp = domain.ProposedTimestamp;
            AcceptedTimestamp = domain.AcceptedTimestamp;
            CompletedTimestamp = domain.CompletedTimestamp;
            Status = domain.Status.Value;
            Error = domain.GetTradeError().GetValueOrDefault();
        }

        public Guid TradeID { get; }
        public string ProposerPublisherName { get; }
        public string ProposerDisplayName { get; }
        public string CounterPartyPublisherName { get; }
        public string CounterPartyDisplayName { get; }
        public IReadOnlyList<MasterGameYearWithCounterPickViewModel> ProposerSendGames { get; }
        public IReadOnlyList<MasterGameYearWithCounterPickViewModel> CounterPartySendGames { get; }
        public uint ProposerBudgetSendAmount { get; }
        public uint CounterPartyBudgetSendAmount { get; }
        public string Message { get; }
        public Instant ProposedTimestamp { get; }
        public Instant? AcceptedTimestamp { get; }
        public Instant? CompletedTimestamp { get; }
        public string Status { get; }
        public string Error { get; }
    }
}
