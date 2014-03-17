using Octgn.Core.Plugin;

namespace Octgn.Library.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.IO.Abstractions;
    using System.Management.Instrumentation;
    using System.Reflection;

    using log4net;

    internal static class PluginManager
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static IFileSystem FS { get; set; }
        internal static Dictionary<string, AppDomain> PluginDomains { get; set; }

        static PluginManager()
        {
            if (FS == null)
                FS = new FileSystem();
            PluginDomains = new Dictionary<string, AppDomain>();
        }

        internal static IQueryable<T> GetPlugins<T>()
        {
            if (!FS.Directory.Exists(Paths.Get().PluginPath)) FS.Directory.CreateDirectory(Paths.Get().PluginPath);

            var folder = FS.DirectoryInfo.FromDirectoryName(Paths.Get().PluginPath);

            var ret = new List<T>();
            foreach (var f in folder.GetDirectories().SelectMany(dir => dir.GetFiles("*.dll", SearchOption.TopDirectoryOnly)))
            {
                try
                {
                    ret.Add(LoadExtension<T>(f.FullName));
                }
                catch (Exception e)
                {
                    Log.Warn("Problem loading plugin " + f.FullName, e);
                }
            }
            // Load all plugins built into OCTGN
            foreach (var e in Assembly.GetEntryAssembly().GetTypes().Where(t => t.GetInterfaces().Any(i => i == typeof(T))))
                ret.Add((T)Activator.CreateInstance(e));
            return ret.AsQueryable();
        }

        internal static T LoadExtension<T>(string path)
        {
            //var pc = new PluginContainer(path);
            var loadedHotAss = Assembly.LoadFile(path);
            var assTypes = loadedHotAss.GetTypes();
            foreach (var t in assTypes.Where(t => t.GetInterfaces().Any(i => i == typeof(T))))
                return (T)Activator.CreateInstance(t);
            throw new InstanceNotFoundException(String.Format("Instance of the plugin type {0} was not found in the file {1}", typeof(T).Name, path));
        }

        internal static AppDomain GetOrCreate(string path)
        {
            if (!PluginDomains.ContainsKey(path))
                PluginDomains[path] = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            return PluginDomains[path];
        }
    }
}
