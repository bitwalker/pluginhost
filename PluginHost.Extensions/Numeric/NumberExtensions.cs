using System;

namespace PluginHost.Extensions.Numeric
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Attempts to convert a string to an integer.
        /// Returns a nullable int. Should only be null if the string
        /// does not contain a number which can be converted to an int.
        /// </summary>
        /// <param name="numString">The string to parse for a number.</param>
        /// <returns>int?</returns>
        public static int? SafeConvertInteger(this string numString)
        {
            int result;
            var valid = Int32.TryParse(numString, out result);
            if (valid)
                return result;
            else
                return null;
        }
    }
}
