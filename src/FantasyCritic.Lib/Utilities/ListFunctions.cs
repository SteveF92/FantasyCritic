namespace FantasyCritic.Lib.Utilities;

public static class ListFunctions
{
    public static IReadOnlyDictionary<TKey, IReadOnlyList<TSource>> SealDictionary<TKey, TSource>(this Dictionary<TKey, List<TSource>> source) where TKey : notnull
    {
        return source.ToDictionary(x => x.Key, y => (IReadOnlyList<TSource>)y.Value);
    }

    public static IReadOnlyDictionary<TKey, IReadOnlyList<TSource>> GroupToDictionary<TKey, TSource>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : notnull
    {
        return source.GroupBy(keySelector).ToDictionary(x => x.Key, y => (IReadOnlyList<TSource>)y.ToList());
    }

    public static IReadOnlyDictionary<TKey, IReadOnlyList<TSource>> GroupToDictionaryAndOrderBy<TKey, TSource, TOrderKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
        Func<TSource, TOrderKey> orderKeySelector)
        where TKey : notnull
        where TOrderKey : notnull
    {
        return source.GroupBy(keySelector).ToDictionary(x => x.Key, y => (IReadOnlyList<TSource>)y.OrderBy(orderKeySelector).ToList());
    }
}
