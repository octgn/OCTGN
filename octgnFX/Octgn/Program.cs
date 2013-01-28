using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Octgn.Data;
using Octgn.DeckBuilder;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Utils;
using Skylabs.Lobby;

using Client = Octgn.Networking.Client;

namespace Octgn
{
    using System.Configuration;

    using Octgn.Windows;

    public static class Program
    {
        public static Windows.DWindow DebugWindow;
        public static Main MainWindow;
        public static Windows.MainNew MainWindowNew;
        public static LauncherWindow LauncherWindow;
        public static DeckBuilderWindow DeckEditor;
        public static PlayWindow PlayWindow;
        public static List<ChatWindow> ChatWindows;
        public static PreGameLobbyWindow PreGameLobbyWindow { get; set; }

        public static Game Game;

        public static string CurrentOnlineGameName = "";
        public static Skylabs.Lobby.Client LobbyClient;
        public static GameSettings GameSettings = new GameSettings();
        public static GamesRepository GamesRepository = new GamesRepository();
        internal static Client Client;

        internal readonly static string WebsitePath;
        internal readonly static string ChatServerPath;
        internal readonly static string GameServerPath;
        internal static readonly string UpdateInfoPath;

        internal static readonly bool UseNewChrome;

        internal static bool IsGameRunning;
        internal static readonly string BasePath = Octgn.Library.Paths.BasePath;
        internal static readonly string GamesPath;
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

        private static bool _locationUpdating;

        static Program()
        {
            WebsitePath = ConfigurationManager.AppSettings["WebsitePath"];
            ChatServerPath = ConfigurationManager.AppSettings["ChatServerPath"];
            GameServerPath = ConfigurationManager.AppSettings["GameServerPath"];
#if(Release_Test)
            UpdateInfoPath = ConfigurationManager.AppSettings["UpdateCheckPathTest"];
#else
            UpdateInfoPath = ConfigurationManager.AppSettings["UpdateCheckPath"];
#endif
            bool.TryParse(ConfigurationManager.AppSettings["UseNewChrome"],out UseNewChrome);

            var pList = Process.GetProcessesByName("OCTGN");
            if(pList != null && pList.Length > 0 && pList.Any(x=>x.Id != Process.GetCurrentProcess().Id))
            {
                var res = MessageBox.Show("Another instance of OCTGN is current running. Would you like to close it?","OCTGN",MessageBoxButton.YesNo,MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    foreach (var p in Process.GetProcessesByName("OCTGN"))
                    {
                        if (p.Id != Process.GetCurrentProcess().Id)
                            p.Kill();
                    }
                }
            }



            LobbyClient = new Skylabs.Lobby.Client(ChatServerPath);
            Debug.Listeners.Add(DebugListener);
            DebugTrace.Listeners.Add(DebugListener);
            Trace.Listeners.Add(DebugListener);
            //BasePath = Path.GetDirectoryName(typeof (Program).Assembly.Location) + '\\';
            GamesPath = BasePath + @"Games\";
            if (UseNewChrome)
            {
                MainWindowNew = new MainNew();
                Application.Current.MainWindow = MainWindowNew;
            }
            else
            {
                LauncherWindow = new LauncherWindow();
                Application.Current.MainWindow = LauncherWindow;
                LobbyClient.Chatting.OnCreateRoom += Chatting_OnCreateRoom;
            }
        }

        static void Chatting_OnCreateRoom(object sender, NewChatRoom room)
        {
            if (ChatWindows.All(x => x.Room.Rid != room.Rid))
            {
                if(MainWindow != null) MainWindow.Dispatcher.Invoke(new Action(() => ChatWindows.Add(new ChatWindow(room))));
                else if(LauncherWindow != null) LauncherWindow.Dispatcher.Invoke(new Action(() => ChatWindows.Add(new ChatWindow(room))));
            }
        }
        internal static void StartGame()
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
            if (Program.PlayWindow != null) return;
            Program.Client.Rpc.Start();
            Program.PlayWindow = new PlayWindow(Program.Game.IsLocal);
            Program.PlayWindow.Show();
            if(Program.PreGameLobbyWindow != null)
                Program.PreGameLobbyWindow.Close();
        }
        public static void StopGame()
        {
            if (Client != null)
            {
                Client.Disconnect();
                Client = null;
            }
            if(Game != null)
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
            Prefs.LoginLocation = new Point(LauncherWindow.Left,LauncherWindow.Top);
            _locationUpdating = false;
        }

        public static void Exit()
        {
            Application.Current.MainWindow = null;
            if (LobbyClient != null)
                LobbyClient.Stop();

            SaveLocation();
            try
            {
                if (DebugWindow != null)
                    if (DebugWindow.IsLoaded)
                        DebugWindow.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            if (LauncherWindow != null)
                if (LauncherWindow.IsLoaded)
                    LauncherWindow.Close();
            if (MainWindow != null)
                if (MainWindow.IsLoaded)
                    MainWindow.Close();
            if (PlayWindow != null)
                if (PlayWindow.IsLoaded)
                    PlayWindow.Close();
            try
            {
                foreach (ChatWindow cw in ChatWindows.Where(cw => cw.IsLoaded))
                {
                    cw.CloseChatWindow();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            //Apparently this can be null sometimes?
            if(Application.Current != null)
                Application.Current.Shutdown(0);
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
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal static void TraceWarning(string message, params object[] args)
        {
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        }
    }
}