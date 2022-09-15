using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Test.TestData;
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

        var systemWideValues = TestDataService.GetSystemWideValues();
        var allActiveBids = TestDataService.GetActiveBids();
        var allActiveDrops = TestDataService.GetActiveDrops();
        var publishers = TestDataService.GetPublishers();
        var masterGameYearDictionary = TestDataService.GetMasterGameYears();

        var results = ActionProcessingFunctions.ProcessActions(systemWideValues, allActiveBids, allActiveDrops, publishers, processingTime, currentDate, masterGameYearDictionary);
    }
}
