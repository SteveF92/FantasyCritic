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
            MinimumReleaseDate = masterGame.MasterGame.MinimumReleaseDate;
            MaximumReleaseDate = masterGame.MasterGame.GetDefiniteMaximumReleaseDate();
            EarlyAccessReleaseDate = masterGame.MasterGame.EarlyAccessReleaseDate;
            InternationalReleaseDate = masterGame.MasterGame.InternationalReleaseDate;
            ReleaseDate = masterGame.MasterGame.ReleaseDate;
            IsReleased = masterGame.MasterGame.IsReleased(clock.GetCurrentInstant());
            WillRelease = masterGame.WillRelease();
            CriticScore = masterGame.MasterGame.CriticScore;
            AveragedScore = masterGame.MasterGame.AveragedScore;
            OpenCriticID = masterGame.MasterGame.OpenCriticID;
            SubGames = masterGame.MasterGame.SubGames.Select(x => new MasterGameYearViewModel(x, masterGame, clock)).ToList();
            Tags = masterGame.MasterGame.Tags.Select(x => x.Name).ToList();
            BoxartFileName = masterGame.MasterGame.BoxartFileName;
            PercentStandardGame = masterGame.PercentStandardGame;
            PercentCounterPick = masterGame.PercentCounterPick;
            AdjustedPercentCounterPick = masterGame.AdjustedPercentCounterPick;

            if (masterGame.MasterGame.EligibilityChanged)
            {
                EligiblePercentStandardGame = masterGame.PercentStandardGame;
            }
            else
            {
                EligiblePercentStandardGame = masterGame.EligiblePercentStandardGame;
            }

            AverageDraftPosition = masterGame.AverageDraftPosition;
            HypeFactor = masterGame.HypeFactor;
            DateAdjustedHypeFactor = masterGame.DateAdjustedHypeFactor;
            ProjectedFantasyPoints = masterGame.GetAlwaysProjectedFantasyPoints(ScoringSystem.GetDefaultScoringSystem(Year), false);
            ProjectedOrRealFantasyPoints = masterGame.GetProjectedOrRealFantasyPoints(ScoringSystem.GetDefaultScoringSystem(Year), false, clock);
            AddedTimestamp = masterGame.MasterGame.AddedTimestamp;
        }

        public MasterGameYearViewModel(MasterSubGame masterSubGame, MasterGameYear masterGame, IClock clock)
        {
            MasterGameID = masterSubGame.MasterGameID;
            Year = masterGame.Year;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            MinimumReleaseDate = masterSubGame.MinimumReleaseDate;
            MaximumReleaseDate = masterSubGame.GetDefiniteMaximumReleaseDate();
            ReleaseDate = masterGame.MasterGame.ReleaseDate;
            IsReleased = masterGame.MasterGame.IsReleased(clock.GetCurrentInstant());
            WillRelease = masterGame.WillRelease();
            CriticScore = masterSubGame.CriticScore;
            AveragedScore = false;
            OpenCriticID = masterSubGame.OpenCriticID;
            SubGames = null;

            PercentStandardGame = masterGame.PercentStandardGame;
            PercentCounterPick = masterGame.PercentCounterPick;
            AdjustedPercentCounterPick = masterGame.AdjustedPercentCounterPick;

            if (masterGame.MasterGame.EligibilityChanged)
            {
                EligiblePercentStandardGame = masterGame.PercentStandardGame;
            }
            else
            {
                EligiblePercentStandardGame = masterGame.EligiblePercentStandardGame;
            }

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
        public LocalDate MinimumReleaseDate { get; }
        public LocalDate MaximumReleaseDate { get; }
        public LocalDate? EarlyAccessReleaseDate { get; }
        public LocalDate? InternationalReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public bool IsReleased { get; }
        public bool WillRelease { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public int? OpenCriticID { get; }
        public IReadOnlyList<MasterGameYearViewModel> SubGames { get; }
        public IReadOnlyList<string> Tags { get; }
        public string BoxartFileName { get; }
        public Instant AddedTimestamp { get; }

        public double PercentStandardGame { get; }
        public double PercentCounterPick { get; }
        public double EligiblePercentStandardGame { get; }
        public double? AdjustedPercentCounterPick { get; }
        public double? AverageDraftPosition { get; }
        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public decimal ProjectedFantasyPoints { get; }
        public decimal ProjectedOrRealFantasyPoints { get; }
    }
}
