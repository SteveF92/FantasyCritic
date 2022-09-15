using FantasyCritic.FakeRepo;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Services;
using FantasyCritic.Test.Mocks;
using NodaTime;
using NodaTime.Testing;
using NodaTime.Text;
using NUnit.Framework;

namespace FantasyCritic.Test;

[TestFixture]
public class ActionProcessingUnitTests
{
    [Test]
    public void ActionProcess()
    {
        IFantasyCriticRepo fantasyCriticRepo = new MockedFantasyCriticRepo();
        IMasterGameRepo masterGameRepo = new FakeMasterGameRepo();
        IClock fakeClock = new FakeClock(InstantPattern.ExtendedIso.Parse("2022-06-19 00:03:02.969549").GetValueOrThrow());

        GameAcquisitionService gameAcquisitionService = new GameAcquisitionService(fantasyCriticRepo, masterGameRepo, fakeClock);
        ActionProcessingService actionProcessingService = new ActionProcessingService(gameAcquisitionService);

    }
}
