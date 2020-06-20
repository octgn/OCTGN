using Octgn.Sdk.Data;
using System;
using System.Linq;
using System.Windows;

namespace Octgn.Desktop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) {
            using (var context = new DataContext()) {
                context.Upgrade(null, default);
            }

#if(DEBUG)
            //TempDev();
#endif
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
