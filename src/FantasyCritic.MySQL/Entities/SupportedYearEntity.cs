namespace FantasyCritic.MySQL.Entities;

public class SupportedYearEntity
{
    public int Year { get; set; }
    public bool OpenForCreation { get; set; }
    public bool OpenForPlay { get; set; }
    public bool OpenForBetaUsers { get; set; }
    public LocalDate StartDate { get; set; }
    public bool Finished { get; set; }

    public SupportedYear ToDomain()
    {
        return new SupportedYear(Year, OpenForCreation, OpenForPlay, OpenForBetaUsers, StartDate, Finished);
    }
}