using FantasyCritic.Lib.Discord;
using FantasyCritic.Lib.Domain.Combinations;
using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Lib.Jobs.Handlers;

internal class SendAllPublicBiddingMessagesJobHandler : IFantasyCriticCronJobHandler
{
    public static FantasyCriticJobType JobType => FantasyCriticJobType.SendAllPublicBiddingMessages;
    public static FantasyCriticJobSchedule Schedule { get; } = FantasyCriticJobSchedule.Weekly(TimeExtensions.PublicBiddingRevealDay, TimeExtensions.PublicBiddingRevealTime);

    private readonly InterLeagueService _interLeagueService;
    private readonly DiscordPushService _discordPushService;
    private readonly GameAcquisitionService _gameAcquisitionService;
    private readonly EmailSendingService _emailSendingService;

    public SendAllPublicBiddingMessagesJobHandler(InterLeagueService interLeagueService, DiscordPushService discordPushService,
        GameAcquisitionService gameAcquisitionService, EmailSendingService emailSendingService)
    {
        _interLeagueService = interLeagueService;
        _discordPushService = discordPushService;
        _gameAcquisitionService = gameAcquisitionService;
        _emailSendingService = emailSendingService;
    }

    public async Task<Result> Run(FantasyCriticJobContext context, CancellationToken cancellationToken)
    {
        var publicBiddingSets = await PublicBiddingJobUtilities.GetPublicBiddingSets(_interLeagueService, _gameAcquisitionService);

        var emailTask = TrySendEmails(publicBiddingSets, cancellationToken);
        var discordTask = TrySendDiscord(publicBiddingSets, cancellationToken);
        await Task.WhenAll(emailTask, discordTask);

        var emailError = await emailTask;
        var discordError = await discordTask;

        if (emailError is null && discordError is null)
        {
            return Result.Success();
        }

        var detailedStatus = BuildDetailedStatus(emailError, discordError);
        await context.UpdateDetailedStatus(detailedStatus);

        var deliveryErrors = new List<Exception>();
        if (emailError is not null)
        {
            deliveryErrors.Add(emailError);
        }
        if (discordError is not null)
        {
            deliveryErrors.Add(discordError);
        }

        throw new AggregateException(detailedStatus, deliveryErrors);
    }

    private async Task<Exception?> TrySendEmails(IReadOnlyList<LeagueYearPublicBiddingSet> publicBiddingSets, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _emailSendingService.SendPublicBidEmails(publicBiddingSets);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ex;
        }
    }

    private async Task<Exception?> TrySendDiscord(IReadOnlyList<LeagueYearPublicBiddingSet> publicBiddingSets, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _discordPushService.SendPublicBiddingSummary(publicBiddingSets);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ex;
        }
    }

    private static string BuildDetailedStatus(Exception? emailError, Exception? discordError)
    {
        var emailStatus = emailError is null ? "Emails: succeeded" : $"Emails: failed — {emailError.Message}";
        var discordStatus = discordError is null ? "Discord: succeeded" : $"Discord: failed — {discordError.Message}";
        return $"{emailStatus}; {discordStatus}";
    }
}
