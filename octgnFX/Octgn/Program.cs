using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Octgn.Play;
using Skylabs.Lobby;
using RE = System.Text.RegularExpressions;
using Octgn.Launcher;

namespace Octgn
{
    public static class Program
    {
        public static DWindow DebugWindow;
        public static Octgn.Launcher.Main ClientWindow;
        public static Launcher.LauncherWindow LauncherWindow;
        public static DeckBuilder.DeckBuilderWindow DeckEditor;
        public static PlayWindow PlayWindow;
        public static List<ChatWindow> ChatWindows;

        public static Prefs prefs = new Prefs();

        public static Game Game;
        public static LobbyClient lobbyClient;
        public static Octgn.Data.GameSettings GameSettings = new Octgn.Data.GameSettings();
        public static Data.GamesRepository GamesRepository;
        internal static Networking.Client Client;

        internal static bool IsGameRunning = false;
        internal readonly static string BasePath;
        internal readonly static string GamesPath;

        internal static ulong PrivateKey = ((ulong)Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom();

        internal static event EventHandler<ServerErrorEventArgs> ServerError;

        internal static bool IsHost { get; set; }

        internal static Dispatcher Dispatcher;

        internal readonly static TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
        internal readonly static TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);
        internal readonly static CacheTraceListener DebugListener = new CacheTraceListener();
        internal static System.Windows.Documents.Inline LastChatTrace;

        private static Process LobbyServerProcess;
        private static bool _locationUpdating = false;

#if(TestServer)
        public static TestServerSettings LobbySettings = TestServerSettings.Default;
#else
    #if(DEBUG)
        public static DEBUGLobbySettings LobbySettings = DEBUGLobbySettings.Default;
    #else
        public static lobbysettings LobbySettings = lobbysettings.Default;
#endif
#endif

        static Program()
        {
            System.Diagnostics.Debug.Listeners.Add(DebugListener);
            DebugTrace.Listeners.Add(DebugListener);
            Trace.Listeners.Add(DebugListener);
            BasePath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + '\\';
            GamesPath = BasePath + @"Games\";
            StartLobbyServer();
            Exception e = new Exception();
            string s = e.Message.Substring(0);
            Program.LauncherWindow = new Launcher.LauncherWindow();
            Application.Current.MainWindow = Program.LauncherWindow;
#if(DEBUG)
            Program.LauncherWindow.Show();
            ChatWindows = new List<ChatWindow>();
#else
            UpdateChecker uc = new UpdateChecker();
            uc.ShowDialog();
            if (!uc.IsClosingDown)
            {
                Program.LauncherWindow.Show();
                ChatWindows = new List<ChatWindow>();
            }
            else
            {
                Application.Current.MainWindow = null;
                Program.LauncherWindow.Close();
                Program.Exit();
            }
#endif
        }
        public static void StartLobbyServer()
        {
#if(DEBUG)
            LobbyServerProcess = new Process();
            LobbyServerProcess.StartInfo.FileName = Directory.GetCurrentDirectory() + "/Skylabs.LobbyServer.exe";
            try
            {
                LobbyServerProcess.Start();
                return;
            }
            catch (Exception e)
            {
                DebugTrace.TraceEvent(TraceEventType.Error,0,e.StackTrace);
            }
#endif
        }
        public static void StopGame()
        {
            if (Client != null)
            {
                Client.Disconnect(); Client = null;
            }
            Game.End(); Game = null;
            Dispatcher = null;
            Database.Close();
            IsGameRunning = false;
        }

        public static void SaveLocation()
        {
            if (!_locationUpdating)
            {
                if (LauncherWindow != null && LauncherWindow.IsLoaded)
                {
                    _locationUpdating = true;
                    Properties.Settings.Default.LoginLeftLoc = LauncherWindow.Left;
                    Properties.Settings.Default.LoginTopLoc = LauncherWindow.Top;
                    Properties.Settings.Default.Save();
                    _locationUpdating = false;
                }
            }
        }

        public static void Exit()
        {
            Application.Current.MainWindow = null;
            if (Program.lobbyClient != null && Program.lobbyClient.Connected)
                Program.lobbyClient.Stop();

            SaveLocation();
            try
            {
                DebugWindow.Close();
            }
            catch(Exception)
            {
            }
            if(LauncherWindow != null)
                if(LauncherWindow.IsLoaded)
                    LauncherWindow.Close();
            if (ClientWindow != null)
                if (ClientWindow.IsLoaded)
                    ClientWindow.Close();
            if(PlayWindow != null)
                if(PlayWindow.IsLoaded)
                    PlayWindow.Close();
            try
            {
                foreach (ChatWindow cw in ChatWindows)
                {
                    cw.CloseChatWindow();
                }
            }
            catch (Exception)
            {
            }
            if (LobbyServerProcess != null)
            {
                try
                {
                    LobbyServerProcess.Kill();
                }
                catch (Exception)
                {
                }
            }
            Application.Current.Shutdown(0);
        }

        //TODO: Get rid of this method at some point
        internal static void OnServerError(string serverMessage)
        {
            var args = new ServerErrorEventArgs() { Message = serverMessage };
            if(ServerError != null)
                ServerError(null, args);
            if(args.Handled) return;

            MessageBox.Show(Application.Current.MainWindow,
                "The server has returned an error:\n" + serverMessage,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal static void Print(Player player, string text)
        {
            string finalText = text;
            int i = 0;
            List<object> args = new List<object>(2);
            RE.Match match = RE.Regex.Match(text, "{([^}]*)}");
            while(match.Success)
            {
                string token = match.Groups[1].Value;
                finalText = finalText.Replace(match.Groups[0].Value, "{" + i + "}");
                i++;
                object tokenValue = token;
                switch(token)
                {
                    case "me": tokenValue = player; break;
                    default:
                        if(token.StartsWith("#"))
                        {
                            int id;
                            if(!int.TryParse(token.Substring(1), out id)) break;
                            var obj = ControllableObject.Find(id);
                            if(obj == null) break;
                            tokenValue = obj;
                            break;
                        }
                        break;
                }
                args.Add(tokenValue);
                match = match.NextMatch();
            }
            args.Add(player);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player) | EventIds.Explicit, finalText, args.ToArray());
        }

        internal static void TracePlayerEvent(Player player, string message, params object[] args)
        {
            List<object> args1 = new List<object>(args);
            args1.Add(player);
            Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), message, args1.ToArray());
        }

        internal static void TraceWarning(string message)
        {
            Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal static void TraceWarning(string message, params object[] args)
        {
            Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        }
    }
}