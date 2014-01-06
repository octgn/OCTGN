using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Play;
using Octgn.Play.Gui;
using Octgn.Scripting.Controls;
using Octgn.Utils;

namespace Octgn
{
    using log4net;

    using Octgn.Core;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.Util;
    using Octgn.DataNew.Entities;
    using Octgn.Extentions;
    using Octgn.Library.Exceptions;
    using Octgn.Scripting;

    using Card = Octgn.Play.Card;
    using Marker = Octgn.Play.Marker;
    using Player = Octgn.Play.Player;

    [Serializable]
    public class GameEngine : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        public Engine ScriptEngine { get; set; }

#pragma warning restore 649

        private const int MaxRecentMarkers = 10;
        private const int MaxRecentCards = 10;

        private readonly Game _definition;
        private readonly SortedList<Guid, DataNew.Entities.Marker> _markersById = new SortedList<Guid, DataNew.Entities.Marker>();
        private readonly List<RandomRequest> _random = new List<RandomRequest>();
        private readonly List<DataNew.Entities.Card> _recentCards = new List<DataNew.Entities.Card>(MaxRecentCards);
        private readonly List<DataNew.Entities.Marker> _recentMarkers = new List<DataNew.Entities.Marker>(MaxRecentMarkers);
        private readonly Table _table;
        internal readonly string Password;

        //wouldn't a heap be best for these caches? 
        private bool _stopTurn;
        private Play.Player _turnPlayer;
        private ushort _uniqueId;
        private bool _BeginCalled;

        private string boardImage;

        internal string Nickname;

        public bool IsLocal { get; private set; }

        public ushort CurrentUniqueId;

        public GameEngine(Game def, string nickname, string password = "", bool isLocal = false)
        {
            IsLocal = isLocal;
            this.Password = password;
            _definition = def;
            _table = new Table(def.Table);
            Variables = new Dictionary<string, int>();
            foreach (var varDef in def.Variables.Where(v => v.Global))
                Variables.Add(varDef.Name, varDef.Default);
            GlobalVariables = new Dictionary<string, string>();
            foreach (var varDef in def.GlobalVariables)
                GlobalVariables.Add(varDef.Name, varDef.DefaultValue);

            this.Nickname = nickname;
            while (String.IsNullOrWhiteSpace(this.Nickname))
            {
                this.Nickname = Prefs.Nickname;
                if (string.IsNullOrWhiteSpace(this.Nickname)) this.Nickname = Skylabs.Lobby.Randomness.GrabRandomNounWord() + new Random().Next(30);
                var retNick = this.Nickname;
                Program.Dispatcher.Invoke(new Action(() =>
                    {
                        var i = new InputDlg("Choose a nickname", "Choose a nickname", this.Nickname);
                        retNick = i.GetString();
                    }));
                this.Nickname = retNick;
            }
            // Load all game markers
            foreach (DataNew.Entities.Marker m in Definition.GetAllMarkers())
            {
                if (!_markersById.ContainsKey(m.Id))
                {
                    _markersById.Add(m.Id, m);
                }
            }
            // Init fields
            CurrentUniqueId = 1;
            TurnNumber = 0;
            TurnPlayer = null;

            CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.GetCardFrontUri());
            CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.GetCardBackUri());
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // clear any existing players
                Play.Player.All.Clear();
                // Create the global player, if any
                if (Definition.GlobalPlayer != null)
                    Play.Player.GlobalPlayer = new Play.Player(Definition);
                // Create the local player
                Play.Player.LocalPlayer = new Play.Player(Definition, this.Nickname, 255, Crypto.ModExp(Prefs.PrivateKey));
            }));
        }

        public int TurnNumber { get; set; }

        public Octgn.Play.Player TurnPlayer
        {
            get { return _turnPlayer; }
            set
            {
                if (_turnPlayer == value) return;
                _turnPlayer = value;
                OnPropertyChanged("TurnPlayer");
            }
        }

        public bool StopTurn
        {
            get { return _stopTurn; }
            set
            {
                if (_stopTurn == value) return;
                _stopTurn = value;
                OnPropertyChanged("StopTurn");
            }
        }

        public Table Table
        {
            get { return _table; }
        }

        public Game Definition
        {
            get { return _definition; }
        }

        public bool IsConnected
        {
            get
            {
                return this.isConnected;
            }
            set
            {
                if (value == this.isConnected) return;
                Log.DebugFormat("IsConnected = {0}", value);
                this.isConnected = value;
				this.OnPropertyChanged("IsConnected");
                if(Program.Dispatcher != null && Program.Dispatcher.CheckAccess() == false)
                    Thread.Sleep(10);
            }
        }

        public BitmapImage CardFrontBitmap { get; private set; }

        public BitmapImage CardBackBitmap { get; private set; }

        public IList<RandomRequest> RandomRequests
        {
            get { return _random; }
        }

        public IList<DataNew.Entities.Marker> Markers
        {
            get { return _markersById.Values; }
        }

        public IList<DataNew.Entities.Marker> RecentMarkers
        {
            get { return _recentMarkers; }
        }

        public IList<DataNew.Entities.Card> RecentCards
        {
            get { return _recentCards; }
        }

        public Dictionary<string, int> Variables { get; private set; }
        public Dictionary<string, string> GlobalVariables { get; private set; }

        public bool IsTableBackgroundFlipped
        {
            get
            {
                return isTableBackgroundFlipped;
            }
            set
            {
                isTableBackgroundFlipped = value;
                this.OnPropertyChanged("IsTableBackgroundFlipped");
            }
        }

        public string BoardImage
        {
            get
            {
                return boardImage;
            }
            set
            {
                if (value == boardImage) return;
                boardImage = value;
                this.OnPropertyChanged("BoardImage");
            }
        }

        private Thickness? boardMargin;
        public Thickness BoardMargin
        {
            get
            {
                if (boardMargin == null)
                {
                    var pos = new Rect(Table.Definition.BoardPosition.X, Table.Definition.BoardPosition.Y, Table.Definition.BoardPosition.Width, Table.Definition.BoardPosition.Height);
                    boardMargin = new Thickness(pos.Left, pos.Top, 0, 0);

                }
                return boardMargin.Value;
            }
        }

        public GameEventProxy EventProxy { get; set; }

        public bool WaitForGameState
        {
            get
            {
                return this.waitForGameState;
            }
            set
            {
                if (value == this.waitForGameState) return;
                Log.DebugFormat("WaitForGameState = {0}", value);
                this.waitForGameState = value;
                this.OnPropertyChanged("WaitForGameState");
            }
        }

        public Guid SessionId { get; set; }

        public bool CardsRevertToOriginalOnGroupChange = false;//As opposed to staying SwitchedWithAlternate

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Begin(bool spectator)
        {
            if (_BeginCalled) return;
            _BeginCalled = true;
            // Register oneself to the server
            Version oversion = Const.OctgnVersion;
            Program.Client.Rpc.Hello(this.Nickname, Player.LocalPlayer.PublicKey,
                                     Const.ClientName, oversion, oversion,
                                     Program.GameEngine.Definition.Id, Program.GameEngine.Definition.Version, this.Password
                                     ,spectator);
            Program.IsGameRunning = true;
        }

        public void TestBegin()
        {
            //Database.Open(Definition, true);
            // Init fields
            CurrentUniqueId = 1;
            TurnNumber = 0;
            TurnPlayer = null;
            const string nick = "TestPlayer";
            //CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Front);
            //CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Back);
            // Create the global player, if any
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (Program.GameEngine.Definition.GlobalPlayer != null)
                    Play.Player.GlobalPlayer = new Play.Player(Program.GameEngine.Definition);
                // Create the local player
                Play.Player.LocalPlayer = new Play.Player(Program.GameEngine.Definition, nick, 255, Crypto.ModExp(Prefs.PrivateKey));
            }));
            // Register oneself to the server
            //Program.Client.Rpc.Hello(nick, Player.LocalPlayer.PublicKey,
            //                       OctgnApp.ClientName, OctgnApp.OctgnVersion, OctgnApp.OctgnVersion,
            //                      Program.Game.Definition.Id, Program.Game.Definition.Version);
            // Load all game markers
            //Program.Game.
            //foreach (MarkerModel m in Database.GetAllMarkers())
            //    _markersById.Add(m.Id, m);

            //Program.IsGameRunning = true;
        }

        public void Resume()
        {
            //throw new NotImplementedException();
            // Register oneself to the server
            this.gameStateCount = 0;
            Version oversion = Const.OctgnVersion;
            Program.Client.Rpc.HelloAgain(Player.LocalPlayer.Id,this.Nickname, Player.LocalPlayer.PublicKey,
                                     Const.ClientName, oversion, oversion,
                                     Program.GameEngine.Definition.Id, Program.GameEngine.Definition.Version, this.Password);
        }

        public void Reset()
        {
            TurnNumber = 0;
            TurnPlayer = null;
            foreach (var p in Player.All)
            {
                foreach (var g in p.Groups)
                    g.Reset();
                foreach (var c in p.Counters)
                    c.Reset();
                foreach (var varDef in Definition.Variables.Where(v => !v.Global && v.Reset))
                    p.Variables[varDef.Name] = varDef.Default;
                foreach (var g in Definition.Player.GlobalVariables)
                    p.GlobalVariables[g.Name] = g.DefaultValue;
            }
            Table.Reset();
            Card.Reset();
            CardIdentity.Reset();
            Selection.Clear();
            RandomRequests.Clear();
            foreach (var varDef in Definition.Variables.Where(v => v.Global && v.Reset))
                Variables[varDef.Name] = varDef.Default;
            foreach (var g in Definition.GlobalVariables)
                GlobalVariables[g.Name] = g.DefaultValue;
            //fix MAINWINDOW bug
            PlayWindow mainWin = WindowManager.PlayWindow;
            mainWin.RaiseEvent(new CardEventArgs(CardControl.CardHoveredEvent, mainWin));
            EventProxy.OnGameStart();
        }

        public void End()
        {
            Player.Reset();
            Card.Reset();
            CardIdentity.Reset();
            History.Reset();
            Selection.Clear();
        }

        public ushort GetUniqueId()
        {
            return CurrentUniqueId++;
        }

        public RandomRequest FindRandomRequest(int id)
        {
            return RandomRequests.FirstOrDefault(r => r.Id == id);
        }

        //Temporarily store group visibility information for LoadDeck. //bug (google) #20

        public void LoadDeck(IDeck deck)
        {
            var def = Program.GameEngine.Definition;
            int nCards = deck.CardCount();
            var ids = new int[nCards];
            var keys = new ulong[nCards];
            var cards = new Card[nCards];
            var groups = new Play.Group[nCards];
            var gtmps = new List<GrpTmp>(); //for temp groups visibility
            int j = 0;
            foreach (ISection section in deck.Sections)
            {
                DeckSection sectionDef = null;
                sectionDef = section.Shared ? def.SharedDeckSections[section.Name] : def.DeckSections[section.Name];
                if (sectionDef == null)
                    throw new InvalidFileFormatException("Invalid section '" + section.Name + "' in deck file.");
                var player = section.Shared ? Player.GlobalPlayer : Player.LocalPlayer;
                Play.Group group = player.Groups.First(x => x.Name == sectionDef.Group);

                //In order to make the clients know what the card is (if visibility is set so that they can see it),
                //we have to set the visibility to Nobody, and then after the cards are sent, set the visibility back
                //to what it was. //bug (google) #20
                var gt = new GrpTmp(group, group.Visibility, group.Viewers.ToList());
                if (!gtmps.Contains(gt))
                {
                    gtmps.Add(gt);
                    group.SetVisibility(false, false);
                }
                foreach (IMultiCard element in section.Cards)
                {
                    //DataNew.Entities.Card mod = Definition.GetCardById(element.Id);
                    for (int i = 0; i < element.Quantity; i++)
                    { 
                        //for every card in the deck, generate a unique key for it, ID for it
                        var card = element.ToPlayCard(player);
                        ids[j] = card.Id;
                        keys[j] = card.GetEncryptedKey();
                        groups[j] = group;
                        cards[j++] = card;
                        group.AddAt(card, group.Count);
                    }

                    // Load images in the background
                    string pictureUri = element.GetPicture();
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        new Func<string, BitmapImage>(ImageUtils.CreateFrozenBitmap),
                        DispatcherPriority.ApplicationIdle, pictureUri);
                }
            }
            Program.Client.Rpc.LoadDeck(ids, keys, groups);

            //reset the visibility to what it was before pushing the deck to everybody. //bug (google) #20
            foreach (GrpTmp g in gtmps)
            {
                switch (g.Visibility)
                {
                    case GroupVisibility.Everybody:
                        g.Group.SetVisibility(true, false);
                        break;
                    case GroupVisibility.Nobody:
                        g.Group.SetVisibility(false, false);
                        break;
                    default:
                        foreach (Player p in g.Viewers)
                        {
                            g.Group.AddViewer(p, false);
                        }
                        break;
                }
            }
            gtmps.Clear();
            gtmps.TrimExcess();
        }

        internal void AddRecentCard(DataNew.Entities.Card card)
        {
            int idx = _recentCards.FindIndex(c => c.Id == card.Id);
            if (idx == 0) return;
            if (idx > 0)
            {
                _recentCards.RemoveAt(idx);
                _recentCards.Insert(0, card);
                return;
            }

            if (_recentCards.Count == MaxRecentCards)
                _recentCards.RemoveAt(MaxRecentCards - 1);
            _recentCards.Insert(0, card);
        }

        internal void AddRecentMarker(DataNew.Entities.Marker marker)
        {
            int idx = _recentMarkers.IndexOf(marker);
            if (idx == 0) return;
            if (idx > 0)
            {
                _recentMarkers.RemoveAt(idx);
                _recentMarkers.Insert(0, marker);
                return;
            }

            if (_recentMarkers.Count == MaxRecentMarkers)
                _recentMarkers.RemoveAt(MaxRecentMarkers - 1);
            _recentMarkers.Insert(0, marker);
        }

        internal DataNew.Entities.Marker GetMarkerModel(Guid id)
        {
            DataNew.Entities.Marker model;
            if (id.CompareTo(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10)) < 0)
            {
                // Get a standard model
                DefaultMarkerModel defaultModel = Marker.DefaultMarkers.First(x => x.Id == id);
                model = defaultModel.Clone();
                model.Id = id;
                return model;
            }
            // Try to find the marker model
            if (!_markersById.TryGetValue(id, out model))
            {
                Program.Trace.TraceEvent(TraceEventType.Verbose, EventIds.NonGame,
                                         "Marker model '{0}' not found, using default marker instead", id);
                DefaultMarkerModel defaultModel = Marker.DefaultMarkers[Crypto.Random(7)];
                model = defaultModel.Clone();
                model.Id = id;
                return model;
            }
            return model;
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }



        #region MEF stuff for easy services composition

        private static readonly AssemblyCatalog Catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
        private readonly CompositionContainer _container = new CompositionContainer(Catalog);

        private bool isTableBackgroundFlipped;

        private bool waitForGameState;

        public void ComposeParts(params object[] attributedParts)
        {
            _container.ComposeParts(attributedParts);
        }

        #endregion MEF stuff for easy services composition

        #region Nested type: GrpTmp

        internal struct GrpTmp : IEquatable<GrpTmp>
        {
            public readonly Play.Group Group;
            public readonly List<Play.Player> Viewers;
            public readonly GroupVisibility Visibility;

            public GrpTmp(Play.Group g, GroupVisibility vis, List<Play.Player> v)
            {
                Group = g;
                Visibility = vis;
                Viewers = v;
            }

            #region IEquatable<GrpTmp> Members

            public bool Equals(GrpTmp gg)
            {
                return Group == gg.Group;
            }

            #endregion
        }

        #endregion

        public void PlaySoundReq(Player player, string name)
        {
            if (Definition.Sounds.ContainsKey(name.ToLowerInvariant()))
            {
                var sound = this.Definition.Sounds[name.ToLowerInvariant()];
                Sounds.PlayGameSound(sound);
            }
        }

        public void Ready()
        {
            Log.Debug("Ready");
            Program.Client.Rpc.Ready(Player.LocalPlayer);
        }

        public void ExecuteRemoteCall(Player fromPlayer, string func, string args)
        {
            // Build args
            try
            {
                //var argo = Newtonsoft.Json.JsonConvert.DeserializeObject<object[]>(args);
                ScriptEngine.ExecuteFunctionNoFormat(func, args);

            }
            catch (Exception e)
            {
                
            }
        }

        private int gameStateCount = 0;

        private bool isConnected;

        public void GotGameState(Player fromPlayer)
        {
            Log.DebugFormat("GotGameState {0} {1}", fromPlayer, gameStateCount);
            gameStateCount++;
            fromPlayer.Ready = true;
            if (gameStateCount == Player.Count - 1)
            {
                Log.DebugFormat("GotGameState Got all states");
                WaitForGameState = false;
                Ready();
            }
        }
    }
}
