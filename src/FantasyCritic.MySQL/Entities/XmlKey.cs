namespace FantasyCritic.MySQL.Entities;
public class XmlKey
{
    public Guid Id { get; set; }
    public string FriendlyName { get; set; } = null!;
    public string Xml { get; set; } = null!;
}
