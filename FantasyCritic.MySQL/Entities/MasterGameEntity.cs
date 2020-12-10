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
            EarlyAccessReleaseDate = masterGame.EarlyAccessReleaseDate?.ToDateTimeUnspecified();
            InternationalReleaseDate = masterGame.InternationalReleaseDate?.ToDateTimeUnspecified();
            ReleaseDate = masterGame.ReleaseDate?.ToDateTimeUnspecified();
            OpenCriticID = masterGame.OpenCriticID;
            CriticScore = masterGame.CriticScore;
            Notes = masterGame.Notes;
            BoxartFileName = masterGame.BoxartFileName;

            FirstCriticScoreTimestamp = masterGame.FirstCriticScoreTimestamp?.ToDateTimeUtc();
            DoNotRefreshDate = masterGame.DoNotRefreshDate;
            DoNotRefreshAnything = masterGame.DoNotRefreshAnything;
            EligibilityChanged = masterGame.EligibilityChanged;
        }

        public Guid MasterGameID { get; set; }
        public string GameName { get; set; }
        public string EstimatedReleaseDate { get; set; }
        public DateTime MinimumReleaseDate { get; set; }
        public DateTime? MaximumReleaseDate { get; set; }
        public DateTime? EarlyAccessReleaseDate { get; set; }
        public DateTime? InternationalReleaseDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? OpenCriticID { get; set; }
        public decimal? CriticScore { get; set; }
        public string Notes { get; set; }
        public string BoxartFileName { get; set; }
        public DateTime? FirstCriticScoreTimestamp { get; set; }
        public bool DoNotRefreshDate { get; set; }
        public bool DoNotRefreshAnything { get; set; }
        public bool EligibilityChanged { get; set; }
        public DateTime AddedTimestamp { get; set; }

        public MasterGame ToDomain(IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags)
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

            LocalDate? earlyAccessReleaseDate = null;
            if (EarlyAccessReleaseDate.HasValue)
            {
                earlyAccessReleaseDate = LocalDate.FromDateTime(EarlyAccessReleaseDate.Value);
            }

            LocalDate? internationalReleaseDate = null;
            if (InternationalReleaseDate.HasValue)
            {
                internationalReleaseDate = LocalDate.FromDateTime(InternationalReleaseDate.Value);
            }

            Instant? firstCriticScoreTimestamp = null;
            if (FirstCriticScoreTimestamp.HasValue)
            {
                firstCriticScoreTimestamp = Instant.FromDateTimeUtc(FirstCriticScoreTimestamp.Value);
            }

            var addedTimestamp = LocalDateTime.FromDateTime(AddedTimestamp).InZoneStrictly(DateTimeZone.Utc).ToInstant();

            return new MasterGame(MasterGameID, GameName, EstimatedReleaseDate, LocalDate.FromDateTime(MinimumReleaseDate), maximumReleaseDate, earlyAccessReleaseDate, internationalReleaseDate, releaseDate, OpenCriticID, CriticScore,
                Notes, BoxartFileName, firstCriticScoreTimestamp, DoNotRefreshDate, DoNotRefreshAnything, EligibilityChanged, addedTimestamp, subGames, tags);
        }
    }
}
