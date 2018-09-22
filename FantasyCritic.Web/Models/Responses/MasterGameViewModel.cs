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
            AveragedScore = masterGame.AveragedScore;
            EligibilityLevel = masterGame.EligibilityLevel;
            OpenCriticID = masterGame.OpenCriticID;
            SubGames = masterGame.SubGames.Select(x => new MasterGameViewModel(x, EligibilityLevel)).ToList();
        }

        public MasterGameViewModel(MasterSubGame masterSubGame, int eligibilityLevel)
        {
            MasterGameID = masterSubGame.MasterGameID;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            ReleaseDate = masterSubGame.ReleaseDate;
            CriticScore = masterSubGame.CriticScore;
            AveragedScore = false;
            EligibilityLevel = eligibilityLevel;
            OpenCriticID = masterSubGame.OpenCriticID;
            SubGames = null;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public int EligibilityLevel { get; }
        public int? OpenCriticID { get; }
        public IReadOnlyList<MasterGameViewModel> SubGames { get; }
    }
}
