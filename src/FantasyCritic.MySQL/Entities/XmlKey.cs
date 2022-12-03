namespace FantasyCritic.MySQL.Entities;
public class XmlKey
{
    public Guid Id { get; set; }
    public required string FriendlyName { get; init; }
    public required string Xml { get; init; }
}
