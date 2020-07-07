using System;

namespace Octgn.Sdk
{
    public interface IPluginDetails
    {
        string Id { get; }

        string Type { get; }

        string Format { get; }

        string Name { get; }

        string Description { get; }

        string Icon { get; }

        string Path { get; }
    }
}
