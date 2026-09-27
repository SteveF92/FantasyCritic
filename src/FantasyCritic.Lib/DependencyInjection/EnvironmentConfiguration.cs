namespace FantasyCritic.Lib.DependencyInjection;

/// <param name="IntegrationTestMode">Never configuration: only the integration tests turn it on, through their DI overrides.</param>
public record EnvironmentConfiguration(string BaseAddress, bool IsProduction, bool IntegrationTestMode);
