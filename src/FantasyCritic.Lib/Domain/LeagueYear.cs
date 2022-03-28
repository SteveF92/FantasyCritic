using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Domain;

public class LeagueYear : IEquatable<LeagueYear>
{
    private readonly IReadOnlyDictionary<MasterGame, EligibilityOverride> _eligibilityOverridesDictionary;
    private readonly IReadOnlyDictionary<MasterGame, TagOverride> _tagOverridesDictionary;
    private readonly IReadOnlyDictionary<Guid, Publisher> _publisherDictionary;

    private readonly Maybe<Publisher> _managerPublisher;

    public LeagueYear(League league, SupportedYear year, LeagueOptions options, PlayStatus playStatus,
        IEnumerable<EligibilityOverride> eligibilityOverrides, IEnumerable<TagOverride> tagOverrides,
        Instant? draftStartedTimestamp, Maybe<FantasyCriticUser> winningUser, IEnumerable<Publisher> publishers)
    {
        League = league;
        SupportedYear = year;
        Options = options;
        PlayStatus = playStatus;
        EligibilityOverrides = eligibilityOverrides.ToList();
        _eligibilityOverridesDictionary = EligibilityOverrides.ToDictionary(x => x.MasterGame);
        TagOverrides = tagOverrides.ToList();
        _tagOverridesDictionary = TagOverrides.ToDictionary(x => x.MasterGame);
        DraftStartedTimestamp = draftStartedTimestamp;
        WinningUser = winningUser;

        _publisherDictionary = publishers.ToDictionary(x => x.PublisherID);
        _managerPublisher = Maybe.From(Publishers.SingleOrDefault(x => x.User.Id == league.LeagueManager.Id));
        StandardGamesTaken = _publisherDictionary.Values.SelectMany(x => x.PublisherGames).Count(x => !x.CounterPick);
    }

    public League League { get; }
    public SupportedYear SupportedYear { get; }
    public int Year => SupportedYear.Year;
    public LeagueOptions Options { get; }
    public PlayStatus PlayStatus { get; }
    public IReadOnlyList<EligibilityOverride> EligibilityOverrides { get; }
    public IReadOnlyList<TagOverride> TagOverrides { get; }
    public Instant? DraftStartedTimestamp { get; }
    public Maybe<FantasyCriticUser> WinningUser { get; }
    public IReadOnlyList<Publisher> Publishers => _publisherDictionary.Values.ToList();
    public int StandardGamesTaken { get; }
    public int TotalNumberOfStandardGames => Options.StandardGames * Publishers.Count;

    public LeagueYearKey Key => new LeagueYearKey(League.LeagueID, Year);

    public string GetGroupName => $"{League.LeagueID}|{Year}";

    public MasterGameWithEligibilityFactors GetEligibilityFactorsForMasterGame(MasterGame masterGame, LocalDate dateOfPotentialAcquisition)
    {
        bool? eligibilityOverride = GetOverriddenEligibility(masterGame);
        IReadOnlyList<MasterGameTag> tagOverrides = GetOverriddenTags(masterGame);
        return new MasterGameWithEligibilityFactors(masterGame, Options, eligibilityOverride, tagOverrides, dateOfPotentialAcquisition);
    }

    public Maybe<MasterGameWithEligibilityFactors> GetEligibilityFactorsForSlot(PublisherSlot publisherSlot)
    {
        if (publisherSlot.PublisherGame.HasNoValue || publisherSlot.PublisherGame.Value.MasterGame.HasNoValue)
        {
            return Maybe<MasterGameWithEligibilityFactors>.None;
        }

        var masterGame = publisherSlot.PublisherGame.Value.MasterGame.Value.MasterGame;
        bool? eligibilityOverride = GetOverriddenEligibility(masterGame);
        IReadOnlyList<MasterGameTag> tagOverrides = GetOverriddenTags(masterGame);
        var acquisitionDate = publisherSlot.PublisherGame.Value.Timestamp.ToEasternDate();
        return new MasterGameWithEligibilityFactors(publisherSlot.PublisherGame.Value.MasterGame.Value.MasterGame, Options, eligibilityOverride, tagOverrides, acquisitionDate);
    }

    public bool GameIsEligibleInAnySlot(MasterGame masterGame, LocalDate dateOfPotentialAcquisition)
    {
        var eligibilityFactors = GetEligibilityFactorsForMasterGame(masterGame, dateOfPotentialAcquisition);
        return SlotEligibilityService.GameIsEligibleInLeagueYear(eligibilityFactors);
    }

    private bool? GetOverriddenEligibility(MasterGame masterGame)
    {
        bool found = _eligibilityOverridesDictionary.TryGetValue(masterGame, out var eligibilityOverride);
        if (!found)
        {
            return null;
        }

        return eligibilityOverride.Eligible;
    }

    private IReadOnlyList<MasterGameTag> GetOverriddenTags(MasterGame masterGame)
    {
        bool found = _tagOverridesDictionary.TryGetValue(masterGame, out var tagOverride);
        if (!found)
        {
            return new List<MasterGameTag>();
        }

        return tagOverride.Tags;
    }

    public Maybe<Publisher> GetManagerPublisher()
    {
        return _managerPublisher;
    }

    public Maybe<Publisher> GetUserPublisher(FantasyCriticUser user)
    {
        var userPublisher = Publishers.SingleOrDefault(x => x.User.Id == user.Id);
        return Maybe<Publisher>.From(userPublisher);
    }

    public IReadOnlyList<Publisher> GetAllPublishersExcept(Publisher publisher)
    {
        return Publishers.Where(x => x.PublisherID != publisher.PublisherID).ToList();
    }

    public Maybe<Publisher> GetPublisherByID(Guid publisherID)
    {
        bool hasPublisher = _publisherDictionary.TryGetValue(publisherID, out var publisher);
        if (!hasPublisher)
        {
            return Maybe<Publisher>.None;
        }

        return publisher;
    }

    public Publisher GetPublisherByOrFakePublisher(Guid publisherID)
    {
        bool hasPublisher = _publisherDictionary.TryGetValue(publisherID, out var publisher);
        if (!hasPublisher)
        {
            return Publisher.GetFakePublisher(Key);
        }

        return publisher;
    }

    public bool Equals(LeagueYear other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Equals(League, other.League) && Year == other.Year;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((LeagueYear)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((League != null ? League.GetHashCode() : 0) * 397) ^ Year;
        }
    }

    public override string ToString() => $"{League}|{Year}";
}
