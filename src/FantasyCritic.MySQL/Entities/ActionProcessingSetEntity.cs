using FantasyCritic.Lib.Domain.LeagueActions;

namespace FantasyCritic.MySQL.Entities;

public class ActionProcessingSetEntity
{
    public ActionProcessingSetEntity()
    {

    }

    public ActionProcessingSetEntity(FinalizedActionProcessingResults results)
    {
        ProcessSetID = results.ProcessSetID;
        ProcessTime = results.ProcessTime;
        ProcessName = results.ProcessName;
    }

    public Guid ProcessSetID { get; set; }
    public Instant ProcessTime { get; set; }
    public string ProcessName { get; set; }

    public ActionProcessingSetMetadata ToDomain()
    {
        return new ActionProcessingSetMetadata(ProcessSetID, ProcessTime, ProcessName);
    }
}
