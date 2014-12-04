using System;

namespace PluginHost.Extensions.Enums
{
    public static class EnumExtensions
    {
        public static string GetName<T>(this T enumeration)
        {
            return Enum.GetName(typeof(T), enumeration);
        }

        public static T ToEnum<T>(this string name) where T : struct
        {
            var enumType = typeof(T).Name;
            T enumeration;
            bool result = Enum.TryParse(name, true, out enumeration);
            if (result)
            {
                return enumeration;
            }
            else
            {
                var format = "Invalid {0} name provided: {1}";
                throw new ArgumentException(string.Format(format, enumType, name), "name");
            }
        }
    }
}