using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGame
    {
        private readonly decimal? _criticScore;

        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore, int minimumReleaseYear)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            _criticScore = criticScore;
            MinimumReleaseYear = minimumReleaseYear;
            SubGames = new List<MasterSubGame>();
        }

        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore, int minimumReleaseYear, IReadOnlyList<MasterSubGame> subGames)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            _criticScore = criticScore;
            MinimumReleaseYear = minimumReleaseYear;
            SubGames = subGames;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public int? OpenCriticID { get; }

        public decimal? CriticScore
        {
            get
            {
                if (_criticScore.HasValue)
                {
                    return _criticScore;
                }

                if (!SubGames.Any(x => x.CriticScore.HasValue))
                {
                    return null;
                }

                decimal average = SubGames.Where(x => x.CriticScore.HasValue).Average(x => x.CriticScore.Value);
                return average;
            }
        }

        public int MinimumReleaseYear { get; }
        public IReadOnlyList<MasterSubGame> SubGames { get; }

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

        public override string ToString() => GameName;
    }
}
