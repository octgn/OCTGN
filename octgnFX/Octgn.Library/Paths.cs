namespace Octgn.Library
{
    using System;
    using System.IO.Abstractions;
    using System.Reflection;
    using System.Threading.Tasks;

    using log4net;

    public interface IPaths
    {
        /// <summary>
        /// The working directory of the executable. NEVER USE THIS IT'S CURRENTLY EVIL!!!
        /// </summary>
        string WorkingDirectory { get; set; }
        /// <summary>
        /// The base path of the executable. Currently the same as WorkingDirectory
        /// </summary>
        string BasePath { get; }
        /// <summary>
        /// The folder just above the OCTGN Install folder
        /// </summary>
        string PluginPath { get; }
        string DataDirectory { get; }
        string DatabasePath { get; }
        string ImageDatabasePath { get; }
        string ConfigDirectory { get; }
        string FeedListPath { get; }
        string LocalFeedPath { get; }
        string MainOctgnFeed { get; }
        string SpoilsFeedPath { get; }
        string CommunityFeedPath { get; }
        string DeckPath { get; }
        string SleevePath { get; }
        string GraveyardPath { get; }
        string UpdatesPath { get; }
        string GameHistoryPath { get; }
    }

    public class Paths : IPaths
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal Paths(string dataDirectory)
        {
            // WARN - Do Not Call Config.Instance from in here!!!
            if (FS == null)
                FS = new FileSystem();
            try
            {
                if (WorkingDirectory == null)
                {
                    if (Assembly.GetEntryAssembly() != null)
                        WorkingDirectory = Assembly.GetEntryAssembly().Location;
                    else
                    {
                        WorkingDirectory = Assembly.GetExecutingAssembly().Location;
                    }
                }
            }
            catch
            {
            }
            BasePath = FS.Path.GetDirectoryName(WorkingDirectory) + "\\";
            DataDirectory = dataDirectory;
            PluginPath = FS.Path.Combine(DataDirectory, "Plugins");
            //DatabasePath = FS.Path.Combine(SimpleConfig.Get().DataDirectory, "Database");
            DatabasePath = FS.Path.Combine(DataDirectory, "GameDatabase");
            ImageDatabasePath = FS.Path.Combine(DataDirectory, "ImageDatabase");

            ConfigDirectory = System.IO.Path.Combine(DataDirectory, "Config");
            FeedListPath = FS.Path.Combine(ConfigDirectory, "feeds.txt");
            LocalFeedPath = FS.Path.Combine(DataDirectory, "LocalFeed");
            FS.Directory.CreateDirectory(LocalFeedPath);
            DeckPath = FS.Path.Combine(DataDirectory, "Decks");
            SleevePath = FS.Path.Combine(DataDirectory, "Sleeves");
            MainOctgnFeed = "https://www.myget.org/F/octgngames/";
            SpoilsFeedPath = "https://www.myget.org/f/thespoils/";
            CommunityFeedPath = "https://www.myget.org/f/octgngamedirectory";
            UpdatesPath = FS.Path.Combine(DataDirectory, "Updates");
            GameHistoryPath = FS.Path.Combine(DataDirectory, "History");

            Task.Factory.StartNew(() =>
            {
                foreach (var prop in this.GetType().GetProperties())
                {
                    Log.InfoFormat("Path {0} = {1}", prop.Name, prop.GetValue(this, null));
                }
            });
        }

        internal IFileSystem FS { get; set; }
        public string WorkingDirectory { get; set; }
        public string BasePath { get; private set; }
        public string PluginPath { get; private set; }
        public string DataDirectory { get; private set; }
        public string DatabasePath { get; set; }
        public string ImageDatabasePath { get; set; }
        public string ConfigDirectory { get; set; }
        public string FeedListPath { get; set; }
        public string LocalFeedPath { get; set; }
        public string SpoilsFeedPath { get; set; }
        public string CommunityFeedPath { get; set; }
        public string MainOctgnFeed { get; set; }
        public string DeckPath { get; set; }
        public string SleevePath { get; set; }

        public string UpdatesPath { get; set; }
        public string GameHistoryPath { get; set; }
        public string GraveyardPath
        {
            get
            {
                var tempPath = FS.Path.GetTempPath();

                var path = FS.Path.Combine(tempPath, "Octgn", "Garbage");

                if (!FS.Directory.Exists(path)) FS.Directory.CreateDirectory(path);

                path = FS.Path.Combine(path, Guid.NewGuid().ToString());

                return path;
            }
        }
    }
}
