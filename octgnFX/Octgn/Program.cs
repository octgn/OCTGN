/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Threading;
using Octgn.Data;
using Octgn.DataNew.Entities;
using Octgn.Utils;
using System.Reflection;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using Octgn.Core;
using Octgn.Core.Play;
using Octgn.Windows;
using log4net;
using Octgn.Controls;
using Octgn.Online.Hosting;
using Octgn.Online;
using Octgn.Communication.Tcp;
using Octgn.Core.DiscordIntegration;

namespace Octgn
{
    public static class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static JodsEngineIntegration JodsEngine { get; private set; }

        public static string CurrentOnlineGameName = "";
        public static Library.Communication.Client LobbyClient;

        public static GameSettings GameSettings { get; set; }
        public static event Action OnOptionsChanged;

        internal static bool IsGameRunning;

        internal static bool InPreGame;

        internal static bool IsHost { get; set; }
        internal static GameMode GameMode { get; set; }

        internal static Dispatcher Dispatcher;

        private static SSLValidationHelper SSLHelper;

        public static GameMessageDispatcher GameMess { get; internal set; }

        public static bool DeveloperMode { get; private set; }

        /// <summary>
        /// Is properly set at Program.Start()
        /// </summary>
        public static bool IsReleaseTest { get; set; }

        public static string SessionKey { get; set; }
        public static string UserId { get; set; }
        public static HostedGame CurrentHostedGame { get; internal set; }

        public static DiscordWrapper Discord { get; private set; }

        private static bool shutDown = false;

        static Program()
        {
            //Do not put anything here, it'll just lead to pain and confusion
        }

        internal static void Start(string[] args, bool isTestRelease)
        {
            Log.Info("Start");
            IsReleaseTest = isTestRelease;

            Log.Info("Setting SSL Validation Helper");
            SSLHelper = new SSLValidationHelper();

            Log.Info("Setting api path");
            Octgn.Site.Api.ApiClient.DefaultUrl = new Uri(AppConfig.WebsitePath);

            JodsEngine = new JodsEngineIntegration();

            Log.Info("Configuring game feeds");
            ConfigureGameFeedTimeout();

            try
            {
                Log.Debug("Setting rendering mode.");
                RenderOptions.ProcessRenderMode = Prefs.UseHardwareRendering ? RenderMode.Default : RenderMode.SoftwareOnly;
            }
            catch (Exception)
            {
                // if the system gets mad, best to leave it alone.
            }

            Log.Info("Setting temp main window");
            Application.Current.MainWindow = new Window();
            try
            {
                Log.Info("Checking if admin");
                var isAdmin = UacHelper.IsProcessElevated && UacHelper.IsUacEnabled;
                if (isAdmin)
                {
                    MessageBox.Show(
                        "You are currently running OCTGN as Administrator. It is recommended that you run as a standard user, or you will most likely run into problems. Please exit OCTGN and run as a standard user.",
                        "WARNING",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
            }
            catch (Exception e)
            {
                Log.Warn("Couldn't check if admin", e);
            }

            Log.Info("Creating Lobby Client");
            var handshaker = new DefaultHandshaker();
            var connectionCreator = new TcpConnectionCreator(handshaker);
            var lobbyClientConfig = new LibraryCommunicationClientConfig(connectionCreator);

            LobbyClient = new Octgn.Library.Communication.Client(
                lobbyClientConfig,
                typeof(Program).Assembly.GetName().Version
            );

            //Log.Info("Adding trace listeners");
            //Debug.Listeners.Add(DebugListener);
            //DebugTrace.Listeners.Add(DebugListener);
            //Trace.Listeners.Add(DebugListener);
            //ChatLog = new CacheTraceListener();
            //Trace.Listeners.Add(ChatLog);

            Log.Info("Setting Games Path");
            GameSettings = new GameSettings();
            if (shutDown)
            {
                Log.Info("Shutdown Time");
                if (Application.Current.MainWindow != null)
                    Application.Current.MainWindow.Close();
                return;
            }

            Log.Info("Decide to ask about wine");
            if (Prefs.AskedIfUsingWine == false)
            {
                Log.Info("Asking about wine");
                var res = MessageBox.Show("Are you running OCTGN on Linux or a Mac using Wine?", "Using Wine",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    Prefs.AskedIfUsingWine = true;
                    Prefs.UsingWine = true;
                    Prefs.UseHardwareRendering = false;
                    Prefs.UseGameFonts = false;
                    Prefs.UseWindowTransparency = false;
                }
                else if (res == MessageBoxResult.No)
                {
                    Prefs.AskedIfUsingWine = true;
                    Prefs.UsingWine = false;
                    Prefs.UseHardwareRendering = true;
                    Prefs.UseGameFonts = true;
                    Prefs.UseWindowTransparency = true;
                }
            }
            // Check for desktop experience
            if (Prefs.UsingWine == false)
            {
                try
                {
                    Log.Debug("Checking for Desktop Experience");
                    var objMC = new ManagementClass("Win32_ServerFeature");
                    // Expected Exception: System.Management.ManagementException
                    // Additional information: Not found
                    var objMOC = objMC.GetInstances();
                    bool gotIt = false;
                    foreach (var objMO in objMOC)
                    {
                        if ((UInt32)objMO["ID"] == 35)
                        {
                            Log.Debug("Found Desktop Experience");
                            gotIt = true;
                            break;
                        }
                    }
                    if (!gotIt)
                    {
                        var res =
                            MessageBox.Show(
                                "You are running OCTGN without the windows Desktop Experience installed. This WILL cause visual, gameplay, and sound issues. Though it isn't required, it is HIGHLY recommended. \n\nWould you like to be shown a site to tell you how to turn it on?",
                                "Windows Desktop Experience Missing", MessageBoxButton.YesNo,
                                MessageBoxImage.Exclamation);
                        if (res == MessageBoxResult.Yes)
                        {
                            LaunchUrl(
                                "http://blogs.msdn.com/b/findnavish/archive/2012/06/01/enabling-win-7-desktop-experience-on-windows-server-2008.aspx");
                        }
                        else
                        {
                            MessageBox.Show("Ok, but you've been warned...", "Warning", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(
                        "Check desktop experience error. An error like 'Not Found' is normal and shouldn't be worried about",
                        e);
                }
            }
            // Send off user/computer stats
            try
            {
                var osver = System.Environment.OSVersion.VersionString;
                var osBit = Win32.Is64BitOperatingSystem;
                var procBit = Win32.Is64BitProcess;
                var issubbed = SubscriptionModule.Get().IsSubscribed;
                var iswine = Prefs.UsingWine;
                // Use the API to submit info
            }
            catch (Exception e)
            {
                Log.Warn("Sending stats error", e);
            }

            Log.Info("Getting Launcher");
            Launchers.ILauncher launcher = CommandLineHandler.Instance.HandleArguments(Environment.GetCommandLineArgs());
            DeveloperMode = CommandLineHandler.Instance.DevMode;

            launcher.Launch().Wait();

            if (launcher.Shutdown)
            {
                if (Application.Current.MainWindow != null)
                    Application.Current.MainWindow.Close();
                return;
            }

            //Discord = new DiscordWrapper();
            //Discord.Error += Discord_Error;
        }

        private static void Discord_Error(object sender, Exception ex)
        {
            Log.Warn($"Discord Error: {ex.Message}", ex);
        }

        internal static void FireOptionsChanged()
        {
            if (OnOptionsChanged != null)
                OnOptionsChanged.Invoke();
        }

        public static void Exit()
        {
            try { SSLHelper.Dispose(); }
            catch (Exception e) {
                Log.Error( "SSLHelper Dispose Exception", e );
            };
            Sounds.Close();
            UpdateManager.Instance.Stop();
            LogManager.Shutdown();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (LobbyClient != null)
                    LobbyClient.Stop();
                WindowManager.Shutdown();

                //Apparently this can be null sometimes?
                if (Application.Current != null)
                    Application.Current.Shutdown(0);
            }));

        }

        public static void LaunchUrl(string url)
        {
            if (url == null) return;
            if (GetDefaultBrowserPath() == null)
            {
                Dispatcher d = Dispatcher;
                if (d == null) d = Application.Current.Dispatcher;
                if (d == null) d = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                if (d == null && Application.Current != null && Application.Current.MainWindow != null) d = Application.Current.MainWindow.Dispatcher;
                if (d == null) return;
                d.Invoke(new Action(() => new BrowserWindow(url).Show()));
                return;
            }
            Process.Start(url);
        }

        public static void LaunchApplication(string path, params string[] args)
        {
            var psi = new ProcessStartInfo(path, String.Join(" ", args));
            try
            {
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode != 1223)
                    Log.Warn("LaunchApplication Error " + path + " " + psi.Arguments, e);
            }
            catch (Exception e)
            {
                Log.Warn("LaunchApplication Error " + path + " " + psi.Arguments, e);
            }

        }

        public static string GetDefaultBrowserPath()
        {
            string defaultBrowserPath = null;
            try
            {
                RegistryKey regkey;

                // Check if we are on Vista or Higher
                OperatingSystem OS = Environment.OSVersion;
                if ((OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6))
                {
                    regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\shell\\Associations\\UrlAssociations\\http\\UserChoice", false);
                    if (regkey != null)
                    {
                        defaultBrowserPath = regkey.GetValue("Progid").ToString();
                    }
                    else
                    {
                        regkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\IE.HTTP\\shell\\open\\command", false);
                        defaultBrowserPath = regkey.GetValue("").ToString();
                    }
                }
                else
                {
                    regkey = Registry.ClassesRoot.OpenSubKey("http\\shell\\open\\command", false);
                    defaultBrowserPath = regkey.GetValue("").ToString();
                }



            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return defaultBrowserPath;
        }

        public static void ConfigureGameFeedTimeout()
        {
            var manager = GameFeedManager.Get();
            manager.FeedUpdateTimeout = TimeSpan.FromSeconds(AppConfig.GameFeedTimeoutSeconds);
        }

        public static void DoCrazyException(Exception e, string action)
        {
            var res = TopMostMessageBox.Show(action + Environment.NewLine + Environment.NewLine + "Are you going to be ok?", "Oh No!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
