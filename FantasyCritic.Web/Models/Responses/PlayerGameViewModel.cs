using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.Web.Models.Responses
{
    public class PlayerGameViewModel
    {
        public PlayerGameViewModel(PlayerGame playerGame)
        {
            Player = new FantasyCriticPlayerViewModel(playerGame.User);
            Year = playerGame.Year;
            GameName = playerGame.GameName;
            Timestamp = playerGame.Timestamp.ToDateTimeUtc();
            Waiver = playerGame.Waiver;
            AntiPick = playerGame.AntiPick;
            FantasyScore = playerGame.FantasyScore;

            if (playerGame.MasterGame.HasValue)
            {
                MasterGame = new MasterGameViewModel(playerGame.MasterGame.Value);
            }
        }

        public FantasyCriticPlayerViewModel Player { get; }
        public int Year { get; }
        public string GameName { get; }
        public DateTime Timestamp { get; }
        public bool Waiver { get; }
        public bool AntiPick { get; }
        public decimal? FantasyScore { get; }
        public MasterGameViewModel MasterGame { get; }
    }
}
