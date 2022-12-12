using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Royale;

public class RoyalePublisherGame : IEquatable<RoyalePublisherGame>
{
    public RoyalePublisherGame(Guid publisherID, RoyaleYearQuarter yearQuarter, MasterGameYear masterGame, Instant timestamp,
        decimal amountSpent, decimal advertisingMoney, decimal? fantasyPoints)
    {
        PublisherID = publisherID;
        YearQuarter = yearQuarter;
        MasterGame = masterGame;
        Timestamp = timestamp;
        AmountSpent = amountSpent;
        AdvertisingMoney = advertisingMoney;
        FantasyPoints = fantasyPoints;
    }

    public Guid PublisherID { get; }
    public RoyaleYearQuarter YearQuarter { get; }
    public MasterGameYear MasterGame { get; }
    public Instant Timestamp { get; }
    public decimal AmountSpent { get; }
    public decimal AdvertisingMoney { get; }
    public decimal? FantasyPoints { get; }

    public bool IsHidden(LocalDate currentDate)
    {
        if (!YearQuarter.HideUnreleasedGames)
        {
            return false;
        }

        if (YearQuarter.Finished)
        {
            return false;
        }

        if (MasterGame.MasterGame.CriticScore.HasValue)
        {
            return false;
        }

        if (MasterGame.MasterGame.IsReleased(currentDate))
        {
            return false;
        }

        if (!MasterGame.CouldReleaseInQuarter(YearQuarter.YearQuarter))
        {
            return false;
        }

        return true;
    }

    public bool CalculateIsCurrentlyIneligible(IEnumerable<MasterGameTag> allMasterGameTags)
    {
        var royaleTags = LeagueTagExtensions.GetRoyaleEligibilitySettings(allMasterGameTags);
        var customCodeTags = royaleTags.Where(x => x.Tag.HasCustomCode).ToList();
        var nonCustomCodeTags = royaleTags.Except(customCodeTags).ToList();

        var masterGame = MasterGame.MasterGame;
        var bannedTags = nonCustomCodeTags.Where(x => x.Status == TagStatus.Banned).Select(x => x.Tag);
        var requiredTags = nonCustomCodeTags.Where(x => x.Status == TagStatus.Required).Select(x => x.Tag);

        var bannedTagsIntersection = masterGame.Tags.Intersect(bannedTags);
        var missingRequiredTags = requiredTags.Except(masterGame.Tags);

        if (bannedTagsIntersection.Any() || missingRequiredTags.Any())
        {
            return true;
        }

        var masterGameCustomCodeTags = masterGame.Tags.Where(x => x.HasCustomCode).ToList();
        if (!masterGameCustomCodeTags.Any())
        {
            return false;
        }

        var dateGameWasAcquired = Timestamp.InZone(TimeExtensions.EasternTimeZone).Date;
        if (masterGame.EarlyAccessReleaseDate.HasValue)
        {
            var plannedForEarlyAccessTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "PlannedForEarlyAccess");
            if (plannedForEarlyAccessTag is not null)
            {
                if (plannedForEarlyAccessTag.Status == TagStatus.Banned)
                {
                    return true;
                }
            }

            var currentlyInEarlyAccessTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "CurrentlyInEarlyAccess");
            if (currentlyInEarlyAccessTag is not null)
            {
                if (currentlyInEarlyAccessTag.Status == TagStatus.Banned)
                {
                    var pickedUpBeforeInEarlyAccess = dateGameWasAcquired < masterGame.EarlyAccessReleaseDate.Value;
                    if (!pickedUpBeforeInEarlyAccess)
                    {
                        return true;
                    }
                }
            }
        }

        if (masterGame.InternationalReleaseDate.HasValue)
        {
            var willReleaseInternationallyFirstTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "WillReleaseInternationallyFirst");
            if (willReleaseInternationallyFirstTag is not null)
            {
                if (willReleaseInternationallyFirstTag.Status == TagStatus.Banned)
                {
                    return true;
                }
            }

            var releasedInternationallyTag = customCodeTags.SingleOrDefault(x => x.Tag.Name == "ReleasedInternationally");
            if (releasedInternationallyTag is not null)
            {
                if (releasedInternationallyTag.Status == TagStatus.Banned)
                {
                    var pickedUpBeforeReleasedInternationally = dateGameWasAcquired < masterGame.InternationalReleaseDate.Value;
                    if (!pickedUpBeforeReleasedInternationally)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public decimal? CalculateFantasyPoints(LocalDate currentDate, IEnumerable<MasterGameTag> allMasterGameTags)
    {
        if (!MasterGame.MasterGame.IsReleased(currentDate))
        {
            return 0m;
        }

        if (CalculateIsCurrentlyIneligible(allMasterGameTags))
        {
            return 0m;
        }

        var basePoints = MasterGame.GetFantasyPoints(ReleaseSystem.MustBeReleased, ScoringSystem.GetDefaultScoringSystem(YearQuarter.YearQuarter.Year), false, currentDate);
        if (!basePoints.HasValue)
        {
            return null;
        }

        var extraPoints = basePoints * AdvertisingMoney * 0.05m;
        var modifiedPoints = basePoints + extraPoints;

        return modifiedPoints;
    }

    public bool IsLocked(LocalDate currentDate, IEnumerable<MasterGameTag> allMasterGameTags)
    {
        if (CalculateIsCurrentlyIneligible(allMasterGameTags))
        {
            return false;
        }

        if (MasterGame.MasterGame.IsReleased(currentDate))
        {
            return true;
        }
        if (MasterGame.MasterGame.CriticScore.HasValue)
        {
            return true;
        }

        return false;
    }

    public override string ToString() => MasterGame.MasterGame.GameName;

    public bool Equals(RoyalePublisherGame? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return PublisherID.Equals(other.PublisherID) && MasterGame.Equals(other.MasterGame);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RoyalePublisherGame) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PublisherID, MasterGame);
    }
}
