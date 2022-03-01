using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Enums;
using NodaTime;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Domain.Trades
{
    public class Trade
    {
        public Trade(Guid tradeID, Publisher proposer, Publisher counterParty, IEnumerable<MasterGameYearWithCounterPick> proposerMasterGames,
            IEnumerable<MasterGameYearWithCounterPick> counterPartyMasterGames, uint proposerBudgetSendAmount, uint counterPartyBudgetSendAmount,
            string message, Instant proposedTimestamp, Instant? acceptedTimestamp, Instant? completedTimestamp, 
            IEnumerable<TradeVote> tradeVotes, TradeStatus status)
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
            TradeVotes = tradeVotes.ToList();
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
        public IReadOnlyList<TradeVote> TradeVotes { get; }
        public TradeStatus Status { get; }

        public Maybe<string> GetTradeError()
        {
            if (Proposer.PublisherID == Guid.Empty || CounterParty.PublisherID == Guid.Empty)
            {
                return "One of the publishers involved in this trade no longer exists.";
            }

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

            bool proposerGamesValid = proposerPublisherGamesWithMasterGames.ContainsAllItems(ProposerMasterGames);
            bool counterPartyGamesValid = counterPartyPublisherGamesWithMasterGames.ContainsAllItems(CounterPartyMasterGames);
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

        public IReadOnlyList<Publisher> GetUpdatedPublishers()
        {
            Proposer.ObtainBudget(CounterPartyBudgetSendAmount);
            Proposer.SpendBudget(ProposerBudgetSendAmount);
            CounterParty.ObtainBudget(ProposerBudgetSendAmount);
            CounterParty.SpendBudget(CounterPartyBudgetSendAmount);
            return new List<Publisher>()
            {
                Proposer,
                CounterParty
            };
        }

        public IReadOnlyList<LeagueAction> GetActions(Instant actionTime)
        {
            var proposerAction = GetActionForPublisher(Proposer, actionTime, CounterPartyMasterGames, CounterPartyBudgetSendAmount);
            var counterPartyAction = GetActionForPublisher(CounterParty, actionTime, ProposerMasterGames, ProposerBudgetSendAmount);
            return new List<LeagueAction>()
            {
                proposerAction,
                counterPartyAction
            };
        }

        private static LeagueAction GetActionForPublisher(Publisher publisher, Instant actionTime, IEnumerable<MasterGameYearWithCounterPick> games, uint budgetSend)
        {
            List<string> acquisitions = new List<string>();
            foreach (var game in games)
            {
                var counterPickString = "";
                if (game.CounterPick)
                {
                    counterPickString = " (Counter Pick)";
                }
                acquisitions.Add($"Acquired {game.MasterGameYear.MasterGame.GameName}{counterPickString}.");
            }
            if (budgetSend > 0)
            {
                acquisitions.Add($"Acquired ${budgetSend} of budget.");
            }

            string finalString = string.Join("\n", acquisitions.Select(x => $"• {x}"));
            var proposerAction = new LeagueAction(publisher, actionTime, "Trade Executed", finalString, true);
            return proposerAction;
        }

        public IReadOnlyList<FormerPublisherGame> GetRemovedPublisherGames(Instant completionTime)
        {
            List<FormerPublisherGame> formerGames = new List<FormerPublisherGame>();
            foreach (var proposerGame in Proposer.PublisherGames)
            {
                var masterGameWithCounterPick = proposerGame.GetMasterGameYearWithCounterPick();
                if (masterGameWithCounterPick.HasNoValue)
                {
                    continue;
                }
                if (ProposerMasterGames.Contains(masterGameWithCounterPick.Value))
                {
                    formerGames.Add(proposerGame.GetFormerPublisherGame(completionTime, $"Traded to {CounterParty.PublisherName}"));
                }
            }
            foreach (var counterPartyGame in CounterParty.PublisherGames)
            {
                var masterGameWithCounterPick = counterPartyGame.GetMasterGameYearWithCounterPick();
                if (masterGameWithCounterPick.HasNoValue)
                {
                    continue;
                }
                if (ProposerMasterGames.Contains(masterGameWithCounterPick.Value))
                {
                    formerGames.Add(counterPartyGame.GetFormerPublisherGame(completionTime, $"Traded to {Proposer.PublisherName}"));
                }
            }

            return formerGames;
        }
    }
}
