using System.Linq;
using System.Configuration;

namespace PluginHost.App.Configuration.Elements
{
    using PluginHost.Extensions.Collections;
    using PluginHost.Interface.Configuration.Types;

    [ConfigurationCollection(typeof(LoggerElement), AddItemName = "logger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class LoggersElement : ConfigurationElementCollection, ILoggersConfiguration
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "logger"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LoggerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as LoggerElement).Name;
        }

        public LoggerElement this[int index]
        {
            get { return (LoggerElement) base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new LoggerElement this[string name]
        {
            get { return (LoggerElement) base.BaseGet(name); }
        }

        public new System.Collections.Generic.IEnumerator<ILoggerConfiguration> GetEnumerator()
        {
            return EnumeratorFactory.Create<ILoggerConfiguration>(base.GetEnumerator());
        }
    }
}
