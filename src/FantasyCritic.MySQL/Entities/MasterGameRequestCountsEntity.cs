namespace FantasyCritic.MySQL.Entities;

public class MasterGameRequestCountsEntity
{
    public int MasterGameRequestCount { get; set; }
    public int MasterGameChangeRequestCount { get; set; }

    public MasterGameRequestCounts ToDomain()
    {
        return new MasterGameRequestCounts(MasterGameRequestCount, MasterGameChangeRequestCount);
    }
}
