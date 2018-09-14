using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGame
    {
        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore, int minimumReleaseYear)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            CriticScore = criticScore;
            MinimumReleaseYear = minimumReleaseYear;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public int? OpenCriticID { get; }
        public decimal? CriticScore { get; }
        public int MinimumReleaseYear { get; }

        public bool IsReleased(IClock clock)
        {
            if (!ReleaseDate.HasValue)
            {
                return false;
            }

            Instant now = clock.GetCurrentInstant();
            LocalDate currentDate = now.InZone(DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York")).LocalDateTime.Date;
            if (currentDate >= ReleaseDate.Value)
            {
                return true;
            }

            return false;
        }
    }
}
