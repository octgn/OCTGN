using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Play;
using Octgn.Play.Gui;
using Octgn.Scripting.Controls;
using Octgn.Utils;

namespace Octgn
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;

    using Card = Octgn.Play.Card;
    using Marker = Octgn.Play.Marker;
    using Player = Octgn.Play.Player;

    public class GameEngine : INotifyPropertyChanged
    {
        private const int MaxRecentMarkers = 10;
        private const int MaxRecentCards = 10;

        private readonly Game _definition;
        private readonly SortedList<Guid, DataNew.Entities.Marker> _markersById = new SortedList<Guid, DataNew.Entities.Marker>();
        private readonly List<RandomRequest> _random = new List<RandomRequest>();
        private readonly List<DataNew.Entities.Card> _recentCards = new List<DataNew.Entities.Card>(MaxRecentCards);
        private readonly List<DataNew.Entities.Marker> _recentMarkers = new List<DataNew.Entities.Marker>(MaxRecentMarkers);
        private readonly Table _table;

        //wouldn't a heap be best for these caches? 
        private bool _stopTurn;
        private Play.Player _turnPlayer;
        private ushort _uniqueId;
        private bool _BeginCalled;

        private string nick;

        public bool IsLocal { get; private set; }

        public GameEngine(Game def, string nickname, bool isLocal = false)
        {
            IsLocal = isLocal;
            _definition = def;
            _table = new Table(def.Table);
            Variables = new Dictionary<string, int>();
            foreach (var varDef in def.Variables.Where(v => v.Global))
                Variables.Add(varDef.Name, varDef.Default);
            GlobalVariables = new Dictionary<string, string>();
            foreach (var varDef in def.GlobalVariables)
                GlobalVariables.Add(varDef.Name, varDef.DefaultValue);

            nick = nickname;
            while(String.IsNullOrWhiteSpace(nick))
            {
                nick = Prefs.Nickname;
                if (string.IsNullOrWhiteSpace(nick)) nick = Skylabs.Lobby.Randomness.GrabRandomNounWord() + new Random().Next(30);
                var retNick = nick;
                Program.Dispatcher.Invoke(new Action(() =>
                    {
                        var i = new InputDlg("Choose a nickname", "Choose a nickname", nick);
                        retNick = i.GetString();
                    }));
                nick = retNick;
            }
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

        public bool CardsRevertToOriginalOnGroupChange = false;//As opposed to staying SwitchedWithAlternate

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Begin()
        {
            if (_BeginCalled) return;
            _BeginCalled = true;
            // Init fields
            _uniqueId = 1;
            TurnNumber = 0;
            TurnPlayer = null;

            CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.GetCardFrontUri());
            CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.GetCardBackUri());
            // Create the global player, if any
            if (Program.GameEngine.Definition.GlobalPlayer != null)
                Play.Player.GlobalPlayer = new Play.Player(Program.GameEngine.Definition);
            // Create the local player
            Play.Player.LocalPlayer = new Play.Player(Program.GameEngine.Definition, nick, 255, Crypto.ModExp(Program.PrivateKey));
            // Register oneself to the server
            Program.Client.Rpc.Hello(nick, Player.LocalPlayer.PublicKey,
                                     Const.ClientName, Const.OctgnVersion, Const.OctgnVersion,
                                     Program.GameEngine.Definition.Id, Program.GameEngine.Definition.Version);
            // Load all game markers
            foreach (DataNew.Entities.Marker m in Definition.GetAllMarkers())
            {
                if (!_markersById.ContainsKey(m.Id))
                {
                    _markersById.Add(m.Id, m);
                }
            }
            Program.IsGameRunning = true;
        }

        public void TestBegin()
        {
            //Database.Open(Definition, true);
            // Init fields
            _uniqueId = 1;
            TurnNumber = 0;
            TurnPlayer = null;
            const string nick = "TestPlayer";
            //CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Front);
            //CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Back);
            // Create the global player, if any
            if (Program.GameEngine.Definition.GlobalPlayer != null)
                Play.Player.GlobalPlayer = new Play.Player(Program.GameEngine.Definition);
            // Create the local player
            Play.Player.LocalPlayer = new Play.Player(Program.GameEngine.Definition, nick, 255, Crypto.ModExp(Program.PrivateKey));
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
            return _uniqueId++;
        }

        internal int GenerateCardId()
        {
            return (Player.LocalPlayer.Id) << 16 | GetUniqueId();
        }

        public RandomRequest FindRandomRequest(int id)
        {
            return RandomRequests.FirstOrDefault(r => r.Id == id);
        }

        //Temporarily store group visibility information for LoadDeck. //bug (google) #20

        public void LoadDeck(DataNew.Entities.IDeck deck)
        {
            Player player = deck.IsShared ? Player.GlobalPlayer : Player.LocalPlayer;
            var def = Program.GameEngine.Definition;
            var deckDef = deck.IsShared ? def.SharedDeckSections : def.DeckSections;
            int nCards = deck.CardCount();
            var ids = new int[nCards];
            var keys = new ulong[nCards];
            var cards = new Card[nCards];
            var groups = new Play.Group[nCards];
            var gtmps = new List<GrpTmp>(); //for temp groups visibility
            int j = 0;
            foreach (DataNew.Entities.Section section in deck.Sections)
            {
                var sectionDef = deckDef[section.Name];
                if (sectionDef == null)
                    throw new InvalidFileFormatException("Invalid section '" + section.Name + "' in deck file.");
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
                foreach (DataNew.Entities.MultiCard element in section.Cards)
                {
                    DataNew.Entities.Card mod = Definition.GetCardById(element.Id);
                    for (int i = 0; i < element.Quantity; i++)
                    { //for every card in the deck, generate a unique key for it, ID for it
                        ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | element.Id.Condense();
                        int id = GenerateCardId();
                        ids[j] = id;
                        keys[j] = Crypto.ModExp(key);
                        groups[j] = group;
                        var card = new Card(player, id, key, mod, true);
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
    }
}