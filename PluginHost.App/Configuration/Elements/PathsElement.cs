using System.Configuration;

namespace PluginHost.App.Configuration.Elements
{
    using PluginHost.Interface.Configuration.Types;

    [ConfigurationCollection(typeof(PathElement), AddItemName = "path", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class PathsElement : ConfigurationElementCollection, IPathsConfiguration
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "path"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PathElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as PathElement).Name;
        }

        public PathElement this[int index]
        {
            get { return (PathElement) base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new IPathConfiguration this[string name]
        {
            get { return (PathElement) base.BaseGet(name); }
        }

        /// <summary>
        /// Path where plugins are located.
        /// </summary>
        public IPathConfiguration Plugins
        {
            get { return this["plugins"]; }
        }

        /// <summary>
        /// Path where locally stored files should be kept.
        /// </summary>
        public IPathConfiguration LocalStorage
        {
            get { return this["localStorage"]; }
        }
    }
}
