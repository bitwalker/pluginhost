using System;

namespace PluginHost.Extensions.Functional
{
    public static class Pipes
    {
        public static T Pipe<T, U>(this U result, Func<U, T> func)
        {
            return func(result);
        }
    }
}
