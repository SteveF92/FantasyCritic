using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Enums;
using NodaTime;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain.Trades
{
    public class Trade
    {
        public Trade(Guid tradeID, Publisher proposer, Publisher counterParty, IEnumerable<MasterGameYearWithCounterPick> proposerMasterGames,
            IEnumerable<MasterGameYearWithCounterPick> counterPartyMasterGames, uint proposerBudgetSendAmount, uint counterPartyBudgetSendAmount,
            string message, Instant proposedTimestamp, Instant? acceptedTimestamp, Instant? completedTimestamp, TradeStatus status)
        {
            TradeID = tradeID;
            Proposer = proposer;
            CounterParty = counterParty;
            ProposerMasterGames = proposerMasterGames.ToList();
            CounterPartyMasterGames = counterPartyMasterGames.ToList();
            ProposerBudgetSendAmount = proposerBudgetSendAmount;
            CounterPartyBudgetSendAmount = counterPartyBudgetSendAmount;
            Message = message;
            ProposedTimestamp = proposedTimestamp;
            AcceptedTimestamp = acceptedTimestamp;
            CompletedTimestamp = completedTimestamp;
            Status = status;
        }

        public Guid TradeID { get; }
        public Publisher Proposer { get; }
        public Publisher CounterParty { get; }
        public IReadOnlyList<MasterGameYearWithCounterPick> ProposerMasterGames { get; }
        public IReadOnlyList<MasterGameYearWithCounterPick> CounterPartyMasterGames { get; }
        public uint ProposerBudgetSendAmount { get; }
        public uint CounterPartyBudgetSendAmount { get; }
        public string Message { get; }
        public Instant ProposedTimestamp { get; }
        public Instant? AcceptedTimestamp { get; }
        public Instant? CompletedTimestamp { get; }
        public TradeStatus Status { get; }

        public Maybe<string> GetTradeError()
        {
            if (Proposer.LeagueYear.Options.TradingSystem.Equals(TradingSystem.NoTrades))
            {
                return "Trades are not enabled for this league year.";
            }

            if (ProposerBudgetSendAmount > Proposer.Budget)
            {
                return $"{Proposer.PublisherName} does not have enough budget for this trade.";
            }

            if (CounterPartyBudgetSendAmount > CounterParty.Budget)
            {
                return $"{CounterParty.PublisherName} does not have enough budget for this trade.";
            }

            var proposerPublisherGamesWithMasterGames = Proposer.PublisherGames.Select(x => x.GetMasterGameYearWithCounterPick()).Where(x => x.HasValue).Select(x => x.Value).ToList();
            var counterPartyPublisherGamesWithMasterGames = CounterParty.PublisherGames.Select(x => x.GetMasterGameYearWithCounterPick()).Where(x => x.HasValue).Select(x => x.Value).ToList();

            bool proposerGamesValid = ListExtensions.SequencesContainSameElements(ProposerMasterGames, proposerPublisherGamesWithMasterGames);
            bool counterPartyGamesValid = ListExtensions.SequencesContainSameElements(CounterPartyMasterGames, counterPartyPublisherGamesWithMasterGames);

            if (!proposerGamesValid)
            {
                return $"{Proposer.PublisherName} no longer has all of the games involved in this trade.";
            }

            if (!counterPartyGamesValid)
            {
                return $"{CounterParty.PublisherName} no longer has all of the games involved in this trade.";
            }

            return Maybe<string>.None;
        }
    }
}
