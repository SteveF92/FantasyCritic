namespace FantasyCritic.Web.Models.Responses.Combined;

public class AdminTaskCountsViewModel
{
    public AdminTaskCountsViewModel(MasterGameRequestCounts? masterGameRequestCounts, int? supportTicketCount)
    {
        MasterGameRequestCount = masterGameRequestCounts?.MasterGameRequestCount;
        MasterGameChangeRequestCount = masterGameRequestCounts?.MasterGameChangeRequestCount;
        SupportTicketCount = supportTicketCount;
        TotalCount = (MasterGameRequestCount ?? 0) + (MasterGameChangeRequestCount ?? 0) + (SupportTicketCount ?? 0);
    }

    //These counts are null when the current user does not have the role that is responsible for them.
    public int? MasterGameRequestCount { get; }
    public int? MasterGameChangeRequestCount { get; }
    public int? SupportTicketCount { get; }
    public int TotalCount { get; }
}
