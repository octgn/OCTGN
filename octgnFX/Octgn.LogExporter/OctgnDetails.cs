using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Octgn.LogExporter
{
    public partial class MainWindow
    {
        private class OctgnDetails
        {
            // Is octgn installed
            public bool IsInstalled { get; set; }

            // octgn version
            public Version Version { get; set; }

            // octgn install path
            public string InstallPath { get; set; }

            // octgn data directory
            public string DataDirectory { get; set; }

            // Is OCTGN running (all running instances)
            public int RunningInstances { get; set; }

            public string ConfigPath { get; set; }

            public void FillDetails() {
                using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                    if (subKey == null) {
                        IsInstalled = false;
                        return;
                    }

                    IsInstalled = true;

                    InstallPath = (string)subKey.GetValue(@"InstallDirectory");

                    if(InstallPath == null) {
                        InstallPath = (string)subKey.GetValue(@"Install_Dir");
                    }

                    DataDirectory = (string)subKey.GetValue(@"DataDirectory");
                }
                
                if(DataDirectory == null) {
                    DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
                }

                ConfigPath = Path.Combine(DataDirectory, "Config", "settings.json");

                if (InstallPath != null) {
                    var octgnPath = Path.Combine(InstallPath, "Octgn.exe");

                    if (File.Exists(octgnPath)) {
                        var fileVersionInfo = FileVersionInfo.GetVersionInfo(octgnPath);

                        Version = Version.Parse(fileVersionInfo.FileVersion);
                    }
                }

                RunningInstances = Process.GetProcessesByName("OCTGN").Length;
            }
        }
    }
}
