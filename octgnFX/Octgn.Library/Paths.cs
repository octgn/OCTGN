using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Library
{
    using System.IO.Abstractions;
    using System.Reflection;

    using Octgn.Data;

    public static class Paths
    {
        internal static IFileSystem FS { get; set; }
        public static string BasePath {get;private set;}
        public static string PluginPath { get; private set; }
        public static string DataDirectory { get; private set; }
        public static string DatabasePath { get; set; }

        static Paths()
        {
            if(FS == null)
                FS = new FileSystem();
            BasePath = FS.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
            PluginPath = FS.Path.Combine(SimpleConfig.DataDirectory, "Plugins");
            DatabasePath = FS.Path.Combine(SimpleConfig.DataDirectory, "Database");
            DatabasePath = FS.Path.Combine(DatabasePath, "master.db3");
            DataDirectory = SimpleConfig.DataDirectory;
        }
    }
}
