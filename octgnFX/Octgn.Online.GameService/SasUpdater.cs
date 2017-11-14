/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using log4net;
using NuGet;
using Octgn.Library;

namespace Octgn.Online.GameService
{
    public class SasUpdater : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static SasUpdater SingletonContext { get; set; }

        private static readonly object SasUpdaterSingletonLocker = new object();

        public static SasUpdater Instance {
            get {
                if (SingletonContext == null)
                {
                    lock (SasUpdaterSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new SasUpdater();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private readonly Timer checkForUpdatesTimer;
        private IPackageRepository _repo;

        public bool IsUpdating { get; private set; }

        private SasUpdater()
        {
            checkForUpdatesTimer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
            checkForUpdatesTimer.Elapsed += CheckForUpdatesTimerOnElapsed;

            // I believe this gets stored like this because this factory has a memory leak in it
            _repo = NuGet.PackageRepositoryFactory.Default.CreateRepository("https://www.myget.org/F/octgn/");
        }

        public void Start()
        {
            checkForUpdatesTimer.Start();
        }

        private void CheckForUpdatesTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                checkForUpdatesTimer.Enabled = false;
                HostedGames.Get(Guid.Empty);

                var newestPackage = GetLatestPackage();

                if (newestPackage == null)
                {
                    Log.Info("newestPackage == null");
                    return;
                }

                var localLatest = LocalLatestVersion();

                if (newestPackage.Version.Version.CompareTo(localLatest) > 0)
                {
                    IsUpdating = true;
                    Log.InfoFormat("{0} > {1}, downloading new version", newestPackage.Version.Version, localLatest);
                    var dir = new DirectoryInfo(Path.Combine("C:\\Server\\sas", newestPackage.Version.Version.ToString()));
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }
                    else
                    {
                        X.Instance.Retry(() => dir.Delete(true));
                    }

                    var files = newestPackage.GetFiles().ToArray();
                    foreach (var file in files)
                    {
                        var p = Path.Combine(dir.FullName, file.Path);
                        var fi = new FileInfo(p);
                        var fileDir = fi.Directory.FullName;
                        Directory.CreateDirectory(fileDir);

                        var byteList = new List<byte>();
                        using (var sr = new BinaryReader(file.GetStream()))
                        {
                            var buffer = new byte[1024];
                            var len = sr.Read(buffer, 0, 1024);
                            while (len > 0)
                            {
                                byteList.AddRange(buffer.Take(len));
                                Array.Clear(buffer, 0, buffer.Length);
                                len = sr.Read(buffer, 0, 1024);
                            }
                            File.WriteAllBytes(p, byteList.ToArray());
                        }
                    }
                    File.Create(Path.Combine(dir.FullName, "complete"));
                    Log.Info("Update complete");
                }
            }
            catch (Exception e)
            {
                Log.Error("CheckForUpdatesTimerOnElapsed Error", e);
            }
            finally
            {
                checkForUpdatesTimer.Enabled = true;
                IsUpdating = false;
            }
        }



        private IPackage GetLatestPackage()
        {
            Log.Info("GettingLatestPackage");
            var newestPackage = _repo.GetPackages()
                                            .Where(x => x.Id.Equals("Octgn.Online.StandAloneServer", StringComparison.InvariantCultureIgnoreCase))
                                            .Where(x => x.IsAbsoluteLatestVersion)
                                            .ToList()
                                            .OrderByDescending(x => x.Version.Version)
                                            .FirstOrDefault();
            if (newestPackage != null)
            {
                Log.InfoFormat("Newest package version = {0}", newestPackage.Version.Version);
            }
            return newestPackage;
        }

        private Version LocalLatestVersion()
        {
            Log.Info("LocalLatestVersion");

            Version ret = null;

            var dir = new DirectoryInfo(Path.Combine("c:\\Server", "sas"));
            if (dir.Exists == false)
                dir.Create();

            var version = dir.GetDirectories()
                .Where(IsComplete)
                .Select(x => x.Name)
                .Where(IsVersion)
                .Select(Version.Parse)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (version == null)
            {
                Log.InfoFormat("Local version ==null");
                ret = new Version(0, 0, 0, 0);
            }
            else
            {
                ret = version;
            }

            Log.InfoFormat("LocalLatestVersion={0}", ret);
            return ret;
        }

        private bool IsVersion(string str)
        {
            Version v = null;

            return Version.TryParse(str, out v);
        }

        private bool IsComplete(DirectoryInfo dir)
        {
            return dir.GetFiles("complete").Length > 0;
        }

        public void Dispose()
        {
            checkForUpdatesTimer.Dispose();
        }
    }
}