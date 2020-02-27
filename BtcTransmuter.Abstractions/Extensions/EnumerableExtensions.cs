using System.Collections.Generic;

namespace BtcTransmuter.Abstractions.Extensions
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }
        
        public static void AddOrReplace<TKey, TValue>(
            this IDictionary<TKey, TValue> dico,
            TKey key,
            TValue value)
        {
            if (dico.ContainsKey(key))
                dico[key] = value;
            else
                dico.Add(key, value);
        }
    }
}
