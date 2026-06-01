using System.Text.Json;

namespace FantasyCritic.Lib.SharedSerialization.Database;

public class MasterGameTagEntity
{
    public MasterGameTagEntity()
    {

    }

    public MasterGameTagEntity(MasterGameTag domain)
    {
        Name = domain.Name;
        ReadableName = domain.ReadableName;
        ShortName = domain.ShortName;
        TagType = domain.TagType.Name;
        HasCustomCode = domain.HasCustomCode;
        SystemTagOnly = domain.SystemTagOnly;
        Description = domain.Description;
        Examples = JsonSerializer.Serialize(domain.Examples);
        BadgeColor = domain.BadgeColor;
    }

    public string Name { get; set; } = null!;
    public string ReadableName { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string TagType { get; set; } = null!;
    public bool HasCustomCode { get; set; }
    public bool SystemTagOnly { get; set; }
    public string Description { get; set; } = null!;
    public string Examples { get; set; } = null!;
    public string BadgeColor { get; set; } = null!;

    public MasterGameTag ToDomain()
    {
        var examples = JsonSerializer.Deserialize<List<string>>(Examples)!;
        return new MasterGameTag(Name, ReadableName, ShortName, new MasterGameTagType(TagType), HasCustomCode, SystemTagOnly, Description, examples, BadgeColor);
    }
}
