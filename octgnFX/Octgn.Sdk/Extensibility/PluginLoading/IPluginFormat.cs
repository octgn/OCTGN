using System;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    public interface IPluginFormat
    {
        string Id { get; }

        IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType);
    }
}
