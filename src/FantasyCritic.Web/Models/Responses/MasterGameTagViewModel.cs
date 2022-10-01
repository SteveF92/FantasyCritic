namespace FantasyCritic.Web.Models.Responses;

public class MasterGameTagViewModel
{
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

    public string Name { get; }
    public string ReadableName { get; }
    public string ShortName { get; }
    public string TagType { get; }
    public string Description { get; }
    public IReadOnlyList<string> Examples { get; }
    public string BadgeColor { get; }
    public bool HasCustomCode { get; }
    public bool SystemTagOnly { get; }
}
