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
            Player = playerGame.User.UserName;
            Year = playerGame.Year;
            GameName = playerGame.GameName;
            Timestamp = playerGame.Timestamp.ToDateTimeUtc();
            Waiver = playerGame.Waiver;
            AntiPick = playerGame.AntiPick;
            FantasyScore = playerGame.FantasyScore;

            if (playerGame.MasterGame.HasValue)
            {
                GameName = playerGame.MasterGame.Value.GameName;
                EstimatedReleaseDate = playerGame.MasterGame.Value.EstimatedReleaseDate;
                if (playerGame.MasterGame.Value.ReleaseDate.HasValue)
                {
                    ReleaseDate = playerGame.MasterGame.Value.ReleaseDate.Value.ToDateTimeUnspecified();
                }
                CriticScore = playerGame.MasterGame.Value.CriticScore;
            }
        }

        public string Player { get; }
        public int Year { get; }
        public string GameName { get; }
        public DateTime Timestamp { get; }
        public bool Waiver { get; }
        public bool AntiPick { get; }
        public string EstimatedReleaseDate { get; }
        public DateTime? ReleaseDate { get; }
        public decimal? FantasyScore { get; }
        public decimal? CriticScore { get; }
    }
}
