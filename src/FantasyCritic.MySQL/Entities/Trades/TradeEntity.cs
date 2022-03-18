using FantasyCritic.Lib.Domain.Trades;

namespace FantasyCritic.MySQL.Entities.Trades;

public class TradeEntity
{
    public TradeEntity()
    {

    }

    public TradeEntity(Trade domain)
    {
        TradeID = domain.TradeID;
        LeagueID = domain.LeagueYear.League.LeagueID;
        Year = domain.LeagueYear.Year;
        ProposerPublisherID = domain.Proposer.PublisherID;
        CounterPartyPublisherID = domain.CounterParty.PublisherID;
        ProposerBudgetSendAmount = domain.ProposerBudgetSendAmount;
        CounterPartyBudgetSendAmount = domain.CounterPartyBudgetSendAmount;
        Message = domain.Message;
        ProposedTimestamp = domain.ProposedTimestamp;
        AcceptedTimestamp = domain.AcceptedTimestamp;
        CompletedTimestamp = domain.CompletedTimestamp;
        Status = domain.Status.Value;
    }

    public Guid TradeID { get; set; }
    public Guid LeagueID { get; set; }
    public int Year { get; set; }
    public Guid ProposerPublisherID { get; set; }
    public Guid CounterPartyPublisherID { get; set; }
    public uint ProposerBudgetSendAmount { get; set; }
    public uint CounterPartyBudgetSendAmount { get; set; }
    public string Message { get; set; }
    public Instant ProposedTimestamp { get; set; }
    public Instant? AcceptedTimestamp { get; set; }
    public Instant? CompletedTimestamp { get; set; }
    public string Status { get; set; }

    public Trade ToDomain(LeagueYear leagueYear, Publisher proposer, Publisher counterParty, IEnumerable<MasterGameYearWithCounterPick> proposerMasterGames,
        IEnumerable<MasterGameYearWithCounterPick> counterPartyMasterGames, IEnumerable<TradeVote> votes)
    {
        return new Trade(TradeID, leagueYear, proposer, counterParty, proposerMasterGames, counterPartyMasterGames,
            ProposerBudgetSendAmount, CounterPartyBudgetSendAmount, Message, ProposedTimestamp, AcceptedTimestamp, CompletedTimestamp,
            votes, TradeStatus.FromValue(Status));
    }
}
