using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Royale;

namespace FantasyCritic.Lib.Domain;

public class MasterGame : IEquatable<MasterGame>
{
    public MasterGame(Guid masterGameID, string gameName, string estimatedReleaseDate, LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate,
        LocalDate? earlyAccessReleaseDate, LocalDate? internationalReleaseDate, LocalDate? announcementDate, LocalDate? releaseDate, int? openCriticID,
        string? ggToken, string? ggSlug, decimal? criticScore, bool hasAnyReviews, string? openCriticSlug,
        string? notes, string? boxartFileName, string? ggCoverArtFileName, Instant? firstCriticScoreTimestamp, bool doNotRefreshDate,
        bool doNotRefreshAnything, bool eligibilityChanged, bool delayContention, bool showNote, Instant addedTimestamp, VeryMinimalFantasyCriticUser addedByUser,
        IEnumerable<MasterSubGame> subGames, IEnumerable<MasterGameTag> tags)
    {
        MasterGameID = masterGameID;
        GameName = gameName;
        EstimatedReleaseDate = estimatedReleaseDate;
        MinimumReleaseDate = minimumReleaseDate;
        MaximumReleaseDate = maximumReleaseDate;
        EarlyAccessReleaseDate = earlyAccessReleaseDate;
        InternationalReleaseDate = internationalReleaseDate;
        AnnouncementDate = announcementDate;
        ReleaseDate = releaseDate;
        OpenCriticID = openCriticID;
        GGToken = ggToken;
        GGSlug = ggSlug;
        RawCriticScore = criticScore;
        HasAnyReviews = hasAnyReviews;
        OpenCriticSlug = openCriticSlug;
        Notes = notes;
        BoxartFileName = boxartFileName;
        GGCoverArtFileName = ggCoverArtFileName;
        FirstCriticScoreTimestamp = firstCriticScoreTimestamp;
        DoNotRefreshDate = doNotRefreshDate;
        DelayContention = delayContention;
        DoNotRefreshAnything = doNotRefreshAnything;
        UseSimpleEligibility = eligibilityChanged;
        ShowNote = showNote;
        AddedTimestamp = addedTimestamp;
        AddedByUser = addedByUser;
        SubGames = subGames.ToList();
        Tags = tags.ToList();
    }

    public Guid MasterGameID { get; }
    public string GameName { get; }
    public string EstimatedReleaseDate { get; }
    public LocalDate MinimumReleaseDate { get; }
    public LocalDate? MaximumReleaseDate { get; }
    public LocalDate? EarlyAccessReleaseDate { get; }
    public LocalDate? InternationalReleaseDate { get; }
    public LocalDate? AnnouncementDate { get; }
    public LocalDate? ReleaseDate { get; }
    public int? OpenCriticID { get; }
    public string? GGToken { get; }
    public string? GGSlug { get; }
    public string? Notes { get; }
    public string? BoxartFileName { get; }
    public string? GGCoverArtFileName { get; }
    public Instant? FirstCriticScoreTimestamp { get; }
    public bool DoNotRefreshDate { get; }
    public bool DelayContention { get; }
    public bool DoNotRefreshAnything { get; }
    public bool UseSimpleEligibility { get; }
    public bool ShowNote { get; }
    public Instant AddedTimestamp { get; }
    public VeryMinimalFantasyCriticUser AddedByUser { get; }
    public IReadOnlyList<MasterSubGame> SubGames { get; }
    public IReadOnlyList<MasterGameTag> Tags { get; }

    public LocalDate GetDefiniteMaximumReleaseDate() => MaximumReleaseDate ?? LocalDate.MaxIsoValue;

    public decimal? RawCriticScore { get; }
    public bool HasAnyReviews { get; }
    public string? OpenCriticSlug { get; }

    public decimal? CriticScore
    {
        get
        {
            if (RawCriticScore.HasValue)
            {
                return RawCriticScore;
            }

            if (!SubGames.Any(x => x.CriticScore.HasValue))
            {
                return null;
            }

            decimal average = SubGames.Where(x => x.CriticScore.HasValue).Average(x => x.CriticScore!.Value);
            return average;
        }
    }

    public bool AveragedScore
    {
        get
        {
            if (RawCriticScore.HasValue)
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

    public WillReleaseStatus GetWillReleaseStatus(int year)
    {
        if (year < MinimumReleaseDate.Year)
        {
            return WillReleaseStatus.WillNotRelease;
        }

        if (Tags.Any(x => x.Name == "Cancelled"))
        {
            return WillReleaseStatus.WillNotRelease;
        }

        if (MaximumReleaseDate.HasValue && MaximumReleaseDate.Value.Year <= year)
        {
            return WillReleaseStatus.WillRelease;
        }

        return WillReleaseStatus.MightRelease;
    }

    public bool WillReleaseInYear(int year)
    {
        return GetWillReleaseStatus(year).Equals(WillReleaseStatus.WillRelease);
    }

    public bool MightReleaseInYear(int year)
    {
        return GetWillReleaseStatus(year).Equals(WillReleaseStatus.MightRelease);
    }

    public bool WillOrMightReleaseInYear(int year)
    {
        return GetWillReleaseStatus(year).Equals(WillReleaseStatus.WillRelease) ||
               GetWillReleaseStatus(year).Equals(WillReleaseStatus.MightRelease);
    }

    public WillReleaseStatus GetWillReleaseStatus(YearQuarter yearQuarter)
    {
        if (ReleaseDate.HasValue && yearQuarter.FirstDateOfQuarter > ReleaseDate.Value)
        {
            return WillReleaseStatus.WillNotRelease;
        }

        if (yearQuarter.LastDateOfQuarter < MinimumReleaseDate)
        {
            return WillReleaseStatus.WillNotRelease;
        }

        if (MaximumReleaseDate.HasValue && MaximumReleaseDate.Value <= yearQuarter.LastDateOfQuarter)
        {
            return WillReleaseStatus.WillRelease;
        }

        return WillReleaseStatus.MightRelease;
    }

    public bool CouldReleaseInYear(int year) => GetWillReleaseStatus(year).CountAsWillRelease;
    public bool CouldReleaseInQuarter(YearQuarter yearQuarter) => GetWillReleaseStatus(yearQuarter).CountAsWillRelease;

    public string GetReleaseDateString()
    {
        if (ReleaseDate.HasValue)
        {
            return ReleaseDate.Value.ToString();
        }
        return EstimatedReleaseDate + " (Estimated)";
    }

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

    public bool Equals(MasterGame? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return MasterGameID.Equals(other.MasterGameID);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MasterGame)obj);
    }

    public override int GetHashCode()
    {
        return MasterGameID.GetHashCode();
    }

    public IReadOnlyList<string> CompareToExistingGame(MasterGame existingMasterGame, LocalDate today)
    {
        List<string> differences = new List<string>();

        if (GameName != existingMasterGame.GameName)
        {
            differences.Add($"Game name changed from {existingMasterGame.GameName} to {GameName}.");
        }

        if (ReleaseDate.HasValue && existingMasterGame.ReleaseDate.HasValue && ReleaseDate != existingMasterGame.ReleaseDate)
        {
            differences.Add($"Release date changed from {existingMasterGame.ReleaseDate.ToNullableLongDate("'")} to {ReleaseDate.ToNullableLongDate("'")}.");
        }
        else if (EstimatedReleaseDate != existingMasterGame.EstimatedReleaseDate ||
                 MinimumReleaseDate != existingMasterGame.MinimumReleaseDate ||
                 MaximumReleaseDate != existingMasterGame.MaximumReleaseDate)
        {
            if (ReleaseDate.HasValue && !existingMasterGame.ReleaseDate.HasValue)
            {
                differences.Add($"Release date changed from '{existingMasterGame.EstimatedReleaseDate}' to {ReleaseDate.ToNullableLongDate("'")}.");
            }
            else if (!ReleaseDate.HasValue && existingMasterGame.ReleaseDate.HasValue)
            {
                differences.Add($"Release date changed from {existingMasterGame.ReleaseDate.ToNullableLongDate("'")} to '{EstimatedReleaseDate}'.");
            }
            else
            {
                differences.Add($"Estimated release date changed from '{existingMasterGame.EstimatedReleaseDate}' to '{EstimatedReleaseDate}'.");
            }
        }
        
        if (EarlyAccessReleaseDate != existingMasterGame.EarlyAccessReleaseDate)
        {
            differences.Add($"Early access release date changed from {existingMasterGame.EarlyAccessReleaseDate.ToNullableLongDate("'")} to {EarlyAccessReleaseDate.ToNullableLongDate("'")}.");
        }

        if (InternationalReleaseDate != existingMasterGame.InternationalReleaseDate)
        {
            differences.Add($"International release date changed from {existingMasterGame.InternationalReleaseDate.ToNullableLongDate("'")} to {InternationalReleaseDate.ToNullableLongDate("'")}.");
        }

        if (AnnouncementDate != existingMasterGame.AnnouncementDate)
        {
            differences.Add($"Announcement date changed from {existingMasterGame.AnnouncementDate.ToNullableLongDate("'")} to {AnnouncementDate.ToNullableLongDate("'")}.");
        }

        if (string.IsNullOrWhiteSpace(Notes) && !string.IsNullOrWhiteSpace(existingMasterGame.Notes))
        {
            differences.Add($"Note removed: '{existingMasterGame.Notes}'.");
        }
        else if (!string.IsNullOrWhiteSpace(Notes) && string.IsNullOrWhiteSpace(existingMasterGame.Notes))
        {
            differences.Add($"Note added: '{Notes}'.");
        }
        else if (!string.IsNullOrWhiteSpace(Notes) && !string.IsNullOrWhiteSpace(existingMasterGame.Notes) &&
                 !Notes.Equals(existingMasterGame.Notes))
        {
            differences.Add($"Note removed: '{existingMasterGame.Notes}'.");
            differences.Add($"Note added: '{Notes}'.");
        }

        if (DelayContention && !existingMasterGame.DelayContention)
        {
            differences.Add("Game set to 'Delay in Contention'.");
        }
        else if (!DelayContention && existingMasterGame.DelayContention)
        {
            differences.Add("Game is no longer 'Delay in Contention'.");
        }

        var orderedExistingTags = existingMasterGame.Tags.OrderBy(t => t.Name).ToList();
        var orderedNewTags = Tags.OrderBy(t => t.Name).ToList();
        if (!orderedNewTags.SequenceEqual(orderedExistingTags))
        {
            differences.Add($"Tags changed from {string.Join(", ", orderedExistingTags.Select(x => x.ReadableName))} to {string.Join(", ", orderedNewTags.Select(x => x.ReadableName))}.");
        }

        return differences;
    }
}
