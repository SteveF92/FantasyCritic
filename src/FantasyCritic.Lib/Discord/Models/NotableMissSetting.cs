namespace FantasyCritic.Lib.Discord.Models;

public class NotableMissSetting : TypeSafeEnum<NotableMissSetting>
{
    /// <summary>
    /// If the notable misses setting is set to none, no notable misses will be sent to League News Server.
    /// </summary>
    public static readonly NotableMissSetting None = new NotableMissSetting("None", "None", "No Notable Miss Game news will be sent to the channel");
    /// <summary>
    /// If the notable misses setting is set to initial score, only if the games initial score is above the threshold will it be sent to League News Server.
    /// If a game goes above threshold after the initial score, it will not be sent to League News Server.
    /// </summary>
    public static readonly NotableMissSetting InitialScore = new NotableMissSetting("InitialScore", "Initial Score", $"Only Notable Missed Games that initially Scored **{Threshold}** or Higher will be sent to the channel");
    /// <summary>
    /// If the notable misses setting is set to score updates, only if the games score is above the threshold will it be sent to League News Server.
    /// </summary>
    public static readonly NotableMissSetting ScoreUpdates = new NotableMissSetting("ScoreUpdates", "Score Updates", "All Notable Miss Game News will be sent to the channel");

    /// <summary>
    /// The score threshold for a game to be considered a notable miss.
    /// </summary>
    public const decimal Threshold = 83m;

    public string ReadableName { get; }

    public string Description { get; }
    public override string ToString() => Value;

    private NotableMissSetting(string value, string readableName, string description)
        : base(value)
    {
        ReadableName = readableName;
        Description = description;
    }
}
