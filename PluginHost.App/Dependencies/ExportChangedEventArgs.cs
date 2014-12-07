using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;

namespace PluginHost.App.Dependencies
{
    /// <summary>
    /// Defines the metadata for when new or removed exports are
    /// detected at runtime. Currently used as the event args type
    /// for the DependencyInjector.ExportChanged event stream.
    /// </summary>
    public struct ExportChangedEventArgs
    {
        public ExportChangeType Type { get; set; }
        public string ContractName { get; set; }
        public IDictionary<string, object> Metadata { get; set; }

        public static ExportChangedEventArgs Added(ExportDefinition export)
        {
            return new ExportChangedEventArgs()
            {
                Type         = ExportChangeType.Added,
                ContractName = export.ContractName,
                Metadata     = export.Metadata
            };
        }

        public static ExportChangedEventArgs Removed(ExportDefinition export)
        {
            return new ExportChangedEventArgs()
            {
                Type         = ExportChangeType.Removed,
                ContractName = export.ContractName,
                Metadata     = export.Metadata
            };
        }
    }
}
