using Octgn.Sdk;
using Octgn.Sdk.Data;
using Octgn.Sdk.Packaging;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Octgn.Desktop
{
    public partial class App : Application
    {
        private readonly DataMapper _dataMapper;
        private readonly PackageFileReader _packageFileReader;

        public App() {
            _dataMapper = new DataMapper();
            _packageFileReader = new PackageFileReader();
        }

        protected override void OnStartup(StartupEventArgs e) {
            UpgradeDatabase();

            InstallBuiltInPackages();

            UpdateBuiltInPackages();

            using(var context = new DataContext()) {
                foreach(var package in context.Packages) {
                    Console.WriteLine(package.Id);
                }
            }

#if(DEBUG)
            //TempDev();
#endif
        }

        internal static void UnhandledException(Exception ex) => throw new NotImplementedException();

        private void UpgradeDatabase() {
            using (var context = new DataContext()) {
                context.Upgrade(null, default);
            }
        }

        private void InstallBuiltInPackages() {
            using (var context = new DataContext()) {
                var jodsPackage = LoadPackageFile("octgn.packages.jodsengine\\jodsengine.package.yaml");

                if (!context.Packages.Any(x => x.Id == jodsPackage.Id)) {
                    InstallPackage(context, jodsPackage);
                }

                var chessPackage = LoadPackageFile("octgn.packages.chessgame\\chessgame.package.xml");

                if (!context.Packages.Any(x => x.Id == chessPackage.Id)) {
                    InstallPackage(context, chessPackage);
                }

                context.SaveChanges();
            }
        }

        private void InstallPackage(DataContext context, PackageFile package) {
            var newRecord = _dataMapper.Map<PackageRecord>(package);
            context.Packages.Add(newRecord);

            foreach (var plugin in package.Plugins) {
                var newPluginRecord = _dataMapper.Map<PluginRecord>(plugin, newRecord.Id, newRecord.Version);

                context.Plugins.Add(newPluginRecord);
            }
        }

        private void UpdateBuiltInPackages() {
            using (var context = new DataContext()) {
                var jodsPackage = LoadPackageFile("octgn.packages.jodsengine\\jodsengine.package.yaml");

                var installedJodsPackage = context.Packages.FirstOrDefault(x => x.Id == jodsPackage.Id && x.Version == jodsPackage.Version);
                if (installedJodsPackage == null) {
                    InstallPackage(context, jodsPackage);
                }

                var chessPackage = LoadPackageFile("octgn.packages.chessgame\\chessgame.package.xml");

                var installedChessPackage = context.Packages.FirstOrDefault(x => x.Id == chessPackage.Id && x.Version == chessPackage.Version);
                if (installedChessPackage == null) {
                    InstallPackage(context, chessPackage);
                }

                context.SaveChanges();
            }
        }

        private PackageFile LoadPackageFile(string path) {
            path = Path.Combine(Environment.CurrentDirectory, "Packages", path);

            var pathInfo = new FileInfo(path);

            if (!pathInfo.Exists)
                throw new FileNotFoundException($"Package '{path}' doesn't exist.");

            return _packageFileReader.ReadFile(path);
        }

        private void TempDev() {
            using (var context = new DataContext()) {
                context.Upgrade(null, default);

                var testPackage = new PackageRecord() {
                    Id = "Octgn.Test",
                    Name = "Test",
                    Icon = "icon.ico",
                    Description = "Just a test",
                    OctgnVersion = "1.2.3.4",
                    Website = "http://www.octgn.net",
                    Version = "1.0.0.0"
                };

                var testPlugin = new PluginRecord() {
                    Id = "Octgn.Test.Plugin1",
                    Name = "Plugin1",
                    Icon = "icon2.ico",
                    Description = "Just a test plugin 1",
                    Path = "plugin1.dll",
                    PackageId = "Octgn.Test",
                    PackageVersion = "1.0.0.0"
                };

                context.Packages.Add(testPackage);

                context.Plugins.Add(testPlugin);

                context.SaveChanges();

                var packages = context.Packages.ToArray();
                var plugins = context.Plugins.ToArray();

                System.Diagnostics.Debugger.Break();
            }
        }
    }
}
