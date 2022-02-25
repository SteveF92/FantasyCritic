using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.MySQL.Entities.Trades
{
    public class TradeComponentEntity
    {
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
}
