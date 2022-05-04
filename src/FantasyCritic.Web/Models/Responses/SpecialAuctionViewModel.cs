using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Web.Models.Responses;
public class SpecialAuctionViewModel
{
    public SpecialAuctionViewModel(SpecialAuction domain, Instant currentInstant)
    {
        MasterGameYear = new MasterGameYearViewModel(domain.MasterGameYear, currentInstant.ToEasternDate());
        CreationTime = domain.CreationTime;
        ScheduledEndTime = domain.ScheduledEndTime;
        Processed = domain.Processed;
        IsLocked = domain.IsLocked(currentInstant);
    }

    public MasterGameYearViewModel MasterGameYear { get; }
    public Instant CreationTime { get; }
    public Instant ScheduledEndTime { get; }
    public bool Processed { get; }
    public bool IsLocked { get; }
}
