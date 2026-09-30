using System;
using System.Collections.Generic;
using FantasyCritic.FakeRepo.TestUtilities;
using FantasyCritic.Lib.Discord.Models;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Enums;
using FantasyCritic.Lib.Identity;
using NodaTime;
using NUnit.Framework;

namespace FantasyCritic.Test.Discord;

[TestFixture]
public class MasterGameEditMessageTests
{
    private const int Year = 2026;

    [Test]
    public void PreviousReleaseStatus_IsTheExistingStatus_WhenTheEditChangesIt()
    {
        var existingGame = CreateMasterGame(new LocalDate(2026, 6, 1), new LocalDate(2026, 6, 1));
        var editedGame = CreateMasterGame(new LocalDate(2027, 3, 1), new LocalDate(2027, 3, 1));

        var message = new MasterGameEditMessage(Guid.NewGuid(), existingGame, editedGame, Year, ["Release date changed."]);

        Assert.That(message.PreviousReleaseStatus, Is.EqualTo(WillReleaseStatus.WillRelease));
    }

    [Test]
    public void PreviousReleaseStatus_IsNull_WhenTheEditKeepsIt()
    {
        var existingGame = CreateMasterGame(new LocalDate(2026, 6, 1), new LocalDate(2026, 6, 1));
        var editedGame = CreateMasterGame(new LocalDate(2026, 9, 1), new LocalDate(2026, 9, 1));

        var message = new MasterGameEditMessage(Guid.NewGuid(), existingGame, editedGame, Year, ["Release date changed."]);

        Assert.That(message.PreviousReleaseStatus, Is.Null);
    }

    private static MasterGame CreateMasterGame(LocalDate minimumReleaseDate, LocalDate maximumReleaseDate)
    {
        return new MasterGame(Guid.Empty, "Test Master Game", "Release Date String", minimumReleaseDate, maximumReleaseDate, null, null, null,
            null, null, null, null, null, false, null, "", null, null, null, false, false, false, false, Instant.MinValue, new FantasyCriticUser() { Id = Guid.Empty }.ToVeryMinimal(),
            new List<MasterSubGame>(), new List<MasterGameTag>() { MasterGameTagDictionary.TagDictionary["NGF"] });
    }
}
