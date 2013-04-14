namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Ionic.Zip;

    using NuGet;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using Octgn.Library.Exceptions;

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
                File.Copy(f.FullName,newPath,true);
            }
            //Sets//setid//Cards//Proxies
            // Clear out all proxies if they exist
            var setsDir = Path.Combine(Paths.Get().DataDirectory, "GameDatabase", package.Id,"Sets");
            foreach (var setdir in new DirectoryInfo(setsDir).GetDirectories())
            {
                var pdir = new DirectoryInfo(Path.Combine(setdir.FullName, "Cards", "Proxies"));
                if(!pdir.Exists)continue;
                try
                {
                    Directory.Delete(pdir.FullName,true);
                }
                catch (Exception e)
                {
                    Log.WarnFormat("Could not delete proxy directory {0}",pdir.FullName);
                }
            }
            this.OnGameListChanged();

        }

        public void Installo8c(string filename)
        {
            try
            {
                if(!Ionic.Zip.ZipFile.IsZipFile(filename))
                    throw new UserMessageException("This is not a valid o8c file.");
                if(!ZipFile.CheckZip(filename))
                    throw new UserMessageException("This is not a valid o8c file.");
                var zipFile = ZipFile.Read(filename);
                zipFile.ExtractAll(Paths.Get().DatabasePath,ExtractExistingFileAction.OverwriteSilently);
            }
            catch (UserMessageException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error("",e);
                throw new UserMessageException("There was an error. If this keeps happening please let us know.");
            }
        }

        public void UninstallGame(Game game)
        {
            var path = Path.Combine(Paths.Get().DataDirectory, "GameDatabase", game.Id.ToString());
            var gamePathDi = new DirectoryInfo(path);
            Directory.Delete(gamePathDi.FullName, true);
            //foreach (var file in gamePathDi.GetFiles("*", SearchOption.AllDirectories))
            //{
            //    File.Delete(file.FullName);
            //}
            //foreach (var dir in gamePathDi.GetDirectories("*", SearchOption.AllDirectories))
            //{
            //    Directory.Delete(dir.FullName,true);
            //}
            //Directory.Delete(gamePathDi.FullName);
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