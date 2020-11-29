namespace FantasyCritic.Lib.Domain
{
    public class MasterGameCalculatedStats
    {
        public MasterGameCalculatedStats(MasterGameYear masterGameYear, double hypeFactor, double dateAdjustedHypeFactor, double linearRegressionHypeFactor)
        {
            MasterGameYear = masterGameYear;
            HypeFactor = hypeFactor;
            DateAdjustedHypeFactor = dateAdjustedHypeFactor;
            LinearRegressionHypeFactor = linearRegressionHypeFactor;
        }

        public MasterGameYear MasterGameYear { get; }
        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public double LinearRegressionHypeFactor { get; }
    }
}
