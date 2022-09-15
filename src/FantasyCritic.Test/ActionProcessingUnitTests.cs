using System;
using System.Collections.Generic;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Domain.LeagueActions;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class ActionProcessingUnitTests
{
    [Test]
    public void ActionProcess()
    {
        Instant processingTime = InstantPattern.ExtendedIso.Parse("2022-06-19 00:03:02.969549").GetValueOrThrow();
        LocalDate currentDate = new LocalDate(2022, 6, 18);

        var systemWideValues = new SystemWideValues(0m, 0m, 0m, new List<AveragePickPositionPoints>());
        var allActiveBids = new Dictionary<LeagueYear, IReadOnlyList<PickupBid>>();
        var allActiveDrops = new Dictionary<LeagueYear, IReadOnlyList<DropRequest>>();
        var publishers = new List<Publisher>();
        var masterGameYearDictionary = new Dictionary<Guid, MasterGameYear>();

        var results = ActionProcessingFunctions.ProcessActions(systemWideValues, allActiveBids, allActiveDrops, publishers, processingTime, currentDate, masterGameYearDictionary);
    }
}
