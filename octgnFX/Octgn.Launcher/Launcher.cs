using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualBasic.Devices;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Octgn.Launcher;

[SupportedOSPlatform("windows")]
internal class Launcher
{
    private static log4net.ILog Log = log4net.LogManager.GetLogger("Launcher");

    private static readonly Version Windows7Version = new Version(6, 1, 0, 0);

    private const string DotNetDownloadUrl = "http://go.microsoft.com/fwlink/?LinkId=2085155";

    private static bool IsDebugMode;

    public Task RunAsync(CancellationToken cancellation) {
        return Task.Run(() => Run(cancellation), cancellation);
    }

    public async Task Run(CancellationToken cancellation) {
        Log.Info("Running Launcher");

        cancellation.ThrowIfCancellationRequested();

        {
            var debugVariable = Environment.GetEnvironmentVariable("OCTGN_LAUNCHER_DEBUG");

            IsDebugMode = debugVariable != null || Debugger.IsAttached;
            Log.Info("IsDebugMode=" + IsDebugMode);

            if (IsDebugMode && !Debugger.IsAttached) {
                Log.Info("Launching Debugger");
                var launched = Debugger.Launch();

                if (launched) {
                    Log.Info("Debugger attached");
                } else {
                    Log.Info("Debugger did not attach");
                }
            }
        }

        cancellation.ThrowIfCancellationRequested();

        Version osVersion;
        string osName;
        {
            var computerInfo = new ComputerInfo();

            osVersion = Version.Parse(computerInfo.OSVersion);
            osName = computerInfo.OSFullName;

            Log.Info($"OS:{osName}:{osVersion}");
        }

        cancellation.ThrowIfCancellationRequested();

        var workingDirectory = Environment.CurrentDirectory;
        Log.Info("Working Dir=" + workingDirectory);

        string commandLineArgs;
        {
            var exe = Environment.GetCommandLineArgs()[0];
            var fullCommandLine = Environment.CommandLine;
            commandLineArgs = fullCommandLine[exe.Length..];

            Log.Info($"Args={commandLineArgs}");
        }

        cancellation.ThrowIfCancellationRequested();

        var octgnPath = GetOctgnPath(IsDebugMode);
        Log.Info($"Octgn Path={octgnPath}");

        if (osVersion < Windows7Version) {
            Log.Warn("Windows version is too low to run OCTGN.");

            MessageBox.Show($"OCTGN is incompatible with {osName} {osVersion} at this time.{Environment.NewLine}{Environment.NewLine}OCTGN will now exit.", "Incompatible OS", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            return;
        }

        cancellation.ThrowIfCancellationRequested();

        if (!IsDotNet48OrLaterInstalled()) {
            Log.Warn(".net 4.8 is not installed.");

            var result = MessageBox.Show($"OCTGN requires .net 4.8 to be installed.{Environment.NewLine}{Environment.NewLine}Press 'Yes' to download .net", ".Net Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);

            if (result == DialogResult.Yes) {
                var download_path = await DownloadDotNetFramework48(cancellation);

                if (download_path is not null) {
                    Log.Info("Launching .net installer: " + download_path);

                    // Launch process download_path, prompting for elevation
                    try {
                        using var process = Process.Start(new ProcessStartInfo(download_path) { Verb = "runas", UseShellExecute = true });

                        if (process is null) throw new InvalidOperationException("Process.Start returned null");

                        await process.WaitForExitAsync(cancellation);

                        if (process.ExitCode != 0) {
                            Log.Error("Running .net installer failed with exit code " + process.ExitCode);

                            return;
                        }
                    } catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) {
                        Log.Error("User cancelled .net installer by declining elevation prompt");

                        return;
                    }
                } else {
                    Log.Warn("Failed to download .net installer");

                    return;
                }
            } else {
                Log.Warn("User cancelled .net download");

                return;
            }
        }

        cancellation.ThrowIfCancellationRequested();

        if (!LaunchOctgn(octgnPath, workingDirectory, commandLineArgs)) {
            Log.Warn("Could not launch OCTGN");

            MessageBox.Show($"OCTGN was unable to be launched. Please try again later", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        Log.Info("Launcher ran successfully");
    }

    private static async Task<string?> DownloadDotNetFramework48(CancellationToken cancellation) {
        Log.Info("Creating download path");

        var downloadPath = Path.Combine(Path.GetTempPath(), "netinstaller" + DateTime.Now.Ticks + ".exe");

        Log.Info($"Downloading .net installer from {DotNetDownloadUrl} to {downloadPath}");

        try {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            using (var wc = new WebClient()) {
                await wc.DownloadFileTaskAsync(DotNetDownloadUrl, downloadPath).WaitAsync(cancellation);

                if (cancellation.IsCancellationRequested) {
                    Log.Warn("Download cancelled");

                    wc.CancelAsync();

                    return null;
                }
            }
#pragma warning restore SYSLIB0014 // Type or member is obsolete

            return downloadPath;
        } catch (OperationCanceledException) {
            Log.Error("Download cancelled");

            return null;
        } catch (Exception ex) {
            Log.Error("Error downloading installer " + DotNetDownloadUrl, ex);

            MessageBox.Show($"There was an error downloading the .Net Framework", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    private static bool IsDotNet48OrLaterInstalled() {
        Log.Info("Getting .net from registry");

        try {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\")) {
                if (ndpKey == null) {
                    Log.Warn("No .net registry key found.");

                    return false;
                }

                var releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));

                Log.Info(".Net Release: " + releaseKey);

                if (releaseKey >= 528040) {
                    return true;
                }

                return false;
            }
        } catch (Exception ex) {
            Log.Error("Error getting .net from registry", ex);

            return false;
        }
    }

    private static string GetOctgnPath(bool isDebug) {
        var root_dir = System.AppContext.BaseDirectory;

        if (root_dir is null)
            throw new InvalidOperationException("Could not get app directory");

        var octgnPath = Path.Combine(root_dir, "Octgn.exe");

        if (isDebug) {
            octgnPath = "..\\..\\..\\..\\..\\Octgn\\bin\\Debug\\Octgn.exe";
        }

        return octgnPath;
    }

    private static bool LaunchOctgn(string octgnPath, string workingDirectory, string commandLineArgs) {
        try {
            var psi = new ProcessStartInfo(octgnPath) {
                Arguments = commandLineArgs,
                WorkingDirectory = workingDirectory
            };

            Process.Start(psi);

            return true;
        } catch (Exception ex) {
            Log.Error("Error launching octgn", ex);

            return false;
        }
    }
}
