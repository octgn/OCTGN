using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Windows.Media;
using Octgn.Data;
using Octgn.Definitions;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Play.Gui;
using Octgn.Script;

namespace Octgn.Scripting
{
    [SecuritySafeCritical]
    public class ScriptApi : MarshalByRefObject
    {
        #region Private members

        private readonly Engine engine;

        internal ScriptApi(Engine engine)
        {
            this.engine = engine;
        }

        #endregion Private members

        #region Player API

        public int LocalPlayerId()
        {
            return Player.LocalPlayer != null ? Player.LocalPlayer.Id : -1;
        }

        public int SharedPlayerId()
        {
            return Player.GlobalPlayer != null ? Player.GlobalPlayer.Id : -1;
        }

        public List<int> AllPlayers()
        {
            return Player.AllExceptGlobal.Select(p => (int) p.Id).ToList();
        }

        public string PlayerName(int id)
        {
            return Player.Find((byte) id).Name;
        }

        public string PlayerColor(int id)
        {
            return Player.Find((byte) id).Color.ToString().Remove(1, 2);
        }

        public bool IsActivePlayer(int id)
        {
            return (Program.Game.TurnPlayer.Id == id);
        }

        public List<KeyValuePair<int, string>> PlayerCounters(int id)
        {
            return Player.Find((byte) id)
                .Counters
                .Select(c => new KeyValuePair<int, string>(c.Id, c.Name))
                .ToList();
        }

        public int PlayerHandId(int id)
        {
            Hand hand = Player.Find((byte) id).Hand;
            return hand != null ? hand.Id : 0;
        }

        public List<KeyValuePair<int, string>> PlayerPiles(int id)
        {
            return Player.Find((byte) id)
                .Groups.OfType<Pile>()
                .Select(g => new KeyValuePair<int, string>(g.Id, g.Name))
                .ToList();
        }

        public bool PlayerHasInvertedTable(int id)
        {
            return Player.Find((byte) id).InvertedTable;
        }

        #endregion Player API

        #region Counter API

        public int CounterGet(int id)
        {
            return Counter.Find(id).Value;
        }

        public void CounterSet(int id, int value)
        {
            Counter counter = Counter.Find(id);
            engine.Invoke(() => counter.Value = value);
        }

        #endregion Counter API

        #region Group API

        internal static string GroupCtor(Group group)
        {
            if (group is Table) return "table";
            if (group is Hand) return string.Format("Hand({0}, Player({1}))", group.Id, group.Owner.Id);
            return string.Format("Pile({0}, '{1}', Player({2}))", group.Id, group.Name.Replace("'", @"\'"),
                                 group.Owner.Id);
        }

        public string GroupCtor(int id)
        {
            return GroupCtor(Group.Find(id));
        }

        public int GroupCount(int id)
        {
            return Group.Find(id).Count;
        }

        public int GroupCard(int id, int index)
        {
            return Group.Find(id)[index].Id;
        }

        public int[] GroupCards(int id)
        {
            return Group.Find(id).Select(c => c.Id).ToArray();
        }

        public void GroupShuffle(int id)
        {
            var pile = (Pile) Group.Find(id);

            var isAsync = engine.Invoke<bool>(() => pile.Shuffle());
            if (!isAsync) return;

            pile.Shuffled += new ShuffleAsync {engine = engine}.Continuation;
            engine.Suspend();
        }

        private class ShuffleAsync
        {
            public Engine engine;

            public void Continuation(object sender, EventArgs e)
            {
                ((Group) sender).Shuffled -= Continuation;
                engine.Resume();
            }
        }

        #endregion Group API

        #region Cards API

        public string[] CardProperties()
        {
            return Program.Game.Definition.CardDefinition.Properties.Keys.ToArray();
        }

        public Tuple<int, int> CardSize()
        {
            CardDef def = Program.Game.Definition.CardDefinition;
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
        {
            return Card.Find(id).Name;
        }

        public string CardModel(int id)
        {
            Card c = Card.Find(id);
            if (!c.FaceUp || c.Type.Model == null) return null;
            return c.Type.Model.Id.ToString();
        }

        public object CardProperty(int id, string property)
        {
            Card c = Card.Find(id);
            if (!c.FaceUp || c.Type.Model == null) return "?";
            return c.Type.Model.Properties[property];
        }

        public int CardOwner(int id)
        {
            return Card.Find(id).Owner.Id;
        }

        public int CardController(int id)
        {
            return Card.Find(id).Controller.Id;
        }

        public int CardGroup(int id)
        {
            return Card.Find(id).Group.Id;
        }

        public bool CardGetFaceUp(int id)
        {
            return Card.Find(id).FaceUp;
        }

        public void CardSetFaceUp(int id, bool value)
        {
            Card card = Card.Find(id);
            engine.Invoke(() => card.FaceUp = value);
        }

        public int CardGetOrientation(int id)
        {
            return (int) Card.Find(id).Orientation;
        }

        public void CardSetOrientation(int id, int rot)
        {
            if (rot < 0 || rot > 3) throw new IndexOutOfRangeException("orientation must be between 0 and 3");
            Card card = Card.Find(id);
            engine.Invoke(() => card.Orientation = (CardOrientation) rot);
        }

        public string CardGetHighlight(int id)
        {
            Color? colorOrNull = Card.Find(id).HighlightColor;
            if (colorOrNull == null) return null;
            Color color = colorOrNull.Value;
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public void CardSetHighlight(int id, string color)
        {
            Card card = Card.Find(id);
            Color? value = color == null ? null : (Color?) ColorConverter.ConvertFromString(color);
            engine.Invoke(() => card.HighlightColor = value);
        }

        public void CardPosition(int id, out double x, out double y)
        {
            Card c = Card.Find(id);
            x = c.X;
            y = c.Y;
        }

        public void CardMoveTo(int cardId, int groupId, int? position)
        {
            Card card = Card.Find(cardId);
            Group group = Group.Find(groupId);
            engine.Invoke(() =>
                              {
                                  if (position == null) card.MoveTo(group, true);
                                  else card.MoveTo(group, true, position.Value);
                              });
        }

        public void CardMoveToTable(int cardId, double x, double y, bool forceFaceDown)
        {
            Card c = Card.Find(cardId);
            bool faceUp = !forceFaceDown && (!(c.Group is Table) || c.FaceUp);
            engine.Invoke(() => c.MoveToTable((int) x, (int) y, faceUp, Program.Game.Table.Count));
        }

        public void CardSelect(int id)
        {
            Card c = Card.Find(id);
            // At the moment, only table and hand support multiple selection
            engine.Invoke(() =>
                              {
                                  if (c.Group is Table || c.Group is Hand)
                                      Selection.Add(c);
                                  else
                                      Selection.Clear();
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
            Card c = Card.Find(CardId);
            if (TableOnly)
            {
                if (c.Group is Table)
                    engine.Invoke(() => c.MoveToTable((int) c.X, (int) c.Y, c.FaceUp, idx));
            }
            else
                engine.Invoke(() => c.MoveToTable((int) c.X, (int) c.Y, c.FaceUp, idx));
        }

        public void CardTarget(int id, bool active)
        {
            Card c = Card.Find(id);
            engine.Invoke(() =>
                              {
                                  if (active) c.Target();
                                  else c.Untarget();
                              });
        }

        public int CardTargeted(int id)
        {
            Card c = Card.Find(id);
            return c.TargetedBy != null ? c.TargetedBy.Id : -1;
        }

        public Tuple<string, string>[] CardGetMarkers(int id)
        {
            return Card.Find(id).Markers.Select(m => Tuple.Create(m.Model.Name, m.Model.id.ToString())).ToArray();
        }

        public int MarkerGetCount(int cardId, string markerName, string markerId)
        {
            Card card = Card.Find(cardId);
            Marker marker = card.FindMarker(Guid.Parse(markerId), markerName);
            if (marker == null) return 0;
            return marker.Count;
        }

        public void MarkerSetCount(int cardId, int count, string markerName, string markerId)
        {
            if (count < 0) count = 0;
            Card card = Card.Find(cardId);
            Guid guid = Guid.Parse(markerId);
            //Marker marker = card.FindMarker(guid, markerName);
            engine.Invoke(() =>
                              {
                                  card.SetMarker(Player.LocalPlayer, guid, markerName, count);
                                  Program.Client.Rpc.SetMarkerReq(card, guid, markerName, (ushort) count);
                              });
        }

        #endregion Cards API

        #region Messages API

        public void Mute(bool muted)
        {
            ScriptJob job = engine.CurrentJob;
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
            return engine.Invoke<bool>(() => OCTGN.Confirm(message));
        }

        public int? AskInteger(string question, int defaultValue)
        {
            return engine.Invoke<int?>(() =>
                                           {
                                               var dlg = new InputDlg("Question", question,
                                                                      defaultValue.ToString(CultureInfo.InvariantCulture));
                                               int result = dlg.GetPositiveInt();
                                               return dlg.DialogResult.GetValueOrDefault() ? result : (int?) null;
                                           });
        }

        public Tuple<string, string, int> AskMarker()
        {
            return engine.Invoke<Tuple<string, string, int>>(() =>
                                                                 {
                                                                     //fix MAINWINDOW bug
                                                                     var dlg = new MarkerDlg
                                                                                   {Owner = Program.PlayWindow};
                                                                     if (!dlg.ShowDialog().GetValueOrDefault())
                                                                         return null;
                                                                     return Tuple.Create(dlg.MarkerModel.Name,
                                                                                         dlg.MarkerModel.id.ToString(),
                                                                                         dlg.Quantity);
                                                                 });
        }

        public Tuple<string, int> AskCard(string restriction)
        {
            return engine.Invoke<Tuple<string, int>>(() =>
                                                         {
                                                             //fix MAINWINDOW bug
                                                             var dlg = new CardDlg(restriction)
                                                                           {Owner = Program.PlayWindow};
                                                             if (!dlg.ShowDialog().GetValueOrDefault()) return null;
                                                             return Tuple.Create(dlg.SelectedCard.Id.ToString(),
                                                                                 dlg.Quantity);
                                                         });
        }

        #endregion Messages API

        #region Random

        public int Random(int min, int max)
        {
            var capture = new RandomAsync {engine = engine, reqId = RandomRequest.GenerateId()};
            RandomRequest.Completed += capture.Continuation;
            using (new Mute(engine.CurrentJob.muted))
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
                var req = (RandomRequest) sender;
                if (req.Id != reqId) return;
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
            if (!Guid.TryParse(modelId, out modelGuid))
                return result; // e.g. modelId may be null if the cloned card is face down.

            engine.Invoke(() =>
                              {
                                  CardModel model = Database.GetCardById(modelGuid);
                                  if (model != null)
                                  {
                                      var ids = new int[quantity];
                                      var keys = new ulong[quantity];
                                      var models = new Guid[quantity];
                                      int[] xs = new int[quantity], ys = new int[quantity];

                                      CardDef def = Program.Game.Definition.CardDefinition;

                                      if (Player.LocalPlayer.InvertedTable)
                                      {
                                          x -= def.Width;
                                          y -= def.Height;
                                      }
                                      var offset = (int) (Math.Min(def.Width, def.Height)*0.2);
                                      if (Program.GameSettings.UseTwoSidedTable && TableControl.IsInInvertedZone(y))
                                          offset = -offset;

                                      for (int i = 0; i < quantity; ++i)
                                      {
                                          ulong key = ((ulong) Crypto.PositiveRandom()) << 32 | model.Id.Condense();
                                          int id = Program.Game.GenerateCardId();

                                          new CreateCard(Player.LocalPlayer, id, key, true, model, x, y, !persist).Do();

                                          ids[i] = id;
                                          keys[i] = key;
                                          models[i] = model.Id;
                                          xs[i] = x;
                                          ys[i] = y;
                                          result.Add(id);

                                          x += offset;
                                          y += offset;
                                      }

                                      Program.Client.Rpc.CreateCardAt(ids, keys, models, xs, ys, true, persist);
                                  }
                              });

            return result;
        }

        public bool IsTwoSided()
        {
            return Program.GameSettings.UseTwoSidedTable;
        }

        public Tuple<String, int> Web_Read(string url)
        {
            int statusCode = 200;
            string result = "";
            StreamReader reader = null;

            try
            {
                //asking for permission to call the specified url.
                var permission = new WebPermission();
                permission.AddPermission(NetworkAccess.Connect, url);
                permission.Assert();


                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();

                reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                //Properly handling http errors here
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse) ex.Response;
                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            result = "eror";
                            statusCode = 404;
                            break;
                        case HttpStatusCode.Forbidden:
                            result = "error";
                            statusCode = 403;
                            break;
                        case HttpStatusCode.InternalServerError:
                            result = "error";
                            statusCode = 500;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //placeholder for other sorts of exceptions
            }
            finally
            {
                // general cleanup
                if (reader != null)
                {
                    reader.Close(); //closes the reader and the response stream it was working on at the same time.
                }
            }

            return Tuple.Create(result, statusCode);
        }

        public bool Open_URL(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://"))
            {
                if (engine.Invoke<bool>(() => OCTGN.Confirm("Do you wish to go to the site: " + url + "?")))
                {
                    try
                    {
                        engine.Invoke(() => { Process.Start(url); });
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return false;
            }
            return false;
        }

        public string OCTGN_Version()
        {
            return OctgnApp.OctgnVersion.ToString();
        }

        public string GameDef_Version()
        {
            return Program.Game.Definition.Version.ToString();
        }

        #endregion Special APIs

        #region GlobalVariables

        public void PlayerSetGlobalVariable(int id, string name, object value)
        {
            string val = String.Format("{0}", value);
            Player p = Player.Find((byte) id);
            if (p == null || p.Id != Player.LocalPlayer.Id)
                return;
            if (Player.LocalPlayer.GlobalVariables.ContainsKey(name))
                engine.Invoke(() => Player.LocalPlayer.GlobalVariables[name] = val);
            else
                engine.Invoke(() => Player.LocalPlayer.GlobalVariables.Add(name, val));
            Program.Client.Rpc.PlayerSetGlobalVariable(Player.LocalPlayer, name, val);
        }

        public string PlayerGetGlobalVariable(int id, string name)
        {
            Player p = Player.Find((byte) id);
            if (p == null)
                return "";
            if (p.GlobalVariables.ContainsKey(name))
                return p.GlobalVariables[name];
            return "";
        }

        public void SetGlobalVariable(string name, object value)
        {
            string val = String.Format("{0}", value);
            if (Program.Game.GlobalVariables.ContainsKey(name))
                engine.Invoke(() => Program.Game.GlobalVariables[name] = val);
            else
                engine.Invoke(() => Program.Game.GlobalVariables.Add(name, val));
            Program.Client.Rpc.SetGlobalVariable(name, val);
        }

        public string GetGlobalVariable(string name)
        {
            if (Program.Game.GlobalVariables.ContainsKey(name))
                return Program.Game.GlobalVariables[name];
            return "";
        }

        #endregion
    }
}