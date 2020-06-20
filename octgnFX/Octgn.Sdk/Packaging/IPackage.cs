using System;
using System.Collections.Generic;

namespace Octgn.Sdk.Packaging
{
    /// <summary>
    /// Package containing <see cref="IPlugin"/>'s for Octgn.
    /// </summary>
    public interface IPackage
    {
        string Id { get; }

        string Name { get; }

        string Description { get; }

        string Website { get; }

        string Icon { get; }

        Version Version { get; }

        Version OctgnVersion { get; }

        IList<IDependency> Dependencies { get; }

        IList<IPlugin> Plugins { get; }
    }
}
