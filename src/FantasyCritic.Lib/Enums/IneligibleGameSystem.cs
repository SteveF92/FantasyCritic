namespace FantasyCritic.Lib.Enums;

public class IneligibleGameSystem : TypeSafeEnum<IneligibleGameSystem>
{
    //Case by Case and Not Droppable are both not droppable in practice
    //Case by Case is just supposed to prompt discussion
    public static readonly IneligibleGameSystem CaseByCase = new IneligibleGameSystem("CaseByCase", "Case by Case Basis", false);
    public static readonly IneligibleGameSystem NotDroppable = new IneligibleGameSystem("NotDroppable", "Not Droppable", false);
    public static readonly IneligibleGameSystem DroppableAsWillNotRelease = new IneligibleGameSystem("DroppableAsWillNotRelease", "Droppable as Will Not Release", true);
    public static readonly IneligibleGameSystem FreelyDroppable = new IneligibleGameSystem("FreelyDroppable", "Freely Droppable", true);

    // Constructor is private: values are defined within this class only!
    private IneligibleGameSystem(string value, string readableName, bool allowsDrops)
        : base(value)
    {
        ReadableName = readableName;
        AllowsDrops = allowsDrops;
    }

    public string ReadableName { get; }
    public bool AllowsDrops { get; }
    public override string ToString() => Value;
}
