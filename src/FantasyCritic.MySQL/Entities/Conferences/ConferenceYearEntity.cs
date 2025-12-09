using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Conferences;
internal class ConferenceYearEntity
{
    public ConferenceYearEntity()
    {
        
    }
    
    public ConferenceYearEntity(Conference conference, int year)
    {
        ConferenceID = conference.ConferenceID;
        Year = year;
    }

    public Guid ConferenceID { get; set; }
    public int Year { get; set; }
    
    public ConferenceYear ToDomain(Conference conference, SupportedYear supportedYear)
    {
        return new ConferenceYear(conference, supportedYear);
    }
}

internal class ConferenceYearKeyWithDetailsEntity
{
    public Guid ConferenceID { get; set; }
    public int Year { get; set; }
    public bool SupportedYearIsFinished { get; set; }
    public bool AtLeastOneDraftIsStarted { get; set; }
}
