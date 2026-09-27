using System.Linq;
using Discord.Interactions;
using FantasyCritic.Hosting;
using FantasyCritic.Lib.Configuration;
using FantasyCritic.Lib.Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace FantasyCritic.Test;

/// <summary>
/// Discord.Net builds a slash command's module from the service provider only when someone runs that command, so
/// ValidateOnBuild never sees the modules' constructors: a missing registration would surface as a command that fails
/// in a Discord channel. This checks every module's constructor against what the bot registers.
/// </summary>
[TestFixture]
public class DiscordCommandModuleTests
{
    [Test]
    public void EveryCommandModule_CanBeBuiltFromWhatTheBotRegisters()
    {
        // What the bot's Program registers, less its health check.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFantasyCriticCore(
            new ConnectionStringsOptions { DefaultConnection = "Server=localhost" },
            new DiscordOptions { BotToken = "a-bot-token" },
            "https://localhost",
            new TestHostEnvironment(Environments.Development));
        services.AddFantasyCriticIdentityCore();
        services.AddFantasyCriticDiscordBot();

        using var provider = services.BuildServiceProvider();
        var isService = provider.GetRequiredService<IServiceProviderIsService>();

        var modules = typeof(SetLeagueCommand).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IInteractionModuleBase).IsAssignableFrom(type))
            .ToList();
        var unresolvable = modules
            .SelectMany(module => module.GetConstructors().Single().GetParameters()
                .Where(parameter => !parameter.HasDefaultValue && !isService.IsService(parameter.ParameterType))
                .Select(parameter => $"{module.Name} needs {parameter.ParameterType.Name}"))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(modules, Is.Not.Empty);
            Assert.That(unresolvable, Is.Empty);
        }
    }
}
