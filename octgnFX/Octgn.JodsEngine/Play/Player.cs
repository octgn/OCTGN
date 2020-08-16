﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using log4net;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.Play;
using Octgn.DataNew.Entities;
using Octgn.Site.Api;

namespace Octgn.Play
{
    public sealed class Player : INotifyPropertyChanged, IPlayPlayer
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Static members

        // Contains all players in this game (TODO: Rename to All, then cleanup all the dependancies)
        private static readonly ObservableCollection<Player> all = new ObservableCollection<Player>();

        private static readonly ObservableCollection<Player> allExceptGlobal = new ObservableCollection<Player>();

        private static readonly ObservableCollection<Player> spectators = new ObservableCollection<Player>();

        private static Color _black = Color.FromRgb(0x00, 0x00, 0x00);

        private static Color[] _playerColors =
        {
            Color.FromRgb(0x00, 0x80, 0x00),
            Color.FromRgb(0xcc, 0x00, 0x00),
            Color.FromRgb(0x00, 0x00, 0x80),
            Color.FromRgb(0x80, 0x00, 0x80),
            Color.FromRgb(0xcc, 0x66, 0x00),
            Color.FromRgb(0x00, 0x80, 0x80),
            Color.FromRgb(0x66, 0x4b, 0x32),
            Color.FromRgb(0x50, 0x20, 0x60),
            Color.FromRgb(0x80, 0x80, 0x00),
            Color.FromRgb(0xFF, 0x00, 0x00),
            Color.FromRgb(0x80, 0x80, 0x80),
            Color.FromRgb(0x20, 0x60, 0x20),
            Color.FromRgb(0xFF, 0x00, 0xFF),
            Color.FromRgb(0x00, 0x00, 0xFF)
        };

        public static Player LocalPlayer;
        // May be null if there's no global lPlayer in the game definition
        public static Player GlobalPlayer;

        // Get all players in the game
        public static ObservableCollection<Player> All
        {
            get { return all; }
        }

        // Get all players in the game, except a possible Global lPlayer
        public static ObservableCollection<Player> AllExceptGlobal
        {
            get
            {
                return allExceptGlobal;
            }
        }

        public static ObservableCollection<Player> Spectators
        {
            get { return spectators; }
        }

        // Number of players
        internal static int Count
        {
            get { return GlobalPlayer == null ? all.Count : all.Count - 1; }
        }

        // Find a lPlayer with his id
        internal static Player Find(byte id)
        {
            return all.FirstOrDefault(p => p.Id == id);
        }

        internal static Player FindIncludingSpectators(byte id)
        {
            return all.Union(spectators).FirstOrDefault(p => p.Id == id);
        }

        // Resets the lPlayer list
        internal static void Reset()
        {
            lock (all)
            {
                all.Clear();
                spectators.Clear();
                LocalPlayer = GlobalPlayer = null;
            }
        }



        public static event Action OnLocalPlayerWelcomed;
        public static void FireLocalPlayerWelcomed()
        {
            if (OnLocalPlayerWelcomed != null)
                OnLocalPlayerWelcomed.Invoke();
        }

        // May be null if we're in pure server mode

        internal static event EventHandler<PlayerEventArgs> PlayerAdded;
        internal static event EventHandler<PlayerEventArgs> PlayerRemoved;

        static Player()
        {
            all.CollectionChanged += (sender, args) =>
            {
                allExceptGlobal.Clear();
                foreach (var p in all.ToArray().Where(x => !x.IsGlobal))
                {
                    allExceptGlobal.Add(p);
                }
                foreach (var p in all.Union(spectators))
                {
                    p.OnPropertyChanged("All");
                    p.OnPropertyChanged("AllExceptGlobal");
                    p.OnPropertyChanged("Count");
                    p.OnPropertyChanged("Spectators");
                    p.OnPropertyChanged("WaitingOnPlayers");
                    p.OnPropertyChanged("Ready");
                }

                if(Program.GameEngine != null)
                    Program.Discord.UpdateStatusInGame(Program.CurrentHostedGame, Program.IsHost, Program.GameEngine.IsReplay, Program.GameEngine.Spectator, Program.InPreGame, Player.AllExceptGlobal.Count());
            };
        }

        #endregion

        #region Public fields and properties

        internal readonly ulong PublicKey; // Public cryptographic key
        private readonly Counter[] _counters; // Counters this lPlayer owns

        private readonly Group[] _groups; // Groups this lPlayer owns
        private Brush _solidBrush;
        private Brush _transparentBrush;
        private bool _invertedTable;
        private string _name;
        private byte _id;
        private bool _ready;
        private bool _spectator;
        private bool _subscriber;
        private int _disconnectPercent;
        private string _userIcon;

        private PlayerState state;

        public bool WaitingOnPlayers
        {
            get
            {
                var ret = AllExceptGlobal.Any(x => !x.Ready);
                Log.Debug("WaitingOnPlayers Checking Players Ready Status");
                foreach (var p in AllExceptGlobal)
                {
                    Log.DebugFormat("Player {0} Ready={1}", p.Name, p.Ready);
                }
                Log.DebugFormat("WaitingOnPlayers = {0}", ret);
                Log.Debug("WaitingOnPlayers Done Checking Players Ready Status");
                return ret;
            }
        }

        public bool Ready
        {
            get
            {
                return _ready;
            }
            set
            {
                //if (value == _ready) return;
                _ready = value;
                Log.DebugFormat("Player {0} Ready = {1}", this.Name, value);
                this.OnPropertyChanged("Ready");
                foreach (var p in all)
                    p.OnPropertyChanged("WaitingOnPlayers");
                foreach (var p in spectators)
                    p.OnPropertyChanged("WaitingOnPlayers");
            }
        }

        public Counter[] Counters
        {
            get { return _counters; }
        }

        public bool Subscriber
        {
            get { return _subscriber; }
        }

        public Group[] IndexedGroups
        {
            get { return _groups; }
        }

        public IEnumerable<Group> Groups
        {
            get { return _groups.Where(g => g != null); }
        }

        public bool CanKick { get; private set; }

        public bool IsHostOrLocal
        {
            get
            {
                return (this == LocalPlayer) || Program.IsHost;
            }
        }

        public bool CanChangePlayerSettings
        {
            get
            {
                return IsHostOrLocal && !Program.GameEngine.IsReplay;
            }
        }

        public bool Spectator
        {
            get { return _spectator; }
            set
            {
                if (Program.Client == null) return;
                Log.InfoFormat("[Spectator]{0} {1}", this, value);
                if (_spectator == value) return;
                this.UpdateSettings(InvertedTable, value, true);
            }
        }

        public Dictionary<string, string> GlobalVariables { get; private set; }

        public byte Id // Identifier
        {
            get { return _id; }
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Name // Nickname
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public string UserId { get; }

        public int DisconnectPercent
        {
            get { return _disconnectPercent; }
            set
            {
                if (_disconnectPercent == value) return;
                _disconnectPercent = value;
                OnPropertyChanged("DisconnectPercent");
            }
        }

        public string UserIcon
        {
            get { return _userIcon; }
            set
            {
                if (_userIcon == value) return;
                _userIcon = value;
                OnPropertyChanged("UserIcon");
            }
        }

        public bool IsGlobalPlayer
        {
            get { return Id == 0; }
        }

        /// <summary>
        /// True if the lPlayer plays on the opposite side of the table (for two-sided table only)
        /// </summary>
        public bool InvertedTable
        {
            get { return _invertedTable; }
            set
            {
                Log.InfoFormat("[InvertedTable]{0} {1}", this, value);
                this.UpdateSettings(value, Spectator, true);
            }
        }

        //Color for the chat.
        // Associated color
        public Color Color { get; set; }
        public Color ActualColor { get; set; }

        // Work around a WPF binding bug ? Borders don't seem to bind correctly to Color!
        public Brush Brush
        {
            get { return _solidBrush; }
            set { _solidBrush = value; }
        }

        public Brush TransparentBrush
        {
            get { return _transparentBrush; }
            set { _transparentBrush = value; }
        }

        public PlayerState State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (value == this.state) return;
                Log.DebugFormat("Player {0} State = {1}", this.Name, value);
                this.state = value;
                this.OnPropertyChanged("State");
                this.OnPropertyChanged("Ready");
                foreach (var p in all)
                    p.OnPropertyChanged("WaitingOnPlayers");
            }
        }

        public BitmapImage SleeveImage {
            get => _sleeveImage;
            set {
                if(value != _sleeveImage) {
                    _sleeveImage = value;
                    OnPropertyChanged(nameof(SleeveImage));
                }
            }
        }
        private BitmapImage _sleeveImage;

        public void SetSleeve(ISleeve sleeve) {
            SleeveImage = sleeve.GetImage();
        }

        //Set the player's color based on their id.
        public void SetPlayerColor(int idx)
        {
            // Create the Player's Color
            Color playerColor;
            if (idx == 0 || idx == 255)
                playerColor = _black;
            else
                playerColor = _playerColors[(idx - 1) % _playerColors.Length];

            ActualColor = playerColor;
            if (this == LocalPlayer)
            {
                return;
            }
            Color = playerColor;
            _solidBrush = new SolidColorBrush(Color);
            _solidBrush.Freeze();
            _transparentBrush = new SolidColorBrush(Color) { Opacity = 0.4 };
            _transparentBrush.Freeze();

            //Notify clients that this has changed
            OnPropertyChanged("Color");
            OnPropertyChanged("Brush");
            OnPropertyChanged("TransparentBrush");
        }

        public void SetPlayerColor(string colorHex)
        {
            var convertFromString = ColorConverter.ConvertFromString(colorHex);
            if (convertFromString != null)
            {
                ActualColor = (Color)convertFromString;

                if (this == LocalPlayer)
                {
                    return;
                }

                Color = (Color) convertFromString;

                _solidBrush = new SolidColorBrush(Color);
                _transparentBrush = new SolidColorBrush(Color) {Opacity = 0.4};

                OnPropertyChanged("Color");
                OnPropertyChanged("Brush");
                OnPropertyChanged("TransparentBrush");
            }
        }

        #endregion

        #region Public interface

        internal void SetupPlayer(bool spectator)
        {
            State = PlayerState.Connected;
        }

        public bool IsLocal { get; }

        // C'tor
        internal Player(DataNew.Entities.Game g, string name, string userId, byte id, ulong pkey, bool spectator, bool local, bool isReplay)
        {
            // Cannot access Program.GameEngine here, it's null.

            Id = id;
            _name = name;
            IsLocal = local;

            if (!string.IsNullOrWhiteSpace(userId)) {
                UserId = userId;

                if (!userId.StartsWith("##LOCAL##")) {
                    Task.Factory.StartNew(async () => {
                        try {
                            var c = new ApiClient();

                            var apiUser = await c.UserFromUserId(userId);
                            if (apiUser != null) {
                                this.DisconnectPercent = apiUser.DisconnectPercent;
                                this.UserIcon = apiUser.IconUrl;
                            }
                        } catch (Exception e) {
                            Log.Warn("Player() Error getting api stuff", e);
                        }
                    });
                }
            } else {
                UserId = $"##LOCAL##{name}:{id}";
            }
            _spectator = spectator;
            SetupPlayer(Spectator);
            PublicKey = pkey;
            if (Spectator == false)
            {
                all.Add(this);
            }
            else
            {
                spectators.Add(this);
            }
            // Assign subscriber status
            _subscriber = SubscriptionModule.Get().IsSubscribed ?? false;
            //Create the color brushes
            SetPlayerColor(id);
            // Create counters
            _counters = new Counter[0];
            if (g.Player.Counters != null)
                _counters = g.Player.Counters.Select(x => new Counter(this, x, isReplay)).ToArray();
            // Create global variables
            GlobalVariables = new Dictionary<string, string>();
            foreach (var varD in g.Player.GlobalVariables)
                GlobalVariables.Add(varD.Value.Name, varD.Value.Value);
            // Create groups
            _groups = new Group[0];
            if (g.Player.Groups != null)
            {
                var tempGroups = g.Player.Groups.ToArray();
                _groups = new Group[tempGroups.Length];
                for (int i = 0; i < IndexedGroups.Length; i++)
                    _groups[i] = new Pile(this, tempGroups[i]);
            }
            if (Spectator == false)
            {
                // Raise the event
                if (PlayerAdded != null) PlayerAdded(null, new PlayerEventArgs(this));
                Ready = false;
                OnPropertyChanged("All");
                OnPropertyChanged("AllExceptGlobal");
                OnPropertyChanged("Count");
            }
            else
            {
                OnPropertyChanged("Spectators");
                Ready = true;
            }
            CanKick = local == false && Program.IsHost;
        }

        public bool IsGlobal { get; }

        // C'tor for global items
        internal Player(DataNew.Entities.Game g, bool isReplay)
        {
            IsGlobal = true;
            _spectator = false;
            SetupPlayer(false);
            var globalDef = g.GlobalPlayer;
            // Register the lPlayer
            lock (all)
                all.Insert(0, this);
            // Init fields
            _name = "Global";
            Id = 0;
            PublicKey = 0;
            if (GlobalVariables == null)
            {
                // Create global variables
                GlobalVariables = new Dictionary<string, string>();
                foreach (var varD in g.Player.GlobalVariables)
                    GlobalVariables.Add(varD.Value.Name, varD.Value.Value);
            }
            // Create counters
            _counters = new Counter[0];
            if (globalDef.Counters != null)
                _counters = globalDef.Counters.Select(x => new Counter(this, x, isReplay)).ToArray();
            // Create global's lPlayer groups
            // TODO: This could fail with a run-time exception on write, make it safe
            // I don't know if the above todo is still relevent - Kelly Elton - 3/18/2013
            if (globalDef.Groups != null)
            {
                var tempGroups = globalDef.Groups.ToArray();
                _groups = new Group[tempGroups.Length];
                for (int i = 0; i < IndexedGroups.Length; i++)
                    _groups[i] = new Pile(this, tempGroups[i]);
            }
            OnPropertyChanged("All");
            OnPropertyChanged("AllExceptGlobal");
            OnPropertyChanged("Count");
            CanKick = false;
        }

        internal void UpdateSettings(bool invertedTable, bool spectator, bool notify)
        {
            Log.InfoFormat("[UpdateSettings]{0} {1} {2}", this, invertedTable, spectator);
            if (Program.InPreGame == false) return;
            _invertedTable = invertedTable;
            _spectator = spectator;
            if (_spectator)
                _invertedTable = false;
            if (this == Player.LocalPlayer)
                Program.GameEngine.Spectator = _spectator;
            OnPropertyChanged("InvertedTable");
            OnPropertyChanged("Spectator");
            OnPropertyChanged("All");
            OnPropertyChanged("AllExceptGlobal");
            OnPropertyChanged("Count");

            if(notify) // used to prevent feedback loops
                Program.Client.Rpc.PlayerSettings(this, _invertedTable, _spectator);
        }

        public static void RefreshSpectators()
        {
            lock (all)
            {
                foreach (var p in all.Where(x => x.Spectator).ToArray())
                {
                    all.Remove(p);
                    spectators.Add(p);
                }
                foreach (var s in spectators.Where(x => x.Spectator == false).ToArray())
                {
                    spectators.Remove(s);
                    all.Add(s);
                }
                foreach (var p in all.Union(spectators))
                {
                    p.OnPropertyChanged("All");
                    p.OnPropertyChanged("AllExceptGlobal");
                    p.OnPropertyChanged("Count");
                    p.OnPropertyChanged("Spectators");
                    p.OnPropertyChanged("WaitingOnPlayers");
                    p.OnPropertyChanged("Ready");
                }
            }
        }

        // Remove the lPlayer from the game
        internal void Delete()
        {

            // Remove from the list
            lock (all)
            {
                all.Remove(this);
                spectators.Remove(this);
            }
            if (Program.GameEngine.ActivePlayer == this)
            {
                Program.GameEngine.ActivePlayer = null;
            }
            this.OnPropertyChanged("Ready");
            foreach (var p in all)
                p.OnPropertyChanged("WaitingOnPlayers");
            // Raise the event
            if (PlayerRemoved != null) PlayerRemoved(null, new PlayerEventArgs(this));
        }

        public override string ToString()
        {
            return _name;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    public class PlayerEventArgs : EventArgs
    {
        public readonly Player Player;

        public PlayerEventArgs(Player p)
        {
            Player = p;
        }
    }
}