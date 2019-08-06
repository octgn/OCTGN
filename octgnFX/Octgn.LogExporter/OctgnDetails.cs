using System;
using System.Diagnostics;
using System.IO;

namespace Octgn.LogExporter
{
    public partial class MainWindow
    {
        private class OctgnDetails
        {
            // octgn version
            public Version Version { get; set; }

            // octgn install path
            public string Location { get; set; }

            // octgn data directory
            public string DataPathFile { get; set; }

            public string DataDirectoryEnvironmentalVariable { get; set; }

            // Is OCTGN running (all running instances)
            public int RunningInstances { get; set; }

            public string ConfigPath { get; set; }

            public void FillDetails() {
                var assLocation = new FileInfo(typeof(OctgnDetails).Assembly.Location);

                Location = assLocation.Directory.FullName;

                DataDirectoryEnvironmentalVariable = Environment.GetEnvironmentVariable("OCTGN_DATA", EnvironmentVariableTarget.Process);

                if (DataDirectoryEnvironmentalVariable == null) {
                    DataDirectoryEnvironmentalVariable = Environment.GetEnvironmentVariable("OCTGN_DATA", EnvironmentVariableTarget.User);
                }

                if (DataDirectoryEnvironmentalVariable == null) {
                    DataDirectoryEnvironmentalVariable = Environment.GetEnvironmentVariable("OCTGN_DATA", EnvironmentVariableTarget.Machine);
                }

                var dataPathFile = Path.Combine(Location, "data.path");

                if (File.Exists(dataPathFile)) {
                    DataPathFile = File.ReadAllText(dataPathFile);
                }

                if (DataPathFile == null) {
                    DataPathFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");

                    if (!Directory.Exists(DataPathFile)) {
                        DataPathFile = null;
                    }
                }

                var dataDirectoryPath = DataDirectoryEnvironmentalVariable ?? DataPathFile;

                if (dataDirectoryPath != null) {
                    dataDirectoryPath = Environment.ExpandEnvironmentVariables(dataDirectoryPath);

                    if (!Path.IsPathRooted(dataDirectoryPath)) {
                        dataDirectoryPath = Path.GetFullPath(dataDirectoryPath);
                    }

                    ConfigPath = Path.Combine(dataDirectoryPath, "Config", "settings.json");
                }

                if (Location != null) {
                    var octgnPath = Path.Combine(Location, "Octgn.exe");

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
