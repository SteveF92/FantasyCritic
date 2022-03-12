namespace FantasyCritic.Lib.Enums;

public class TagStatus : TypeSafeEnum<TagStatus>
{
    // Define values here.
    public static readonly TagStatus Banned = new TagStatus("Banned");
    public static readonly TagStatus Required = new TagStatus("Required");

    // Constructor is private: values are defined within this class only!
    private TagStatus(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}
