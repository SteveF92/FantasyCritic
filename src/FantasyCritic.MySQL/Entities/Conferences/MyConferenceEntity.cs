using FantasyCritic.Lib.Domain.Conferences;

namespace FantasyCritic.MySQL.Entities.Conferences;

internal class MyConferenceEntity
{
    public MyConferenceEntity()
    {

    }



    public Guid ConferenceID { get; set; }
    public string ConferenceName { get; set; } = null!;
    public Guid ConferenceManagerID { get; set; }
    public string ConferenceManagerDisplayName { get; set; } = null!;
    public bool CustomRulesConference { get; set; }

    public MinimalConference ToDomain(IEnumerable<int> years)
    {
        return new MinimalConference(ConferenceID, ConferenceName, years.ToList(), CustomRulesConference, new VeryMinimalFantasyCriticUser(ConferenceManagerID, ConferenceManagerDisplayName));
    }
}

internal class ConferenceIDYearEntity
{
    public Guid ConferenceID { get; set; }
    public int Year { get; set; }
}
