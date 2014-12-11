using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace PluginHost.Extensions.Reflection
{
    public class DynamicExtensions
    {
        public static T Proxy<T>(this dynamic obj)
        {
            dynamic result = new Proxy<T>(obj);
            return (T) result;
        }
    }

    public class Proxy<T> : DynamicObject
    {
        private readonly dynamic _proxied;
        public Proxy(dynamic target)
        {
            _proxied = target;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            Type bindingType = binder.Type;
            if (bindingType.IsInstanceOfType(typeof(T)))
            {
                result = _proxied;
                return true;
            }

            result = null;
            return false;
        }
    }
}
