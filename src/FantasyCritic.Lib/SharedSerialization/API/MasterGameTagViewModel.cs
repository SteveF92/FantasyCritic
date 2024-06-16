namespace FantasyCritic.Lib.SharedSerialization.API;

public class MasterGameTagViewModel
{
    public MasterGameTagViewModel()
    {
        
    }

    public MasterGameTagViewModel(MasterGameTag domain)
    {
        Name = domain.Name;
        ReadableName = domain.ReadableName;
        ShortName = domain.ShortName;
        TagType = domain.TagType.Name;
        Description = domain.Description;
        Examples = domain.Examples;
        BadgeColor = domain.BadgeColor;
        HasCustomCode = domain.HasCustomCode;
        SystemTagOnly = domain.SystemTagOnly;
    }

    public string Name { get; init; } = null!;
    public string ReadableName { get; init; } = null!;
    public string ShortName { get; init; } = null!;
    public string TagType { get; init; } = null!;
    public string Description { get; init; } = null!;
    public IReadOnlyList<string> Examples { get; init; } = null!;
    public string BadgeColor { get; init; } = null!;
    public bool HasCustomCode { get; init; }
    public bool SystemTagOnly { get; init; }

    public MasterGameTag ToDomain()
    {
        return new MasterGameTag(Name, ReadableName, ShortName, new MasterGameTagType(TagType), HasCustomCode, SystemTagOnly, Description, Examples, BadgeColor);
    }
}
