using FantasyCritic.Lib.Domain.Results;

namespace FantasyCritic.Lib.Domain;

public class PublicBiddingMasterGame
{
    public PublicBiddingMasterGame(MasterGameYear masterGameYear, bool counterPick, IEnumerable<ClaimError> claimErrors)
    {
        MasterGameYear = masterGameYear;
        CounterPick = counterPick;
        ClaimErrors = claimErrors.ToList();
    }

    public MasterGameYear MasterGameYear { get; }
    public bool CounterPick { get; }
    public IReadOnlyList<ClaimError> ClaimErrors { get; }
}