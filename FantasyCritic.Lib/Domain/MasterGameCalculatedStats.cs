namespace FantasyCritic.Lib.Domain
{
    public class MasterGameCalculatedStats
    {
        public MasterGameCalculatedStats(MasterGame masterGame, int year, double hypeFactor, double dateAdjustedHypeFactor, double linearRegressionHypeFactor)
        {
            MasterGame = masterGame;
            HypeFactor = hypeFactor;
            DateAdjustedHypeFactor = dateAdjustedHypeFactor;
            LinearRegressionHypeFactor = linearRegressionHypeFactor;
        }

        public MasterGame MasterGame { get; }
        public int Year { get; }
        public double HypeFactor { get; }
        public double DateAdjustedHypeFactor { get; }
        public double LinearRegressionHypeFactor { get; }
    }
}
