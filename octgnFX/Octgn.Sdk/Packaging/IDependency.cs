using System;

namespace Octgn.Sdk.Packaging
{
    public interface IDependency
    {
        string Dependency { get; }

        string Name { get; }

        Version Version { get; }
    }
}
