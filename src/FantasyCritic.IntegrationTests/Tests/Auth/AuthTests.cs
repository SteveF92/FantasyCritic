using System.Threading.Tasks;
using FantasyCritic.IntegrationTests;
using NUnit.Framework;

namespace FantasyCritic.IntegrationTests.Tests.Auth;

[TestFixture]
public class AuthTests
{
    private FantasyCriticWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new FantasyCriticWebApplicationFactory();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task Factory_Boots_AndHealthEndpointResponds()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        // Any response (even 200 or a redirect) confirms the factory booted
        Assert.That((int)response.StatusCode, Is.LessThan(500));
    }
}
