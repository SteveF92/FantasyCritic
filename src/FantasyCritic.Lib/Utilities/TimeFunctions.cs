using FantasyCritic.Lib.Extensions;

namespace FantasyCritic.Lib.Utilities;

public static class TimeFunctions
{
    public static EstimatedReleaseDateRange ParseEstimatedReleaseDate(string estimatedReleaseDate, IClock clock)
    {
        var range = ParseEstimatedReleaseDateInner(estimatedReleaseDate, clock);
        var tomorrow = clock.GetToday().PlusDays(1);
        LocalDate minimumDate = range.MinimumReleaseDate;
        LocalDate? maximumDate = range.MaximumReleaseDate;
        if (range.MinimumReleaseDate < tomorrow)
        {
            minimumDate = tomorrow;
        }

        if (estimatedReleaseDate.EndsWith("Or Later", StringComparison.OrdinalIgnoreCase))
        {
            maximumDate = null;
        }

        return new EstimatedReleaseDateRange(minimumDate, maximumDate);
    }

    private static EstimatedReleaseDateRange ParseEstimatedReleaseDateInner(string estimatedReleaseDate, IClock clock)
    {
        var tomorrow = clock.GetToday().PlusDays(1);
        if (estimatedReleaseDate == "TBA")
        {
            return new EstimatedReleaseDateRange(tomorrow, null);
        }

        var trimOrLater = estimatedReleaseDate.ToLower().TrimStartingFromFirstInstance(" or later");

        int? yearPart;
        var splitString = trimOrLater.Split(" ");
        if (splitString.Length == 1)
        {
            yearPart = TryParseYear(splitString.Single());
            if (yearPart.HasValue)
            {
                return new EstimatedReleaseDateRange(new LocalDate(yearPart.Value, 1, 1), new LocalDate(yearPart.Value, 12, 31));
            }
        }
        else if (splitString.Length == 2)
        {
            yearPart = TryParseYear(splitString.Last());
            if (yearPart.HasValue)
            {
                var recognizedSubYears = GetRecognizedSubYears(yearPart.Value);
                if (recognizedSubYears.TryGetValue(splitString.First().ToLower(), out var subYearPart))
                {
                    return new EstimatedReleaseDateRange(subYearPart.MinimumReleaseDate, subYearPart.MaximumReleaseDate);
                }

                return new EstimatedReleaseDateRange(new LocalDate(yearPart.Value, 1, 1), new LocalDate(yearPart.Value, 12, 31));
            }
        }

        return new EstimatedReleaseDateRange(tomorrow, null);
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

    private static IReadOnlyDictionary<string, EstimatedReleaseDateRange> GetRecognizedSubYears(int year)
    {
        LocalDate endOfFebruary = new LocalDate(year, 2, 28);
        if (CalendarSystem.Iso.IsLeapYear(year))
        {
            endOfFebruary = new LocalDate(year, 2, 29);
        }

        var dictionary = new Dictionary<string, (LocalDate MinimumDate, LocalDate MaximumDate)>()
        {
            {"early", (new LocalDate(year, 1, 1), new LocalDate(year, 6, 30))},
            {"mid", (new LocalDate(year, 3, 1), new LocalDate(year, 10, 31))},
            {"late", (new LocalDate(year, 7, 1), new LocalDate(year, 12, 31))},
            {"q1", (new LocalDate(year, 1, 1), new LocalDate(year, 3, 31))},
            {"q2", (new LocalDate(year, 4, 1), new LocalDate(year, 7, 31))},
            {"q3", (new LocalDate(year, 7, 1), new LocalDate(year, 10, 31))},
            {"q4", (new LocalDate(year, 10, 1), new LocalDate(year, 12, 31))},
            {"winter", (new LocalDate(year, 1, 1), new LocalDate(year, 12, 31))},
            {"spring", (new LocalDate(year, 3, 21), new LocalDate(year, 6, 21))},
            {"summer", (new LocalDate(year, 6, 21), new LocalDate(year, 9, 21))},
            {"fall", (new LocalDate(year, 9, 21), new LocalDate(year, 12, 21))},
            {"january", (new LocalDate(year, 1, 1), new LocalDate(year, 1, 31))},
            {"february", (new LocalDate(year, 2, 1), endOfFebruary)},
            {"march", (new LocalDate(year, 3, 1), new LocalDate(year, 3, 31))},
            {"april", (new LocalDate(year, 4, 1), new LocalDate(year, 4, 30))},
            {"may", (new LocalDate(year, 5, 1), new LocalDate(year, 5, 31))},
            {"june", (new LocalDate(year, 6, 1), new LocalDate(year, 6, 30))},
            {"july", (new LocalDate(year, 7, 1), new LocalDate(year, 7, 31))},
            {"august", (new LocalDate(year, 8, 1), new LocalDate(year, 8, 31))},
            {"september", (new LocalDate(year, 9, 1), new LocalDate(year, 9, 30))},
            {"october", (new LocalDate(year, 10, 1), new LocalDate(year, 10, 31))},
            {"november", (new LocalDate(year, 11, 1), new LocalDate(year, 11, 30))},
            {"december", (new LocalDate(year, 12, 1), new LocalDate(year, 12, 31))},
        };

        var typedDictionary = dictionary.ToDictionary(x => x.Key, y => new EstimatedReleaseDateRange(y.Value.MinimumDate, y.Value.MaximumDate));
        return typedDictionary;
    }
}
