using FantasyCritic.Lib.Domain.Trades;

namespace FantasyCritic.Web.Models.Responses
{
    public class TradeViewModel
    {
        public TradeViewModel(Trade domain, LocalDate currentDate)
        {
            TradeID = domain.TradeID;
            ProposerUserID = domain.Proposer.User.Id;
            ProposerPublisherID = domain.Proposer.PublisherID;
            ProposerPublisherName = domain.Proposer.PublisherName;
            ProposerDisplayName = domain.Proposer.User.UserName;
            CounterPartyUserID = domain.CounterParty.User.Id;
            CounterPartyPublisherID = domain.CounterParty.PublisherID;
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
            Votes = domain.TradeVotes.Select(x => new TradeVoteViewModel(x)).ToList();
        }

        public Guid TradeID { get; }
        public Guid ProposerUserID { get; }
        public Guid ProposerPublisherID { get; }
        public string ProposerPublisherName { get; }
        public string ProposerDisplayName { get; }
        public Guid CounterPartyUserID { get; }
        public Guid CounterPartyPublisherID { get; }
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
        public IReadOnlyList<TradeVoteViewModel> Votes { get; }
    }
}
