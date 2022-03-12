using FantasyCritic.Lib.Domain;

namespace FantasyCritic.AWS;

public class HypeConstantsEntity
{
    public double BaseScore { get; set; }
    public double EligiblePercentStandardGame { get; set; }
    public double AdjustedPercentCounterPick { get; set; }
    public double DateAdjustedHypeFactor { get; set; }

    public HypeConstants ToDomain()
    {
        return new HypeConstants(BaseScore, EligiblePercentStandardGame, AdjustedPercentCounterPick, DateAdjustedHypeFactor);
    }
}