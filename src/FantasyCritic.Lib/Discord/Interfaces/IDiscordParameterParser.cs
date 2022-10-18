namespace FantasyCritic.Lib.Discord.Interfaces;

public interface IDiscordParameterParser
{
    LocalDate? GetDateFromProvidedYear(int? providedYear = null);
}
