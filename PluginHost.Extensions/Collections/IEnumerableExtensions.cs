using System;
using System.Linq;
using System.Collections.Generic;

namespace PluginHost.Extensions.Collections
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Determine if the source collection is null or empty.
        /// </summary>
        /// <param name="this">The collection to check</param>
        public static bool IsEmpty<T>(this IEnumerable<T> @this)
        {
            if (@this == null)
                return true;

            if (!@this.Any())
                return true;

            return false;
        }

        /// <summary>
        /// Filter out elements that match the predicate.
        /// </summary>
        /// <param name="predicate">A boolean function to match elements against.</param>
        public static IEnumerable<T> Reject<T>(this IEnumerable<T> @this, Func<T, bool> predicate)
        {
            return @this.Where(x => false == predicate(x));
        }

        /// <summary>
        /// Returns a shuffled version of the source enumerable.
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> @this)
        {
            var rand = new Random();
            return @this.OrderBy(x => rand.Next());
        }

        /// <summary>
        /// Map an action over each element in a collection
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection</typeparam>
        /// <param name="this">The collection</param>
        /// <param name="func">The mapping function</param>
        /// <returns>The collection</returns>
        public static IEnumerable<T> Map<T>(this IEnumerable<T> @this, Action<T> func)
        {
            var result = @this.ToList();
            foreach (var item in result)
            {
                func(item);
            }

            return result;
        }

        /// <summary>
        /// Map a transformation function over each element in a collection
        /// </summary>
        /// <typeparam name="T">The source type of items in the collection</typeparam>
        /// <typeparam name="U">The target type of items in the collection</typeparam>
        /// <param name="this">The collection</param>
        /// <param name="func">The mapping function</param>
        /// <returns>An IEnumerable of transformed items</returns>
        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> @this, Func<T, U> func)
        {
            foreach (var item in @this)
            {
                yield return func(item);
            }
        }
    }
}
