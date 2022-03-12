namespace FantasyCritic.Lib.DependencyInjection;

public record RepositoryConfiguration(string ConnectionString, IClock Clock);