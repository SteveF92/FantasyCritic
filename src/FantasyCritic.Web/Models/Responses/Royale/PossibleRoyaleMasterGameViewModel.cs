using FantasyCritic.Lib.Extensions;
using FantasyCritic.Lib.Royale;
using FantasyCritic.Lib.Services;

namespace FantasyCritic.Web.Models.Responses.Royale;

public class PossibleRoyaleMasterGameViewModel
{
    private readonly RoyalePurchaseGameValidation _purchaseValidation;

    public PossibleRoyaleMasterGameViewModel(MasterGameYear masterGame, LocalDate currentDate, RoyalePublisher publisher, IEnumerable<MasterGameTag> masterGameTags)
    {
        _purchaseValidation = RoyaleService.ValidatePurchaseGame(publisher, masterGame, masterGameTags, currentDate);
        MasterGame = new MasterGameYearViewModel(masterGame, currentDate);
        WillReleaseInQuarter = masterGame.CouldReleaseInQuarter(publisher.YearQuarter.YearQuarter);
        AlreadyOwned = publisher.PublisherGames.Any(y => y.MasterGame.MasterGame.Equals(masterGame.MasterGame));
        IsEligible = !LeagueTagExtensions.GetRoyaleClaimErrors(masterGameTags, masterGame.MasterGame, currentDate, publisher.YearQuarter).Any();
        IsReleased = masterGame.MasterGame.IsReleased(currentDate);
        HasScore = masterGame.MasterGame.CriticScore.HasValue;
        Cost = masterGame.GetRoyaleGameCost();
    }

    public MasterGameYearViewModel MasterGame { get; }
    public bool WillReleaseInQuarter { get; }
    public decimal Cost { get; }
    public bool AlreadyOwned { get; }
    public bool IsEligible { get; }
    public bool IsReleased { get; }
    public bool HasScore { get; }

    public bool IsAvailable => _purchaseValidation.CanPurchase;

    public string Status
    {
        get
        {
            if (AlreadyOwned)
            {
                return "Already Owned";
            }

            if (_purchaseValidation.CanPurchase)
            {
                return "Available";
            }

            return _purchaseValidation.BlockingReason!;
        }
    }
}
