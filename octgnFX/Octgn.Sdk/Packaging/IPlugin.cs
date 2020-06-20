using System;

namespace Octgn.Sdk.Packaging
{
    public interface IPlugin
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        string Icon { get; }

        string Path { get; }
    }
}
