using System;
using System.Linq;
using System.Collections.Generic;

namespace PluginHost.Interface.Shell
{
    public abstract class Command : IShellCommand
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        protected Command(string name, string description = "No description.")
        {
            Name = name;
            Description = description;
        }

        public abstract bool CanExecute(ShellInput input);
        public abstract void Execute(params string[] arguments);

        protected void WriteColor(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        protected void WriteColor(ConsoleColor color, string format, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.Write(format, args);
            Console.ResetColor();
        }

        protected static object CoerceArgument(Type requiredType, string inputValue)
        {
            var requiredTypeCode = Type.GetTypeCode(requiredType);
            var exceptionMessage = string.Format(
                "Cannnot coerce the input argument {0} to required type {1}",
                inputValue,
                requiredType.Name
            );

            object result = null;
            switch (requiredTypeCode)
            {
                case TypeCode.String:
                    result = inputValue;
                    break;

                case TypeCode.Int16:
                    short number16;
                    if (Int16.TryParse(inputValue, out number16))
                        result = number16;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Int32:
                    int number32;
                    if (Int32.TryParse(inputValue, out number32))
                        result = number32;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Int64:
                    long number64;
                    if (Int64.TryParse(inputValue, out number64))
                        result = number64;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Boolean:
                    bool trueFalse;
                    if (bool.TryParse(inputValue, out trueFalse))
                        result = trueFalse;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Byte:
                    byte byteValue;
                    if (byte.TryParse(inputValue, out byteValue))
                        result = byteValue;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Char:
                    char charValue;
                    if (char.TryParse(inputValue, out charValue))
                        result = charValue;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.DateTime:
                    DateTime dateValue;
                    if (DateTime.TryParse(inputValue, out dateValue))
                        result = dateValue;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Decimal:
                    decimal decimalValue;
                    if (Decimal.TryParse(inputValue, out decimalValue))
                        result = decimalValue;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Double:
                    double doubleValue;
                    if (Double.TryParse(inputValue, out doubleValue))
                        result = doubleValue;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.Single:
                    Single singleValue;
                    if (Single.TryParse(inputValue, out singleValue))
                        result = singleValue;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.UInt16:
                    UInt16 uInt16Value;
                    if (UInt16.TryParse(inputValue, out uInt16Value))
                        result = uInt16Value;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.UInt32:
                    UInt32 uInt32Value;
                    if (UInt32.TryParse(inputValue, out uInt32Value))
                        result = uInt32Value;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                case TypeCode.UInt64:
                    UInt64 uInt64Value;
                    if (UInt64.TryParse(inputValue, out uInt64Value))
                        result = uInt64Value;
                    else
                        throw new ArgumentException(exceptionMessage);
                    break;

                default:
                    throw new ArgumentException(exceptionMessage);
            }
            return result;
        }
    }
}
