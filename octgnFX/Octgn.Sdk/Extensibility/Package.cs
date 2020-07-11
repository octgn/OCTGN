using Octgn.Sdk.Data;
using Octgn.Sdk.Extensibility.PluginLoading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Octgn.Sdk.Extensibility
{
    [DebuggerDisplay("Package({FullName})")]
    public class Package
    {
        public virtual string FullName => Record.Id;

        public PackageRecord Record { get; }

        public IEnumerable<DependencyPackage> Dependencies => _dependencies.Values;
        public IEnumerable<IPlugin> Plugins => _plugins.Values;

        private AssemblyLoadContext _pluginAssemblyContext;

        private readonly Dictionary<string, DependencyPackage> _dependencies;
        private readonly Dictionary<string, IPlugin> _plugins;
        private readonly HashSet<string> _searchPaths = new HashSet<string>();

        public Package(PackageRecord record) {
            Record = record ?? throw new ArgumentNullException(nameof(record));

            _dependencies = new Dictionary<string, DependencyPackage>();
            _plugins = new Dictionary<string, IPlugin>();
        }

        public Assembly LoadAssembly(string assemblyPath) {
            var root = Path.GetDirectoryName(assemblyPath);

            _searchPaths.Add(root);

            var ass = _pluginAssemblyContext.LoadFromAssemblyPath(assemblyPath);

            return ass;
        }

        private Assembly _pluginAssemblyContext_Resolving(AssemblyLoadContext arg1, AssemblyName arg2) {
            foreach (var rootPath in _searchPaths) {
                var path = Path.Combine(rootPath, arg2.Name + ".dll");

                if (File.Exists(path)) {
                    return arg1.LoadFromAssemblyPath(path);
                }

                path = Path.Combine(rootPath, arg2.Name + ".exe");

                if (File.Exists(path)) {
                    return arg1.LoadFromAssemblyPath(path);
                }
            }

            return null;
        }

        public void Load(DataContext dataContext, PluginLoader pluginLoader) {
            //TODO: null checks
            //TODO: make single load
            //TODO: Unload context on dispose
            _pluginAssemblyContext = new AssemblyLoadContext(FullName);
            //TODO: Clean this up
            _pluginAssemblyContext.Resolving += _pluginAssemblyContext_Resolving;

            LoadPluginDepenedencies(dataContext, pluginLoader);

            LoadPlugins(dataContext, pluginLoader);
        }

        private void LoadPluginDepenedencies(DataContext dataContext, PluginLoader pluginLoader) {
            foreach (var dependency in Record.Dependencies()) {
                var dependencyPackageRecord = dataContext.Packages.Find(dependency.Id, dependency.Version);

                var package = new DependencyPackage(dependencyPackageRecord, this);

                package.Load(dataContext, pluginLoader);

                _dependencies.Add(dependencyPackageRecord.Id, package);
            }
        }

        private void LoadPlugins(DataContext dataContext, PluginLoader pluginLoader) {
            var pluginRecords = dataContext.PackagePlugins(Record);

            foreach (var pluginRecord in pluginRecords) {
                LoadPlugin(dataContext, pluginRecord, pluginLoader);
            }
        }

        private void LoadPlugin(DataContext dataContext, PluginRecord pluginRecord, PluginLoader pluginLoader) {
            var rootPath = Path.GetDirectoryName(Record.Path);

            var path = Path.Combine(rootPath, pluginRecord.Path);

            if (!File.Exists(path) && path != "integrated")
                throw new FileNotFoundException($"File {path} not found");

            pluginRecord.Path = path;

            var plugin = pluginLoader.Load(this, pluginRecord);

            if (plugin.Details == null) {
                plugin.Details = pluginRecord;
            }

            plugin.Initialize(this);

            _plugins.Add(pluginRecord.Id, plugin);

            if (plugin is IPluginFormat pluginFormat) {
                pluginLoader.RegisterPluginFormat(pluginFormat);
            }
        }

        public IEnumerable<IPlugin> FindByType(string type, bool searchDependencies) {
            foreach (var plugin in Plugins) {
                if (plugin.Details.Type == type)
                    yield return plugin;
            }

            if (!searchDependencies) yield break;

            foreach (var dependency in Dependencies) {
                foreach (var plugin in dependency.FindByType(type, searchDependencies)) {
                    yield return plugin;
                }
            }
        }
    }
}
