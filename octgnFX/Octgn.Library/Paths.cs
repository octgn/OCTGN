namespace Octgn.Library
{
    using System;
    using System.IO.Abstractions;
    using System.Reflection;

    public static class Paths
    {
        internal static IFileSystem FS { get; set; }
        internal static string WorkingDirectory { get; set; }
        public static string BasePath {get;private set;}
        public static string PluginPath { get; private set; }
        public static string DataDirectory { get; private set; }
        public static string DatabasePath { get; set; }
        public static string ConfigDirectory { get; set; }
        public static string FeedListPath { get; set; }

        static Paths()
        {
            if(FS == null)
                FS = new FileSystem();
            try
            {
                if (WorkingDirectory == null) 
                    WorkingDirectory = Assembly.GetEntryAssembly().Location;
            }
            catch
            {
            }
            BasePath = FS.Path.GetDirectoryName(WorkingDirectory) + "\\";
            PluginPath = FS.Path.Combine(SimpleConfig.DataDirectory, "Plugins");
            DatabasePath = FS.Path.Combine(SimpleConfig.DataDirectory, "Database");
            DatabasePath = FS.Path.Combine(DatabasePath, "master.db3");
            DataDirectory = SimpleConfig.DataDirectory;
            ConfigDirectory = FS.Path.Combine(SimpleConfig.DataDirectory, "Config");
            FeedListPath = FS.Path.Combine(ConfigDirectory, "feeds.txt");
        }
    }
}
