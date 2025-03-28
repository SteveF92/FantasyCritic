namespace FantasyCritic.Lib.Statistics;

public class MasterGameYearScriptInput
{
    public MasterGameYearScriptInput(MasterGameYear masterGameYear)
    {
        Year = masterGameYear.Year;
        MasterGameID = masterGameYear.MasterGame.MasterGameID;
        GameName = masterGameYear.MasterGame.GameName;

        EligiblePercentStandardGame = masterGameYear.EligiblePercentStandardGame;
        AdjustedPercentCounterPick = masterGameYear.AdjustedPercentCounterPick ?? 0;
        AverageDraftPosition = masterGameYear.AverageDraftPosition;
        DateAdjustedHypeFactor = masterGameYear.DateAdjustedHypeFactor;
        TotalBidAmount = masterGameYear.TotalBidAmount;
        BidPercentile = masterGameYear.BidPercentile;

        if (masterGameYear.MasterGame.CriticScore.HasValue)
        {
            CriticScore = (double) masterGameYear.MasterGame.CriticScore.Value;
        }
        else if (!masterGameYear.CouldRelease())
        {
            CriticScore = 70.0;
        }
    }

    public int Year { get; }
    public Guid MasterGameID { get; }
    public string GameName { get; }
    public double EligiblePercentStandardGame { get; }
    public double AdjustedPercentCounterPick { get; }
    public double DateAdjustedHypeFactor { get; }
    public double? AverageDraftPosition { get; }
    public int TotalBidAmount { get; }
    public double BidPercentile { get; }
    public double? CriticScore { get; }
}
