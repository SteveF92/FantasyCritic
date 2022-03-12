using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasyCritic.Lib.Enums;
using NodaTime;

namespace FantasyCritic.Lib.Domain
{
    public class LeagueTagStatus : IEquatable<LeagueTagStatus>
    {
        public LeagueTagStatus(MasterGameTag tag, TagStatus status)
        {
            Tag = tag;
            Status = status;
        }

        public MasterGameTag Tag { get; }
        public TagStatus Status { get; }

        public bool GameMeetsTagCriteria(MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            if (!Tag.HasCustomCode)
            {
                var simpleTagIsPresent = masterGameTags.Any(x => x.Name == Tag.Name);
                if (Status.Equals(TagStatus.Banned))
                {
                    return !simpleTagIsPresent;
                }
                if (Status.Equals(TagStatus.Required))
                {
                    return simpleTagIsPresent;
                }
                throw new NotImplementedException($"Invalid tag status: {Status}");
            }

            if (Tag.Name == "UnannouncedGame")
            {
                return GameMeetsUnannouncedGameStatus(masterGame, masterGameTags, dateOfAcquisition);
            }

            if (Tag.Name == "PlannedForEarlyAccess")
            {
                return GameMeetsPlannedForEarlyAccessStatus(masterGame, masterGameTags, dateOfAcquisition);
            }

            if (Tag.Name == "CurrentlyInEarlyAccess")
            {
                return GameMeetsCurrentlyInEarlyAccessStatus(masterGame, masterGameTags, dateOfAcquisition);
            }

            if (Tag.Name == "WillReleaseInternationallyFirst")
            {
                return GameMeetsWillReleaseInternationallyFirstStatus(masterGame, masterGameTags, dateOfAcquisition);
            }

            if (Tag.Name == "ReleasedInternationally")
            {
                return GameMeetsReleasedInternationallyStatus(masterGame, masterGameTags, dateOfAcquisition);
            }

            throw new NotImplementedException($"Unknown custom code tag: {Tag.Name}");
        }

        private bool GameMeetsUnannouncedGameStatus(MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            bool hasTag = masterGameTags.Any(x => x.Name == "UnannouncedGame");
            if (Status.Equals(TagStatus.Banned))
            {
                return !hasTag;
            }
            else if (Status.Equals(TagStatus.Required))
            {
                var accquiredWhenGameWasUnannounced = masterGame.AnnouncementDate.HasValue && masterGame.AnnouncementDate > dateOfAcquisition;
                return hasTag || accquiredWhenGameWasUnannounced;
            }

            throw new NotImplementedException($"Invalid tag status: {Status}");
        }

        private bool GameMeetsPlannedForEarlyAccessStatus(MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            bool hasTag = masterGameTags.Any(x => x.Name == "PlannedForEarlyAccess");
            if (Status.Equals(TagStatus.Banned))
            {
                return !hasTag;
            }
            else if (Status.Equals(TagStatus.Required))
            {
                var accquiredWhenGameWasPlannedForEarlyAccess = masterGame.EarlyAccessReleaseDate.HasValue && masterGame.EarlyAccessReleaseDate > dateOfAcquisition;
                return hasTag || accquiredWhenGameWasPlannedForEarlyAccess;
            }

            throw new NotImplementedException($"Invalid tag status: {Status}");
        }

        private bool GameMeetsWillReleaseInternationallyFirstStatus(MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            bool hasTag = masterGameTags.Any(x => x.Name == "WillReleaseInternationallyFirst");
            if (Status.Equals(TagStatus.Banned))
            {
                return !hasTag;
            }
            else if (Status.Equals(TagStatus.Required))
            {
                var accquiredWhenGameWasWillReleaseInternationallyFirst = masterGame.InternationalReleaseDate.HasValue && masterGame.InternationalReleaseDate > dateOfAcquisition;
                return hasTag || accquiredWhenGameWasWillReleaseInternationallyFirst;
            }

            throw new NotImplementedException($"Invalid tag status: {Status}");
        }

        private bool GameMeetsCurrentlyInEarlyAccessStatus(MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            bool hasTag = masterGameTags.Any(x => x.Name == "CurrentlyInEarlyAccess");
            if (Status.Equals(TagStatus.Banned))
            {
                var accquiredWhenGameWasPlannedForEarlyAccess = masterGame.EarlyAccessReleaseDate.HasValue && masterGame.EarlyAccessReleaseDate > dateOfAcquisition;
                return !hasTag || accquiredWhenGameWasPlannedForEarlyAccess;
            }
            else if (Status.Equals(TagStatus.Required))
            {
                return hasTag;
            }

            throw new NotImplementedException($"Invalid tag status: {Status}");
        }

        private bool GameMeetsReleasedInternationallyStatus(MasterGame masterGame, IEnumerable<MasterGameTag> masterGameTags, LocalDate dateOfAcquisition)
        {
            bool hasTag = masterGameTags.Any(x => x.Name == "ReleasedInternationally");
            if (Status.Equals(TagStatus.Banned))
            {
                var accquiredWhenGameWasWillReleaseInternationallyFirst = masterGame.InternationalReleaseDate.HasValue && masterGame.InternationalReleaseDate > dateOfAcquisition;
                return !hasTag || accquiredWhenGameWasWillReleaseInternationallyFirst;
            }
            else if (Status.Equals(TagStatus.Required))
            {
                return hasTag;
            }

            throw new NotImplementedException($"Invalid tag status: {Status}");
        }

        public bool Equals(LeagueTagStatus other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Tag, other.Tag) && Equals(Status, other.Status);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LeagueTagStatus)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, Status);
        }
    }
}
