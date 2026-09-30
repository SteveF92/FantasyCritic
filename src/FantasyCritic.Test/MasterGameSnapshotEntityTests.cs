using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.MySQL.Entities;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class MasterGameSnapshotEntityTests
{
    private static readonly Guid MasterGameID = new Guid("3f7c2a4e-9b1d-4c8e-a6f0-5d2b7e9c1a34");
    private static readonly IReadOnlyDictionary<string, MasterGameTag> TagsByName = MasterGameTagDictionary.TagDictionary.Values.ToDictionary(x => x.Name);

    [Test]
    public void RoundTrip_KeepsEveryField()
    {
        var subGame = CreateSubGame("Snapshot Game: Part One", 82.5m);
        var original = CreateMasterGame(88.1234m, subGame);

        var roundTripped = RoundTrip(original, TagsByName);

        Assert.That(SerializeDomain(roundTripped), Is.EqualTo(SerializeDomain(original)));
    }

    [Test]
    public void RoundTrip_KeepsAnAveragedScoreAveraged()
    {
        var original = CreateMasterGame(null, CreateSubGame("Snapshot Game: Part One", 80m), CreateSubGame("Snapshot Game: Part Two", 90m));

        var roundTripped = RoundTrip(original, TagsByName);

        Assert.Multiple(() =>
        {
            Assert.That(roundTripped.RawCriticScore, Is.Null);
            Assert.That(roundTripped.CriticScore, Is.EqualTo(85m));
            Assert.That(roundTripped.AveragedScore, Is.True);
            Assert.That(SerializeDomain(roundTripped), Is.EqualTo(SerializeDomain(original)));
        });
    }

    [Test]
    public void ToDomain_ThrowsForATagThatNoLongerExists()
    {
        var original = CreateMasterGame(88m);
        var tagsWithoutPort = TagsByName.Where(x => x.Key != "Port").ToDictionary(x => x.Key, x => x.Value);

        Assert.That(() => RoundTrip(original, tagsWithoutPort), Throws.Exception.With.Message.Contains("Port"));
    }

    private static MasterGame RoundTrip(MasterGame masterGame, IReadOnlyDictionary<string, MasterGameTag> tagsByName)
    {
        var json = new MasterGameSnapshotEntity(masterGame).ToJson();
        return MasterGameSnapshotEntity.FromJson(json).ToDomain(tagsByName);
    }

    //Serializing the domain object covers every public property, so a field the snapshot drops shows up as a difference.
    private static string SerializeDomain(MasterGame masterGame) => JsonSerializer.Serialize(masterGame, FantasyCriticJsonOptions.Default);

    private static MasterSubGame CreateSubGame(string gameName, decimal? criticScore)
    {
        return new MasterSubGame(Guid.NewGuid(), MasterGameID, gameName, "Q2 2026", new LocalDate(2026, 4, 1), new LocalDate(2026, 6, 30),
            new LocalDate(2026, 5, 15), 67890, criticScore);
    }

    private static MasterGame CreateMasterGame(decimal? rawCriticScore, params MasterSubGame[] subGames)
    {
        var firstCriticScoreTimestamp = Instant.FromUtc(2026, 5, 14, 12, 0, 0).PlusTicks(1234567);
        var addedTimestamp = Instant.FromUtc(2025, 6, 8, 18, 30, 15).PlusTicks(7654321);
        var addedByUser = new VeryMinimalFantasyCriticUser(new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), "Fact Checker");
        return new MasterGame(MasterGameID, "Snapshot Game", "Q2 2026", new LocalDate(2026, 4, 1), new LocalDate(2026, 6, 30),
            new LocalDate(2026, 2, 1), new LocalDate(2026, 3, 1), new LocalDate(2025, 6, 8), new LocalDate(2026, 5, 15), 12345,
            "abc123", "snapshot-game", rawCriticScore, true, "snapshot-game-oc", "A note.", "boxart.jpg", "cover.jpg",
            firstCriticScoreTimestamp, true, true, true, true, addedTimestamp, addedByUser,
            subGames, [TagsByName["NewGamingFranchise"], TagsByName["Port"]]);
    }
}
