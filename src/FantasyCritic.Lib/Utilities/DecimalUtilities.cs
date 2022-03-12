namespace FantasyCritic.Lib.Utilities;

public static class DecimalUtilities
{
    public static decimal TruncateToPrecision(this decimal value, int precision)
    {
        decimal step = (decimal)Math.Pow(10, precision);
        decimal tmp = Math.Truncate(step * value);
        return tmp / step;
    }
}