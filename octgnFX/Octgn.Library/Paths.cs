namespace Octgn.Library
{
    using System;
    using System.IO.Abstractions;
    using System.Reflection;

    public interface IPaths
    {
        string WorkingDirectory { get; set; }
        string BasePath { get; }
        string PluginPath { get; }
        string DataDirectory { get; }
        string DatabasePath { get; }
        string ConfigDirectory { get; }
        string FeedListPath { get; }
        string LocalFeedPath { get; }
        string MainOctgnFeed { get; }
        string DeckPath { get; }
        string GraveyardPath { get; }
    }

    public class Paths : IPaths
    {
        #region Singleton

        internal static IPaths SingletonContext { get; set; }

        private static readonly object PathsSingletonLocker = new object();

        public static IPaths Get()
        {
            lock (PathsSingletonLocker) return SingletonContext ?? (SingletonContext = new Paths());
        }

        internal Paths()
        {
            if (FS == null)
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
            PluginPath = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "Plugins");
            //DatabasePath = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "Database");
            DatabasePath = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "GameDatabase");
            DataDirectory = SimpleConfig.Get().DataDirectory;
            ConfigDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config");
            //ConfigDirectory = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "Config");
            FeedListPath = FS.Path.Combine(ConfigDirectory, "feeds.txt");
            LocalFeedPath = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "LocalFeed");
            FS.Directory.CreateDirectory(LocalFeedPath);
            DeckPath = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "Decks");
            MainOctgnFeed = "http://www.myget.org/F/octgngames/";
        }

        #endregion Singleton

        internal IFileSystem FS { get; set; }
        public string WorkingDirectory { get; set; }
        public string BasePath {get;private set;}
        public string PluginPath { get; private set; }
        public string DataDirectory { get; private set; }
        public string DatabasePath { get; set; }
        public string ConfigDirectory { get; set; }
        public string FeedListPath { get; set; }
        public string LocalFeedPath { get; set; }
        public string MainOctgnFeed { get; set; }
        public string DeckPath { get; set; }
        public string GraveyardPath {get
        {
            var ret = System.IO.Path.Combine(FS.Path.GetTempPath(), "OCTGN", "Graveyard", Guid.NewGuid().ToString());
            return ret;
        }}
    }
}
