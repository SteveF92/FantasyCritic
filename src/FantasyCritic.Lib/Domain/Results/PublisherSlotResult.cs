namespace FantasyCritic.Lib.Domain.Results;

public class PublisherSlotAcquisitionResult
{
    public PublisherSlotAcquisitionResult(LeagueSlot slot)
    {
        Slot = slot;
        ClaimErrors = new List<ClaimError>();
    }

    public PublisherSlotAcquisitionResult(IEnumerable<ClaimError> claimErrors)
    {
        Slot = null;
        ClaimErrors = claimErrors.ToList();
    }

    public LeagueSlot? Slot { get; }
    public IReadOnlyList<ClaimError> ClaimErrors { get; }
}
