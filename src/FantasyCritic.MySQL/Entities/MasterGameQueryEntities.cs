namespace FantasyCritic.MySQL.Entities;

internal class LongestTenuredReleasedGameRow
{
    public Guid MasterGameID { get; set; }
    public LocalDate ReleaseDate { get; set; }
}

internal class LongestTenuredDreamsStatsRow
{
    public Guid MasterGameID { get; set; }
    public int DreamsDashed { get; set; }
    public int DreamsRealized { get; set; }
}

internal class LongestTenuredPeakHypeYearRow
{
    public Guid MasterGameID { get; set; }
    public int Year { get; set; }
    public int YearCount { get; set; }
}

internal class LongestTenuredPeakHypeRow
{
    public LongestTenuredPeakHypeRow(int year, int yearCount)
    {
        Year = year;
        YearCount = yearCount;
    }

    public int Year { get; }
    public int YearCount { get; }
}

internal class MasterGameDesireRow
{
    public Guid MasterGameID { get; set; }
    public int DesireFactor { get; set; }
}
