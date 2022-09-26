namespace FantasyCritic.Lib.Enums;

public abstract class TypeSafeEnum : IEquatable<TypeSafeEnum>
{
    protected TypeSafeEnum(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public abstract override string ToString();

    public bool Equals(TypeSafeEnum? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TypeSafeEnum)obj);
    }

    public override int GetHashCode() => Value.GetHashCode();
}

public abstract class TypeSafeEnum<TEnum> : TypeSafeEnum where TEnum : TypeSafeEnum<TEnum>
{
    private static readonly IReadOnlyList<TEnum> _allValues = EnumUtils.GetAllPossibleValues<TEnum>();
    private static readonly IReadOnlyDictionary<string, TEnum> _allComparisons = GetAllPossibleValues().ToDictionary(x => x.Value.Replace(" ", "").ToLower());

    protected TypeSafeEnum(string value) : base(value)
    {

    }

    public static IReadOnlyList<TEnum> GetAllPossibleValues() => _allValues;

    public static TEnum FromValue(string searchName)
    {
        var result = TryFromValue(searchName);
        if (result is null)
        {
            throw new ArgumentException($"Could not match {searchName} to a known value.");
        }
        return result;
    }

    public static TEnum? TryFromValue(string searchName)
    {
        if (string.IsNullOrWhiteSpace(searchName))
        {
            return null;
        }

        string paramString = searchName.Replace(" ", "").ToLower();
        return _allComparisons.GetValueOrDefault(paramString);
    }
}
