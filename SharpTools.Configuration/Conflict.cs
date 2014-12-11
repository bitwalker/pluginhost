using System.Reflection;

namespace SharpTools.Configuration
{
    public struct Conflict
    {
        public PropertyInfo TargetProperty { get; set; }
        public PropertyInfo SourceProperty { get; set; }
        public object TargetValue { get; set; }
        public object SourceValue { get; set; }
    }
}
