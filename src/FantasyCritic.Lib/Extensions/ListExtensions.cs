namespace FantasyCritic.Lib.Extensions;

public static class ListExtensions
{
    public static bool SequencesContainSameElements<T>(IEnumerable<T> sequenceOne, IEnumerable<T> sequenceTwo)
        where T : IEquatable<T>
    {
        var set1 = new HashSet<T>(sequenceOne);
        var set2 = new HashSet<T>(sequenceTwo);
        return set1.SetEquals(set2);
    }

    public static bool ContainsAllItems<T>(this IEnumerable<T> a, IEnumerable<T> b)
    {
        return !b.Except(a).Any();
    }

    public static IEnumerable<TSource> WhereMin<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : notnull
    {
        var minValue = source.Select(selector).Min();
        return source.Where(item => selector.Invoke(item).Equals(minValue));
    }

    public static IEnumerable<TSource> WhereMax<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) where TKey : notnull
    {
        var minValue = source.Select(selector).Max();
        return source.Where(item => selector.Invoke(item).Equals(minValue));
    }

    public static IEnumerable<TSource> SelectNotNull<TSource>(this IEnumerable<TSource?> source)
    {
        return source.Select(x => x!);
    }
}
