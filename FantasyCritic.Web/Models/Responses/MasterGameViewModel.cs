using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameViewModel
    {
        public MasterGameViewModel(MasterGame masterGame)
        {
            MasterGameID = masterGame.MasterGameID;
            GameName = masterGame.GameName;
            EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
            ReleaseDate = masterGame.ReleaseDate;
            CriticScore = masterGame.CriticScore;
            SubGames = masterGame.SubGames.Select(x => new MasterGameViewModel(x)).ToList();
        }

        public MasterGameViewModel(MasterSubGame masterSubGame)
        {
            MasterGameID = masterSubGame.MasterGameID;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            ReleaseDate = masterSubGame.ReleaseDate;
            CriticScore = masterSubGame.CriticScore;
            SubGames = null;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public decimal? CriticScore { get; }
        public IReadOnlyList<MasterGameViewModel> SubGames { get; }
    }
}
