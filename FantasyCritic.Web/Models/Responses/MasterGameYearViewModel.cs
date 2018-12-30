using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameYearViewModel
    {
        public MasterGameYearViewModel(MasterGameYear masterGame, IClock clock)
        {
            MasterGameID = masterGame.MasterGame.MasterGameID;
            GameName = masterGame.MasterGame.GameName;
            EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
            ReleaseDate = masterGame.MasterGame.ReleaseDate;
            IsReleased = masterGame.MasterGame.IsReleased(clock);
            CriticScore = masterGame.MasterGame.CriticScore;
            AveragedScore = masterGame.MasterGame.AveragedScore;
            EligibilityLevel = new EligibilityLevelViewModel(masterGame.MasterGame.EligibilityLevel, false);
            OpenCriticID = masterGame.MasterGame.OpenCriticID;
            SubGames = masterGame.MasterGame.SubGames.Select(x => new MasterGameYearViewModel(x, masterGame, clock)).ToList();
            BoxartFileName = masterGame.MasterGame.BoxartFileName;
            PercentStandardGame = masterGame.PercentStandardGame;
            PercentCounterPick = masterGame.PercentCounterPick;
            AverageDraftPosition = masterGame.AverageDraftPosition;
        }

        public MasterGameYearViewModel(MasterSubGame masterSubGame, MasterGameYear masterGame, IClock clock)
        {
            MasterGameID = masterSubGame.MasterGameID;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            ReleaseDate = masterSubGame.ReleaseDate;
            IsReleased = masterSubGame.IsReleased(clock);
            CriticScore = masterSubGame.CriticScore;
            AveragedScore = false;
            EligibilityLevel = new EligibilityLevelViewModel(masterGame.MasterGame.EligibilityLevel, false);
            OpenCriticID = masterSubGame.OpenCriticID;
            SubGames = null;

            PercentStandardGame = masterGame.PercentStandardGame;
            PercentCounterPick = masterGame.PercentCounterPick;
            AverageDraftPosition = masterGame.AverageDraftPosition;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public bool IsReleased { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public EligibilityLevelViewModel EligibilityLevel { get; }
        public int? OpenCriticID { get; }
        public IReadOnlyList<MasterGameYearViewModel> SubGames { get; }
        public string BoxartFileName { get; }

        public decimal PercentStandardGame { get; }
        public decimal PercentCounterPick { get; }
        public decimal? AverageDraftPosition { get; }
    }
}
