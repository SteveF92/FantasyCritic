using Duende.IdentityServer.Models;

namespace FantasyCritic.MySQL.Entities.Identity;

internal class PersistedGrantEntity
{
    public PersistedGrantEntity()
    {

    }

    public PersistedGrantEntity(PersistedGrant domain)
    {
        Key = domain.Key;
        Type = domain.Type;
        SubjectId = domain.SubjectId;
        ClientId = domain.ClientId;
        CreationTime = domain.CreationTime;
        ConsumedTime = domain.ConsumedTime;
        Expiration = domain.Expiration;
        Data = domain.Data;
        Description = domain.Description;
        SessionId = domain.SessionId;
    }

    public string Key { get; set; }
    public string Type { get; set; }
    public string SubjectId { get; set; }
    public string ClientId { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? ConsumedTime { get; set; }
    public DateTime? Expiration { get; set; }
    public string Data { get; set; }
    public string Description { get; set; }
    public string SessionId { get; set; }

    public PersistedGrant ToDomain()
    {
        return new PersistedGrant()
        {
            Key = Key,
            Type = Type,
            ClientId = ClientId,
            ConsumedTime = ConsumedTime,
            CreationTime = CreationTime,
            Data = Data,
            Description = Description,
            Expiration = Expiration,
            SessionId = SessionId,
            SubjectId = SubjectId
        };
    }
}