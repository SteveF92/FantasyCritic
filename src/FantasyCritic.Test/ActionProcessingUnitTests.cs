using FantasyCritic.FakeRepo;
using FantasyCritic.Lib.BusinessLogicFunctions;
using FantasyCritic.Lib.Interfaces;
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
        IClock fakeClock = new FakeClock(InstantPattern.ExtendedIso.Parse("2022-06-19 00:03:02.969549").GetValueOrThrow());

        //ActionProcessingFunctions.ProcessActions();
    }
}
