using System;

namespace Octgn.Sdk.Packaging
{
    public interface IDependency
    {
        string Id { get; }

        string Version { get; }
    }
}
