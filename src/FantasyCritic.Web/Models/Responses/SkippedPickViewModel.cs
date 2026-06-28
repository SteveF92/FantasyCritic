using FantasyCritic.Lib.Domain.Draft;

namespace FantasyCritic.Web.Models.Responses;

public class SkippedPickViewModel
{
    public SkippedPickViewModel(PastDraftPick domain)
    {
        PublisherName = domain.Publisher.GetPublisherAndUserDisplayName();
        RoundNumber = domain.RoundNumber;
        CounterPick = domain.CounterPick;
        IsManualSkip = domain.IsManualSkip
            ?? throw new ArgumentException("Only skipped picks may be mapped to SkippedPickViewModel.", nameof(domain));
    }

    public string PublisherName { get; }
    public int RoundNumber { get; }
    public bool CounterPick { get; }
    public bool IsManualSkip { get; }
}
