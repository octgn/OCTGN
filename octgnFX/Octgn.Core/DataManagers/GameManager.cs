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
    using Octgn.Library.ExtensionMethods;

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

        public int GameCount
        {
            get
            {
                return DbContext.Get().Games.Count();
            }
        }

        public Game GetById(Guid id)
        {
            try
            {
                //Log.InfoFormat("Getting game by id {0}",id);
                return DbContext.Get().GameById(id);
            }
            finally
            {
                //Log.InfoFormat("Finished {0}", id);
            }
        }

        public IEnumerable<Game> Games
        {
            get
            {
                try
                {
                    //Log.Info("Getting games");
                    return DbContext.Get().Games.OrderBy(x => x.Name);
                }
                finally
                {
                    //Log.Info("Finished");
                }
            }
        }

        public void UninstallAllGames()
        {
            var games = Games.ToList();
            foreach (var g in games)
                UninstallGame(g);
        }

        public void InstallGame(IPackage package, Action<int, int> onProgressUpdate = null)
        {
            if (onProgressUpdate == null) onProgressUpdate = (i, i1) => { };

            Log.InfoFormat("Installing game {0} {1}", package.Id, package.Title);
            try
            {
                onProgressUpdate(-1, 1);
                Log.InfoFormat("Creating path {0} {1}", package.Id, package.Title);
                var dirPath = Path.GetTempPath();
                dirPath = Path.Combine(dirPath, "o8ginstall-" + Guid.NewGuid());
                Log.InfoFormat("Extracting package {0} {1} {2}", dirPath, package.Id, package.Title);
                GameFeedManager.Get().ExtractPackage(dirPath, package, onProgressUpdate);
                onProgressUpdate(-1, 1);
                Log.InfoFormat("Making def path {0} {1}", package.Id, package.Title);
                var defPath = Path.Combine(dirPath, "def");
                if (!Directory.Exists(defPath))
                {
                    Log.WarnFormat("Def path doesn't exist in the game package {0} {1}", package.Id, package.Title);
                    return;
                }
                var di = new DirectoryInfo(defPath);
                Log.InfoFormat("Copying temp files {0} {1}", package.Id, package.Title);
                var files = di.GetFiles("*", SearchOption.AllDirectories).ToArray();
                var curFileNum = 0;
                onProgressUpdate(curFileNum, files.Length);
                foreach (var f in files)
                {
                    Log.InfoFormat("Copying temp file {0} {1} {2}", f.FullName, package.Id, package.Title);
                    var relPath = f.FullName.Replace(di.FullName, "");
                    relPath = relPath.TrimStart('\\', '/');
                    var newPath = Path.Combine(Paths.Get().DatabasePath, package.Id);
                    newPath = Path.Combine(newPath, relPath);
                    var newFileInfo = new FileInfo(newPath);
                    if (newFileInfo.Directory != null)
                    {
                        Log.InfoFormat("Creating directory {0} {1} {2}", newFileInfo.Directory.FullName, package.Id, package.Title);
                        Directory.CreateDirectory(newFileInfo.Directory.FullName);
                    }
                    Log.InfoFormat("Copying file {0} {1} {2} {3}", f.FullName, newPath, package.Id, package.Title);
                    f.MegaCopyTo(newPath);
                    Log.InfoFormat("File copied {0} {1} {2} {3}", f.FullName, newPath, package.Id, package.Title);
                    curFileNum++;
                    onProgressUpdate(curFileNum, files.Length);
                }
                onProgressUpdate(-1, 1);
                //Sets//setid//Cards//Proxies

                var setsDir = Path.Combine(Paths.Get().DatabasePath, package.Id, "Sets");
                var imageSetsDir = Path.Combine(Paths.Get().ImageDatabasePath, package.Id, "Sets");
                if (!Directory.Exists(imageSetsDir))
                {
                    Directory.CreateDirectory(imageSetsDir);
                }

                Log.InfoFormat("Installing decks {0} {1}", package.Id, package.Title);
                var game = GameManager.Get().GetById(new Guid(package.Id));
                if (game == null)
                    throw new UserMessageException("Game {0} could not be installed. Please restart your computer and try again", package.Title);
                if (Directory.Exists(Path.Combine(game.InstallPath, "Decks")))
                {
                    var deckFiles = new DirectoryInfo(Path.Combine(game.InstallPath, "Decks")).GetFiles("*.o8d", SearchOption.AllDirectories).ToArray();
                    var curDeckFileNum = 0;
                    onProgressUpdate(curDeckFileNum, deckFiles.Length);
                    foreach (var f in deckFiles)
                    {
                        Log.InfoFormat("Found deck file {0} {1} {2}", f.FullName, package.Id, package.Title);
                        var relPath = f.FullName.Replace(new DirectoryInfo(Path.Combine(game.InstallPath, "Decks")).FullName, "").TrimStart('\\');
                        var newPath = Path.Combine(Paths.Get().DeckPath, game.Name, relPath);
                        Log.InfoFormat("Creating directories {0} {1} {2}", f.FullName, package.Id, package.Title);
                        if (new DirectoryInfo(newPath).Exists)
                            Directory.Move(newPath, Paths.Get().GraveyardPath);
                        Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                        Log.InfoFormat("Copying deck to {0} {1} {2} {3}", f.FullName, newPath, package.Id, package.Title);
                        f.MegaCopyTo(newPath);
                        curDeckFileNum++;
                        onProgressUpdate(curDeckFileNum, deckFiles.Length);
                    }
                }
                onProgressUpdate(-1, 1);

                var setsDeckFolders =
                    game.Sets()
                        .Select(x => new { Set = x, DeckDirectory = new DirectoryInfo(x.DeckPath) })
                        .Where(x => x.DeckDirectory.Exists)
                        .Select(x => new { Set = x.Set, DeckDirectory = x.DeckDirectory, files = x.DeckDirectory.GetFiles("*.o8d", SearchOption.AllDirectories) })
                        .ToArray();
                var max = setsDeckFolders.SelectMany(x => x.files).Count();
                var curSetDeckNum = 0;
                onProgressUpdate(curSetDeckNum, max);
                foreach (var set in setsDeckFolders)
                {
                    foreach (var deck in set.files)
                    {
                        Log.InfoFormat("Found deck file {0} {1} {2} {3}", deck.FullName, setsDir, package.Id, package.Title);
                        var relPath = deck.FullName.Replace(set.DeckDirectory.FullName, "").TrimStart('\\');
                        var newPath = Path.Combine(Paths.Get().DeckPath, game.Name, relPath);
                        Log.InfoFormat("Creating directories {0} {1} {2} {3}", deck.FullName, setsDir, package.Id, package.Title);
                        Directory.CreateDirectory(new FileInfo(newPath).Directory.FullName);
                        Log.InfoFormat("Copying deck to {0} {1} {2} {3} {4}", deck.FullName, newPath, setsDir, package.Id, package.Title);
                        deck.MegaCopyTo(newPath);
                        curSetDeckNum++;
                        onProgressUpdate(curSetDeckNum, max);
                    }
                }
                onProgressUpdate(-1, 1);

                Log.InfoFormat("Deleting proxy cards {0} {1} {2}", setsDir, package.Id, package.Title);
                // Clear out all proxies if they exist
                var proxyFiles = new DirectoryInfo(imageSetsDir).GetDirectories().Select(x => new DirectoryInfo(Path.Combine(x.FullName, "Cards", "Proxies"))).Where(x => x.Exists).ToArray();
                var currentProxyFilesNum = 0;
                onProgressUpdate(currentProxyFilesNum, proxyFiles.Length);
                foreach (var pdir in proxyFiles)
                {
                    try
                    {
                        var pstring = pdir.FullName;
                        Log.InfoFormat("Deleting proxy dir {0} {1} {2}", pdir, package.Id, package.Title);
                        pdir.MoveTo(Paths.Get().GraveyardPath);
                        Log.InfoFormat("Deleted proxy dir {0} {1} {2}", pdir, package.Id, package.Title);
                        Directory.CreateDirectory(pstring);
                    }
                    catch (Exception e)
                    {
                        Log.WarnFormat("Could not delete proxy directory {0}", pdir.FullName);
                    }
                    currentProxyFilesNum++;
                    onProgressUpdate(currentProxyFilesNum, proxyFiles.Length);
                }
                onProgressUpdate(-1, 1);
                Log.InfoFormat("Fire game list changed {0} {1}", package.Id, package.Title);
                this.OnGameListChanged();
                Log.InfoFormat("Game list changed fired {0} {1}", package.Id, package.Title);

                //copy images over to imagedatabase
                var cardImageList = new DirectoryInfo(setsDir)
					.GetDirectories()
					.Select(x =>new {SetDirectory=x,CardsDirectory= new DirectoryInfo(Path.Combine(x.FullName, "Cards"))})
					.Where(x => x.CardsDirectory.Exists)
                    .Select(x => new
                                 {
                                     SetDirectory=x.SetDirectory,
                                     CardsDirectory=x.CardsDirectory,
                                     ImageFiles=x.CardsDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(s => !s.FullName.EndsWith(".xml", StringComparison.CurrentCultureIgnoreCase)).ToArray()
                                 })
                    .SelectMany(x => x.ImageFiles.Select( i=>
								new {
                                     SetDirectory=x.SetDirectory,
									 CardsDirectory=x.CardsDirectory,
									 Image=i
                                 }))
					.ToArray();

				X.Instance.ForEachProgress(cardImageList.Length,cardImageList,
				    x =>
				    {
                        string copyDirPath = Path.Combine(Paths.Get().ImageDatabasePath, package.Id, "Sets", x.SetDirectory.Name, "Cards");
                        if (!Directory.Exists(copyDirPath))
                        {
                            Directory.CreateDirectory(copyDirPath);
                        }
                        x.Image.CopyTo(Path.Combine(copyDirPath, x.Image.Name), true);
                        x.Image.Delete();
				    },onProgressUpdate);

            }
            finally
            {
                Log.InfoFormat("Done {0} {1}", package.Id, package.Title);
            }
        }

        public void Installo8c(string filename)
        {
            try
            {
                Log.InfoFormat("Checking if zip file {0}", filename);
                if (!Ionic.Zip.ZipFile.IsZipFile(filename)) throw new UserMessageException("This is not a valid o8c file.");
                Log.InfoFormat("Checking if zip file {0}", filename);
                if (!ZipFile.CheckZip(filename)) throw new UserMessageException("This is not a valid o8c file.");

                Guid gameGuid = Guid.Empty;

                Log.InfoFormat("Reading zip file {0}", filename);
                using (var zip = ZipFile.Read(filename))
                {
                    Log.InfoFormat("Getting zip files {0}", filename);
                    var selection = from e in zip.Entries where !e.IsDirectory select e;

                    foreach (var e in selection)
                    {
                        Log.InfoFormat("Checking zip file {0} {1}", e.FileName, filename);
                        if (e.FileName.ToLowerInvariant().EndsWith("db"))
                        {
                            continue;
                        }
                        bool extracted = extract(e, out gameGuid, gameGuid);
                        if (!extracted)
                        {
                            Log.Warn(string.Format("Invalid entry in {0}. Entry: {1}.", filename, e.FileName));
                            throw new UserMessageException(
                                "Image pack invalid. Please contact the game developer about this.");
                        }
                        Log.InfoFormat("Extracted {0} {1}", e.FileName, filename);
                    }
                }
                Log.InfoFormat("Installed successfully {0}", filename);

                //zipFile.ExtractAll(Paths.Get().DatabasePath,ExtractExistingFileAction.OverwriteSilently);
            }
            catch (ZipException e)
            {
                throw new UserMessageException("The o8c file {0} is invalid.", filename);
            }
            catch (UserMessageException e)
            {
                Log.Warn("User message error", e);
                throw;
            }
            catch (Exception e)
            {
                Log.Error("", e);
                throw new UserMessageException("There was an error. If this keeps happening please let us know.");
            }
        }

        internal struct O8cEntry
        {
            public string gameGuid;
            public string setsDir;
            public string setGuid;
            public string cardsDir;
            public string cardImage;
        }


        internal bool extract(ZipEntry entry, out Guid gameGuid, Guid testGuid)
        {
            try
            {
                Log.InfoFormat("Extracting {0},{1}", entry.FileName, testGuid);
                bool ret = false;
                gameGuid = testGuid;
                string[] split = entry.FileName.Split('/');
                Log.InfoFormat("Split file name {0},{1}", entry.FileName, testGuid);
                if (split.Length == 5)
                {
                    Log.InfoFormat("File name right count {0},{1}", entry.FileName, testGuid);
                    O8cEntry o8cEntry = new O8cEntry()
                                            {
                                                gameGuid = split[0],
                                                setsDir = split[1],
                                                setGuid = split[2],
                                                cardsDir = split[3],
                                                cardImage = split[4]
                                            };
                    Log.InfoFormat("Checking if testGuid is empty {0},{1}", entry.FileName, testGuid);
                    if (testGuid.Equals(Guid.Empty))
                    {
                        Log.InfoFormat("testGuid is empty {0},{1}", entry.FileName, testGuid);
                        testGuid = Guid.Parse(o8cEntry.gameGuid);
                        gameGuid = Guid.Parse(o8cEntry.gameGuid);
                        Log.InfoFormat("Setting gameguid and testguid {0},{1},{2}", entry.FileName, testGuid, gameGuid);
                    }
                    Log.InfoFormat("Checking if {0}=={1} {2}", testGuid, o8cEntry.gameGuid, entry.FileName);
                    if (!testGuid.Equals(Guid.Parse(o8cEntry.gameGuid)))
                    {
                        Log.InfoFormat("{0}!={1} {2}", testGuid, o8cEntry.gameGuid, entry.FileName);
                        return (ret);
                    }
                    Log.InfoFormat("Checking if should extract part {0},{1}", entry.FileName, testGuid);
                    if (ShouldExtract(o8cEntry))
                    {
                        Log.InfoFormat(
                            "Should extract, so extracting {0},{1},{2}",
                            Paths.Get().ImageDatabasePath,
                            entry.FileName,
                            testGuid);
                        entry.Extract(Paths.Get().ImageDatabasePath, ExtractExistingFileAction.OverwriteSilently);
                        Log.InfoFormat("Extracted {0},{1},{2}", Paths.Get().ImageDatabasePath, entry.FileName, testGuid);
                        ret = true;
                    }
                }
                Log.InfoFormat("Finishing {0},{1},{2}", ret, entry.FileName, testGuid);
                return (ret);

            }
            catch (IOException e)
            {
                throw new UserMessageException("Error extracting {0} to {1}\n{2}", entry.FileName, Paths.Get().DatabasePath, e.Message);
            }
            finally
            {
                Log.InfoFormat("Finished {0},{1}", entry.FileName, testGuid);
            }
        }

        internal bool ShouldExtract(O8cEntry o8centry)
        {
            try
            {
                Log.InfoFormat("Checking if should extract {0}", o8centry.cardImage);
                bool ret = false;
                Log.InfoFormat("Grabbing game {0},{1}", o8centry.gameGuid, o8centry.cardImage);
                var game = GetById(Guid.Parse(o8centry.gameGuid));
                if (game != null)
                {
                    Log.InfoFormat("Game exists {0},{1}", o8centry.gameGuid, o8centry.cardImage);
                    Guid cardGuid = Guid.Parse(o8centry.cardImage.Split('.')[0]);
                    Log.InfoFormat("Checking Paths {0},{1},{2}", o8centry.setsDir, o8centry.cardsDir, o8centry.cardImage);
                    if (o8centry.setsDir == "Sets" && o8centry.cardsDir == "Cards")
                    {
                        Log.InfoFormat("Paths good {0},{1},{2}", o8centry.setsDir, o8centry.cardsDir, o8centry.cardImage);
                        ret = true;
                    }
                }
                else
                {
                    Log.InfoFormat("Couldn't find game {0},{1}", o8centry.gameGuid, o8centry.cardImage);
                }
                Log.InfoFormat("Finishing {0}", o8centry.cardImage);
                return (ret);

            }
            finally
            {
                Log.InfoFormat("Finished {0}", o8centry.cardImage);
            }
        }

        public void UninstallGame(Game game)
        {
            try
            {
                Log.InfoFormat("Uninstalling game {0}", game.Id);
                var path = Path.Combine(Paths.Get().DatabasePath, game.Id.ToString());
                var gamePathDi = new DirectoryInfo(path);
                Log.InfoFormat("Deleting folder {0} {1}", path, game.Id);
                int tryCount = 0;
                while (tryCount < 5)
                {
                    try
                    {
                        DbContext.Get().Invalidate(game);
                        gamePathDi.ClearReadonlyFlag();
                        gamePathDi.MoveTo(Paths.Get().GraveyardPath);
                        break;
                    }
                    catch
                    {
                        tryCount++;
                        if (tryCount == 4) throw;
                    }
                }
                Log.InfoFormat("Folder deleted {0} {1}", path, game.Id);
            }
            finally
            {
                Log.InfoFormat("Finished {0}", game.Id);
            }
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
