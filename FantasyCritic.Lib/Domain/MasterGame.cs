using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Utilities;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class MasterGame : IEquatable<MasterGame>
    {
        private readonly decimal? _criticScore;

        public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate,
            LocalDate? earlyAccessReleaseDate, LocalDate? internationalReleaseDate,  LocalDate? releaseDate, int? openCriticID, decimal? criticScore, 
            string notes, string boxartFileName, Instant? firstCriticScoreTimestamp, bool doNotRefreshDate, 
            bool doNotRefreshAnything, bool eligibilityChanged, Instant addedTimestamp, IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags)
        {
            MasterGameID = masterGameID;
            GameName = gameName;
            EstimatedReleaseDate = estimatedReleaseDate;
            MinimumReleaseDate = minimumReleaseDate;
            MaximumReleaseDate = maximumReleaseDate;
            EarlyAccessReleaseDate = earlyAccessReleaseDate;
            InternationalReleaseDate = internationalReleaseDate;
            ReleaseDate = releaseDate;
            OpenCriticID = openCriticID;
            _criticScore = criticScore;
            Notes = notes;
            SubGames = subGames.ToList();
            BoxartFileName = boxartFileName;
            FirstCriticScoreTimestamp = firstCriticScoreTimestamp;
            DoNotRefreshDate = doNotRefreshDate;
            DoNotRefreshAnything = doNotRefreshAnything;
            EligibilityChanged = eligibilityChanged;
            AddedTimestamp = addedTimestamp;
            Tags = tags.ToList();
        }

        public Guid MasterGameID { get; }
        public string GameName { get; }
        public string EstimatedReleaseDate { get; }
        public LocalDate MinimumReleaseDate { get; }
        public LocalDate? MaximumReleaseDate { get; }
        public LocalDate? EarlyAccessReleaseDate { get; }
        public LocalDate? InternationalReleaseDate { get; }
        public LocalDate? ReleaseDate { get; }
        public int? OpenCriticID { get; }

        public string BoxartFileName { get; }
        public Instant? FirstCriticScoreTimestamp { get; }
        public bool DoNotRefreshDate { get; }
        public bool DoNotRefreshAnything { get; }
        public bool EligibilityChanged { get; }
        public Instant AddedTimestamp { get; }
        public IReadOnlyList<MasterGameTag> Tags { get; }

        public LocalDate GetDefiniteMaximumReleaseDate() => MaximumReleaseDate ?? LocalDate.MaxIsoValue;

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

        public string Notes { get; }
        public IReadOnlyList<MasterSubGame> SubGames { get; }

        public bool IsReleased(LocalDate currentDate)
        {
            if (SubGames.Any(x => x.IsReleased(currentDate)))
            {
                return true;
            }

            if (!ReleaseDate.HasValue)
            {
                return false;
            }

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
