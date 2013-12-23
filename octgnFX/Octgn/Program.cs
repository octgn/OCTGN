using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Octgn
{
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Windows.Interop;
    using System.Windows.Media;

    using Microsoft.Win32;

    using Octgn.Core;
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
        internal static ClientSocket Client;
        public static event Action OnOptionsChanged;


        internal static bool IsGameRunning;

        internal static bool InPreGame;

#pragma warning disable 67
        internal static event EventHandler<ServerErrorEventArgs> ServerError;
#pragma warning restore 67

        internal static bool IsHost { get; set; }

        internal static Dispatcher Dispatcher;

        internal static readonly TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
        internal static readonly TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);
        internal static readonly CacheTraceListener DebugListener = new CacheTraceListener();
        internal static Inline LastChatTrace;

        private static readonly SSLValidationHelper SSLHelper = new SSLValidationHelper();

        static Program()
        {
            Log.Info("Starting OCTGN");
            Octgn.Site.Api.ApiClient.Site = new Uri(AppConfig.WebsitePath);
            try
            {
                Log.Debug("Setting rendering mode.");
                RenderOptions.ProcessRenderMode = Prefs.UseHardwareRendering ? RenderMode.Default : RenderMode.SoftwareOnly;
            }
            catch (Exception)
            {
                // if the system gets mad, best to leave it alone.
            }
	    
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
            LobbyClient = new Skylabs.Lobby.Client(LobbyConfig.Get());
            Log.Info("Adding trace listeners");
            Debug.Listeners.Add(DebugListener);
            DebugTrace.Listeners.Add(DebugListener);
            Trace.Listeners.Add(DebugListener);
            //BasePath = Path.GetDirectoryName(typeof (Program).Assembly.Location) + '\\';
            Log.Info("Setting Games Path");
        }

        internal static void Start(string[] args)
        {
            //var win = new ShareDeck();
            //win.ShowDialog();
            //return;
			var launcher = CommandLineHandler.Instance.HandleArguments(Environment.GetCommandLineArgs());
            launcher.Launch();
            if (launcher.Shutdown)
            {
				if(Application.Current.MainWindow != null)
					Application.Current.MainWindow.Close();
                return;
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
            try
            {
                Program.Client.Rpc.Leave(Player.LocalPlayer);
            }
            catch
            {

            }
            if (Client != null)
            {
                Client.ForceDisconnect();
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
            try{SSLHelper.Dispose();}catch{}
            Sounds.Close();
            try
            {
                Program.Client.Rpc.Leave(Player.LocalPlayer);
            }
            catch
            {

            }
            UpdateManager.Instance.Stop();
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
            try
            {
                foreach (var w in WindowManager.ChatWindows.ToArray())
                {
                    try
                    {
                        if (w.IsLoaded) w.CloseDown();
                        w.Dispose();
                    }
                    catch(Exception e)
                    {
                        Log.Warn("Close chat window error", e);
                    }
                }
                WindowManager.ChatWindows = new ConcurrentBag<ChatWindow>();
            }
            catch (Exception e)
            {
                Log.Warn("Close chat window enumerate error",e);
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
                    Log.Warn("LaunchApplication Error " + path + " " + psi.Arguments,e);
            }
            catch (Exception e)
            {
                Log.Warn("LaunchApplication Error " + path + " " + psi.Arguments,e);
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

        public static void DoCrazyException(Exception e, string action)
        {
            var res = TopMostMessageBox.Show(action + Environment.NewLine + Environment.NewLine + "Are you going to be ok?", "Oh No!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.No)
            {
                res = TopMostMessageBox.Show(
                    "There there...It'll all be alright..." + Environment.NewLine + Environment.NewLine +
                    "Do you feel that we properly comforted you in this time of great sorrow?", "Comfort Dialog",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    TopMostMessageBox.Show(
                        "Great! Maybe you could swing by my server room later and we can hug it out.",
                        "Inappropriate Gesture Dialog", MessageBoxButton.OK, MessageBoxImage.Question);
                    TopMostMessageBox.Show("I'll be waiting...", "Creepy Dialog Box", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else if (res == MessageBoxResult.No)
                {
                    TopMostMessageBox.Show(
                        "Ok. We will sack the person responsible for that not so comforting message. Have a nice day!",
                        "Repercussion Dialog", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }
    }
}
