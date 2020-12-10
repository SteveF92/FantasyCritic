using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Utilities
{
    public static class ListFunctions
    {
        public static IReadOnlyDictionary<TKey, IReadOnlyList<TSource>> SealDictionary<TKey, TSource>(this Dictionary<TKey, List<TSource>> source)
        {
            return source.ToDictionary(x => x.Key, y => (IReadOnlyList<TSource>)y.Value);
        }
    }
}
