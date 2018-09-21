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
        public MasterGameViewModel(IMasterGame masterGame)
        {
            MasterGameID = masterGame.MasterGameID;
            GameName = masterGame.GameName;
            EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
            ReleaseDate = masterGame.ReleaseDate;
            CriticScore = masterGame.CriticScore;

            if (masterGame is MasterGame rootMasterGame)
            {
                SubGames = rootMasterGame.SubGames.Select(x => new MasterGameViewModel(x)).ToList();
            }
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public decimal? CriticScore { get; }
        public IReadOnlyList<MasterGameViewModel> SubGames { get; }
    }
}
