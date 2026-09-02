using System;
using System.Collections.Generic;
using System.Linq;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain.LeagueActions;
using FantasyCritic.Lib.Extensions;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class TopBidsAndDropsFunctionsTests
{
    private static Instant GetInstantFromEasternString(string dateTimeString)
    {
        var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTimeString).GetValueOrThrow();
        return localDateTime.InZoneStrictly(TimeExtensions.EasternTimeZone).ToInstant();
    }

    [Test]
    public void GetActionProcessingWeeks_MergesMultipleWeekEndingsOnSameEasternDate()
    {
        var morningWeekEndId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var afternoonWeekEndId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var morningBidSetId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var afternoonBidSetId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var processDate = new LocalDate(2020, 8, 31);
        IReadOnlyList<ActionProcessingSetMetadata> actionProcessingSets =
        [
            new ActionProcessingSetMetadata(morningBidSetId, GetInstantFromEasternString("2020-08-31T10:00:00"), "Bid Processing"),
            new ActionProcessingSetMetadata(morningWeekEndId, GetInstantFromEasternString("2020-08-31T12:00:00"), "Drop/Bid Processing"),
            new ActionProcessingSetMetadata(afternoonBidSetId, GetInstantFromEasternString("2020-08-31T14:00:00"), "Bid Processing"),
            new ActionProcessingSetMetadata(afternoonWeekEndId, GetInstantFromEasternString("2020-08-31T16:00:00"), "Drop/Bid Processing"),
        ];

        var weeks = TopBidsAndDropsFunctions.GetActionProcessingWeeks(actionProcessingSets);

        Assert.Multiple(() =>
        {
            Assert.That(weeks, Has.Count.EqualTo(1));
            Assert.That(weeks[0].ProcessDate, Is.EqualTo(processDate));
            Assert.That(weeks[0].ProcessingSets.Select(x => x.ProcessSetID), Is.EquivalentTo(new[]
            {
                morningBidSetId,
                morningWeekEndId,
                afternoonBidSetId,
                afternoonWeekEndId,
            }));
        });
    }
}
