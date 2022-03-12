using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Utilities;

public static class TimeFunctions
{
    public static (LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate) ParseEstimatedReleaseDate(string estimatedReleaseDate, IClock clock)
    {
        var range = ParseEstimatedReleaseDateInner(estimatedReleaseDate, clock);
        var tomorrow = clock.GetToday().PlusDays(1);
        if (range.minimumReleaseDate < tomorrow)
        {
            return new(tomorrow, range.maximumReleaseDate);
        }

        return range;
    }

    private static (LocalDate minimumReleaseDate, LocalDate? maximumReleaseDate) ParseEstimatedReleaseDateInner(string estimatedReleaseDate, IClock clock)
    {
        var tomorrow = clock.GetToday().PlusDays(1);
        if (estimatedReleaseDate == "TBA")
        {
            return (tomorrow, null);
        }

        int? yearPart;
        var splitString = estimatedReleaseDate.Split(" ");
        if (splitString.Length == 1)
        {
            yearPart = TryParseYear(splitString.Single());
            if (yearPart.HasValue)
            {
                return (new LocalDate(yearPart.Value, 1, 1), new LocalDate(yearPart.Value, 12, 31));
            }
        }
        else if (splitString.Length == 2)
        {
            yearPart = TryParseYear(splitString.Last());
            bool hasSubYear = _recognizedSubYears.TryGetValue(splitString.First().ToLower(), out var subYearPart);
            if (hasSubYear)
            {
                return (new LocalDate(yearPart.Value, subYearPart.minimumDate.Month, subYearPart.minimumDate.Day),
                    new LocalDate(yearPart.Value, subYearPart.maximumDate.Month, subYearPart.maximumDate.Day));
            }

            return (new LocalDate(yearPart.Value, 1, 1), new LocalDate(yearPart.Value, 12, 31));
        }

        return (tomorrow, null);
    }

    private static int? TryParseYear(string yearPart)
    {
        bool success = int.TryParse(yearPart, out var result);
        if (success && result > 2018 && result < 2100)
        {
            return result;
        }

        return null;
    }

    private static readonly Dictionary<string, (LocalDate minimumDate, LocalDate maximumDate)> _recognizedSubYears =
        new Dictionary<string, (LocalDate minimumDate, LocalDate maximumDate)>()
        {
            {"early", (new LocalDate(1, 1, 1), new LocalDate(1, 6, 30))},
            {"mid", (new LocalDate(1, 3, 1), new LocalDate(1, 10, 31))},
            {"late", (new LocalDate(1, 7, 1), new LocalDate(1, 12, 31))},
            {"q1", (new LocalDate(1, 1, 1), new LocalDate(1, 3, 31))},
            {"q2", (new LocalDate(1, 4, 1), new LocalDate(1, 7, 31))},
            {"q3", (new LocalDate(1, 7, 1), new LocalDate(1, 10, 31))},
            {"q4", (new LocalDate(1, 10, 1), new LocalDate(1, 12, 31))},
            {"winter", (new LocalDate(1, 1, 1), new LocalDate(1, 12, 31))},
            {"spring", (new LocalDate(1, 3, 21), new LocalDate(1, 6, 21))},
            {"summer", (new LocalDate(1, 6, 21), new LocalDate(1, 9, 21))},
            {"fall", (new LocalDate(1, 9, 21), new LocalDate(1, 12, 21))},
            {"january", (new LocalDate(1, 1, 1), new LocalDate(1, 1, 31))},
            {"february", (new LocalDate(1, 2, 1), new LocalDate(1, 2, 28))},
            {"march", (new LocalDate(1, 3, 1), new LocalDate(1, 3, 31))},
            {"april", (new LocalDate(1, 4, 1), new LocalDate(1, 4, 30))},
            {"may", (new LocalDate(1, 5, 1), new LocalDate(1, 5, 31))},
            {"june", (new LocalDate(1, 6, 1), new LocalDate(1, 6, 30))},
            {"july", (new LocalDate(1, 7, 1), new LocalDate(1, 7, 31))},
            {"august", (new LocalDate(1, 8, 1), new LocalDate(1, 8, 31))},
            {"september", (new LocalDate(1, 9, 1), new LocalDate(1, 9, 30))},
            {"october", (new LocalDate(1, 10, 1), new LocalDate(1, 10, 31))},
            {"november", (new LocalDate(1, 11, 1), new LocalDate(1, 11, 30))},
            {"december", (new LocalDate(1, 12, 1), new LocalDate(1, 12, 31))},
        };
}
