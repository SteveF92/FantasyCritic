namespace FantasyCritic.MySQL.Entities.Identity;

internal class RecoveryCodeEntity
{
    public RecoveryCodeEntity()
    {

    }

    public RecoveryCodeEntity(Guid userID, string recoveryCode)
    {
        UserID = userID;
        RecoveryCode = recoveryCode;
    }

    public Guid UserID { get; set; }
    public string RecoveryCode { get; set; }
}