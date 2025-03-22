namespace FantasyCritic.Lib.Domain;

public record HypeConstants(double BaseScore, double StandardGameConstant, double CounterPickConstant, double HypeFactorConstant)
{
    public override string ToString()
    {
        return
            $"BaseScore= {BaseScore}  StandardGameConstant= {StandardGameConstant}  CounterPickConstant= {CounterPickConstant}  " +
            $"HypeFactorConstant= {HypeFactorConstant}";
    }
}
