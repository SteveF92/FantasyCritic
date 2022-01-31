using FantasyCritic.Lib.Domain;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Web.Models.Responses
{
    public class PublicBiddingMasterGameViewModel
    {
        public PublicBiddingMasterGameViewModel(PublicBiddingMasterGame masterGame, LocalDate currentDate)
        {
            MasterGame = new MasterGameYearViewModel(masterGame.MasterGameYear, currentDate);
            CounterPick = masterGame.CounterPick;
        }

        public MasterGameYearViewModel MasterGame { get; }
        public bool CounterPick { get; }
    }
}
