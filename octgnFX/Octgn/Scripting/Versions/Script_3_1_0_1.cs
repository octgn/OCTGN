using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Octgn.Core;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.Util;
using Octgn.DataNew.Entities;
using Octgn.Extentions;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Play.Gui;
using Octgn.Scripting.Controls;
using Octgn.Utils;
using Card = Octgn.Play.Card;
using Counter = Octgn.Play.Counter;
using Group = Octgn.Play.Group;
using Marker = Octgn.Play.Marker;
using Player = Octgn.Play.Player;
using Octgn.Library.Utils;

namespace Octgn.Scripting.Versions
{
    [Versioned("3.1.0.1")]
    public class Script_3_1_0_1 : ScriptApi
    {
        #region Player API

        public string LocalPlayerId()
        {
            return Player.LocalPlayer.Id.ToString();
        }

        public string SharedPlayerId()
        {
            return (Player.GlobalPlayer?.Id).ToString();
        }

        public List<string> AllPlayers()
        {
            return Player.AllExceptGlobal.Select(p => p.Id.ToString()).ToList();
        }

        public string PlayerName(string id)
        {
            return Player.Find(Guid.Parse(id)).Name;
        }

        public string PlayerColor(string id)
        {
            return Player.Find(Guid.Parse(id)).Color.ToString().Remove(1, 2);
        }

        public bool IsActivePlayer(string id)
        {
            if (Program.GameEngine.TurnPlayer == null)
                return false;
            return (Program.GameEngine.TurnPlayer.Id.Equals(Guid.Parse(id)));
        }

        public void setActivePlayer(string id)
        {
            if (Program.GameEngine.TurnPlayer == null || Program.GameEngine.TurnPlayer == Player.LocalPlayer)
                Program.Client.Rpc.NextTurn(Player.Find(Guid.Parse(id)));
        }

        public bool IsSubscriber(string id)
        {
            return Player.Find(Guid.Parse(id)).Subscriber;
        }

        public List<KeyValuePair<string, string>> PlayerCounters(string id)
        {
            return Player.Find(Guid.Parse(id))
                .Counters
                .Select(c => new KeyValuePair<string, string>(c.Key.ToString(), c.Value.Name))
                .ToList();
        }

        public string PlayerHandId(string id)
        {
            Hand hand = Player.Find(Guid.Parse(id)).Hand;
            return hand.Id.ToString();
        }

        public List<KeyValuePair<string, string>> PlayerPiles(string id)
        {
            return Player.Find(Guid.Parse(id))
                .Groups.OfType<Pile>()
                .Select(g => new KeyValuePair<string, string>(g.Id.ToString(), g.Name))
                .ToList();
        }

        public bool PlayerHasInvertedTable(string id)
        {
            return Player.Find(Guid.Parse(id)).InvertedTable;
        }

        #endregion Player API

        #region Counter API

        public int CounterGet(string id)
        {
            return Counter.Find(Guid.Parse(id)).Value;
        }

        public void CounterSet(string id, int value)
        {
            Counter counter = Counter.Find(Guid.Parse(id));
            QueueAction(
                () =>
                {
                    //Program.GameEngine.EventProxy.MuteEvents = true;
                    counter.Value = value;
                    //Program.GameEngine.EventProxy.MuteEvents = false;
                });
        }

        #endregion Counter API

        #region Group API

        public string GroupCtor(string id)
        {
            return PythonConverter.GroupCtor(Group.Find(Guid.Parse(id)));
        }

        public int GroupCount(string id)
        {
            return Group.Find(Guid.Parse(id)).Count;
        }

        public string GroupCard(string id, int index)
        {
            var c = Group.Find(Guid.Parse(id))[index];
            if (c == null) return null;
            return c.Id.ToString();
        }

        public string[] GroupCards(string id)
        {
            return Group.Find(Guid.Parse(id)).Select(c => c.Id.ToString()).ToArray();
        }

        public void GroupShuffle(string id)
        {
            var pile = (Pile)Group.Find(Guid.Parse(id));
            if (pile.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't shuffle {1} because they don't control it.", Player.LocalPlayer.Name, pile.Name));

            QueueAction(() => pile.Shuffle());

        }

        public string GroupGetVisibility(string id)
        {
            Group g = Group.Find(Guid.Parse(id));
            DataNew.Entities.GroupVisibility vis = g.Visibility;
            switch (vis)
            {
                case DataNew.Entities.GroupVisibility.Everybody:
                    return "all";
                case DataNew.Entities.GroupVisibility.Nobody:
                    return "none";
                case DataNew.Entities.GroupVisibility.Owner:
                    return "me";
                case DataNew.Entities.GroupVisibility.Undefined:
                    return "undefined";
                case DataNew.Entities.GroupVisibility.Custom:
                    if (g.Viewers.Count == 1 && g.Viewers[0] == g.Controller)
                        return "me";
                    else
                        return "custom";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void GroupSetVisibility(string id, string v)
        {
            Group group = Group.Find(Guid.Parse(id));
            if (group.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning("{0} can't set visibility on {0} because they don't control it.", Player.LocalPlayer.Name, group.Name);
                return;
            }
            QueueAction(
                () =>
                {
                    switch (v.ToLower())
                    {
                        case "none":
                            group.SetVisibility(false, true);
                            return;
                        case "all":
                            group.SetVisibility(true, true);
                            return;
                        case "undefined":
                            group.SetVisibility(null, true);
                            return;
                        case "me":
                            group.SetVisibility(false, true);
                            group.AddViewer(Player.LocalPlayer, true);
                            return;
                        default:
                            Program.GameMess.Warning("Invalid visibility type '{0}'", v);
                            return;
                    }
                });
        }
        public bool GroupGetCollapsed(string id)
        {
            var g = Group.Find(Guid.Parse(id));
            if (!(g is Pile)) return false;
            Pile pile = (Pile)g;
            return pile.Collapsed;
        }
        public void GroupSetCollapsed(string id, bool value)
        {
            var g = Group.Find(Guid.Parse(id));
            if (!(g is Pile)) return;
            Pile pile = (Pile)g;
            QueueAction(() => pile.Collapsed = value);
        }
        public void GroupLookAt(string id, int value, bool isTop)
        {
            var g = (Pile)Group.Find(Guid.Parse(id));
            if (g.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning(String.Format("{0} can't look at {1} because they don't control it.", Player.LocalPlayer.Name, g.Name));
            }
            PlayWindow playWindow = WindowManager.PlayWindow;
            if (playWindow == null) return;
            Octgn.Controls.ChildWindowManager manager = playWindow.wndManager;
            if (value > 0)
            {
                if (isTop) QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.Top, value)));
                else QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.Bottom, value)));
            }

            else if (value == 0)
            {
                int count;
                if (isTop)
                {
                    count = QueueAction<int>(() => Dialog.InputPositiveInt("View top cards", "How many cards do you want to see?", 1));
                    QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.Top, count)));
                }
                else
                {
                    count = QueueAction<int>(() => Dialog.InputPositiveInt("View bottom cards", "How many cards do you want to see?", 1));
                    QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.Bottom, count)));
                }
            }
            else QueueAction(() => manager.Show(new GroupWindow(@g, PilePosition.All, 0)));
        }
        public string[] GroupViewers(string id)
        {
            return Group.Find(Guid.Parse(id)).Viewers.Select(p => p.Id.ToString()).ToArray();
        }
        public void GroupAddViewer(string id, string pid)
        {
            Group group = Group.Find(Guid.Parse(id));
            Player player = Player.Find(Guid.Parse(pid));
            if (group.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning("{0} can't set visibility on {0} because they don't control it.", Player.LocalPlayer.Name, group.Name);
                return;
            }
            if (group.Viewers.Contains(player)) return;
            else
            {
                QueueAction(() => group.AddViewer(player, true));
            }
        }
        public void GroupRemoveViewer(string id, string pid)
        {
            Group group = Group.Find(Guid.Parse(id));
            Player player = Player.Find(Guid.Parse(pid));
            if (group.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning("{0} can't set visibility on {0} because they don't control it.", Player.LocalPlayer.Name, group.Name);
                return;
            }
            if (!group.Viewers.Contains(player)) return;
            else
            {
                QueueAction(() => group.RemoveViewer(player, true));
            }
        }
        public string GroupController(string id)
        {
            return Group.Find(Guid.Parse(id)).Controller.Id.ToString();
        }

        public bool IsTableBackgroundFlipped()
        {
            return Program.GameEngine.IsTableBackgroundFlipped;
        }

        public void SetTableBackgroundFlipped(bool isFlipped)
        {
            Program.Client.Rpc.IsTableBackgroundFlipped(isFlipped);
        }

        public void GroupSetController(string id, string player)
        {
            var g = Group.Find(Guid.Parse(id));
            var p = Player.Find(Guid.Parse(player));

            if (p == Player.LocalPlayer)
            {
                if (g.Controller == Player.LocalPlayer) return;
                QueueAction(() => g.TakeControl());
            }
            else
            {
                if (g.Controller != Player.LocalPlayer) return;
                QueueAction(() => g.PassControlTo(p));
            }
        }


        #endregion Group API

        #region Cards API

        public string[] CardProperties()
        {
            return Program.GameEngine.Definition.CustomProperties.Select(x => x.Name).ToArray();
        }

        public void CardSwitchTo(string id, string alternate)
        {
            var c = Card.Find(Guid.Parse(id));
            if (c == null) return;
            QueueAction(() => c.SwitchTo(Player.LocalPlayer, alternate));

        }

        public CardSize CardSize(string id)
        {
            var c = Card.Find(Guid.Parse(id));
            if (c == null) return null;
            return c.Size;
        }

        public string[] CardAlternates(string id)
        {
            var c = Card.Find(Guid.Parse(id));
            if (c == null) return new string[0];
            //if ((!c.FaceUp && !c.PeekingPlayers.Contains(Player.LocalPlayer)) || c.Type.Model == null) return new string[0];
            return c.Alternates();
        }

        public string CardAlternate(string id)
        {
            var c = Card.Find(Guid.Parse(id));
            if (c == null) return "";
            //if ((!c.FaceUp && !c.PeekingPlayers.Contains(Player.LocalPlayer)) || c.Type.Model == null) return "";
            return c.Alternate();
        }

        public string CardName(string id)
        {
            return Card.Find(Guid.Parse(id)).Name;
        }

        public string CardModel(string id)
        //Why is this public? I would expect the model to be private - (V)_V
        // Ur dumb that's why.
        {
            Card c = Card.Find(Guid.Parse(id));
            //if (!c.FaceUp || c.Type.Model == null) return null;
            return c.Type.Model.Id.ToString();
        }

        public string CardSet(string id)
        {
            Card c = Card.Find(Guid.Parse(id));
            string set = c.Type.Model.GetSet().Name;
            return set;
        }

        public string CardSetId(string id)
        {
            Card c = Card.Find(Guid.Parse(id));
            string setId = c.Type.Model.SetId.ToString();
            return setId;
        }

        public object CardProperty(string id, string property)
        {
            Card c = Card.Find(Guid.Parse(id));
            property = property.ToLowerInvariant();
            return c.GetProperty(property, "", StringComparison.InvariantCultureIgnoreCase, c.Alternate());
        }

        public object CardAlternateProperty(string id, string alt, string property)
        {
            Card c = Card.Find(Guid.Parse(id));
            property = property.ToLowerInvariant();
            return c.GetProperty(property, "", StringComparison.InvariantCultureIgnoreCase, alt);
        }

        public string CardOwner(string id)
        {
            return Card.Find(Guid.Parse(id)).Owner.Id.ToString();
        }

        public string CardController(string id)
        {
            return Card.Find(Guid.Parse(id)).Controller.Id.ToString();
        }

        public void SetController(string id, string player)
        {
            Card c = Card.Find(Guid.Parse(id));
            Player p = Player.Find(Guid.Parse(player));
            Player controller = c.Controller;

            if (p == Player.LocalPlayer)
            {
                if (c.Controller == Player.LocalPlayer) return;
                QueueAction(() => c.TakeControl());
            }
            else
            {
                if (c.Controller != Player.LocalPlayer) return;
                QueueAction(() => c.PassControlTo(p));
            }
        }

        public string CardGroup(string id)
        {
            return Card.Find(Guid.Parse(id)).Group.Id.ToString();
        }

        public bool CardGetFaceUp(string id)
        {
            return Card.Find(Guid.Parse(id)).FaceUp;
        }

        public void CardSetFaceUp(string id, bool value)
        {
            Card card = Card.Find(Guid.Parse(id));

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't flip up {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));

            QueueAction(() => card.FaceUp = value);
        }

        public int CardGetOrientation(string id)
        {
            return (int)Card.Find(Guid.Parse(id)).Orientation;
        }

        public void CardSetOrientation(string id, int rot)
        {
            if (rot < 0 || rot > 3) throw new IndexOutOfRangeException("orientation must be between 0 and 3");
            Card card = Card.Find(Guid.Parse(id));

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't rotate {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));

            QueueAction(() => card.Orientation = (CardOrientation)rot);
        }

        public string CardGetHighlight(string id)
        {
            Color? colorOrNull = Card.Find(Guid.Parse(id)).HighlightColor;
            if (colorOrNull == null) return null;
            Color color = colorOrNull.Value;
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public void CardSetHighlight(string id, string color)
        {
            Card card = Card.Find(Guid.Parse(id));
            Color? value = color == null ? null : (Color?)ColorConverter.ConvertFromString(color);

            /*if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't highlight {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));
            */
            // Will add in checks or controls to handle/allow this. - DS
            QueueAction(() => card.HighlightColor = value);
        }

        public string CardGetFilter(string id)
        {
            Color? colorOrNull = Card.Find(Guid.Parse(id)).FilterColor;
            if (colorOrNull == null) return null;
            Color color = colorOrNull.Value;
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public void CardSetFilter(string id, string color)
        {
            Card card = Card.Find(Guid.Parse(id));
            Color? value = color == null ? null : (Color?)ColorConverter.ConvertFromString(color);

            /*if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't highlight {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));
            */
            // Will add in checks or controls to handle/allow this. - DS
            QueueAction(() => card.FilterColor = value);
        }

        public void CardPosition(string id, out double x, out double y)
        {
            Card c = Card.Find(Guid.Parse(id));
            x = c.X;
            y = c.Y;
        }

        public void CardMoveTo(string cardId, string groupId, int? position)
        {
            Card card = Card.Find(Guid.Parse(cardId));
            Group group = Group.Find(Guid.Parse(groupId));

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} to {2} because they don't control {1}.", Player.LocalPlayer.Name, card.Name, card.Name));

            if (group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} to {2} because they don't control {1}.", Player.LocalPlayer.Name, card.Name, group.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} from {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            QueueAction(() =>
            {
                //Program.GameEngine.EventProxy.MuteEvents = true;
                if (position == null) card.MoveTo(group, true, true);
                else card.MoveTo(group, true, position.Value, true);
                //Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public void CardMoveToTable(string cardId, double x, double y, bool forceFaceDown)
        {
            Card card = Card.Find(Guid.Parse(cardId));

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} to Table because they don't control {1}.", Player.LocalPlayer.Name, card.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't move {1} from {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            bool faceUp = !forceFaceDown && (!(card.Group is Table) || card.FaceUp);
            QueueAction(
                () =>
                {
                    //Program.GameEngine.EventProxy.MuteEvents = true;
                    card.MoveToTable((int)x, (int)y, faceUp, Program.GameEngine.Table.Count, true);
                    //Program.GameEngine.EventProxy.MuteEvents = false;
                });
        }

        public void CardSelect(string id)
        {
            Card c = Card.Find(Guid.Parse(id));
            // At the moment, only table and hand support multiple selection
            QueueAction(() =>
            {
                if (c.Group is Table || c.Group is Hand)
                    Selection.Add(c);
                else
                    Selection.Clear();
            });
        }

        //Returns the card's index
        //ralig98
        public int CardGetIndex(string CardId)
        {
            return Card.Find(Guid.Parse(CardId)).GetIndex();
        }

        //Set's the card's index to idx.  Enforces a TableOnly rule, since the index's on other piles/groups are inverted.
        //ralig98
        public void CardSetIndex(string CardId, int idx, bool TableOnly = false)
        {
            if (idx < 0)
            {
                Program.GameMess.Warning("Cannot setIndex({0}), number is less than 0", idx);
                return;
            }
            Card card = Card.Find(Guid.Parse(CardId));

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set index of {1} to Table because they don't control {1}.", Player.LocalPlayer.Name, card.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set index of {1} in {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            if (TableOnly)
            {
                if (card.Group is Table)
                    QueueAction(
                        () =>
                        {
                            //Program.GameEngine.EventProxy.MuteEvents = true;
                            card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, idx, true);
                            //Program.GameEngine.EventProxy.MuteEvents = false;
                        });
            }
            else
                QueueAction(
                    () =>
                    {
                        //Program.GameEngine.EventProxy.MuteEvents = true;
                        card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, idx, true);
                        //Program.GameEngine.EventProxy.MuteEvents = false;
                    });
        }

        public void CardTarget(string id, bool active)
        {
            Card c = Card.Find(Guid.Parse(id));
            QueueAction(() =>
            {
                //Program.GameEngine.EventProxy.MuteEvents = true;
                if (active) c.Target(true);
                else c.Untarget(true);
                //Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public void CardPeek(string id)
        {
            Card c = Card.Find(Guid.Parse(id));
            QueueAction(() =>
            {
                c.Peek();
            });
        }



        public void CardTargetArrow(string id, string targetId, bool active)
        {
            Card c = Card.Find(Guid.Parse(id));
            Card target = Card.Find(Guid.Parse(targetId));
            QueueAction(() =>
            {
                //Program.GameEngine.EventProxy.MuteEvents = true;
                if (active) c.Target(target, true);
                else c.Untarget(true);
                //Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public string CardTargeted(string id)
        {
            Card c = Card.Find(Guid.Parse(id));
            return (c.TargetedBy?.Id).ToString();
        }

        public Tuple<string, string>[] CardGetMarkers(string id)
        {
            return Card.Find(Guid.Parse(id)).Markers.Select(m => Tuple.Create(m.Model.Name, m.Model.Id.ToString())).ToArray();
        }

        public int MarkerGetCount(string cardId, string markerName, string markerId)
        {
            Card card = Card.Find(Guid.Parse(cardId));
            Marker marker = card.FindMarker(Guid.Parse(markerId), markerName);
            return marker == null ? 0 : marker.Count;
        }

        public void MarkerSetCount(string cardId, int count, string markerName, string markerId)
        {
            if (count < 0) count = 0;
            Card card = Card.Find(Guid.Parse(cardId));
            Guid guid = Guid.Parse(markerId);
            Marker marker = card.FindMarker(guid, markerName);
            int origCount = 0;

            /*if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set markers on {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));
            */
            // Will add in checks or controls to handle/allow this. -- DS
            QueueAction(() =>
            {
                if (marker == null)
                {
                    DataNew.Entities.Marker model = Program.GameEngine.GetMarkerModel(guid);
                    model.Name = markerName;
                    //card.SetMarker(Player.LocalPlayer, guid, markerName, count);
                    Program.Client.Rpc.AddMarkerReq(card, guid, markerName, (ushort)count, (ushort)origCount, true);
                    card.AddMarker(model, (ushort)count);
                }
                else
                {
                    origCount = marker.Count;
                    if (origCount < count)
                    {
                        Program.Client.Rpc.AddMarkerReq(card, guid, markerName, (ushort)(count - origCount), (ushort)origCount, true);
                        card.AddMarker(marker.Model, (ushort)(count - origCount));
                    }
                    else if (origCount > count)
                    {
                        Program.Client.Rpc.RemoveMarkerReq(card, guid, markerName, (ushort)(origCount - count), (ushort)origCount, true);
                        card.RemoveMarker(marker, (ushort)(origCount - count));
                    }
                }
            });
        }

        // TODO: Replace this hack with an actual delete function.
        public void CardDelete(string cardId)
        {
            Card c = Card.Find(Guid.Parse(cardId));
            var card = c;
            if (c == null)
                return;

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't delete {1} to Table because they don't control {1}.", Player.LocalPlayer.Name, card.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't delete {1} from {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            if (c.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning("Cannot delete({0}), because you do not control it. ", cardId);
                return;
            }
            QueueAction(() =>
            {
                if (c == null)
                    return;
                Program.Client.Rpc.DeleteCard(c, Player.LocalPlayer);
                c.Group.Remove(c);
            });
        }

        public bool CardAnchored(string cardId)
        {
            var card = Card.Find(Guid.Parse(cardId));
            if (card == null)
                return false;

            return card.Anchored;
        }

        public void CardSetAnchored(string cardId, bool anchored)
        {
            var card = Card.Find(Guid.Parse(cardId));
            if (card == null)
                return;

            if (card.Group.Definition.Id != Program.GameEngine.Definition.Table.Id)
            {
                Program.GameMess.Warning(String.Format("You can't anchor a card that's not on the table."));
                return;
            }

            if (card.Controller != Player.LocalPlayer)
            {
                Program.GameMess.Warning(String.Format("{0} Can't anchor {1} to Table because they don't control it.", Player.LocalPlayer.Name, card.Name));
                return;
            }

            QueueAction(() =>
            {
                if (card == null)
                    return;
                card.SetAnchored(false, anchored);
            });
        }

        public void CardSetProperty(string cardId, string name, string val)
        {
            var card = Card.Find(Guid.Parse(cardId));
            if (card == null)
            {
                Program.GameMess.Warning("Card " + cardId + " doesn't exist.");
                return;
            }

            card.SetProperty(name, val);
        }

        public void CardResetProperties(string cardId)
        {
            var card = Card.Find(Guid.Parse(cardId));
            if (card == null)
            {
                Program.GameMess.Warning("Card " + cardId + " doesn't exist.");
                return;
            }

            card.ResetProperties();
        }

        #endregion Cards API

        #region Messages API

        public void Mute(bool muted)
        {
            ScriptJobBase job = ScriptEngine.CurrentJob;
            ScriptEngine.CurrentJob.Muted = muted ? (Guid?)job.id : null;
        }

        public void Notify(string message)
        {
            QueueAction(() =>
            {
                Program.Client.Rpc.PrintReq(message);
                Program.Print(Player.LocalPlayer, message);
            });
        }

        public void Whisper(string message)
        {
            QueueAction(() => Program.Print(Player.LocalPlayer, message));
        }

        public void NotifyBar(string color, string message)
        {
            QueueAction(() =>
            {
                //Program.Client.Rpc.PrintReq(message);
                Program.Print(Player.LocalPlayer, message, color);
            });
        }

        public bool Confirm(string message)
        {
            return QueueAction<bool>(() => Dialog.Confirm(message));
        }

        public int? AskInteger(string question, int defaultValue)
        {
            return QueueAction<int?>(() =>
            {
                var dlg = new InputDlg("Question", question,
                                       defaultValue.ToString(
                                           CultureInfo.InvariantCulture));
                int result = dlg.GetPositiveInt();
                return dlg.DialogResult.GetValueOrDefault() ? result : (int?)null;
            });
        }

        public int? AskChoice(string question, List<string> choices, List<string> colors, List<string> buttons)
        {
            return QueueAction<int?>(() =>
            {
                var dlg = new ChoiceDlg("Choose One", question, choices, colors, buttons);
                int? result = dlg.GetChoice();
                return dlg.DialogResult.GetValueOrDefault() ? result : 0;
            });
        }

        public Tuple<string, string, int> AskMarker()
        {
            return QueueAction<Tuple<string, string, int>>(() =>
            {
                //fix MAINWINDOW bug
                var dlg = new MarkerDlg { Owner = WindowManager.PlayWindow };
                if (!dlg.ShowDialog().GetValueOrDefault())
                    return null;
                return Tuple.Create(dlg.MarkerModel.Name,
                                    dlg.MarkerModel.Id.ToString(),
                                    dlg.Quantity);
            });
        }

        public string AskString(string question, string defaultValue)
        {
            return QueueAction<string>(() =>
            {
                var dlg = new InputDlg("Question", question, defaultValue);
                var result = dlg.GetString();
                return dlg.DialogResult.GetValueOrDefault() ? result : null;
            });
        }


        public int? SelectCard(List<string> idList, string question, string title)
        {
            return QueueAction<int?>(() =>
            {
                var cardList = idList.Select(x => Card.Find(Guid.Parse(x))).ToList();
                var dlg = new SelectCardsDlg(cardList, question, title) { Owner = WindowManager.PlayWindow };
                if (!dlg.ShowDialog().GetValueOrDefault()) return null;
                return dlg.returnIndex;
            });
        }

        public Tuple<string, int> AskCard(Dictionary<string, List<string>> properties, string op, string title)
        {
            return QueueAction<Tuple<string, int>>(() =>
            {
                //fix MAINWINDOW bug
                var dlg = new CardDlg(properties, op, title) { Owner = WindowManager.PlayWindow };
                if (!dlg.ShowDialog().GetValueOrDefault()) return null;
                return Tuple.Create(dlg.SelectedCard.Id.ToString(),
                                    dlg.Quantity);
            });
        }
        #endregion Messages API


        #region Special APIs

        public string GetGameName()
        {
            return Program.CurrentOnlineGameName;
        }

        public int TurnNumber()
        {
            return (int)Program.GameEngine.TurnNumber;
        }

        public List<string> Create(string modelId, string groupId, int quantity)
        {
            var ret = new List<string>();

            Guid modelGuid;
            if (!Guid.TryParse(modelId, out modelGuid)) return ret;

            var model = Program.GameEngine.Definition.GetCardById(modelGuid);
            if (model == null) return ret;

            var group = Group.Find(Guid.Parse(groupId));
            if (group == null) return ret;

            if (group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't create card in {1} because they don't control it.", Player.LocalPlayer.Name, group.Name));

            QueueAction(
                () =>
                {
                    var gt = new GameEngine.GrpTmp(group, group.Visibility, group.Viewers.ToList());
                    group.SetVisibility(false, false);


                    var ids = new Guid[quantity];
                    var keys = new Guid[quantity];
                    var sizes = new string[quantity];
                    for (int i = 0; i < quantity; ++i)
                    {
                        var card = model.ToPlayCard(Player.LocalPlayer);
                        ids[i] = card.Id;
                        keys[i] = card.Type.Model.Id;
                        sizes[i] = card.Size.Name;
                        ret.Add(card.Id.ToString());
                        group.AddAt(card, group.Count);
                    }

                    string pictureUri = model.GetPicture();
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        new Func<string, BitmapImage>(ImageUtils.CreateFrozenBitmap),
                        DispatcherPriority.Background, pictureUri);

                    Program.Client.Rpc.CreateCard(ids, keys, sizes, group);

                    switch (gt.Visibility)
                    {
                        case DataNew.Entities.GroupVisibility.Everybody:
                            group.SetVisibility(true, false);
                            break;
                        case DataNew.Entities.GroupVisibility.Nobody:
                            group.SetVisibility(false, false);
                            break;
                        default:
                            foreach (Player p in gt.Viewers)
                            {
                                group.AddViewer(p, false);
                            }
                            break;
                    }
                });
            return ret;
        }

        public List<string> CreateOnTable(string modelId, int x, int y, bool persist, int quantity, bool faceDown)
        {
            var result = new List<string>();

            Guid modelGuid;
            if (!Guid.TryParse(modelId, out modelGuid))
                return result; // e.g. modelId may be null if the cloned card is face down.

            QueueAction(() =>
            {
                DataNew.Entities.Card model = Program.GameEngine.Definition.GetCardById(modelGuid);
                if (model == null)
                {
                }
                else
                {
                    var ids = new Guid[quantity];
                    var models = new Guid[quantity];
                    int[] xs = new int[quantity], ys = new int[quantity];

                    for (int i = 0; i < quantity; ++i)
                    {
                        ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | model.Id.Condense();
                        var id = Guid.NewGuid();

                        new CreateCard(Player.LocalPlayer, id, faceDown != true, model, x, y, !persist).Do();

                        ids[i] = id;
                        models[i] = model.Id;
                        xs[i] = x;
                        ys[i] = y;
                        result.Add(id.ToString());

                        //       x += offset;
                        //       y += offset;
                    }
                    string pictureUri = model.GetPicture();
                    Dispatcher.CurrentDispatcher.BeginInvoke(
                        new Func<string, BitmapImage>(ImageUtils.CreateFrozenBitmap),
                        DispatcherPriority.Background, pictureUri);
                    Program.Client.Rpc.CreateCardAt(ids, models, xs, ys, faceDown != true, persist);
                }
            });

            return result;
        }

        public bool IsTwoSided()
        {
            return Program.GameSettings.UseTwoSidedTable;
        }

        //status code initial value set to 0
        //It should be 200 for succes
        //204 for succes but empty response
        //any other return code is an error
        //408 is a timeout error.
        public Tuple<String, int> Web_Read(string url, int timeout)
        {
	        return DoWebRequest(url, timeout);
        }

        //see Web_Read(string url, int timeout)
        public Tuple<String, int> Web_Read(string url)
        {
            return (Web_Read(url, 0));
        }

        public Tuple<string, int> Web_Post(string url, string data, int timeout)
        {
	        return DoWebRequest(url, timeout, data);
        }

	    internal Tuple<string, int> DoWebRequest(string url, int timeout = 0, string data = null)
	    {
            int statusCode = 0;
            string result = "";

            try
            {
				Uri uriResult;
	            if (!Uri.TryCreate(url, UriKind.Absolute, out uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)) {
					return Tuple.Create("URL is in an invalid format", 0);
	            }

				//asking for permission to call the specified url.
				result = "Failed Setting Web Permissions";
                var permission = new WebPermission();
                permission.AddPermission(NetworkAccess.Connect, url);
                permission.Assert();

	            result = "Failed Constructing WebRequest";
				var request = WebRequest.Create(url);
				request.Timeout = (timeout == 0) ? request.Timeout : timeout;
				request.Headers["UserAgent"] = "OCTGN_" + Const.OctgnVersion.ToString() + "/" + Program.GameEngine.Definition.Name + "_" + Program.GameEngine.Definition.Version.ToString();
				request.Method = data == null ? "GET" : "POST";

	            if (data != null) {
					var byteArray = Encoding.UTF8.GetBytes(data);
					request.ContentType = "application/x-www-form-urlencoded";
					request.ContentLength = byteArray.Length;

					using (var webpageStream = request.GetRequestStream()) {
						webpageStream.Write(byteArray, 0, byteArray.Length);
					}
				}

	            result = "Failed Making WebRequest";
	            using (var response = (HttpWebResponse) request.GetResponse())
				using (var grs = response.GetResponseStream()) {
					if (grs == null)
						return Tuple.Create("Null Stream Error", 0);

					using (var reader = new StreamReader(grs)) {
						result = reader.ReadToEnd();

						//if the response is empty it will officially return a 204 status code.
						if(result.Length == 0)
							return Tuple.Create("No Content Error", 204);
						statusCode = 200;
					}
				}
            } catch (WebException ex) {
                var resp = (HttpWebResponse)ex.Response;
	            if (resp == null) {
		            result = "Unknown Error: " + ex.ToString();
		            statusCode = 500;
	            } else {
					result = "Error";
					statusCode = (int)resp.StatusCode;
                }
            } catch (Exception e) {
                Log.Warn("Web_Read", e);
	            result = "Unknown Error: " + result + " " + e.ToString();
				statusCode = 500;
			}

	        return Tuple.Create(result, statusCode);
	    }

        public bool Open_URL(string url)
        {
            if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://"))
            {
                if (QueueAction<bool>(() => Dialog.Confirm("Do you wish to go to the site: " + url + "?")))
                {
                    try
                    {
                        QueueAction(() => Program.LaunchUrl(url));
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
            return Const.OctgnVersion.ToString();
        }

        public string GameDef_Version()
        {
            return Program.GameEngine.Definition.Version.ToString();
        }

        public void SaveSetting<T>(string setName, T val)
        {
            this.QueueAction(() => Prefs.SetGameSetting(Program.GameEngine.Definition, setName, val));
        }

        public T GetSetting<T>(string setName, T def)
        {
            return this.QueueAction<T>(() => Prefs.GetGameSetting(Program.GameEngine.Definition, setName, def));
        }

        public void SetBoardImage(string source)
        {
            if (String.IsNullOrWhiteSpace(source)) return;
            Program.GameEngine.BoardImage = source;
        }

        public void SetBoard(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return;
            if (!GetBoardList().Contains(name)) return;
            QueueAction(() => Program.GameEngine.ChangeGameBoard(name));
            Program.Client.Rpc.SetBoard(name);

        }
        public string GetBoard()
        {
            return Program.GameEngine.GameBoard.Name;
        }
        public string[] GetBoardList()
        {
            return Program.GameEngine.Definition.GameBoards.Keys.ToArray();
        }

        #endregion Special APIs

        #region GlobalVariables

        public void PlayerSetGlobalVariable(string id, string name, object value)
        {
            string val = String.Format("{0}", value);
            Player p = Player.Find(Guid.Parse(id));
            if (p == null || p.Id != Player.LocalPlayer.Id)
                return;
            string oldvalue = null;
            if (Player.LocalPlayer.GlobalVariables.ContainsKey(name))
            {
                oldvalue = Player.LocalPlayer.GlobalVariables[name];
                QueueAction(() => Player.LocalPlayer.GlobalVariables[name] = val);
            }
            else
                QueueAction(() => Player.LocalPlayer.GlobalVariables.Add(name, val));
            Program.Client.Rpc.PlayerSetGlobalVariable(Player.LocalPlayer, name, oldvalue ?? "",val);
        }

        public string PlayerGetGlobalVariable(string id, string name)
        {
            Player p = Player.Find(Guid.Parse(id));
            if (p == null)
                return "";
            if (!p.GlobalVariables.ContainsKey(name))
            {
                Program.GameMess.Warning("Global variable '{0}' isn't defined for player '{1}'", name, p.Name);
                return "";
            }
            return p.GlobalVariables[name];
        }

        public void SetGlobalVariable(string name, object value)
        {
            string val = String.Format("{0}", value);
            string oldvalue = null;
            if (Program.GameEngine.GlobalVariables.ContainsKey(name))
            {
                oldvalue = Program.GameEngine.GlobalVariables[name];
                QueueAction(() => Program.GameEngine.GlobalVariables[name] = val);
            }
            else
                QueueAction(() => Program.GameEngine.GlobalVariables.Add(name, val));
            Program.Client.Rpc.SetGlobalVariable(name, oldvalue ?? "", val);
        }

        public string GetGlobalVariable(string name)
        {
            if (Program.GameEngine.GlobalVariables.ContainsKey(name) == false)
            {
                Program.GameMess.Warning("Global variable '{0}' isn't defined", name);
                return "";
            }
            return Program.GameEngine.GlobalVariables[name];
        }

        #endregion

        public void Update()
        {
            var up = new UpdateAsync(this.ScriptEngine);
            up.Start();
        }

        internal class UpdateAsync
        {
            private readonly Engine engine;

            public UpdateAsync(Engine scriptEngine)
            {
                this.engine = scriptEngine;
            }

            public void Start()
            {
                this.engine.Invoke(Action);
                this.engine.Suspend();
            }

            public void Action()
            {
                //Thread.Sleep(30);
                Program.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(this.Continuation));
            }

            public void Continuation()
            {
                this.engine.Resume();
            }
        }

        public void PlaySound(string name)
        {
            if (Program.GameEngine.Definition.Sounds.ContainsKey(name.ToLowerInvariant()))
            {
                var sound = Program.GameEngine.Definition.Sounds[name.ToLowerInvariant()];
                Program.Client.Rpc.PlaySound(Player.LocalPlayer, sound.Name.ToLowerInvariant());
                Sounds.PlayGameSound(sound);
            }
        }

        public void RemoteCall(string playerid, string func, string args = "")
        {
            var player = Player.Find(Guid.Parse(playerid));
            using (CreateMute())
                Program.Client.Rpc.RemoteCall(player, func, args);
        }

        public void SwitchSides()
        {
            QueueAction(() => Player.LocalPlayer.InvertedTable = Player.LocalPlayer.InvertedTable == false);
        }

        public void ForceDisconnect()
        {
            Program.Client.SeverConnectionAtTheKnee();
        }
        public void ResetGame()
        {
            QueueAction(() => Program.Client.Rpc.ResetReq());
        }

        public void ShowWinForm(Form form)
        {
            QueueAction(() =>
            {
                form.ShowDialog();
                return;
            });
        }
    }
}