namespace FantasyCritic.Lib.Domain;

public record HypeConstants(double BaseScore, double StandardGameConstant, double CounterPickConstant, double HypeFactorConstant)
{
    public override string ToString()
    {
        return
            $"BaseScore= {BaseScore}  StandardGameConstant= {StandardGameConstant}  CounterPickConstant= {CounterPickConstant}  " +
            $"HypeFactorConstant= {HypeFactorConstant}";
    }

    //These were the correct values on 2026-06-25 and are likely good enough.
    public static HypeConstants DefaultValues => new HypeConstants(73.2460457170206, 0.0675878234740534, -2.7836334342253632, 0.1711085390579201);
}
