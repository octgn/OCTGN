using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Octgn.Data;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Utils;

using Client = Octgn.Networking.Client;

namespace Octgn
{
    using System.Reflection;
    using System.Windows.Interop;
    using System.Windows.Media;

    using Microsoft.Win32;

    using Octgn.DataNew;
    using Octgn.Launcher;
    using Octgn.Windows;

    using log4net;
    using Octgn.Controls;

    public static class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static GameEngine GameEngine;

        public static string CurrentOnlineGameName = "";
        public static Skylabs.Lobby.Client LobbyClient;
        public static GameSettings GameSettings = new GameSettings();
        internal static Client Client;
        public static event Action OnOptionsChanged;


        internal static bool IsGameRunning;
        internal static ulong PrivateKey = ((ulong) Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom();

#pragma warning disable 67
        internal static event EventHandler<ServerErrorEventArgs> ServerError;
#pragma warning restore 67

        internal static bool IsHost { get; set; }

        internal static Dispatcher Dispatcher;

        internal static readonly TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
        internal static readonly TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);
        internal static readonly CacheTraceListener DebugListener = new CacheTraceListener();
        internal static Inline LastChatTrace;

        internal static bool TableOnly;

        private static bool _locationUpdating;

        static Program()
        {
            Log.Info("Starting OCTGN");
            try
            {
                Log.Debug("Setting rendering mode.");
                RenderOptions.ProcessRenderMode = Prefs.UseHardwareRendering ? RenderMode.Default : RenderMode.SoftwareOnly;
            }
            catch (Exception)
            {
                // if the system gets mad, best to leave it alone.
            }

            CheckSSLCertValidation();
            
            Log.Info("Creating Lobby Client");
            LobbyClient = new Skylabs.Lobby.Client(AppConfig.ChatServerPath);
            Log.Info("Adding trace listeners");
            Debug.Listeners.Add(DebugListener);
            DebugTrace.Listeners.Add(DebugListener);
            Trace.Listeners.Add(DebugListener);
            //BasePath = Path.GetDirectoryName(typeof (Program).Assembly.Location) + '\\';
            Log.Info("Setting Games Path");
        }

        internal static void Start()
        {
            Application.Current.MainWindow = new Window();
            KillOtherOctgn();
            bool isUpdate = RunUpdateChecker();
            if (isUpdate)
            {
                KillOtherOctgn(true);
                Program.Exit();
                return;
            }
            Log.Info("Ping back");
            System.Threading.Tasks.Task.Factory.StartNew(pingOB);

            bool tableOnlyFailed = false;

            int? hostport = null;
            Guid? gameid = null;

            var os = new Mono.Options.OptionSet()
                         {
                             { "t|table", x => TableOnly = true },
                             { "g|game=",x=> gameid=Guid.Parse(x)}
                         };
            try
            {
                os.Parse(Environment.GetCommandLineArgs());
            }
            catch (Exception e)
            {
                Log.Warn("Parse args exception: " +String.Join(",",Environment.GetCommandLineArgs()),e);
            }

            if (TableOnly)
            {
                try
                {
                    new GameTableLauncher().Launch(hostport,gameid);
                }
                catch (Exception e)
                {
                    Log.Warn("Couldn't host/join table mode",e);
                    tableOnlyFailed = true;
                    Program.Exit();
                }
            }

            if (!TableOnly || tableOnlyFailed)
            {

                Log.Info("Creating main window...");
                WindowManager.Main = new Main();
                Log.Info("Main window Created, Launching it.");
                Application.Current.MainWindow = WindowManager.Main;
                Log.Info("Main window set.");
                Log.Info("Launching Main Window");
                WindowManager.Main.Show();
                Log.Info("Main Window Launched");
            }

        }

        internal static void CheckSSLCertValidation()
        {
            Log.Info(string.Format("Bypass SSL certificate validation set to: {0}", Prefs.IgnoreSSLCertificates));
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
        }

        internal static List<string> hostList = new List<string>();
        internal static bool CertificateValidationCallBack(
         object sender,
         System.Security.Cryptography.X509Certificates.X509Certificate certificate,
         System.Security.Cryptography.X509Certificates.X509Chain chain,
         System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if(Prefs.IgnoreSSLCertificates)
            {
                return (true);
            }
            bool ret = (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None);
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)sender;

            if (!ret)
            {
                bool answer = ShowSSLErrorMessage(request.RequestUri.Host);
                if (answer)
                {
                    Prefs.IgnoreSSLCertificates = true;
                    KillOtherOctgn(true);
                    Program.Exit();
                }
                else
                {
                    ret = true;
                }
            }
            return (ret);
        }

        internal static bool ShowSSLErrorMessage(string host)
        {
            bool ret = false;
            if (hostList.Contains(host))
            {
                return (ret);
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Your machine isn't properly handling SSL Certificates. Would you like to disable ssl verification?");
            sb.AppendLine("If you do not disable SSL verification while these problems are happening everytime you might not be able to update/install/play games.");
            //sb.AppendLine(string.Format("{0}", host));

            MessageBoxResult result = MessageBox.Show(sb.ToString(), "SSL Error", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ret = true;
            }
            else
            {
                hostList.Add(host);
            }

            sb.Clear();
            sb = null;
            return (ret);
        }

        internal static void pingOB()
        {
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create("http://www.octgn.net/ping.php");
                request.GetResponse();
            }
            catch (Exception ex)
            {
                int i = 0;
            }
        }

        /// <summary>
        /// Runs update checker
        /// </summary>
        /// <returns>True if there is an update, else false</returns>
        internal static bool RunUpdateChecker()
        {
            Log.Info("Launching UpdateChecker");
            var uc = new UpdateChecker();
            uc.ShowDialog();
            Log.Info("UpdateChecker Done.");
            return uc.IsClosingDown;
        }

        internal static void KillOtherOctgn(bool force = false)
        {
            if (Environment.GetCommandLineArgs().Any(x => x.ToLowerInvariant().Contains("table"))) return;
            Log.Info("Getting octgn processes...");
            var pList = Process.GetProcessesByName("OCTGN");
            Log.Info("Got process list");
            if (pList != null && pList.Length > 0 && pList.Any(x => x.Id != Process.GetCurrentProcess().Id))
            {
                Log.Info("Found other octgn processes");
                if (!force)
                {
                    var res =
                        TopMostMessageBox.Show(
                            "Another instance of OCTGN is current running. Would you like to close it?",
                            "OCTGN",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                    if (res == MessageBoxResult.Yes)
                    {
                        foreach (var p in Process.GetProcessesByName("OCTGN"))
                        {
                            if (p.Id != Process.GetCurrentProcess().Id)
                            {
                                Log.Info("Killing process...");
                                try
                                {
                                    p.Kill();
                                }
                                catch (Exception ex)
                                {
                                    TopMostMessageBox.Show(
                                        "Could not kill other OCTGN's. If you are updating you will need to manually kill them or reboot your machine first.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                                    Log.Warn("KillOtherOctgn",ex);
                                } 
                                Log.Info("Killed Process");
                            }
                        }
                    }
                }
                else
                {
                    foreach (var p in Process.GetProcessesByName("OCTGN"))
                    {
                        if (p.Id != Process.GetCurrentProcess().Id)
                        {
                            Log.Info("Killing process...");
                                try
                                {
                                    p.Kill();
                                }
                                catch (Exception ex)
                                {
                                    TopMostMessageBox.Show(
                                        "Could not kill other OCTGN's. If you are updating you will need to manually kill them or reboot your machine first.",
                                        "Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                                    Log.Warn("KillOtherOctgn",ex);
                                } 
                            Log.Info("Killed Process");
                        }
                    }
                }
            }
        }

        internal static void FireOptionsChanged()
        {
            if(OnOptionsChanged != null)
                OnOptionsChanged.Invoke();
        }

        internal static void StartGame()
        {
            try
            {
                // Reset the InvertedTable flags if they were set and they are not used
                if (!Program.GameSettings.UseTwoSidedTable)
                    foreach (Player player in Player.AllExceptGlobal)
                        player.InvertedTable = false;

                // At start the global items belong to the player with the lowest id
                if (Player.GlobalPlayer != null)
                {
                    Player host = Player.AllExceptGlobal.OrderBy(p => p.Id).First();
                    foreach (Octgn.Play.Group group in Player.GlobalPlayer.Groups)
                        group.Controller = host;
                }
                if (WindowManager.PlayWindow != null) return;
                Program.Client.Rpc.Start();
                WindowManager.PlayWindow = new PlayWindow(Program.GameEngine.IsLocal);
                WindowManager.PlayWindow.Show();
                if (WindowManager.PreGameLobbyWindow != null)
                    WindowManager.PreGameLobbyWindow.Close();

            }
            catch (Exception e)
            {
                TopMostMessageBox.Show(
                    "Could not start game, there was an error with the specific game. Please contact the game developer");
                Log.Warn("StartGame Error",e);
            }
        }
        public static void StopGame()
        {
            if (Client != null)
            {
                Client.Disconnect();
                Client = null;
            }
            if(GameEngine != null)
                GameEngine.End();
            GameEngine = null;
            Dispatcher = null;
            IsGameRunning = false;
        }

        public static void Exit()
        {
            LogManager.Shutdown();
            Application.Current.Dispatcher.Invoke(new Action(() => { 
            Application.Current.MainWindow = null;
            if (LobbyClient != null)
                LobbyClient.Stop();

            try
            {
                if (WindowManager.DebugWindow != null)
                    if (WindowManager.DebugWindow.IsLoaded)
                        WindowManager.DebugWindow.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            if (WindowManager.PlayWindow != null)
                if (WindowManager.PlayWindow.IsLoaded)
                    WindowManager.PlayWindow.Close();
            //Apparently this can be null sometimes?
            if(Application.Current != null)
                Application.Current.Shutdown(0);
            }));
        }

        internal static void Print(Player player, string text)
        {
            string finalText = text;
            int i = 0;
            var args = new List<object>(2);
            Match match = Regex.Match(text, "{([^}]*)}");
            while (match.Success)
            {
                string token = match.Groups[1].Value;
                finalText = finalText.Replace(match.Groups[0].Value, "##$$%%^^LEFTBRACKET^^%%$$##" + i + "##$$%%^^RIGHTBRACKET^^%%$$##");
                i++;
                object tokenValue = token;
                switch (token)
                {
                    case "me":
                        tokenValue = player;
                        break;
                    default:
                        if (token.StartsWith("#"))
                        {
                            int id;
                            if (!int.TryParse(token.Substring(1), out id)) break;
                            ControllableObject obj = ControllableObject.Find(id);
                            if (obj == null) break;
                            tokenValue = obj;
                            break;
                        }
                        break;
                }
                args.Add(tokenValue);
                match = match.NextMatch();
            }
            args.Add(player);
            finalText = finalText.Replace("{", "").Replace("}", "");
            finalText = finalText.Replace("##$$%%^^LEFTBRACKET^^%%$$##", "{").Replace(
                "##$$%%^^RIGHTBRACKET^^%%$$##", "}");
            Trace.TraceEvent(TraceEventType.Information,
                             EventIds.Event | EventIds.PlayerFlag(player) | EventIds.Explicit, finalText, args.ToArray());
        }

        internal static void TracePlayerEvent(Player player, string message, params object[] args)
        {
            var args1 = new List<object>(args) {player};
            Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), message,
                             args1.ToArray());
        }

        internal static void TraceWarning(string message)
        {
            if (message == null) message = "";
            if (Trace == null) return;
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal static void TraceWarning(string message, params object[] args)
        {
            if (message == null) message = "";
            if (Trace == null) return;
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
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
    }
}
