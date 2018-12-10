using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Octgn.Installer.Shared
{
    public class InstalledOctgn {
        public static InstalledOctgn Get() {
            var installedOctgn = new InstalledOctgn();

            var oldPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            oldPath = Path.Combine(oldPath, "Octgn", "OCTGN");

            installedOctgn.InstalledDirectory = new DirectoryInfo(oldPath);

            //TODO: This should be able to check the registry or something, the previous installer should have left some artifact we can use. This may be unneccisary though.
            if (Directory.Exists(oldPath)) {
                installedOctgn.IsIncompatible = true;
                installedOctgn.IsInstalled = true;
            } else {
                installedOctgn.IsIncompatible = false;

                // Get install path from registry
                using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                    if (subKey != null) {
                        var installDirectoryString = (string)subKey.GetValue(@"InstallDirectory");

                        if (!string.IsNullOrWhiteSpace(installDirectoryString)) {
                            var installDirectory = new DirectoryInfo(installDirectoryString);

                            installedOctgn.InstalledDirectory = installDirectory;
                            installedOctgn.IsInstalled = true;
                        }
                    }
                }
            }

            // nothing to configure if octgn isn't installed
            if (!installedOctgn.IsInstalled) return installedOctgn;

            if (installedOctgn.IsIncompatible) {
                // Get data directory of incompatible octgn

                var configPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                configPath = Path.Combine(configPath, "Octgn", "Config", "settings.json");

                string dataDirectoryString = null;

                if (File.Exists(configPath)) {

                    var configText = File.ReadAllText(configPath);

                    dynamic config = JsonConvert.DeserializeObject(configText);

                    dataDirectoryString = config.datadirectory; 
                }

                if(dataDirectoryString == null) {
                    dataDirectoryString = Paths.Get.DefaultDataDirectory;
                }

                installedOctgn.DataDirectory = new DirectoryInfo(dataDirectoryString);
            } else {
                // Get data directory from registry

                using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                    if (subKey != null) {
                        var dataDirectoryString = (string)subKey.GetValue(@"DataDirectory");

                        installedOctgn.DataDirectory = new DirectoryInfo(dataDirectoryString);
                    }
                }
            }

            return installedOctgn;
        }

        public bool IsInstalled { get; set; }

        public bool IsIncompatible { get; set; }

        public DirectoryInfo InstalledDirectory { get; set; }

        public DirectoryInfo DataDirectory { get; set; }
    }
}