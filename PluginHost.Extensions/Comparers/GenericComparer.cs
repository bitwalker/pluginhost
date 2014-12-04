using System;
using System.Collections;
using System.Collections.Generic;

namespace PluginHost.Extensions.Comparers
{
    /// <summary>
    /// Factory class for GenericComparers.
    /// </summary>
    /// <typeparam name="T">The type to create a GenericComparer for</typeparam>
    public static class GenericComparer<T>
    {
        /// <summary>
        /// Creates a new GenericComparer for type T, using the value extracted
        /// via the provided extractor Func. You can optionally define whether
        /// to compare in ascending (default), or descending (reverse) order.
        /// </summary>
        /// <typeparam name="U">The type of the value used in the comparison</typeparam>
        /// <param name="extractor">An expression defining how to extract the comparison value from each instance of T</param>
        /// <param name="inAscendingOrder">True if this comparer will compare in ascending (normal) or descending (reverse) order.</param>
        /// <returns>A strongly-typed GenericComparer</returns>
        public static GenericComparer<T, U> Create<U>(Func<T, U> extractor, bool inAscendingOrder = true)
            where U : IComparable, IComparable<U>
        {
            return new GenericComparer<T, U>(extractor, inAscendingOrder);
        }
    }

    /// <summary>
    /// Defines a comparer for some type T, given an expression defining
    /// how to compare two instances of T. See the examples for more detail.
    /// </summary>
    /// <example>
    /// Suppose you have a type called Person, containing a FirstName and LastName.
    /// Out of the box, C# does not know how to compare two instances of Person, so how
    /// then do you sort a list of Persons? You could implement the <see cref="IComparer"/>
    /// or <see cref="IComparer{T}"/> interfaces, but that's tedious if you have many such
    /// types, or even worse, if you want to sort Persons in more than one way!
    ///
    /// GenericComparer does two things for you: First, it allows you to create an <see cref="IComparer"/>
    /// / <see cref="IComparer{T}"/> for any type you wish on the fly, even those which already implement those
    /// interfaces. You do so by providing an expression which defines how to get the value for the
    /// comparison. Secondly, it allows you to easily create multiple comparers over the same type, as
    /// well as control the direction of the comparison (in addition to the standard ascending order,
    /// you can specify descending order as well, i.e. 10 comes before 5 instead of 5 before 10)
    ///
    /// To see this in action, we'll use the Person example from up above. I want to sort a collection of Persons
    /// in reverse (descending) order, and I want to sort the list based on their full name, a property which does
    /// not exist, but which can be expressed easily enough. All I have to do is the following:
    ///
    ///     Func{Person, string} fullName = (p) => string.Format("{0} {1}", p.FirstName, p.LastName);
    ///     var byDescendingFullName = GenericComparer{Person}.Create(fullName, false);
    ///     var people = new [] {
    ///         new Person("Andrew Smith"),
    ///         new Person("Paul Schoenfelder"),
    ///         new Person("Paul Anderson")
    ///     };
    ///     people.Sort(byDescendingFullName);
    ///     var expected = new [] { "Paul Schoenfelder", "Paul Anderson", "Andrew Smith" };
    ///     Assert.IsTrue(expected.SequenceEqual(people.Select(fullName)));
    ///
    /// Used in combination with MultiComparer{T}, you can get an immense amount of power over how you
    /// sort collections!
    /// </example>
    /// <typeparam name="T">The type to compare</typeparam>
    /// <typeparam name="U">The type of the value used to perform the comparison</typeparam>
    public sealed class GenericComparer<T, U> : IComparer<T>, IComparer, IEqualityComparer<T>
        where U : IComparable, IComparable<U>
    {
        private readonly Func<T, U> _extractor;
        private readonly bool _inAscendingOrder = true;

        /// <summary>
        /// Creates a new GenericComparer
        /// </summary>
        /// <param name="extractor">
        /// An expression which extracts the value to use for comparing instances of type <see cref="T"/>.
        /// </param>
        /// <param name="inAscendingOrder">
        /// Determines whether to use the default behavior when performing comparisons (ascending order)
        /// Defaults to true (sort ascending) if not overridden.
        /// </param>
        public GenericComparer(Func<T, U> extractor, bool inAscendingOrder = true)
        {
            _inAscendingOrder = inAscendingOrder;
            _extractor = extractor;
        }

        public int Compare(T x, T y)
        {
            var valX = _extractor(x);
            var valY = _extractor(y);

            var result = valX.CompareTo(valY);
            if (_inAscendingOrder)
                return result;
            else
                return result * -1;
        }

        int IComparer.Compare(object x, object y)
        {
            if ((x is T) == false)
                throw new ArgumentException(string.Format("x is not an instance of type {0}", typeof(T).Name));
            if ((y is T) == false)
                throw new ArgumentException(string.Format("y is not an instance of type {0}", typeof(T).Name));

            return Compare((T) x, (T) y);
        }

        public bool Equals(T x, T y)
        {
            var valX = _extractor(x);
            var valY = _extractor(y);

            return valX.Equals(valY);
        }

        public int GetHashCode(T obj)
        {
            var val = _extractor(obj);
            if (val == null)
                return 0;

            return val.GetHashCode();
        }
    }
}
