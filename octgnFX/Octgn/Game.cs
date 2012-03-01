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
using Octgn.Data;
using Octgn.Definitions;
using Octgn.Play;
using Octgn.Play.Gui;
using Octgn.Utils;

namespace Octgn
{
    public class Game : INotifyPropertyChanged
    {
        private const int MaxRecentMarkers = 10;
        private const int MaxRecentCards = 10;

        private readonly GameDef _definition;
        // TODO: why a SortedList? Wouldn't a Dictionary be sufficient?
        private readonly SortedList<Guid, MarkerModel> _markersById = new SortedList<Guid, MarkerModel>();
        private readonly List<RandomRequest> _random = new List<RandomRequest>();
        private readonly List<CardModel> _recentCards = new List<CardModel>(MaxRecentCards);
        private readonly List<MarkerModel> _recentMarkers = new List<MarkerModel>(MaxRecentMarkers);
        private readonly Table _table;

        //wouldn't a heap be best for these caches? 
        private bool _stopTurn;
        private Player _turnPlayer;
        private ushort _uniqueId;

        public Game(GameDef def)
        {
            _definition = def;
            _table = new Table(def.TableDefinition);
            Variables = new Dictionary<string, int>();
            foreach (VariableDef varDef in def.Variables.Where(v => v.Global))
                Variables.Add(varDef.Name, varDef.DefaultValue);
            GlobalVariables = new Dictionary<string, string>();
            foreach (GlobalVariableDef varDef in def.GlobalVariables)
                GlobalVariables.Add(varDef.Name, varDef.DefaultValue);
        }

        public int TurnNumber { get; set; }

        public Player TurnPlayer
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

        public GameDef Definition
        {
            get { return _definition; }
        }

        public BitmapImage CardFrontBitmap { get; private set; }

        public BitmapImage CardBackBitmap { get; private set; }

        public IList<RandomRequest> RandomRequests
        {
            get { return _random; }
        }

        public IList<MarkerModel> Markers
        {
            get { return _markersById.Values; }
        }

        public IList<MarkerModel> RecentMarkers
        {
            get { return _recentMarkers; }
        }

        public IList<CardModel> RecentCards
        {
            get { return _recentCards; }
        }

        public Dictionary<string, int> Variables { get; private set; }
        public Dictionary<string, string> GlobalVariables { get; private set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Begin()
        {
            Database.Open(Definition, true);
            // Init fields
            _uniqueId = 1;
            TurnNumber = 0;
            TurnPlayer = null;
            string nick = Program.LobbyClient.Me.DisplayName;
            CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Front);
            CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Back);
            // Create the global player, if any
            if (Program.Game.Definition.GlobalDefinition != null)
                Player.GlobalPlayer = new Player(Program.Game.Definition);
            // Create the local player
            Player.LocalPlayer = new Player(Program.Game.Definition, nick, 255, Crypto.ModExp(Program.PrivateKey));
            // Register oneself to the server
            Program.Client.Rpc.Hello(nick, Player.LocalPlayer.PublicKey,
                                     OctgnApp.ClientName, OctgnApp.OctgnVersion, OctgnApp.OctgnVersion,
                                     Program.Game.Definition.Id, Program.Game.Definition.Version);
            // Load all game markers
            foreach (MarkerModel m in Database.GetAllMarkers())
                _markersById.Add(m.Id, m);

            Program.IsGameRunning = true;
        }

        public void TestBegin()
        {
            Database.Open(Definition, true);
            // Init fields
            _uniqueId = 1;
            TurnNumber = 0;
            TurnPlayer = null;
            const string nick = "TestPlayer";
            //CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Front);
            //CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Back);
            // Create the global player, if any
            if (Program.Game.Definition.GlobalDefinition != null)
                Player.GlobalPlayer = new Player(Program.Game.Definition);
            // Create the local player
            Player.LocalPlayer = new Player(Program.Game.Definition, nick, 255, Crypto.ModExp(Program.PrivateKey));
            // Register oneself to the server
            //Program.Client.Rpc.Hello(nick, Player.LocalPlayer.PublicKey,
            //                       OctgnApp.ClientName, OctgnApp.OctgnVersion, OctgnApp.OctgnVersion,
            //                      Program.Game.Definition.Id, Program.Game.Definition.Version);
            // Load all game markers
            foreach (MarkerModel m in Database.GetAllMarkers())
                _markersById.Add(m.Id, m);

            //Program.IsGameRunning = true;
        }

        public void Reset()
        {
            TurnNumber = 0;
            TurnPlayer = null;
            foreach (Player p in Player.All)
            {
                foreach (Group g in p.Groups)
                    g.Reset();
                foreach (Counter c in p.Counters)
                    c.Reset();
                foreach (VariableDef varDef in Definition.Variables.Where(v => !v.Global && v.Reset))
                    p.Variables[varDef.Name] = varDef.DefaultValue;
                foreach (GlobalVariableDef g in Definition.PlayerDefinition.GlobalVariables)
                    p.GlobalVariables[g.Name] = g.DefaultValue;
            }
            Table.Reset();
            Card.Reset();
            CardIdentity.Reset();
            Selection.Clear();
            RandomRequests.Clear();
            foreach (VariableDef varDef in Definition.Variables.Where(v => v.Global && v.Reset))
                Variables[varDef.Name] = varDef.DefaultValue;
            foreach (GlobalVariableDef g in Definition.GlobalVariables)
                GlobalVariables[g.Name] = g.DefaultValue;
            //fix MAINWINDOW bug
            PlayWindow mainWin = Program.PlayWindow;
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

        public void LoadDeck(Deck deck)
        {
            Player player = deck.IsShared ? Player.GlobalPlayer : Player.LocalPlayer;
            GameDef def = Program.Game.Definition;
            DeckDef deckDef = deck.IsShared ? def.SharedDeckDefinition : def.DeckDefinition;
            CardDef cardDef = def.CardDefinition;
            int nCards = deck.CardCount;
            var ids = new int[nCards];
            var keys = new ulong[nCards];
            var cards = new Card[nCards];
            var groups = new Group[nCards];
            var gtmps = new List<GrpTmp>(); //for temp groups visibility
            int j = 0;
            foreach (Deck.Section section in deck.Sections)
            {
                DeckSectionDef sectionDef = deckDef.Sections[section.Name];
                if (sectionDef == null)
                    throw new InvalidFileFormatException("Invalid section '" + section.Name + "' in deck file.");
                Group group = player.Groups.First(x => x.Name == sectionDef.Group);

                //In order to make the clients know what the card is (if visibility is set so that they can see it),
                //we have to set the visibility to Nobody, and then after the cards are sent, set the visibility back
                //to what it was. //bug (google) #20
                var gt = new GrpTmp(group, group.Visibility, group.Viewers.ToList());
                if (!gtmps.Contains(gt))
                {
                    gtmps.Add(gt);
                    group.SetVisibility(false, false);
                }
                foreach (Deck.Element element in section.Cards)
                {
                    for (int i = 0; i < element.Quantity; i++)
                    { //for every card in the deck, generate a unique key for it, ID for it
                        ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | element.Card.Id.Condense();
                        int id = GenerateCardId();
                        ids[j] = id;
                        keys[j] = Crypto.ModExp(key);
                        groups[j] = group;
                        var card = new Card(player, id, key, cardDef, Database.GetCardById(element.Card.Id), true);
                        cards[j++] = card;
                        group.AddAt(card, group.Count);
                    }

                    // Load images in the background
                    string pictureUri = element.Card.Picture;
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

        internal void AddRecentCard(CardModel card)
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

        internal void AddRecentMarker(MarkerModel marker)
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

        internal MarkerModel GetMarkerModel(Guid id)
        {
            MarkerModel model;
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

        public void ComposeParts(params object[] attributedParts)
        {
            _container.ComposeParts(attributedParts);
        }

        #endregion MEF stuff for easy services composition

        #region Nested type: GrpTmp

        private struct GrpTmp : IEquatable<GrpTmp>
        {
            public readonly Group Group;
            public readonly List<Player> Viewers;
            public readonly GroupVisibility Visibility;

            public GrpTmp(Group g, GroupVisibility vis, List<Player> v)
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