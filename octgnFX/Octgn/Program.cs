using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Octgn.Data;
using Octgn.DeckBuilder;
using Octgn.Launcher;
using Octgn.Networking;
using Octgn.Play;
using Skylabs.Lobby;
using RE = System.Text.RegularExpressions;

namespace Octgn
{
    public static class Program
    {
        public static DWindow DebugWindow;
        public static Main ClientWindow;
        public static LauncherWindow LauncherWindow;
        public static DeckBuilderWindow DeckEditor;
        public static PlayWindow PlayWindow;
        public static List<ChatWindow> ChatWindows;

        public static Game Game;
        public static LobbyClient LobbyClient;
        public static GameSettings GameSettings = new GameSettings();
        public static GamesRepository GamesRepository;
        internal static Client Client;

        internal static bool IsGameRunning;
        internal static readonly string BasePath;
        internal static readonly string GamesPath;

        internal static ulong PrivateKey = ((ulong) Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom();

        internal static event EventHandler<ServerErrorEventArgs> ServerError;

        internal static bool IsHost { get; set; }

        internal static Dispatcher Dispatcher;

        internal static readonly TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
        internal static readonly TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);
        internal static readonly CacheTraceListener DebugListener = new CacheTraceListener();
        internal static Inline LastChatTrace;

        private static Process _lobbyServerProcess;
        private static bool _locationUpdating;

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
            Debug.Listeners.Add(DebugListener);
            DebugTrace.Listeners.Add(DebugListener);
            Trace.Listeners.Add(DebugListener);
            BasePath = Path.GetDirectoryName(typeof (Program).Assembly.Location) + '\\';
            GamesPath = BasePath + @"Games\";
            StartLobbyServer();
            //var e = new Exception();
            //string s = e.Message.Substring(0);
            LauncherWindow = new LauncherWindow();
            Application.Current.MainWindow = LauncherWindow;
        }

        public static void StartLobbyServer()
        {
#if(DEBUG)
            _lobbyServerProcess = new Process
                                      {
                                          StartInfo =
                                              {FileName = Directory.GetCurrentDirectory() + "/Skylabs.LobbyServer.exe"}
                                      };
            try
            {
                _lobbyServerProcess.Start();
            }
            catch (Exception e)
            {
                DebugTrace.TraceEvent(TraceEventType.Error, 0, e.StackTrace);
            }
#endif
        }

        public static void StopGame()
        {
            if (Client != null)
            {
                Client.Disconnect();
                Client = null;
            }
            Game.End();
            Game = null;
            Dispatcher = null;
            Database.Close();
            IsGameRunning = false;
        }

        public static void SaveLocation()
        {
            if (_locationUpdating) return;
            if (LauncherWindow == null || !LauncherWindow.IsLoaded) return;
            _locationUpdating = true;
            SimpleConfig.WriteValue("LoginLeftLoc", LauncherWindow.Left.ToString(CultureInfo.InvariantCulture));
            SimpleConfig.WriteValue("LoginTopLoc", LauncherWindow.Top.ToString(CultureInfo.InvariantCulture));
            _locationUpdating = false;
        }

        public static void Exit()
        {
            Application.Current.MainWindow = null;
            if (LobbyClient != null && LobbyClient.Connected)
                LobbyClient.Stop();

            SaveLocation();
            try
            {
                DebugWindow.Close();
            }
            catch (Exception)
            {
            }
            if (LauncherWindow != null)
                if (LauncherWindow.IsLoaded)
                    LauncherWindow.Close();
            if (ClientWindow != null)
                if (ClientWindow.IsLoaded)
                    ClientWindow.Close();
            if (PlayWindow != null)
                if (PlayWindow.IsLoaded)
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
            if (_lobbyServerProcess != null)
            {
                try
                {
                    _lobbyServerProcess.Kill();
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
            var args = new ServerErrorEventArgs {Message = serverMessage};
            if (ServerError != null)
                ServerError(null, args);
            if (args.Handled) return;

            MessageBox.Show(Application.Current.MainWindow,
                            "The server has returned an error:\n" + serverMessage,
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                finalText = finalText.Replace(match.Groups[0].Value, "{" + i + "}");
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
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal static void TraceWarning(string message, params object[] args)
        {
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        }
    }
}