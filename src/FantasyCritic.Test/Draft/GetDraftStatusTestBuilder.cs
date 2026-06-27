using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.Draft;
using FantasyCritic.Lib.Domain.ScoringSystems;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;

namespace FantasyCritic.Test.Draft;

internal sealed class GetDraftStatusTestBuilder
{
    private readonly Guid _leagueID = Guid.NewGuid();
    private readonly int _year = 2025;
    private readonly List<PublisherSpec> _publisherSpecs = [];
    private readonly List<DraftSpec> _draftSpecs = [];
    private int _nextDraftNumber = 1;

    public GetDraftStatusTestBuilder WithPublishers(int count)
    {
        if (count < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "At least two publishers are required.");
        }

        _publisherSpecs.Clear();
        for (var index = 0; index < count; index++)
        {
            _publisherSpecs.Add(new PublisherSpec(Guid.NewGuid(), $"Publisher {index + 1}", index + 1));
        }

        return this;
    }

    public DraftScenarioBuilder WithDraft(int gamesToDraft, int counterPicksToDraft, PlayStatus playStatus, string? name = null)
    {
        var draftNumber = _nextDraftNumber++;
        var draftSpec = new DraftSpec(draftNumber, gamesToDraft, counterPicksToDraft, playStatus, name ?? $"Draft {draftNumber}");
        _draftSpecs.Add(draftSpec);
        return new DraftScenarioBuilder(this, draftSpec);
    }

    public LeagueYear Build() => BuildLeagueYear(forceActiveDraftNumber: null);

    internal LeagueYear BuildLeagueYear(int? forceActiveDraftNumber)
    {
        if (_publisherSpecs.Count < 2)
        {
            throw new InvalidOperationException("Call WithPublishers before Build.");
        }

        if (_draftSpecs.Count == 0)
        {
            throw new InvalidOperationException("At least one draft is required.");
        }

        var leagueYearKey = new LeagueYearKey(_leagueID, _year);
        var orderedDrafts = _draftSpecs.OrderBy(x => x.DraftNumber).ToList();
        var leagueDrafts = orderedDrafts
            .Select(draftSpec => CreateLeagueDraft(leagueYearKey, draftSpec, forceActiveDraftNumber, _publisherSpecs))
            .ToList();

        var publishers = _publisherSpecs
            .Select(publisherSpec =>
            {
                var draftInfos = orderedDrafts
                    .Select(draftSpec => new PublisherDraftInfo(
                        DraftIDFor(draftSpec.DraftNumber),
                        draftSpec.DraftNumber,
                        publisherSpec.PublisherID,
                        publisherSpec.DraftPosition,
                        new List<PublisherDraftPickSkip>()))
                    .ToList();

                var games = publisherSpec.Games
                    .Select(gameSpec => CreatePublisherGame(publisherSpec.PublisherID, gameSpec))
                    .ToList();

                return new Publisher(
                    publisherSpec.PublisherID,
                    leagueYearKey,
                    FantasyCriticUser.GetFakeUser(),
                    publisherSpec.Name,
                    null,
                    null,
                    draftInfos,
                    games,
                    [],
                    100,
                    0,
                    0,
                    0,
                    0,
                    new AutoDraftSettings(AutoDraftMode.Off, false));
            })
            .ToList();

        var league = new League(
            _leagueID,
            "Test League",
            new MinimalFantasyCriticUser(Guid.NewGuid(), "Manager", "manager@test.com"),
            null,
            null,
            [new MinimalLeagueYearInfo(_year, false, true)],
            true,
            false,
            false,
            false,
            0);

        var supportedYear = new SupportedYear(_year, true, true, true, new LocalDate(2024, 12, 8), false);
        var leagueOptions = new LeagueOptions(
            10,
            5,
            orderedDrafts.Max(x => x.GamesToDraft),
            orderedDrafts.Max(x => x.CounterPicksToDraft),
            0,
            false,
            false,
            false,
            false,
            0,
            true,
            [new LeagueTagStatus(MasterGameTagDictionary.TagDictionary["PRT"], TagStatus.Banned)],
            [],
            DraftSystem.Flexible,
            PickupSystem.SemiPublicBiddingSecretCounterPicks,
            ScoringSystem.GetDefaultScoringSystem(_year),
            TradingSystem.Standard,
            TiebreakSystem.LowestProjectedPoints,
            ReleaseSystem.MustBeReleased,
            IneligibleGameSystem.CaseByCase,
            new AnnualDate(10, 1),
            new AnnualDate(10, 1));

        return new LeagueYear(
            league,
            supportedYear,
            leagueOptions,
            leagueDrafts,
            [],
            [],
            null,
            publishers,
            null,
            false,
            null);
    }

    internal void AddGame(DraftSpec draft, Publisher publisher, bool counterPick, int publisherDraftPosition, int overallDraftPosition, Instant timestamp)
    {
        var publisherSpec = _publisherSpecs.Single(x => x.PublisherID == publisher.PublisherID);
        publisherSpec.Games.Add(new GameSpec(
            draft.DraftNumber,
            counterPick,
            publisherDraftPosition,
            overallDraftPosition,
            timestamp));
    }

    private static LeagueDraft CreateLeagueDraft(
        LeagueYearKey leagueYearKey,
        GetDraftStatusTestBuilder.DraftSpec draftSpec,
        int? forceActiveDraftNumber,
        IReadOnlyList<GetDraftStatusTestBuilder.PublisherSpec> publisherSpecs)
    {
        var draftID = DraftIDFor(draftSpec.DraftNumber);
        var playStatus = draftSpec.PlayStatus;
        if (forceActiveDraftNumber == draftSpec.DraftNumber)
        {
            playStatus = PlayStatus.Drafting;
        }

        var publisherDraftInfo = publisherSpecs
            .Select(publisher => new PublisherDraftInfo(draftID, draftSpec.DraftNumber, publisher.PublisherID, publisher.DraftPosition, new List<PublisherDraftPickSkip>()))
            .ToList();

        return new LeagueDraft(
            draftID,
            leagueYearKey,
            draftSpec.DraftNumber,
            draftSpec.Name,
            null,
            draftSpec.GamesToDraft,
            draftSpec.CounterPicksToDraft,
            true,
            playStatus,
            publisherDraftInfo,
            playStatus.PlayStarted ? Instant.FromUtc(2025, 6, 1, 12, 0) : null);
    }

    internal static Guid DraftIDFor(int draftNumber) => Guid.Parse($"{draftNumber:D8}-0000-0000-0000-{draftNumber:D012}");

    private static PublisherGame CreatePublisherGame(Guid publisherID, GameSpec gameSpec)
    {
        var draftID = DraftIDFor(gameSpec.DraftNumber);
        return new PublisherGame(
            publisherID,
            Guid.NewGuid(),
            $"Game d{gameSpec.DraftNumber}-{(gameSpec.CounterPick ? "cp" : "std")}-{gameSpec.OverallDraftPosition}",
            gameSpec.Timestamp,
            gameSpec.CounterPick,
            null,
            false,
            null,
            null,
            gameSpec.OverallDraftPosition,
            gameSpec.PublisherDraftPosition,
            gameSpec.OverallDraftPosition,
            null,
            null,
            draftID);
    }

    internal sealed class PublisherSpec(Guid publisherID, string name, int draftPosition)
    {
        public Guid PublisherID { get; } = publisherID;
        public string Name { get; } = name;
        public int DraftPosition { get; } = draftPosition;
        public List<GameSpec> Games { get; } = [];
    }

    internal sealed class DraftSpec(int draftNumber, int gamesToDraft, int counterPicksToDraft, PlayStatus playStatus, string name)
    {
        public int DraftNumber { get; } = draftNumber;
        public int GamesToDraft { get; } = gamesToDraft;
        public int CounterPicksToDraft { get; } = counterPicksToDraft;
        public PlayStatus PlayStatus { get; } = playStatus;
        public string Name { get; } = name;
        public int PickSequence { get; set; }
    }

    internal sealed record GameSpec(int DraftNumber, bool CounterPick, int PublisherDraftPosition, int OverallDraftPosition, Instant Timestamp);
}

internal sealed class DraftScenarioBuilder
{
    private readonly GetDraftStatusTestBuilder _root;
    private readonly GetDraftStatusTestBuilder.DraftSpec _draft;

    internal DraftScenarioBuilder(GetDraftStatusTestBuilder root, GetDraftStatusTestBuilder.DraftSpec draft)
    {
        _root = root;
        _draft = draft;
    }

    public DraftScenarioBuilder PickStandard() => Pick(counterPick: false, DraftPhase.StandardGames);

    public DraftScenarioBuilder PickCounterPick() => Pick(counterPick: true, DraftPhase.CounterPicks);

    public DraftScenarioBuilder WithDraft(int gamesToDraft, int counterPicksToDraft, PlayStatus playStatus, string? name = null) =>
        _root.WithDraft(gamesToDraft, counterPicksToDraft, playStatus, name);

    public LeagueYear Build() => _root.Build();

    private DraftScenarioBuilder Pick(bool counterPick, DraftPhase expectedPhase)
    {
        var leagueYear = _root.BuildLeagueYear(forceActiveDraftNumber: _draft.DraftNumber);
        var status = DraftFunctions.GetDraftStatus(leagueYear)
            ?? throw new InvalidOperationException($"Cannot pick in draft {_draft.DraftNumber}: draft has no active turn.");

        if (!status.DraftPhase.Equals(expectedPhase))
        {
            throw new InvalidOperationException(
                $"Cannot {(counterPick ? "counter-pick" : "pick a standard game")} in draft {_draft.DraftNumber}: current phase is {status.DraftPhase}.");
        }

        _draft.PickSequence++;
        var timestamp = Instant.FromUtc(2025, 6, 1, 12, _draft.PickSequence);
        _root.AddGame(_draft, status.NextDraftPublisher, counterPick, status.RoundNumber, status.OverallPickNumber, timestamp);
        return this;
    }
}
