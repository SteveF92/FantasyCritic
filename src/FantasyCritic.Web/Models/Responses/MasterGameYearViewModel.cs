using FantasyCritic.Lib.Domain.ScoringSystems;

namespace FantasyCritic.Web.Models.Responses
{
    public class MasterGameYearViewModel
    {
        public MasterGameYearViewModel(MasterGameYear masterGame, LocalDate currentDate)
        {
            MasterGameID = masterGame.MasterGame.MasterGameID;
            Year = masterGame.Year;
            GameName = masterGame.MasterGame.GameName;
            EstimatedReleaseDate = masterGame.MasterGame.EstimatedReleaseDate;
            MinimumReleaseDate = masterGame.MasterGame.MinimumReleaseDate;
            MaximumReleaseDate = masterGame.MasterGame.GetDefiniteMaximumReleaseDate();
            EarlyAccessReleaseDate = masterGame.MasterGame.EarlyAccessReleaseDate;
            InternationalReleaseDate = masterGame.MasterGame.InternationalReleaseDate;
            AnnouncementDate = masterGame.MasterGame.AnnouncementDate;
            ReleaseDate = masterGame.MasterGame.ReleaseDate;
            IsReleased = masterGame.MasterGame.IsReleased(currentDate);
            WillRelease = masterGame.WillRelease();
            DelayContention = masterGame.MasterGame.DelayContention;
            CriticScore = masterGame.MasterGame.CriticScore;
            AveragedScore = masterGame.MasterGame.AveragedScore;
            OpenCriticID = masterGame.MasterGame.OpenCriticID;
            GGToken = masterGame.MasterGame.GGToken.GetValueOrDefault();
            SubGames = masterGame.MasterGame.SubGames.Select(x => new MasterGameYearViewModel(x, masterGame, currentDate)).ToList();
            Tags = masterGame.MasterGame.Tags.Select(x => x.Name).ToList();
            ReadableTags = masterGame.MasterGame.Tags.Select(x => x.ReadableName).ToList();
            BoxartFileName = masterGame.MasterGame.BoxartFileName;
            GGCoverArtFileName = masterGame.MasterGame.GGCoverArtFileName;
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
            PeakHypeFactor = masterGame.PeakHypeFactor;
            ProjectedFantasyPoints = masterGame.GetAlwaysProjectedFantasyPoints(ScoringSystem.GetDefaultScoringSystem(Year), false);
            ProjectedOrRealFantasyPoints = masterGame.GetProjectedOrRealFantasyPoints(ScoringSystem.GetDefaultScoringSystem(Year), false, currentDate);
            AddedTimestamp = masterGame.MasterGame.AddedTimestamp;
        }

        public MasterGameYearViewModel(MasterSubGame masterSubGame, MasterGameYear masterGame, LocalDate currentDate)
        {
            MasterGameID = masterSubGame.MasterGameID;
            Year = masterGame.Year;
            GameName = masterSubGame.GameName;
            EstimatedReleaseDate = masterSubGame.EstimatedReleaseDate;
            MinimumReleaseDate = masterSubGame.MinimumReleaseDate;
            MaximumReleaseDate = masterSubGame.GetDefiniteMaximumReleaseDate();
            ReleaseDate = masterGame.MasterGame.ReleaseDate;
            IsReleased = masterGame.MasterGame.IsReleased(currentDate);
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
            PeakHypeFactor = masterGame.PeakHypeFactor;
            ProjectedFantasyPoints = masterGame.GetAlwaysProjectedFantasyPoints(new StandardScoringSystem(), false);
            ProjectedOrRealFantasyPoints = masterGame.GetProjectedOrRealFantasyPoints(new StandardScoringSystem(), false, currentDate);
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
        public LocalDate? AnnouncementDate { get; }
        public LocalDate? ReleaseDate { get; }
        public bool IsReleased { get; }
        public bool WillRelease { get; }
        public bool DelayContention { get; }
        public decimal? CriticScore { get; }
        public bool AveragedScore { get; }
        public int? OpenCriticID { get; }
        public string GGToken { get; }
        public IReadOnlyList<MasterGameYearViewModel> SubGames { get; }
        public IReadOnlyList<string> Tags { get; }
        public IReadOnlyList<string> ReadableTags { get; }
        public string BoxartFileName { get; }
        public string GGCoverArtFileName { get; }
        public Instant AddedTimestamp { get; }

        public double PercentStandardGame { get; }
        public double PercentCounterPick { get; }
        public double EligiblePercentStandardGame { get; }
        public double? AdjustedPercentCounterPick { get; }
        public double? AverageDraftPosition { get; }
        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public double PeakHypeFactor { get; }
        public decimal ProjectedFantasyPoints { get; }
        public decimal ProjectedOrRealFantasyPoints { get; }
    }
}
