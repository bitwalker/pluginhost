using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PluginHost.Extensions.Text
{
    public static class StringExtensions
    {
        /// <summary>
        /// Determine if two strings are equal, optionally ignoring casing.
        /// The invariant culture is used for comparisons.
        /// </summary>
        /// <param name="s">A string</param>
        /// <param name="s2">The string to compare with</param>
        /// <param name="ignoreCase">True to ignore casing in the two strings during comparison</param>
        /// <returns></returns>
        public static bool IsEqualTo(this string s, string s2, bool ignoreCase = true)
        {
            var comparison = ignoreCase
                ? StringComparison.InvariantCultureIgnoreCase
                : StringComparison.InvariantCulture;
            return String.Compare(s, s2, comparison) == 0;
        }

        /// <summary>
        /// Returns true if the string starts with any of the strings passed in the array.
        /// </summary>
        /// <param name="s">the string to test</param>
        /// <param name="stringsItCouldStartWith">Array of strings to test for .StartsWith</param>
        /// <returns>true if the string starts with any item passed in the array.</returns>
        public static bool StartsWithAny(this string s, IEnumerable<string> stringsItCouldStartWith)
        {
            var result = false;
            var listOfItemsToTest = new List<string>(stringsItCouldStartWith);
            var i = 0;
            while (!result && i < listOfItemsToTest.Count)
            {
                result = s.StartsWith(listOfItemsToTest[i]);
                i++;
            }

            return result;
        }

        public static bool Contains(this string s, string compareValue, StringComparison comparisonMethod)
        {
            return s.IndexOf(compareValue, comparisonMethod) > -1;
        }

        /// <summary>
        /// Will collapse multiple spaces into one, and trim any trailing whitespace
        /// </summary>
        public static string CollapseAndTrim(this string s)
        {
            return Regex.Replace(s, @"\s+", " ").Trim();
        }

        public static string EnsurePostfix(this string s, string post)
        {
            if (string.IsNullOrWhiteSpace(s) || string.IsNullOrWhiteSpace(post)) return s;
            return s.EndsWith(post) ? s : s + post;
        }

        /// <summary>
        /// Ensures a given string has a specific prefix.
        /// Will check if already exists, if not, add it.
        /// </summary>
        /// <param name="s">String to check prefix on</param>
        /// <param name="prefix">Prefix to ensure is there</param>
        /// <returns>string with prefix</returns>
        public static string EnsurePrefix(this string s, string prefix)
        {
            if (String.IsNullOrWhiteSpace(s)) return s;
            if (!s.StartsWith(prefix))
            {
                s = prefix + s;
            }
            return s;
        }

        /// <summary>
        /// Returns the string split into individual lines.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <returns>An array of all the lines in the string.</returns>
        public static string[] Lines(this string s)
        {
            return s.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Wraps the string at the given column index.
        /// </summary>
        /// <param name="s">The string to process.</param>
        /// <param name="column">The column at which to wrap the string.</param>
        /// <returns>A stream of strings representing the wrapped lines. String.Length is &lt;= column.</returns>
        public static IEnumerable<string> WordWrapAt(this string s, int column)
        {
            char[] whitespaceCharacters = { ' ', '\n', '\t', '\r' };

            foreach (var line in s.Lines())
            {
                if (line.Length <= column)
                {
                    yield return line;
                }
                else
                {
                    // the line is longer than the allowed column width, so we start
                    // at the column index, then search backwards for the last whitespace
                    // char; we return that string, then advance the marker and repeat
                    // if there is no whitespace char, then just return the whole string
                    int mark = 0;
                    for (int i = line.LastIndexOfAny(whitespaceCharacters, column);
                    i < line.Length && i >= 0 && mark < i;
                    mark = i, i = line.LastIndexOfAny(whitespaceCharacters, Math.Min(i + column - 1, line.Length - 1)))
                    {
                        yield return line.Substring(mark, i - mark);
                    }
                    if (mark < line.Length)
                    {
                        yield return line.Substring(mark, line.Length - mark);
                    }
                }
            }
        }

        public static string ValueOrDefault(this string s, string @default)
        {
            return string.IsNullOrWhiteSpace(s) ? @default : s;
        }

        /// <summary>
        /// Duplicates a given string `count` many times.
        /// </summary>
        public static string Duplicate(this string s, int count)
        {
            return String.Join("", Enumerable.Range(0, count).Select(i => s).ToArray());
        }

        public static string Indent(this string s, int tabs)
        {
            var lines = s.Lines();
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendFormat("{0}{1}{2}", Duplicate("\t", tabs), line, Environment.NewLine);
            }
            return sb.ToString();
        }

        public static string Inject(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static string StripHtml(this string s)
        {
            var pattern = @"(<[^>]+/?>)|(<[^>]+>[^<]*</[^>]+>)";
            var stripHtml = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Multiline);

            return stripHtml.Replace(s, "");
        }

        public static string ToUrl(this string s)
        {
            var bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(s);
            s = System.Text.Encoding.ASCII.GetString(bytes);
            s = s.ToLower();
            s = Regex.Replace(s, @"[^a-z0-9\s-]", ""); // Remove all non valid chars          
            s = Regex.Replace(s, @"\s+", " ").Trim(); // convert multiple spaces into one space  
            s = Regex.Replace(s, @"\s", "-"); // Replace spaces by dashes
            return s;
        }

        public static string Capitalize(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            var first = Convert.ToInt32(s[0]);
            char upper;
            if (first >= 97 && first <= 122)
                upper = Convert.ToChar(first - 32);
            else
                upper = s[0];

            return string.Format("{0}{1}", upper, s.Substring(1, s.Length - 1));
        }

        /// <summary>
        /// Extract a string slice from a StringBuilder. This can be useful to check for
        /// trailing characters, etc.
        /// </summary>
        /// <param name="s">The StringBuilder to slice from.</param>
        /// <param name="startIndex">The index to start slicing from.</param>
        /// <param name="length">
        /// The length of the slice. If no value is given, or the value is negative, 
        /// the entire string starting from the startIndex will be returned.
        /// </param>
        public static string Slice(this StringBuilder s, int startIndex, int length = -1)
        {
            bool infinite = length < 0 || length == s.Length;

            // We're asking for the whole string
            if (startIndex <= 0 && infinite)
                return s.ToString();
            // There is no string to slice, return empty
            if (startIndex > s.Length - 1)
                return string.Empty;

            // Handle infinite slices where the slice is less than 500 characters.
            // 500 characters is roughly where StringBuilder performance begins to
            // overtake raw string concatenation.
            var str = "";
            if (infinite && s.Length < 500)
            {
                for (var i = startIndex; i < s.Length; i++)
                    str += s[i];
                return str;
            }

            var sb = new StringBuilder(string.Empty);
            // Since this is an infinite slice, we don't need the extra
            // conditinal branching in the inner loop.
            if (infinite)
            {
                for (var i = startIndex; i < s.Length; i++)
                {
                    sb.Append(s[i]);
                }
                return sb.ToString();
            }
            // Loop until we've collected the entire slice requested.
            else
            {
                var chars = 0;
                for (var i = startIndex; i < s.Length; i++)
                {
                    if (chars < length)
                    {
                        sb.Append(s[i]);
                        chars++;
                        continue;
                    }

                    break;
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Remove occurrances of a given trailing string from the current StringBuilder.
        /// </summary>
        /// <param name="s">The current StringBuilder</param>
        /// <param name="trailing">The string to remove (if it exists)</param>
        public static StringBuilder RemoveTrailing(this StringBuilder s, string trailing)
        {
            if (string.IsNullOrWhiteSpace(trailing))
                return s;

            var startIndex = s.Length - trailing.Length;
            if (startIndex < 0)
                return s;

            var length = trailing.Length;
            if (s.Slice(startIndex, length).Equals(trailing))
            {
                return s.Remove(startIndex, length);
            }
            else return s;
        }
    }
}