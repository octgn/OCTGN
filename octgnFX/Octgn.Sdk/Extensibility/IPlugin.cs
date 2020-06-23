using Octgn.Sdk.Packaging;
using System;
using System.Threading.Tasks;

namespace Octgn.Sdk.Extensibility
{
    public interface IPlugin
    {
        IPackage Package { get; }

        Task OnStart();
    }

    public class Plugin : IPlugin
    {
        public IPackage Package { get; }

        public Plugin(IPackage package) {
            Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public virtual Task OnStart() {
            return Task.CompletedTask;
        }
    }

    public interface IPackageExtension
    {
        void Config(PackageConfig config);
    }

    public class PackageExtension : IPackageExtension
    {
        public virtual void Config(PackageConfig config) {

        }
    }
}
