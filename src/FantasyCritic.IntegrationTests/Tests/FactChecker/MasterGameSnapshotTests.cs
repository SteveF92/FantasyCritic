using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FantasyCritic.IntegrationTests.Helpers;
using FantasyCritic.Lib;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Identity;
using FantasyCritic.Lib.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.FactChecker;

//Goes through IMasterGameRepo rather than the API, because the API can't set sub-games, scores, slugs or art,
//and a field left empty on both sides of the round trip would hide a snapshot that drops it.
[TestFixture]
public class MasterGameSnapshotTests : IntegrationTestBase
{
    [Test]
    public async Task PendingNewGame_KeepsEveryField()
    {
        var addedByUser = await RegisterUserAsync();
        var masterGameID = Guid.NewGuid();
        var original = await CreateMasterGameAsync(masterGameID, addedByUser, 88.1234m, CreateSubGame(masterGameID, "Part One", 82.5m));

        var snapshot = await GetPendingNewGameAsync(masterGameID);

        Assert.That(SerializeDomain(snapshot), Is.EqualTo(SerializeDomain(original)));
    }

    [Test]
    public async Task PendingNewGame_KeepsAnAveragedScoreAveraged()
    {
        var addedByUser = await RegisterUserAsync();
        var masterGameID = Guid.NewGuid();
        var original = await CreateMasterGameAsync(masterGameID, addedByUser, null,
            CreateSubGame(masterGameID, "Part One", 80m), CreateSubGame(masterGameID, "Part Two", 90m));

        var snapshot = await GetPendingNewGameAsync(masterGameID);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshot.RawCriticScore, Is.Null);
            Assert.That(snapshot.CriticScore, Is.EqualTo(85m));
            Assert.That(snapshot.AveragedScore, Is.True);
            Assert.That(SerializeDomain(snapshot), Is.EqualTo(SerializeDomain(original)));
        }
    }

    private async Task<VeryMinimalFantasyCriticUser> RegisterUserAsync()
    {
        var (email, password, displayName) = NewUser();
        using var session = new ApiSession(Factory);
        await session.RegisterAsync(email, password, displayName);
        var me = await session.Account.CurrentUserAsync();
        return new VeryMinimalFantasyCriticUser(me.UserID, me.DisplayName);
    }

    private async Task<MasterGame> CreateMasterGameAsync(Guid masterGameID, VeryMinimalFantasyCriticUser addedByUser, decimal? rawCriticScore, params MasterSubGame[] subGames)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var masterGameRepo = scope.ServiceProvider.GetRequiredService<IMasterGameRepo>();
        var tagsByName = await masterGameRepo.GetMasterGameTagDictionary();

        var firstCriticScoreTimestamp = Instant.FromUtc(2026, 5, 14, 12, 0, 0).PlusTicks(1234567);
        var addedTimestamp = Instant.FromUtc(2025, 6, 8, 18, 30, 15).PlusTicks(7654321);
        var masterGame = new MasterGame(masterGameID, $"Snapshot Game {masterGameID:N}", "Q2 2026", new LocalDate(2026, 4, 1), new LocalDate(2026, 6, 30),
            new LocalDate(2026, 2, 1), new LocalDate(2026, 3, 1), new LocalDate(2025, 6, 8), new LocalDate(2026, 5, 15), 12345,
            "abc123", "snapshot-game", rawCriticScore, true, "snapshot-game-oc", "A note.", "boxart.jpg", "cover.jpg",
            firstCriticScoreTimestamp, true, true, true, true, addedTimestamp, addedByUser,
            subGames, [tagsByName["NewGamingFranchise"], tagsByName["Port"]]);

        await masterGameRepo.CreateMasterGame(masterGame);
        return masterGame;
    }

    private async Task<MasterGame> GetPendingNewGameAsync(Guid masterGameID)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var masterGameRepo = scope.ServiceProvider.GetRequiredService<IMasterGameRepo>();
        var pending = await masterGameRepo.GetPendingMasterGameUpdates();
        return pending.NewGames.Single(x => x.MasterGame.MasterGameID == masterGameID).MasterGame;
    }

    private static MasterSubGame CreateSubGame(Guid masterGameID, string gameName, decimal? criticScore)
    {
        return new MasterSubGame(Guid.NewGuid(), masterGameID, gameName, "Q2 2026", new LocalDate(2026, 4, 1), new LocalDate(2026, 6, 30),
            new LocalDate(2026, 5, 15), 67890, criticScore);
    }

    //Serializing the domain object covers every public property, so a field the snapshot drops shows up as a difference.
    private static string SerializeDomain(MasterGame masterGame) => JsonSerializer.Serialize(masterGame, FantasyCriticJsonOptions.Default);
}
