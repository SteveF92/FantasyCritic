using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameViewModel
    {
        public MasterGameViewModel(MasterGame masterGame, IClock clock)
        {
            MasterGameID = masterGame.MasterGameID;
            GameName = masterGame.GameName;
            EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
            SortableEstimatedReleaseDate = masterGame.SortableEstimatedReleaseDate ?? LocalDate.MaxIsoValue;
            ReleaseDate = masterGame.ReleaseDate;
            IsReleased = masterGame.IsReleased(clock);
            CriticScore = masterGame.CriticScore;
            AveragedScore = masterGame.AveragedScore;
            EligibilitySettings = new EligibilitySettingsViewModel(masterGame.EligibilitySettings, false);
            OpenCriticID = masterGame.OpenCriticID;
            SubGames = masterGame.SubGames.Select(x => new MasterGameViewModel(x, masterGame.EligibilitySettings, clock)).ToList();
            BoxartFileName = masterGame.BoxartFileName;
            AddedTimestamp = masterGame.AddedTimestamp;
        }

        public MasterGameViewModel(MasterSubGame masterSubGame, EligibilitySettings eligibilitySettings, IClock clock)
        {
            MasterGameID = masterSubGame.MasterGameID;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            SortableEstimatedReleaseDate = masterSubGame.SortableEstimatedReleaseDate;
            ReleaseDate = masterSubGame.ReleaseDate;
            IsReleased = masterSubGame.IsReleased(clock);
            CriticScore = masterSubGame.CriticScore;
            AveragedScore = false;
            EligibilitySettings = new EligibilitySettingsViewModel(eligibilitySettings, false);
            OpenCriticID = masterSubGame.OpenCriticID;
            SubGames = null;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate SortableEstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public bool IsReleased { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public EligibilitySettingsViewModel EligibilitySettings { get; }
        public int? OpenCriticID { get; }
        public IReadOnlyList<MasterGameViewModel> SubGames { get; }
        public string BoxartFileName { get; }
        public Instant AddedTimestamp { get; }
    }
}
