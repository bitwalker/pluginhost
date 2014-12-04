using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PluginHost.Extensions.Text
{
    [DebuggerDisplay("( {Token} )")]
    public struct FormattingToken
    {
        public string Token { get; set; }
        public int ArgsIndex { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get { return (EndIndex - StartIndex) + 1; } }
    }

    public static class FormatStringTokenParser
    {
        private const short ZERO = 48;

        public static IEnumerable<FormattingToken> ExtractFormatTokens(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return new FormattingToken[0];

            var tokens = new List<FormattingToken>();

            var insideToken = false;
            var startIndex  = -1;
            var tokenParts  = new char[2] {'0','0'};
            var places      = tokenParts.Length;

            for (var i = 0; i < @this.Length; i++)
            {
                if (insideToken)
                {
                    switch (@this[i])
                    {
                        case '}':
                            // This was just a string with {} in it
                            if (i - startIndex == 1)
                                break;

                            // We've gathered a complete token
                            tokens.Add(new FormattingToken() {
                                StartIndex = startIndex,
                                EndIndex   = i,
                                ArgsIndex  = UnpackInteger(tokenParts),
                                Token      = @this.Substring(startIndex, (i - startIndex) + 1)
                            });
                            break;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            places--;
                            tokenParts[places] = @this[i];
                            continue;
                        default:
                            break;
                    }

                    startIndex    = -1;
                    tokenParts[0] = (char) ZERO;
                    tokenParts[1] = (char) ZERO;
                    places        = tokenParts.Length;
                    insideToken   = false;
                }
                else
                {
                    switch (@this[i])
                    {
                        case '{':
                            startIndex  = i;
                            insideToken = true;
                            continue;
                        default:
                            continue;
                    }
                }
            }

            return tokens;
        }

        /// <summary>
        /// Given a number where it's individual digits are stored as elements in a char array:
        /// (i.e. 105 == new char[] { 1, 0, 5 }). This function extracts the integer value back out,
        /// by iterating over each digit, adding it's value to the accumulator using the formula:
        ///
        ///     value = (digit * 10^(arrayLength - (index + 1)))
        ///
        /// </summary>
        private static int UnpackInteger(this char[] chars)
        {
            if (chars.Length == 0) return 0;

            int result = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                int factor = (int) Math.Pow(10, chars.Length - (i + 1));
                result += (factor * (chars[i] - 48));
            }

            return result;
        }
    }
}
