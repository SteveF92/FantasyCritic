using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Conferences;
internal class ConferenceYearEntity
{
    public ConferenceYearEntity()
    {
        
    }
    
    public ConferenceYearEntity(Conference conference, int year, bool openForDrafting)
    {
        ConferenceID = conference.ConferenceID;
        Year = year;
        OpenForDrafting = openForDrafting;
    }

    public Guid ConferenceID { get; set; }
    public int Year { get; set; }
    public bool OpenForDrafting { get; set; }
    
    public ConferenceYear ToDomain(Conference conference, SupportedYear supportedYear)
    {
        return new ConferenceYear(conference, supportedYear, OpenForDrafting);
    }
}
