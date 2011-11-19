using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows;
using Octgn.Play;
using Media = System.Windows.Media;
using System.Net;

namespace Octgn.Scripting
{
    [SecuritySafeCritical]
    public class ScriptApi : MarshalByRefObject
    {
        #region Private members

        private Engine engine;

        internal ScriptApi(Engine engine)
        { this.engine = engine; }

        #endregion Private members

        #region Player API

        public int LocalPlayerId()
        { return Player.LocalPlayer != null ? Player.LocalPlayer.Id : -1; }

        public int SharedPlayerId()
        { return Player.GlobalPlayer != null ? Player.GlobalPlayer.Id : -1; }

        public List<int> AllPlayers()
        { return Player.AllExceptGlobal.Select(p => (int)p.Id).ToList(); }

        public string PlayerName(int id)
        { return Player.Find((byte)id).Name; }

        public string PlayerColor(int id)
        { return Player.Find((byte)id).Color.ToString().Remove(1,2); }

        public List<KeyValuePair<int, string>> PlayerCounters(int id)
        {
            return Player.Find((byte)id)
                         .Counters
                         .Select(c => new KeyValuePair<int, string>(c.Id, c.Name))
                         .ToList();
        }

        public int PlayerHandId(int id)
        {
            var hand = Player.Find((byte)id).Hand;
            return hand != null ? hand.Id : 0;
        }

        public List<KeyValuePair<int, string>> PlayerPiles(int id)
        {
            return Player.Find((byte)id)
                         .Groups.OfType<Pile>()
                         .Select(g => new KeyValuePair<int, string>(g.Id, g.Name))
                         .ToList();
        }

        #endregion Player API

        #region Counter API

        public int CounterGet(int id)
        { return Counter.Find(id).Value; }

        public void CounterSet(int id, int value)
        {
            var counter = Counter.Find(id);
            engine.Invoke(() => counter.Value = value);
        }

        #endregion Counter API

        #region Group API

        internal static string GroupCtor(Group group)
        {
            if(group is Table) return "table";
            if(group is Hand) return string.Format("Hand({0}, Player({1}))", group.Id, group.Owner.Id);
            return string.Format("Pile({0}, '{1}', Player({2}))", group.Id, group.Name.Replace("'", @"\'"), group.Owner.Id);
        }

        public string GroupCtor(int id)
        { return GroupCtor(Group.Find(id)); }

        public int GroupCount(int id)
        { return Group.Find(id).Count; }

        public int GroupCard(int id, int index)
        { return Group.Find(id)[index].Id; }

        public int[] GroupCards(int id)
        { return Group.Find(id).Select(c => c.Id).ToArray(); }

        public void GroupShuffle(int id)
        {
            var pile = (Pile)Group.Find(id);

            bool isAsync = engine.Invoke<bool>(() => pile.Shuffle());
            if(!isAsync) return;

            pile.Shuffled += new ShuffleAsync { engine = engine }.Continuation;
            engine.Suspend();
        }

        private class ShuffleAsync
        {
            public Engine engine;

            public void Continuation(object sender, EventArgs e)
            {
                ((Group)sender).Shuffled -= Continuation;
                engine.Resume();
            }
        }

        #endregion Group API

        #region Cards API

        public string[] CardProperties()
        { return Program.Game.Definition.CardDefinition.Properties.Keys.ToArray(); }

        public Tuple<int, int> CardSize()
        {
            var def = Program.Game.Definition.CardDefinition;
            return Tuple.Create(def.Width, def.Height);
        }

        public bool IsAlternateImage(int id)
        {
            return Card.Find(id).IsAlternateImage;
        }

        public void SwitchImage(int id)
        {
            Card c = Card.Find(id);
            //c.IsAlternateImage = (c.IsAlternateImage != true);
            engine.Invoke(() => { c.IsAlternateImage = (c.IsAlternateImage != true); });
        }

        public string CardName(int id)
        { return Card.Find(id).Name; }

        public string CardModel(int id)
        {
            var c = Card.Find(id);
            if(!c.FaceUp || c.Type.model == null) return null;
            return c.Type.model.Id.ToString();
        }

        public object CardProperty(int id, string property)
        {
            var c = Card.Find(id);
            if(!c.FaceUp || c.Type.model == null) return "?";
            return c.Type.model.Properties[property];
        }

        public int CardOwner(int id)
        { return Card.Find(id).Owner.Id; }

        public int CardController(int id)
        { return Card.Find(id).Controller.Id; }

        public int CardGroup(int id)
        { return Card.Find(id).Group.Id; }

        public bool CardGetFaceUp(int id)
        { return Card.Find(id).FaceUp; }

        public void CardSetFaceUp(int id, bool value)
        {
            var card = Card.Find(id);
            engine.Invoke(() => card.FaceUp = value);
        }

        public int CardGetOrientation(int id)
        { return (int)Card.Find(id).Orientation; }

        public void CardSetOrientation(int id, int rot)
        {
            if(rot < 0 || rot > 3) throw new IndexOutOfRangeException("orientation must be between 0 and 3");
            var card = Card.Find(id);
            engine.Invoke(() => card.Orientation = (CardOrientation)rot);
        }

        public string CardGetHighlight(int id)
        {
            var colorOrNull = Card.Find(id).HighlightColor;
            if(colorOrNull == null) return null;
            var color = colorOrNull.Value;
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public void CardSetHighlight(int id, string color)
        {
            var card = Card.Find(id);
            var value = color == null ? null : (Media.Color?)Media.ColorConverter.ConvertFromString(color);
            engine.Invoke(() => card.HighlightColor = value);
        }

        public void CardPosition(int id, out double x, out double y)
        {
            var c = Card.Find(id);
            x = c.X; y = c.Y;
        }

        public void CardMoveTo(int cardId, int groupId, int? position)
        {
            var card = Card.Find(cardId);
            var group = Group.Find(groupId);
            engine.Invoke(() =>
            {
                if(position == null) card.MoveTo(group, true);
                else card.MoveTo(group, true, position.Value);
            });
        }

        public void CardMoveToTable(int cardId, double x, double y, bool forceFaceDown)
        {
            var c = Card.Find(cardId);
            bool faceUp = forceFaceDown ? false :
                          c.Group is Table ? c.FaceUp : true;
            engine.Invoke(() => c.MoveToTable((int)x, (int)y, faceUp, Program.Game.Table.Count));
        }

        public void CardSelect(int id)
        {
            var c = Card.Find(id);
            // At the moment, only table and hand support multiple selection
            engine.Invoke(() =>
            {
                if(c.Group is Table || c.Group is Hand)
                    Play.Gui.Selection.Add(c);
                else
                    Play.Gui.Selection.Clear();
            });
        }

        //Returns the card's index
        //ralig98
        public int CardGetIndex(int CardId)
        {
            return Card.Find(CardId).GetIndex();
        }

        //Set's the card's index to idx.  Enforces a TableOnly rule, since the index's on other piles/groups are inverted.
        //ralig98
        public void CardSetIndex(int CardId, int idx, bool TableOnly = false)
        {
            var c = Card.Find(CardId);
            if (TableOnly)
            {
                if (c.Group is Table)
                    engine.Invoke(() => c.MoveToTable((int)c.X, (int)c.Y, c.FaceUp, idx)); 
            }
            else
                engine.Invoke(() => c.MoveToTable((int)c.X, (int)c.Y, c.FaceUp, idx));
        }

        public void CardTarget(int id, bool active)
        {
            var c = Card.Find(id);
            engine.Invoke(() =>
            {
                if(active) c.Target(); else c.Untarget();
            });
        }

        public int CardTargeted(int id)
        {
            var c = Card.Find(id);
            return c.TargetedBy != null ? c.TargetedBy.Id : -1;
        }

        public Tuple<string, string>[] CardGetMarkers(int id)
        {
            return Card.Find(id).Markers.Select(m => Tuple.Create(m.Model.Name, m.Model.id.ToString())).ToArray();
        }

        public int MarkerGetCount(int cardId, string markerName, string markerId)
        {
            var card = Card.Find(cardId);
            var marker = card.FindMarker(Guid.Parse(markerId), markerName);
            if(marker == null) return 0;
            return marker.Count;
        }

        public void MarkerSetCount(int cardId, int count, string markerName, string markerId)
        {
            if(count < 0) count = 0;
            var card = Card.Find(cardId);
            Guid guid = Guid.Parse(markerId);
            var marker = card.FindMarker(guid, markerName);
            engine.Invoke(() =>
              {
                  card.SetMarker(Player.LocalPlayer, guid, markerName, count);
                  Program.Client.Rpc.SetMarkerReq(card, guid, markerName, (ushort)count);
              });
        }

        #endregion Cards API

        #region Messages API

        public void Mute(bool muted)
        {
            var job = engine.CurrentJob;
            engine.CurrentJob.muted = muted ? job.id : 0;
        }

        public void Notify(string message)
        {
            engine.Invoke(() => Program.Client.Rpc.PrintReq(message));
        }

        public void Whisper(string message)
        {
            engine.Invoke(() => Program.Print(Player.LocalPlayer, message));
        }

        public bool Confirm(string message)
        {
            return engine.Invoke<bool>(() => Octgn.Script.OCTGN.Confirm(message));
        }

        public int? AskInteger(string question, int defaultValue)
        {
            return engine.Invoke<int?>(() =>
            {
                var dlg = new Octgn.Script.InputDlg("Question", question, defaultValue.ToString());
                int result = dlg.GetPositiveInt();
                return dlg.DialogResult.GetValueOrDefault() ? result : (int?)null;
            });
        }

        public Tuple<string, string, int> AskMarker()
        {
            return engine.Invoke<Tuple<string, string, int>>(() =>
            {
                var dlg = new Octgn.Script.MarkerDlg() { Owner = Application.Current.MainWindow };
                if(!dlg.ShowDialog().GetValueOrDefault()) return null;
                return Tuple.Create(dlg.MarkerModel.Name, dlg.MarkerModel.id.ToString(), dlg.Quantity);
            });
        }

        public Tuple<string, int> AskCard(string restriction)
        {
            return engine.Invoke<Tuple<string, int>>(() =>
            {
                var dlg = new Octgn.Script.CardDlg(restriction) { Owner = Application.Current.MainWindow };
                if(!dlg.ShowDialog().GetValueOrDefault()) return null;
                return Tuple.Create(dlg.SelectedCard.Id.ToString(), dlg.Quantity);
            });
        }

        #endregion Messages API

        #region Random

        public int Random(int min, int max)
        {
            var capture = new RandomAsync { engine = engine, reqId = RandomRequest.GenerateId() };
            RandomRequest.Completed += capture.Continuation;
            using(new Networking.Mute(engine.CurrentJob.muted))
                Program.Client.Rpc.RandomReq(capture.reqId, min, max);
            engine.Suspend();
            return capture.result;
        }

        private class RandomAsync
        {
            public Engine engine;
            public int reqId;
            public int result;

            public void Continuation(object sender, EventArgs e)
            {
                var req = (RandomRequest)sender;
                if(req.Id != reqId) return;
                RandomRequest.Completed -= Continuation;

                result = req.Result;
                engine.Resume();
            }
        }

        #endregion Random

        #region Special APIs

        public List<int> CreateOnTable(string modelId, int x, int y, bool persist, int quantity)
        {
            var result = new List<int>();

            Guid modelGuid;
            if(!Guid.TryParse(modelId, out modelGuid))
                return result;   // e.g. modelId may be null if the cloned card is face down.

            engine.Invoke(() =>
            {
                var model = Database.GetCardById(modelGuid);
                int[] ids = new int[quantity];
                ulong[] keys = new ulong[quantity];
                Guid[] models = new Guid[quantity];
                int[] xs = new int[quantity], ys = new int[quantity];

                var def = Program.Game.Definition.CardDefinition;

                if(Player.LocalPlayer.InvertedTable)
                {
                    x -= def.Width; y -= def.Height;
                }
                int offset = (int)(Math.Min(def.Width, def.Height) * 0.2);
                if(Program.GameSettings.UseTwoSidedTable && Play.Gui.TableControl.IsInInvertedZone(y))
                    offset = -offset;

                for(int i = 0; i < quantity; ++i)
                {
                    ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | model.Id.Condense();
                    int id = Program.Game.GenerateCardId();

                    new Play.Actions.CreateCard(Player.LocalPlayer, id, key, true, model, x, y, !persist).Do();

                    ids[i] = id; keys[i] = key; models[i] = model.Id; xs[i] = x; ys[i] = y;
                    result.Add(id);

                    x += offset; y += offset;
                }

                Program.Client.Rpc.CreateCardAt(ids, keys, models, xs, ys, true, persist);
            });

            return result;
        }

        public bool IsTwoSided()
        { return Program.GameSettings.UseTwoSidedTable; }

        public string Web_Read(string url)
        {
            string result = "";
            try
            {
                WebClient client = new WebClient();
                result = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                
            }
            return result;
        }

        #endregion Special APIs
    }
}