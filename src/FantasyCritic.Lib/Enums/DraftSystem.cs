namespace FantasyCritic.Lib.Enums;

public class DraftSystem : TypeSafeEnum<DraftSystem>
{
    // Define values here.
    public static readonly DraftSystem Manual = new DraftSystem("Manual");
    public static readonly DraftSystem Flexible = new DraftSystem("Flexible");

    // Constructor is private: values are defined within this class only!
    private DraftSystem(string value)
        : base(value)
    {

    }

    public override string ToString() => Value;
}