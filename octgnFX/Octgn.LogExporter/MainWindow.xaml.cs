using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Octgn.LogExporter
{
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();

            tbFilePath.Text = CreateDumpPath();
        }

        private string CreateDumpPath() {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var dateTimeString = DateTime.Now.ToString("u").Replace(":", ".");
            var fileName = $"OCTGNLogDump {dateTimeString}.zip";
            var fullPath = Path.Combine(dir, fileName);

            return fullPath;
        }

        private void Browse_Click(object sender, RoutedEventArgs e) {
            try {
                var sfd = new SaveFileDialog();
                sfd.Title = "Save Log Dump To...";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                var dateTimeString = DateTime.Now.ToString("u").Replace(":", ".");

                sfd.FileName = $"OCTGNLogDump {dateTimeString}.zip";
                sfd.OverwritePrompt = true;
                sfd.Filter = "Zip File (.zip) | *.zip";
                if ((sfd.ShowDialog() ?? false)) {
                    tbFilePath.Text = sfd.FileName;
                }
            } catch (Exception ex) {
                ShowError("Error browsing for an export path", ex);
            }
        }

        private async void Export_Click(object sender, RoutedEventArgs e) {
            try {
                this.IsEnabled = false;
                System.Windows.Input.Mouse.OverrideCursor = Cursors.Wait;

                // ---- Configure Paths

                FileInfo filePath = null;

                try {
                    filePath = new FileInfo(tbFilePath.Text);
                } catch (ArgumentException) {
                    ShowError("Export path is not valid.");
                    return;
                }

                if (!filePath.Directory.Exists) {
                    filePath.Directory.Create();
                }

                var fileNameNoExtension = Path.GetFileNameWithoutExtension(filePath.Name);

                var buildDirectoryPath = Path.Combine(filePath.Directory.FullName, fileNameNoExtension);

                if (!Directory.Exists(buildDirectoryPath)) {
                    Directory.CreateDirectory(buildDirectoryPath);
                }

                var log = new LogFile(buildDirectoryPath);

                // ---- Gather sytem info
                try {
                    await Step_GatherAdditionalInfo(log, buildDirectoryPath);
                } catch (Exception ex) {
                    log.Error("Error gather system info", ex);
                }

                // ---- Export the EventLog
                try {
                    log.Info("Exporting Event Log...");

                    var eventLogPath = Path.Combine(buildDirectoryPath, "Octgn.evtx");
                    var session = EventLogSession.GlobalSession;
                    session.ExportLogAndMessages("Octgn", PathType.LogName, "*", eventLogPath);
                } catch (EventLogException ex) {
                    log.Error("Event Log OCTGN not found", ex);
                }

                // ---- Export installer logs
                try {
                    var installerLogsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");

                    foreach(var file in Directory.EnumerateFiles(installerLogsFolder, "Octgn*.log")) {
                        var fileName = Path.GetFileName(file);

                        var copyToPath = Path.Combine(buildDirectoryPath, fileName);

                        log.Info("Copying installer log : " + file);

                        File.Copy(file, copyToPath);
                    }
                } catch(Exception ex) {
                    log.Error("Error exporting installer logs", ex);
                }

                // ---- Create Zip File
                log.Info("Creating zip file " + filePath.FullName);

                ZipFile.CreateFromDirectory(buildDirectoryPath, filePath.FullName);

                // ---- Delete build directory
                await Task.Delay(5000);

                Directory.Delete(buildDirectoryPath, true);
            } catch(Exception ex) {
                ShowError("Unexpected Error", ex);
            } finally {
                tbFilePath.Text = CreateDumpPath();
                this.IsEnabled = true;
                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

        private async Task Step_GatherAdditionalInfo(LogFile log, string buildDirectoryPath) {
            var computerInfo = new ComputerInfo();

            // windows version
            log.Info(computerInfo.OSFullName + " " + computerInfo.OSVersion);
            log.Info("OS Bit: " + (Environment.Is64BitOperatingSystem ? "64" : "32"));
            log.Info("Program Bit: " + (Environment.Is64BitProcess ? "64" : "32"));

            // Highest .net version installed
            var dotNetVersion = GetDotNetVersion();
            log.Info(".Net Version: " + dotNetVersion);

            // processor count
            log.Info("Processors: " + Environment.ProcessorCount);

            // total ram
            var totalGBRam = Convert.ToInt32((computerInfo.TotalPhysicalMemory / (Math.Pow(1024, 3))) + 0.5);
            log.Info("Total Ram: " + totalGBRam + "GB");

            // ram usage
            var availableGBRam = Math.Round(computerInfo.AvailablePhysicalMemory / (Math.Pow(1024, 3)), 2);
            log.Info("Available Ram: " + availableGBRam + "GB");

            // pings to various sites
            var octgnPing = await Ping("octgn.net");
            log.Info("OCTGN Ping: " + octgnPing);

            var googlePing = await Ping("google.com");
            log.Info("Google Ping: " + googlePing);

            var yahooPing = await Ping("yahoo.com");
            log.Info("Yahoo Ping: " + yahooPing);

            var mygetPing = await Ping("myget.org");
            log.Info("MyGet Ping: " + mygetPing);

            // OCTGN Details
            var details = new OctgnDetails();
            details.FillDetails();

            // Is octgn installed
            log.Info("Octgn Installed: " + details.Location);
            log.Info("Data Path File: " + details.DataPathFile);
            log.Info("Data Directory Env Var : " + details.DataDirectoryEnvironmentalVariable);
            log.Info("OCTGN Version: " + details.Version);
            log.Info("Running Instance Count: " + details.RunningInstances);

            var copyTo = Path.Combine(buildDirectoryPath, "settings.json");

            try {
                File.Copy(details.ConfigPath, copyTo);
            } catch (Exception ex) {
                log.Error($"Couldn't copy {details.ConfigPath} to {copyTo}", ex);
            }
        }

        private static string GetDotNetVersion() {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\")) {
                if (ndpKey == null) return "Unable to detect .net in the registry";

                int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                return CheckFor45DotVersion(releaseKey);
            }
        }

        // Checking the version using >= will enable forward compatibility,
        // however you should always compile your code on newer versions of
        // the framework to ensure your app works the same.
        private static string CheckFor45DotVersion(int releaseKey) {
            if (releaseKey >= 461808) {
                return "4.7.2 or later";
            }
            if (releaseKey >= 461308) {
                return "4.7.1 or later";
            }
            if (releaseKey >= 460798) {
                return "4.7 or later";
            }
            return "No 4.7 or later version detected";
        }

        private static void ShowError(Exception exception) {
            MessageBox.Show("Error" + Environment.NewLine + exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private static void ShowError(string message) {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void ShowError(string message, Exception exception) {
            MessageBox.Show(message + Environment.NewLine + exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static async Task<string> Ping(string url) {
            try {
                using (var ping = new Ping()) {
                    var buffer = new byte[128];

                    var result = await ping.SendPingAsync(url, 10000, buffer);

                    var status = Enum.GetName(typeof(IPStatus), result.Status);
                    var latency = TimeSpan.FromMilliseconds(result.RoundtripTime);

                    return status + ": " + latency.TotalMilliseconds + "ms";
                }
            } catch (Exception ex) {
                return $"Ping {url} Failed: " + ex.ToString();
            }
        }
    }
}
