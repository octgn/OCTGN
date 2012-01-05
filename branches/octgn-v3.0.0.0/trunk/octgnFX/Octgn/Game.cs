using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Definitions;
using Octgn.Play;

namespace Octgn
{
    public class Game : INotifyPropertyChanged
    {
        private const int MaxRecentMarkers = 10;
        private const int MaxRecentCards = 10;

        private ushort uniqueId;
        private readonly GameDef _definition;
        private readonly Table _table;
        private Player _turnPlayer;
        private bool _stopTurn;
        private List<RandomRequest> _random = new List<RandomRequest>();
        // TODO: why a SortedList? Wouldn't a Dictionary be sufficient?
        private SortedList<Guid, Data.MarkerModel> markersById = new SortedList<Guid, Data.MarkerModel>();
        private List<Data.MarkerModel> recentMarkers = new List<Data.MarkerModel>(MaxRecentMarkers);
        private List<Data.CardModel> recentCards = new List<Data.CardModel>(MaxRecentCards);

        public int TurnNumber
        { get; set; }

        public Player TurnPlayer
        {
            get { return _turnPlayer; }
            set
            {
                if(_turnPlayer != value)
                {
                    _turnPlayer = value;
                    OnPropertyChanged("TurnPlayer");
                }
            }
        }

        public bool StopTurn
        {
            get { return _stopTurn; }
            set
            {
                if(_stopTurn != value)
                {
                    _stopTurn = value;
                    OnPropertyChanged("StopTurn");
                }
            }
        }

        public Table Table
        { get { return _table; } }

        public GameDef Definition
        { get { return _definition; } }

        public BitmapImage CardFrontBitmap
        { get; private set; }

        public BitmapImage CardBackBitmap
        { get; private set; }

        public IList<RandomRequest> RandomRequests
        { get { return _random; } }

        public IList<Data.MarkerModel> Markers
        { get { return markersById.Values; } }

        public IList<Data.MarkerModel> RecentMarkers
        { get { return recentMarkers; } }

        public IList<Data.CardModel> RecentCards
        { get { return recentCards; } }

        public Dictionary<string, int> Variables
        { get; private set; }

        public Game(GameDef def)
        {
            _definition = def;
            _table = new Table(def.TableDefinition);
            Variables = new Dictionary<string, int>();
            foreach(var varDef in def.Variables.Where(v => v.Global))
                Variables.Add(varDef.Name, varDef.DefaultValue);
        }

        public void Begin()
        {
            Database.Open(Definition, true);
            // Init fields
            uniqueId = 1; TurnNumber = 0; TurnPlayer = null;
            string nick = Properties.Settings.Default.NickName;
            CardFrontBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Front);
            CardBackBitmap = ImageUtils.CreateFrozenBitmap(Definition.CardDefinition.Back);
            // Create the global player, if any
            if(Program.Game.Definition.GlobalDefinition != null)
                Player.GlobalPlayer = new Player(Program.Game.Definition);
            // Create the local player
            Player.LocalPlayer = new Player(Program.Game.Definition, nick, 255, Crypto.ModExp(Program.PrivateKey));
            // Register oneself to the server
            Program.Client.Rpc.Hello(nick, Player.LocalPlayer.PublicKey,
                                    OctgnApp.ClientName, OctgnApp.OctgnVersion, OctgnApp.OctgnVersion,
                                    Program.Game.Definition.Id, Program.Game.Definition.Version);
            // Load all game markers
            foreach(Data.MarkerModel m in Database.GetAllMarkers())
                markersById.Add(m.id, m);

            Program.IsGameRunning = true;
        }

        public void Reset()
        {
            TurnNumber = 0; TurnPlayer = null;
            foreach(Player p in Player.All)
            {
                foreach(Group g in p.Groups)
                    g.Reset();
                foreach(Counter c in p.Counters)
                    c.Reset();
                foreach(var varDef in Definition.Variables.Where(v => !v.Global && v.Reset))
                    p.Variables[varDef.Name] = varDef.DefaultValue;
            }
            Table.Reset();
            Card.Reset(); CardIdentity.Reset();
            Play.Gui.Selection.Clear();
            RandomRequests.Clear();
            foreach(var varDef in Definition.Variables.Where(v => v.Global && v.Reset))
                Variables[varDef.Name] = varDef.DefaultValue;

            //fix MAINWINDOW bug
            var mainWin = Program.PlayWindow;
            mainWin.RaiseEvent(new Octgn.Play.Gui.CardEventArgs(Octgn.Play.Gui.CardControl.CardHoveredEvent, mainWin));
        }

        public void End()
        {
            Player.Reset();
            Card.Reset(); CardIdentity.Reset();
            History.Reset();
            Play.Gui.Selection.Clear();
        }

        public ushort GetUniqueId()
        { return uniqueId++; }

        internal int GenerateCardId()
        {
            return ((int)Player.LocalPlayer.Id) << 16 | GetUniqueId();
        }

        public RandomRequest FindRandomRequest(int id)
        {
            foreach(var r in RandomRequests)
                if(r.Id == id) return r;
            return null;
        }

        //Temporarily store group visibility information for LoadDeck. //bug #20
        private struct grp_tmp
        {
            public Group group;
            public GroupVisibility visibility;
            public List<Player> viewers;

            public grp_tmp(Group g, GroupVisibility vis, List<Player> v)
            {
                group = g;
                visibility = vis;
                viewers = v;
            }
        }

        public void LoadDeck(Data.Deck deck)
        {
            var player = deck.IsShared ? Player.GlobalPlayer : Player.LocalPlayer;
            var def = Program.Game.Definition;
            var deckDef = deck.IsShared ? def.SharedDeckDefinition : def.DeckDefinition;
            var cardDef = def.CardDefinition;
            int nCards = deck.CardCount;
            int[] ids = new int[nCards];
            ulong[] keys = new ulong[nCards];
            Card[] cards = new Card[nCards];
            Group[] groups = new Group[nCards];
            List<grp_tmp> gtmps = new List<grp_tmp>();  //for temp groups visibility
            int j = 0;
            foreach(Data.Deck.Section section in deck.Sections)
            {
                var sectionDef = deckDef.Sections[section.Name];
                if(sectionDef == null)
                    throw new Data.InvalidFileFormatException("Invalid section '" + section.Name + "' in deck file.");
                var group = player.Groups.First(x => x.Name == sectionDef.Group);

                //In order to make the clients know what the card is (if visibility is set so that they can see it),
                //we have to set the visibility to Nobody, and then after the cards are sent, set the visibility back
                //to what it was. //bug #20
                gtmps.Add(new grp_tmp(group, group.Visibility, group.viewers.ToList()));
                group.SetVisibility(false, false);

                foreach(Data.Deck.Element element in section.Cards)
                {
                    for(int i = 0; i < element.Quantity; i++)
                    {
                        ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | element.Card.Id.Condense();
                        int id = GenerateCardId();
                        ids[j] = id; keys[j] = Crypto.ModExp(key);
                        groups[j] = group;
                        var card = new Card(player, id, key, cardDef, element.Card, true);
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

            //reset the visibility to what it was before pushing the deck to everybody. //bug #20
            foreach (grp_tmp g in gtmps)
            {
                if (g.visibility == GroupVisibility.Everybody)
                    g.group.SetVisibility(true, false);
                else if (g.visibility == GroupVisibility.Nobody)
                    g.group.SetVisibility(false, false);
                else
                {
                    foreach (Player p in g.viewers)
                    {
                        g.group.AddViewer(p, false);
                    }
                }
            }
            gtmps.Clear();
            gtmps.TrimExcess();
        }

        internal void AddRecentCard(Data.CardModel card)
        {
            int idx = recentCards.FindIndex(c => c.Id == card.Id);
            if(idx == 0) return;
            if(idx > 0)
            {
                recentCards.RemoveAt(idx);
                recentCards.Insert(0, card);
                return;
            }

            if(recentCards.Count == MaxRecentCards)
                recentCards.RemoveAt(MaxRecentCards - 1);
            recentCards.Insert(0, card);
        }

        internal void AddRecentMarker(Data.MarkerModel marker)
        {
            int idx = recentMarkers.IndexOf(marker);
            if(idx == 0) return;
            if(idx > 0)
            {
                recentMarkers.RemoveAt(idx);
                recentMarkers.Insert(0, marker);
                return;
            }

            if(recentMarkers.Count == MaxRecentMarkers)
                recentMarkers.RemoveAt(MaxRecentMarkers - 1);
            recentMarkers.Insert(0, marker);
        }

        internal Data.MarkerModel GetMarkerModel(Guid id)
        {
            Data.MarkerModel model;
            if(id.CompareTo(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10)) < 0)
            {
                // Get a standard model
                DefaultMarkerModel defaultModel = Marker.DefaultMarkers.First(x => x.id == id);
                model = defaultModel.Clone();
                model.id = id;
                return model;
            }
            // Try to find the marker model
            if(!markersById.TryGetValue(id, out model))
            {
                Program.Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, (int)EventIds.NonGame, "Marker model '{0}' not found, using default marker instead", id);
                DefaultMarkerModel defaultModel = Marker.DefaultMarkers[Crypto.Random(7)];
                model = defaultModel.Clone();
                model.id = id;
                return model;
            }
            return model;
        }

        #region MEF stuff for easy services composition

        private static AssemblyCatalog catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
        private CompositionContainer container = new CompositionContainer(catalog);

        public void ComposeParts(params object[] attributedParts)
        {
            container.ComposeParts(attributedParts);
        }

        #endregion MEF stuff for easy services composition

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members
    }
}