using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyCritic.Lib.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<IEnumerable<T>> SplitList<T>(this IEnumerable<T> originalEnumerable, int batchSize)
        {
            var list = new List<List<T>>();

            List<T> originalList = originalEnumerable.ToList();

            for (int i = 0; i < originalList.Count; i += batchSize)
            {
                list.Add(originalList.GetRange(i, Math.Min(batchSize, originalList.Count - i)));
            }

            return list;
        }

        public static List<List<T>> SplitList<T>(this List<T> originalList, int batchSize)
        {
            var list = new List<List<T>>();
            for (int i = 0; i < originalList.Count; i += batchSize)
            {
                list.Add(originalList.GetRange(i, Math.Min(batchSize, originalList.Count - i)));
            }

            return list;
        }
    }
}
