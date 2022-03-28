namespace FantasyCritic.Lib.Domain;

public class HypeConstants
{
    public HypeConstants(double baseScore, double standardGameConstant, double counterPickConstant, double hypeFactorConstant)
    {
        BaseScore = baseScore;
        StandardGameConstant = standardGameConstant;
        CounterPickConstant = counterPickConstant;
        HypeFactorConstant = hypeFactorConstant;
    }

    public double BaseScore { get; }
    public double StandardGameConstant { get; }
    public double CounterPickConstant { get; }
    public double HypeFactorConstant { get; }

    public override string ToString()
    {
        return
            $"BaseScore= {BaseScore}  StandardGameConstant= {StandardGameConstant}  CounterPickConstant= {CounterPickConstant}  " +
            $"HypeFactorConstant= {HypeFactorConstant}";
    }

    public static HypeConstants GetReasonableDefaults()
    {
        //These were the correct values on 2022-03-28 and are likely good enough.
        return new HypeConstants(72.15607447516923, 1.599796665594027, -2.4574452697921627, 0.10576458996831795);
    }
}
