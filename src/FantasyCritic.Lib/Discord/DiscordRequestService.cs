using FantasyCritic.Lib.DependencyInjection;

namespace FantasyCritic.Lib.Discord;
public class DiscordRequestService : BaseDiscordService
{
    public DiscordRequestService(DiscordConfiguration configuration)
        : base(configuration)
    {

    }
}
