using System;
using System.Collections;
using System.Collections.Generic;

namespace PluginHost.Extensions.Collections
{
    /// <summary>
    /// Provides functions for generating enumerators
    /// </summary>
    public static class EnumeratorFactory
    {
        /// <summary>
        /// Generates a typed enumerator from an IEnumerator instance,
        /// where each element is explicitly casted to the given type.
        /// 
        /// If the actual underlying type of the source enumerator cannot
        /// be converted to T, then an exception will be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source enumerator</typeparam>
        /// <param name="enumerator">The source enumerator</param>
        /// <returns>A strongly-typed IEnumerator</returns>
        public static IEnumerator<T> Create<T>(IEnumerator enumerator)
        {
            return new TypedEnumerator<T>(enumerator);
        }

        /// <summary>
        /// Generates a typed enumerator from an IEnumerator instance,
        /// where each element is converted to T using the provided conversion function.
        /// 
        /// If the conversion function throws an exception, enumeration will fail.
        /// </summary>
        /// <typeparam name="T">The type of the elements produced by the result enumerator</typeparam>
        /// <param name="enumerator">The source enumerator</param>
        /// <param name="converter">
        /// The conversion function which receives each element of the source
        /// enumerator as an object, and returns an object of type T
        /// </param>
        /// <returns>A strongly-typed IEnumerator</returns>
        public static IEnumerator<T> Create<T>(IEnumerator enumerator, Func<object, T> converter)
        {
            return new TypedEnumerator<T>(enumerator, converter);
        }

        /// <summary>
        /// Generates a typed enumerator from an IEnumerator instance,
        /// where each element is converted to T using the provided conversion function.
        /// Enumeration is controlled via the provided stepping function.
        /// 
        /// If either the conversion or stepping functions throw an exception, enumeration will fail.
        /// </summary>
        /// <typeparam name="T">The type of the elements produced by the result enumerator</typeparam>
        /// <param name="enumerator">The source enumerator</param>
        /// <param name="converter">
        /// The conversion function which receives each element of the source
        /// enumerator as an object, and returns an object of type T
        /// </param>
        /// <param name="moveNext">
        /// The stepping function which receives the underyling enumerator, and returns
        /// a boolean which determines whether enumeration should continue.
        /// </param>
        /// <returns>A strongly-typed IEnumerator</returns>
        public static IEnumerator<T> Create<T>(IEnumerator enumerator, Func<object, T> converter, Func<IEnumerator, bool> moveNext)
        {
            return new TypedEnumerator<T>(enumerator, converter, moveNext);
        }
    }
}
