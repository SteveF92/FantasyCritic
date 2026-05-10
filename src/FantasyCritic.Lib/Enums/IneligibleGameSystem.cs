namespace FantasyCritic.Lib.Enums;

public class IneligibleGameSystem : TypeSafeEnum<IneligibleGameSystem>
{
    // Define values here.
    public static readonly IneligibleGameSystem CaseByCase = new IneligibleGameSystem("CaseByCase", "Case by Case Basis");
    public static readonly IneligibleGameSystem DroppableAsWillNotRelease = new IneligibleGameSystem("DroppableAsWillNotRelease", "Droppable as Will Not Release");
    public static readonly IneligibleGameSystem DroppableAsWillRelease = new IneligibleGameSystem("DroppableAsWillRelease", "Droppable as Will Release");
    public static readonly IneligibleGameSystem NotDroppable = new IneligibleGameSystem("NotDroppable", "Not Droppable");

    // Constructor is private: values are defined within this class only!
    private IneligibleGameSystem(string value, string readableName)
        : base(value)
    {
        ReadableName = readableName;
    }

    public string ReadableName { get; }
    public override string ToString() => Value;
}
