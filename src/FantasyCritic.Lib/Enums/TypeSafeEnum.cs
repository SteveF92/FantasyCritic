using System.Reflection;

namespace FantasyCritic.Lib.Enums;

public abstract class TypeSafeEnum<TEnum> : IEquatable<TypeSafeEnum<TEnum>> where TEnum : TypeSafeEnum<TEnum>
{
    protected TypeSafeEnum(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static IReadOnlyList<TEnum> GetAllPossibleValues()
    {
        Type enumType = typeof(TEnum);
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var readonlyFields = fields.Where(x => x.IsInitOnly);
        var returnsEnumType = readonlyFields.Where(x => x.FieldType == enumType);
        List<TEnum> enumValues = returnsEnumType.Select(enumField => (TEnum)enumField.GetValue(null)).ToList();
        return enumValues;
    }

    public static TEnum FromValue(string searchName)
    {
        foreach (TEnum enumObject in GetAllPossibleValues())
        {
            string enumString = enumObject.Value.Replace(" ", string.Empty).ToLower();
            string paramString = searchName.Replace(" ", string.Empty).ToLower();

            if (enumString == paramString)
            {
                return enumObject;
            }
        }

        var typeName = typeof(TEnum).Name;

        throw new ArgumentException($"The given {typeName} type did not match any known types: {searchName}");
    }
    public abstract override string ToString();

    public bool Equals(TypeSafeEnum<TEnum> other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TypeSafeEnum<TEnum>)obj);
    }

    public override int GetHashCode()
    {
        return (Value != null ? Value.GetHashCode() : 0);
    }
}
