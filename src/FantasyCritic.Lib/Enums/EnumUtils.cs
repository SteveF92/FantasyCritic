using System.Reflection;

namespace FantasyCritic.Lib.Enums;
public static class EnumUtils
{
    public static IReadOnlyList<TEnum> GetAllPossibleValues<TEnum>()
    {
        Type enumType = typeof(TEnum);
        var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
        var readonlyFields = fields.Where(x => x.IsInitOnly);
        var returnsEnumType = readonlyFields.Where(x => x.FieldType == enumType);
        List<TEnum> enumValues = returnsEnumType.Select(enumField => (TEnum)enumField.GetValue(null)!).ToList();
        return enumValues;
    }
}
