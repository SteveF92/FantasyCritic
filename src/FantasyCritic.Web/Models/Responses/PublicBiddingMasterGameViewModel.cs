namespace FantasyCritic.Web.Models.Responses
{
    public class PublicBiddingMasterGameViewModel
    {
        public PublicBiddingMasterGameViewModel(PublicBiddingMasterGame masterGame, LocalDate currentDate)
        {
            MasterGame = new MasterGameYearViewModel(masterGame.MasterGameYear, currentDate);
            CounterPick = masterGame.CounterPick;
            EligibilityErrors = masterGame.ClaimErrors.Select(x => x.Error).ToList();
        }

        public MasterGameYearViewModel MasterGame { get; }
        public bool CounterPick { get; }
        public IReadOnlyList<string> EligibilityErrors { get; }
    }
}
