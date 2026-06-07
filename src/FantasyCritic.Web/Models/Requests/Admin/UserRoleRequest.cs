namespace FantasyCritic.Web.Models.Requests.Admin;

public class UserRoleRequest
{
    public Guid UserID { get; init; }
    public string RoleName { get; init; } = "";
}
