namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using NuGet;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library;

    using log4net;

    public class GameManager
    {
        #region Singleton
        private static GameManager Context { get; set; }
        private static object locker = new object();
        public static GameManager Get()
        {
            lock (locker) return Context ?? (Context = new GameManager());
        }
        internal GameManager()
        {

        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler GameListChanged;

        public int GameCount {
            get
            {
                return DbContext.Get().Games.Count();
            }
        }

        public Game GetById(Guid id)
        {
            return DbContext.Get().Games.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Game> Games
        {
            get
            {
                return DbContext.Get().Games;
            }
        }

        public void UninstallAllGames()
        {
            var games = Games.ToList();
            foreach(var g in games)
                UninstallGame(g);
        }

        public void InstallGame(IPackage package)
        {
            var dirPath = Path.GetTempPath();
            dirPath = Path.Combine(dirPath, "o8ginstall-" + Guid.NewGuid());
            GameFeedManager.Get().ExtractPackage(dirPath,package);
            var defPath = Path.Combine(dirPath, "def");
            if (!Directory.Exists(defPath)) return;
            var di = new DirectoryInfo(defPath);
            foreach (var f in di.GetFiles("*",SearchOption.AllDirectories))
            {
                var relPath = f.FullName.Replace(di.FullName, "");
                relPath = relPath.TrimStart('\\', '/');
                var newPath = Path.Combine(Paths.Get().DataDirectory, "GameDatabase", package.Id);
                newPath = Path.Combine(newPath, relPath);
                var newFileInfo = new FileInfo(newPath);
                if(newFileInfo.Directory != null)
                    Directory.CreateDirectory(newFileInfo.Directory.FullName);
                File.Copy(f.FullName,newPath);
            }
            this.OnGameListChanged();

        }

        public void UninstallGame(Game game)
        {
            var path = Path.Combine(Paths.Get().DataDirectory, "GameDatabase", game.Id.ToString());
            var gamePathDi = new DirectoryInfo(path);
            foreach (var file in gamePathDi.GetFiles("*", SearchOption.AllDirectories))
            {
                File.Delete(file.FullName);
            }
            foreach (var dir in gamePathDi.GetDirectories("*", SearchOption.AllDirectories))
            {
                Directory.Delete(dir.FullName);
            }
            Directory.Delete(gamePathDi.FullName);
            this.OnGameListChanged();
        }

        protected virtual void OnGameListChanged()
        {
            var handler = this.GameListChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}