using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Web.Models.RoundTrip;
using NodaTime;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameYearViewModel
    {
        public MasterGameYearViewModel(MasterGameYear masterGame, IClock clock)
        {
            MasterGameID = masterGame.MasterGame.MasterGameID;
            Year = masterGame.Year;
            GameName = masterGame.MasterGame.GameName;
            EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
            SortableEstimatedReleaseDate = masterGame.MasterGame.GetDefiniteSortableEstimatedReleaseDate();
            ReleaseDate = masterGame.MasterGame.ReleaseDate;
            IsReleased = masterGame.MasterGame.IsReleased(clock.GetCurrentInstant());
            WillRelease = masterGame.WillRelease();
            CriticScore = masterGame.MasterGame.CriticScore;
            AveragedScore = masterGame.MasterGame.AveragedScore;
            EligibilitySettings = new EligibilitySettingsViewModel(masterGame.MasterGame.EligibilitySettings, false);
            OpenCriticID = masterGame.MasterGame.OpenCriticID;
            SubGames = masterGame.MasterGame.SubGames.Select(x => new MasterGameYearViewModel(x, masterGame, clock)).ToList();
            BoxartFileName = masterGame.MasterGame.BoxartFileName;
            PercentStandardGame = masterGame.PercentStandardGame;
            PercentCounterPick = masterGame.PercentCounterPick;
            EligiblePercentStandardGame = masterGame.EligiblePercentStandardGame;
            EligiblePercentCounterPick = masterGame.EligiblePercentCounterPick;
            AverageDraftPosition = masterGame.AverageDraftPosition;
            HypeFactor = masterGame.HypeFactor;
            DateAdjustedHypeFactor = masterGame.DateAdjustedHypeFactor;
            ProjectedFantasyPoints = masterGame.GetAlwaysProjectedFantasyPoints(new StandardScoringSystem(), false);
            ProjectedOrRealFantasyPoints = masterGame.GetProjectedOrRealFantasyPoints(new StandardScoringSystem(), false, clock);
            AddedTimestamp = masterGame.MasterGame.AddedTimestamp;
        }

        public MasterGameYearViewModel(MasterSubGame masterSubGame, MasterGameYear masterGame, IClock clock)
        {
            MasterGameID = masterSubGame.MasterGameID;
            Year = masterGame.Year;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            SortableEstimatedReleaseDate = masterSubGame.SortableEstimatedReleaseDate;
            ReleaseDate = masterSubGame.ReleaseDate;
            IsReleased = masterSubGame.IsReleased(clock.GetCurrentInstant());
            WillRelease = masterGame.WillRelease();
            CriticScore = masterSubGame.CriticScore;
            AveragedScore = false;
            EligibilitySettings = new EligibilitySettingsViewModel(masterGame.MasterGame.EligibilitySettings, false);
            OpenCriticID = masterSubGame.OpenCriticID;
            SubGames = null;

            PercentStandardGame = masterGame.PercentStandardGame;
            PercentCounterPick = masterGame.PercentCounterPick;
            EligiblePercentStandardGame = masterGame.EligiblePercentStandardGame;
            EligiblePercentCounterPick = masterGame.EligiblePercentCounterPick;
            AverageDraftPosition = masterGame.AverageDraftPosition;
            HypeFactor = masterGame.HypeFactor;
            DateAdjustedHypeFactor = masterGame.DateAdjustedHypeFactor;
            ProjectedFantasyPoints = masterGame.GetAlwaysProjectedFantasyPoints(new StandardScoringSystem(), false);
            ProjectedOrRealFantasyPoints = masterGame.GetProjectedOrRealFantasyPoints(new StandardScoringSystem(), false, clock);
            AddedTimestamp = masterGame.MasterGame.AddedTimestamp;
        }

        public Guid MasterGameID { get; }
        public int Year { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate SortableEstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public bool IsReleased { get; }
        public bool WillRelease { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public EligibilitySettingsViewModel EligibilitySettings { get; }
        public int? OpenCriticID { get; }
        public IReadOnlyList<MasterGameYearViewModel> SubGames { get; }
        public string BoxartFileName { get; }
        public Instant AddedTimestamp { get; }

        public double PercentStandardGame { get; }
        public double PercentCounterPick { get; }
        public double EligiblePercentStandardGame { get; }
        public double EligiblePercentCounterPick { get; }
        public double? AverageDraftPosition { get; }
        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public decimal ProjectedFantasyPoints { get; }
        public decimal ProjectedOrRealFantasyPoints { get; }
    }
}
