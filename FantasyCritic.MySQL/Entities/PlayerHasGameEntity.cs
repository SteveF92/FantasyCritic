using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;

namespace FantasyCritic.MySQL.Entities
{
    internal class PlayerHasGameEntity
    {
        public PlayerHasGameEntity()
        {

        }

        public PlayerHasGameEntity(FantasyCriticLeague requestLeague, FantasyCriticUser requestUser, PlayerGame playerGame)
        {
            LeagueID = requestLeague.LeagueID;
            Year = playerGame.Year;
            UserID = requestUser.UserID;
            GameName = playerGame.GameName;
            Timestamp = playerGame.Timestamp.ToDateTimeOffset();
            Waiver = playerGame.Waiver;
            AntiPick = playerGame.AntiPick;
            FantasyScore = playerGame.FantasyScore;

            if (playerGame.MasterGame.HasValue)
            {
                MasterGameID = playerGame.MasterGame.Value.MasterGameID;
            }
        }

        public Guid LeagueID { get; set; }
        public int Year { get; set; }
        public Guid UserID { get; set; }
        public string GameName { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool Waiver { get; set; }
        public bool AntiPick { get; set; }
        public decimal? FantasyScore { get; set; }
        public Guid? MasterGameID { get; set; }
    }
}
