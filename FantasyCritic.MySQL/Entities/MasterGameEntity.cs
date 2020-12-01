using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Domain;
using NodaTime;

namespace FantasyCritic.MySQL.Entities
{
    internal class MasterGameEntity
    {
        public MasterGameEntity()
        {

        }

        public MasterGameEntity(MasterGame masterGame)
        {
            MasterGameID = masterGame.MasterGameID;
            GameName = masterGame.GameName;
            EstimatedReleaseDate = masterGame.EstimatedReleaseDate;
            MinimumReleaseDate = masterGame.MinimumReleaseDate.ToDateTimeUnspecified();
            MaximumReleaseDate = masterGame.MaximumReleaseDate?.ToDateTimeUnspecified();
            InternationalReleaseDate = masterGame.InternationalReleaseDate?.ToDateTimeUnspecified();
            EarlyAccessReleaseDate = masterGame.EarlyAccessReleaseDate?.ToDateTimeUnspecified();
            ReleaseDate = masterGame.ReleaseDate?.ToDateTimeUnspecified();
            OpenCriticID = masterGame.OpenCriticID;
            CriticScore = masterGame.CriticScore;
            EligibilityLevel = masterGame.EligibilitySettings.EligibilityLevel.Level;
            YearlyInstallment = masterGame.EligibilitySettings.YearlyInstallment;
            EarlyAccess = masterGame.EligibilitySettings.EarlyAccess;
            FreeToPlay = masterGame.EligibilitySettings.FreeToPlay;
            ReleasedInternationally = masterGame.EligibilitySettings.ReleasedInternationally;
            ExpansionPack = masterGame.EligibilitySettings.ExpansionPack;
            UnannouncedGame = masterGame.EligibilitySettings.UnannouncedGame;
            Notes = masterGame.Notes;
            BoxartFileName = masterGame.BoxartFileName;
        }

        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public DateTime MinimumReleaseDate { get; set; }
        public DateTime? MaximumReleaseDate { get; set; }
        public DateTime? InternationalReleaseDate { get; set; }
        public DateTime? EarlyAccessReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public int EligibilityLevel { get; set; }
        public bool YearlyInstallment { get; set; }
        public bool EarlyAccess { get; set; }
        public bool FreeToPlay { get; set; }
        public bool ReleasedInternationally { get; set; }
        public bool ExpansionPack { get; set; }
        public bool UnannouncedGame { get; set; }
        public string Notes { get; set; }
        public string BoxartFileName { get; set; }
        public DateTime? FirstCriticScoreTimestamp { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public bool EligibilityChanged { get; set; }
        public DateTime AddedTimestamp { get; set; }

        public MasterGame ToDomain(IEnumerable<MasterSubGame> subGames, EligibilityLevel eligibilityLevel)
        {
            LocalDate? releaseDate = null;
            if (ReleaseDate.HasValue)
            {
                releaseDate = LocalDate.FromDateTime(ReleaseDate.Value);
            }

            LocalDate? maximumReleaseDate = null;
            if (MaximumReleaseDate.HasValue)
            {
                maximumReleaseDate = LocalDate.FromDateTime(MaximumReleaseDate.Value);
            }

            LocalDate? internationalReleaseDate = null;
            if (InternationalReleaseDate.HasValue)
            {
                internationalReleaseDate = LocalDate.FromDateTime(InternationalReleaseDate.Value);
            }

            LocalDate? earlyAccessReleaseDate = null;
            if (EarlyAccessReleaseDate.HasValue)
            {
                earlyAccessReleaseDate = LocalDate.FromDateTime(EarlyAccessReleaseDate.Value);
            }

            Instant? firstCriticScoreTimestamp = null;
            if (FirstCriticScoreTimestamp.HasValue)
            {
                firstCriticScoreTimestamp = Instant.FromDateTimeUtc(FirstCriticScoreTimestamp.Value);
            }

            var addedTimestamp = LocalDateTime.FromDateTime(AddedTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();
            var eligibilitySettings = new EligibilitySettings(eligibilityLevel, YearlyInstallment, EarlyAccess, FreeToPlay, ReleasedInternationally, ExpansionPack, UnannouncedGame);

            return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, LocalDate.FromDateTime(MinimumReleaseDate), maximumReleaseDate, internationalReleaseDate, earlyAccessReleaseDate, releaseDate, OpenCriticID, CriticScore,
                eligibilitySettings, Notes, BoxartFileName, firstCriticScoreTimestamp, DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, addedTimestamp);
        }
    }
}
