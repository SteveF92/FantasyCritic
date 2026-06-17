namespace FantasyCritic.Lib.Domain;

public record HypeConstants(double BaseScore, double StandardGameConstant, double CounterPickConstant, double HypeFactorConstant)
{
    public override string ToString()
    {
        return
            $"BaseScore= {BaseScore}  StandardGameConstant= {StandardGameConstant}  CounterPickConstant= {CounterPickConstant}  " +
            $"HypeFactorConstant= {HypeFactorConstant}";
    }

    //These were the correct values on 2025-01-21 and are likely good enough.
    public static HypeConstants DefaultValues => new HypeConstants(72.95706879412415, 2.2150845010046707, -5.817742823276648, 0.10208646257897502);
}
