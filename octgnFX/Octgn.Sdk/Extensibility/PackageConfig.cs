using System;
using System.Collections.Generic;
using Octgn.Sdk.Packaging;

namespace Octgn.Sdk.Extensibility
{
    public class PackageConfig
    {
        public IPackage Package { get; }

        public HashSet<Type> Plugins { get; } = new HashSet<Type>();

        public PackageConfig(IPackage package) {
            Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public void Register<T>() where T : IPlugin {
            Plugins.Add(typeof(T));
        }
    }
}
