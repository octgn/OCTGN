using Octgn.Sdk.Data;
using Octgn.Sdk.Extensibility.PluginLoading;
using System;
using System.Collections.Generic;

namespace Octgn.Sdk.Extensibility
{
    public class PluginIntegration
    {
        public PluginLoader Loader { get; }

        public IEnumerable<Package> Packages => _packages.Values;

        private readonly Dictionary<string, Package> _packages;

        public PluginIntegration() {
            Loader = new PluginLoader();

            _packages = new Dictionary<string, Package>();
        }

        public void Initialize() {
            Loader.Initialize(this);

            Loader.RegisterPluginFormat(new IntegratedPluginFormat());

            using (var context = new DataContext()) {
                foreach (var packageRecord in context.Packages) {
                    var package = LoadPackage(context, packageRecord);

                    if (_packages.ContainsKey(packageRecord.Id))
                        throw new InvalidOperationException($"Package {packageRecord.Id} was already loaded.");

                    _packages.Add(packageRecord.Id, package);
                }
            }
        }

        private Package LoadPackage(DataContext data, PackageRecord packageRecord) {
            var package = new Package(packageRecord);

            package.Load(data, Loader);

            return package;

        }
    }
}
