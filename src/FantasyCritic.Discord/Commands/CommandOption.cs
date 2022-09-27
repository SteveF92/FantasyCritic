using Discord;

namespace FantasyCritic.Discord.Commands;
public class CommandOption
{
    public CommandOption(string name, string description, ApplicationCommandType type, bool isRequired = false)
    {
        Name = name;
        Type = type;
        Description = description;
    }

    public string Name { get; set; }
    public ApplicationCommandType Type { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
}
