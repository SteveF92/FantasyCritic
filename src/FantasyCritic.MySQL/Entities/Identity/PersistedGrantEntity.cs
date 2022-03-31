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

    public string Key { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string SubjectId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? ConsumedTime { get; set; }
    public DateTime? Expiration { get; set; }
    public string Data { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string SessionId { get; set; } = null!;

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
