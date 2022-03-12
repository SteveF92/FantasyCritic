namespace FantasyCritic.MySQL.Entities.Trades;

public class TradeComponentEntity
{
    public TradeComponentEntity()
    {

    }

    public TradeComponentEntity(Guid tradeID, TradingParty party, MasterGameYearWithCounterPick masterGame)
    {
        TradeID = tradeID;
        CurrentParty = party.Value;
        MasterGameID = masterGame.MasterGameYear.MasterGame.MasterGameID;
        CounterPick = masterGame.CounterPick;
    }

    public Guid TradeID { get; set; }
    public string CurrentParty { get; set; }
    public Guid MasterGameID { get; set; }
    public bool CounterPick { get; set; }
}