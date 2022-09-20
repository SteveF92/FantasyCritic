
namespace FantasyCritic.Lib.Utilities;
public static class DictionaryFunctions
{
    public static TValue? GetValueOrDefaultNullable<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey? key) where TKey : struct, IEquatable<TKey>
    {
        if (key is null)
        {
            return default;
        }
        
        return dictionary.GetValueOrDefault(key.Value);
    }

    public static TValue? GetValueOrDefaultNullable<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey? key) where TKey : class, IEquatable<TKey>
    {
        if (key is null)
        {
            return default;
        }

        return dictionary.GetValueOrDefault(key);
    }
}
