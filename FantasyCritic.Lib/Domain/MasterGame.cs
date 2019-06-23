using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGame : IEquatable<MasterGame>
    {
        private readonly decimal? _criticScore;

        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore, 
            int minimumReleaseYear, EligibilitySettings eligibilitySettings, string boxartFileName, Instant? firstCriticScoreTimestamp,
            bool doNotRefreshDate, bool doNotRefreshAnything, Instant addedTimestamp)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            _criticScore = criticScore;
            MinimumReleaseYear = minimumReleaseYear;
            EligibilitySettings = eligibilitySettings;
            SubGames = new List<MasterSubGame>();
            BoxartFileName = boxartFileName;
            FirstCriticScoreTimestamp = firstCriticScoreTimestamp;
            DoNotRefreshDate = doNotRefreshDate;
            DoNotRefreshAnything = doNotRefreshAnything;
            AddedTimestamp = addedTimestamp;
        }

        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate? releaseDate, int? openCriticID, decimal? criticScore, 
            int minimumReleaseYear, EligibilitySettings eligibilitySettings, IReadOnlyList<MasterSubGame> subGames, string boxartFileName, 
            Instant? firstCriticScoreTimestamp, bool doNotRefreshDate, bool doNotRefreshAnything, Instant addedTimestamp)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            _criticScore = criticScore;
            MinimumReleaseYear = minimumReleaseYear;
            EligibilitySettings = eligibilitySettings;
            SubGames = subGames;
            BoxartFileName = boxartFileName;
            FirstCriticScoreTimestamp = firstCriticScoreTimestamp;
            DoNotRefreshDate = doNotRefreshDate;
            DoNotRefreshAnything = doNotRefreshAnything;
            AddedTimestamp = addedTimestamp;
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public int? OpenCriticID { get; }

        public string BoxartFileName { get; }
        public Instant? FirstCriticScoreTimestamp { get; }
        public bool DoNotRefreshDate { get; }
        public bool DoNotRefreshAnything { get; }
        public Instant AddedTimestamp { get; }

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

        public bool AveragedScore
        {
            get
            {
                if (_criticScore.HasValue)
                {
                    return false;
                }

                if (!SubGames.Any(x => x.CriticScore.HasValue))
                {
                    return false;
                }

                return true;
            }
        }

        public int MinimumReleaseYear { get; }
        public EligibilitySettings EligibilitySettings { get; }
        public IReadOnlyList<MasterSubGame> SubGames { get; }

        public bool IsReleased(IClock clock)
        {
            if (SubGames.Any(x => x.IsReleased(clock)))
            {
                return true;
            }

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

        public bool Equals(MasterGame other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return MasterGameID.Equals(other.MasterGameID);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MasterGame) obj);
        }

        public override int GetHashCode()
        {
            return MasterGameID.GetHashCode();
        }
    }
}
