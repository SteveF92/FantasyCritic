using FantasyCritic.Lib.DependencyInjection;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class ProcessActionsJobHandler : IFantasyCriticJobHandler
{
    private static readonly IReadOnlyList<IsoDayOfWeek> ProcessingDays = [IsoDayOfWeek.Saturday, IsoDayOfWeek.Sunday];

    private readonly InterLeagueService _interLeagueService;
    private readonly AdminService _adminService;
    private readonly EnvironmentConfiguration _environmentConfiguration;
    private readonly IClock _clock;

    public ProcessActionsJobHandler(InterLeagueService interLeagueService, AdminService adminService, EnvironmentConfiguration environmentConfiguration, IClock clock)
    {
        _interLeagueService = interLeagueService;
        _adminService = adminService;
        _environmentConfiguration = environmentConfiguration;
        _clock = clock;
    }

    public static FantasyCriticJobType JobType => FantasyCriticJobType.ProcessActions;

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        //The same preconditions the action runner's endpoint enforces.
        var systemWideSettings = await _interLeagueService.GetSystemWideSettings();
        if (!systemWideSettings.ActionProcessingMode)
        {
            return Result.Failure("Turn on action processing mode first.");
        }

        var today = _clock.GetToday();
        if (_environmentConfiguration.IsProduction && !ProcessingDays.Contains(today.DayOfWeek))
        {
            return Result.Failure($"You probably didn't mean to process pickups on a {today.DayOfWeek}.");
        }

        var systemWideValues = await _interLeagueService.GetSystemWideValues();
        var supportedYears = await _interLeagueService.GetSupportedYears();
        foreach (var supportedYear in supportedYears.Where(x => x.OpenForPlay && !x.Finished))
        {
            await context.UpdateDetailedStatus($"Processing actions for {supportedYear.Year}.");
            await _adminService.ProcessActions(systemWideValues, supportedYear.Year);
        }

        await context.UpdateDetailedStatus("Processed actions for all active years.");
        return Result.Success();
    }
}
