namespace FantasyCritic.MySQL.Entities.Identity;

public class FantasyCriticUserHasRoleEntity
{
    public FantasyCriticUserHasRoleEntity()
    {

    }

    public FantasyCriticUserHasRoleEntity(Guid userID, int roleID, bool programmaticallyAssigned)
    {
        UserID = userID;
        RoleID = roleID;
        ProgrammaticallyAssigned = programmaticallyAssigned;
    }

    public Guid UserID { get; set; }
    public int RoleID { get; set; }
    public bool ProgrammaticallyAssigned { get; set; }
}
