namespace FantasyCritic.Lib.Utilities;

public static class ListFunctions
{
    public static IReadOnlyDictionary<TKey, IReadOnlyList<TSource>> SealDictionary<TKey, TSource>(this Dictionary<TKey, List<TSource>> source)
    {
        return source.ToDictionary(x => x.Key, y => (IReadOnlyList<TSource>)y.Value);
    }

    public static IReadOnlyDictionary<TKey, IReadOnlyList<TSource>> GroupToDictionary<TKey, TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        return source.GroupBy(keySelector).ToDictionary(x => x.Key, y => (IReadOnlyList<TSource>)y.ToList());
    }

    public static IReadOnlyDictionary<TKey, TCombinedValue> CombineDictionaries<TKey, TValue1, TValue2, TCombinedValue>(
        IReadOnlyDictionary<TKey, IReadOnlyList<TValue1>> dictionaryOne,
        IReadOnlyDictionary<TKey, IReadOnlyList<TValue2>> dictionaryTwo,
        Func<IEnumerable<TValue1>, IEnumerable<TValue2>, TCombinedValue> combiner)
    {
        Dictionary<TKey, TCombinedValue> combinedDictionary = new Dictionary<TKey, TCombinedValue>();
        var keys = dictionaryOne.Keys.Concat(dictionaryTwo.Keys).Distinct();
        foreach (var key in keys)
        {
            if (!dictionaryOne.TryGetValue(key, out var value1))
            {
                value1 = new List<TValue1>();
            }
            if (!dictionaryTwo.TryGetValue(key, out var value2))
            {
                value2 = new List<TValue2>();
            }
            combinedDictionary[key] = combiner.Invoke(value1, value2);
        }

        return combinedDictionary;
    }
}