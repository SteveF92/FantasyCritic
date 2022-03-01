using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Trades;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class TradeViewModel
    {
        public TradeViewModel(Trade domain, LocalDate currentDate, Maybe<Publisher> userPublisher)
        {
            TradeID = domain.TradeID;
            ProposerPublisherID = domain.Proposer.PublisherID;
            ProposerPublisherName = domain.Proposer.PublisherName;
            ProposerDisplayName = domain.Proposer.User.UserName;
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

            var alreadyVotedUsers = domain.TradeVotes.Select(x => x.User.Id).ToHashSet();
            if (userPublisher.HasValue)
            {
                var userIsProposer = domain.Proposer.User.Id == userPublisher.Value.User.Id;
                var userIsCounterParty = domain.CounterParty.User.Id == userPublisher.Value.User.Id;
                WaitingForUserResponse = domain.Status.Equals(TradeStatus.Proposed) && userIsCounterParty;
                UserCanVote = !userIsProposer && !userIsCounterParty && !alreadyVotedUsers.Contains(userPublisher.Value.User.Id);
            }

            Votes = domain.TradeVotes.Select(x => new TradeVoteViewModel(x)).ToList();
        }

        public Guid TradeID { get; }
        public Guid ProposerPublisherID { get; }
        public string ProposerPublisherName { get; }
        public string ProposerDisplayName { get; }
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
        public bool WaitingForUserResponse { get; }
        public bool UserCanVote { get; }
        public IReadOnlyList<TradeVoteViewModel> Votes { get; }
    }
}
