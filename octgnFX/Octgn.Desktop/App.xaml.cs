using Microsoft.EntityFrameworkCore.Internal;
using Octgn.Desktop.Interfaces.Easy;
using Octgn.Sdk;
using Octgn.Sdk.Data;
using Octgn.Sdk.Extensibility;
using Octgn.Sdk.Extensibility.Desktop;
using Octgn.Sdk.Packaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Octgn.Desktop
{
    public partial class App : Application
    {
        public new MainWindow MainWindow {
            get => (MainWindow)base.MainWindow;
            set => base.MainWindow = value;
        }

        private DataMapper _dataMapper;
        private PluginIntegration _pluginIntegration;
        private Settings _settings;
        private NavigationService _navigationService;

        //TODO: Reduce this to 150ms total until something is visible. Will probably have to use win32 window for loading screen or something idk.
        public App() {
            DebugTimed(TimeSpan.FromMilliseconds(20), "Took too long to Init App. There is no UI visible, so this needs to be fast.", () => {
                _dataMapper = new DataMapper();
                _pluginIntegration = new PluginIntegration();
                _settings = new Settings();
                _navigationService = new NavigationService();
            });

            DebugTimed(TimeSpan.FromMilliseconds(1000), "Took too long to create Main Window. There is no UI visible, so this needs to be fast.", () => {
                MainWindow = new MainWindow(_navigationService);
            });
        }

        [ThreadStatic]
        private static readonly Stopwatch _sw = new Stopwatch();

        [DebuggerStepThrough]
        private static void DebugTimed(TimeSpan maxTime, string errorMessage, Action action) {
            _sw.Restart();

            action();

            _sw.Stop();

            Debug.Assert(_sw.Elapsed <= maxTime, _sw.ElapsedMilliseconds.ToString() + "ms:" + errorMessage);
        }

        internal static void UnhandledException(Exception ex) => throw new NotImplementedException();

        protected override async void OnStartup(StartupEventArgs e) {
            try {
                MainWindow.Show();

                await Task.Run(Load).ConfigureAwait(false);
            } catch (Exception ex) {
                App.UnhandledException(ex);
            }
        }

        private void Load() {
            File.Delete("temp.db");

            UpgradeDatabase();

            InstallBuiltInPackages();

            UpdateBuiltInPackages();

            LoadPlugins();

            var defaultGameRecord = ChooseDefaultGame();

            var startScreen = ChooseStartScreen(defaultGameRecord);

            _navigationService.NavigateTo(startScreen);
        }

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

        private void LoadPlugins() {
            //_pluginIntegration.Loader.RegisterStandard(MenuPlugin.PluginTypeName);
            _pluginIntegration.Initialize();
        }

        private PackageFile LoadPackageFile(string path) {
            path = Path.Combine(Environment.CurrentDirectory, "Packages", path);

            var pathInfo = new FileInfo(path);

            if (!pathInfo.Exists)
                throw new FileNotFoundException($"Package '{path}' doesn't exist.");

            var packageFileReader = new PackageFileReader();

            return packageFileReader.ReadFile(path);
        }

        private PluginRecord ChooseDefaultGame() {
            var defaultGameId = _settings.DefaultGame;

            using (var context = new DataContext()) {
                var allGames = context.Games().ToArray();

                var plugin = allGames
                    .Where(x => x.Id == defaultGameId)
                    .OrderByDescending(x => x.PackageVersion)
                    .FirstOrDefault();

                if (plugin == null) {
                    plugin = allGames.First();
                }

                return plugin;
            }
        }

        private Screen ChooseStartScreen(PluginRecord gamePluginRecord) {
            using var context = new DataContext();

            var gamePlugin = _pluginIntegration
                .Packages
                .SelectMany(package => package.Plugins)
                .Where(plugin => plugin.Details.Id == gamePluginRecord.Id)
                .First();

            var gamePackage = gamePlugin.Package;

            var menuPlugin = gamePackage
                .FindByType(MenuPlugin.PluginTypeName, true)
                .SingleOrDefault();

            if (menuPlugin == null) {
                throw new NotImplementedException();
            }

            var mp = (MenuPlugin)menuPlugin;

            Screen screen = null;
            Dispatcher.Invoke(() => {
                screen = new MenuScreen(gamePlugin.Details, mp);
            });

            return screen
                ?? throw new InvalidOperationException($"page null");
        }
    }
}
