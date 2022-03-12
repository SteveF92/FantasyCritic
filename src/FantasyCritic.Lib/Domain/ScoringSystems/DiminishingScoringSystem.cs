namespace FantasyCritic.Lib.Domain.ScoringSystems;

public class DiminishingScoringSystem : ScoringSystem
{
    public static string StaticName => "Diminishing";
    public override string Name => StaticName;

    public override decimal GetPointsForScore(decimal criticScore, bool counterPick)
    {
        decimal fantasyPoints;
        if (criticScore < 10m)
        {
            fantasyPoints = (1m / 64m) * (criticScore - 10) - 19.6875m;
        }
        else if (criticScore < 20m)
        {
            fantasyPoints = (1m / 32m) * (criticScore - 20) - 19.375m;
        }
        else if (criticScore < 30m)
        {
            fantasyPoints = (1m / 16m) * (criticScore - 30) - 18.75m;
        }
        else if (criticScore < 40m)
        {
            fantasyPoints = (1m / 8m) * (criticScore - 40) - 17.5m;
        }
        else if (criticScore < 50m)
        {
            fantasyPoints = (1m / 4m) * (criticScore - 50) - 15;
        }
        else if (criticScore < 60m)
        {
            fantasyPoints = (1m / 2m) * (criticScore - 60) - 10;
        }
        else if (criticScore < 90m)
        {
            fantasyPoints = criticScore - 70;
        }
        else
        {
            fantasyPoints = 2 * (criticScore - 90) + 20;
        }

        if (counterPick)
        {
            fantasyPoints *= -1;
        }

        return fantasyPoints;
    }
}
