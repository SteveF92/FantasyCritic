namespace FantasyCritic.Lib.Domain.ScoringSystems;

public class LegacyScoringSystem : ScoringSystem
{
    public static string StaticName => "Legacy";
    public override string Name => StaticName;

    public override bool SupportedInYear(int year)
    {
        return year < 2021;
    }

    public override bool SupportedForNewLeagues() => false;

    public override decimal GetPointsForScore(decimal criticScore, bool counterPick)
    {
        decimal fantasyPoints = 0m;
        decimal criticPointsOver90 = (criticScore - 90);
        if (criticPointsOver90 > 0)
        {
            fantasyPoints += criticPointsOver90;
        }

        decimal criticPointsOver70 = (criticScore - 70);
        fantasyPoints += criticPointsOver70;

        if (counterPick)
        {
            fantasyPoints *= -1;
        }

        return fantasyPoints;
    }

    public override decimal GetMinimumScore() => -70;
    public override decimal GetMaximumScore() => 40;

    public override string GetReadableString() => "Legacy";
}
