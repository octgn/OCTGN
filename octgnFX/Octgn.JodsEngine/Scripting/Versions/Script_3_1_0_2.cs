﻿using System;
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
using Octgn.Library;
using Octgn.Networking;
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

namespace Octgn.Scripting.Versions
{
    [Versioned("3.1.0.2")]
    public class Script_3_1_0_2 : ScriptApi
    {
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
            return Player.AllExceptGlobal.Select(p => (int)p.Id).ToList();
        }

        public string PlayerName(int id)
        {
            return Player.Find((byte)id).Name;
        }

        public string PlayerColor(int id)
        {
            return Player.Find((byte)id).ActualColor.ToString().Remove(1, 2);
        }

        public void NextTurn(bool force)
        {
            if (Program.GameEngine.ActivePlayer == null || Program.GameEngine.ActivePlayer == Player.LocalPlayer)
                Program.Client.Rpc.NextTurn(Player.LocalPlayer, false, force);
        }

        public void NextTurn(int id, bool force)
        {
            var player = Player.Find((byte)id);
            if (Program.GameEngine.ActivePlayer == null || Program.GameEngine.ActivePlayer == Player.LocalPlayer)
                Program.Client.Rpc.NextTurn(player, true, force);
        }

        public int? GetActivePlayer()
        {
            if (Program.GameEngine.ActivePlayer == null) return null;
            return Program.GameEngine.ActivePlayer.Id;
        }

        public void SetActivePlayer()
        {
            if (Program.GameEngine.ActivePlayer == Player.LocalPlayer)
                Program.Client.Rpc.ClearActivePlayer();
        }

        public void SetActivePlayer(int id)
        {
            var player = Player.Find((byte)id);
            if (Program.GameEngine.ActivePlayer == player) return;
            if (Program.GameEngine.ActivePlayer == null || Program.GameEngine.ActivePlayer == Player.LocalPlayer)
                Program.Client.Rpc.SetActivePlayer(player);
        }

        public Tuple<string, int> GetCurrentPhase()
        {
            var phase = Program.GameEngine.CurrentPhase;
            if (phase == null) return new Tuple<string, int>(null, 0);
            return new Tuple<string, int>(phase.Name, phase.Id);
        }

        public void SetPhase(int phase, bool force)
        {
            if (Phase.Find((byte)phase) == null) return;
            if (Program.GameEngine.ActivePlayer == null || Program.GameEngine.ActivePlayer == Player.LocalPlayer)
                Program.Client.Rpc.SetPhaseReq((byte)phase, force);
        }

        public bool GetStop(int id)
        {
            if (id == 0) return Program.GameEngine.StopTurn;
            var phase = Phase.Find((byte)id);
            if (phase == null) return false;
            return phase.Hold;
        }

        public void SetStop(int id, bool stop)
        {
            if (id == 0)
            {
                if (Program.GameEngine.StopTurn == stop) return;
                Program.GameEngine.StopTurn = stop;
                Program.Client.Rpc.StopTurnReq(Program.GameEngine.TurnNumber, stop);
            }
            else
            {
                var phase = Phase.Find((byte)id);
                if (phase == null) return;
                if (phase.Hold == stop) return;
                phase.Hold = stop;
                Program.Client.Rpc.StopPhaseReq(phase.Id, phase.Hold);
            }
        }

        public bool IsSubscriber(int id)
        {
            return Player.Find((byte)id).Subscriber;
        }

        public List<KeyValuePair<int, string>> PlayerCounters(int id)
        {
            return Player.Find((byte)id)
                .Counters
                .Select(c => new KeyValuePair<int, string>(c.Id, c.Name))
                .ToList();
        }
        
        public List<KeyValuePair<int, string>> PlayerPiles(int id)
        {
            return Player.Find((byte)id)
                .Groups.OfType<Pile>()
                .Select(g => new KeyValuePair<int, string>(g.Id, g.Name))
                .ToList();
        }

        public bool PlayerHasInvertedTable(int id)
        {
            return Player.Find((byte)id).InvertedTable;
        }

	    public void SetPlayerColor(int playerId, string colorHex)
	    {
			if (playerId == Player.LocalPlayer.Id)
				Program.Client.Rpc.SetPlayerColor(Player.Find((byte) playerId), colorHex);
			else
			{
				Whisper("You can only change your own color.");
			}
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
            QueueAction(
                () =>
                {
                    //Program.GameEngine.EventProxy.MuteEvents = true;
                    counter.SetValue(value, Player.LocalPlayer, true, true);
                    //counter.Value = value;
                    //Program.GameEngine.EventProxy.MuteEvents = false;
                });
        }

        #endregion Counter API

        #region Group API

        public string GroupCtor(int id)
        {
            return PythonConverter.GroupCtor(Group.Find(id));
        }

        public int GroupCount(int id)
        {
            return Group.Find(id).Count;
        }

        public int GroupCard(int id, int index)
        {
            var c = Group.Find(id)[index];
            if (c == null) return -1;
            return c.Id;
        }

        public int[] GroupCards(int id)
        {
            return Group.Find(id).Select(c => c.Id).ToArray();
        }

        public void GroupShuffle(int id)
        {
            var pile = (Pile)Group.Find(id);
            if (pile.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't shuffle {1} because they don't control it.", Player.LocalPlayer.Name, pile.Name));

            QueueAction(() => pile.Shuffle());

        }

        public string GroupGetVisibility(int id)
        {
            Group g = Group.Find(id);
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
        public void GroupSetVisibility(int id, string v)
        {
            Group group = Group.Find(id);
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
        public bool GroupGetCollapsed(int id)
        {
            var g = Group.Find(id);
            if (!(g is Pile)) return false;
            Pile pile = (Pile)g;
            return pile.ViewState == GroupViewState.Collapsed;
        }
        public void GroupSetCollapsed(int id, bool value)
        {
            var g = Group.Find(id);
            if (!(g is Pile)) return;
            Pile pile = (Pile)g;
            QueueAction(() => pile.ViewState = (value == true) ? GroupViewState.Collapsed : GroupViewState.Pile);
        }

        public string PileGetViewState(int id)
        {
            var group = Group.Find(id);
            if (!(group is Pile)) return null;
            Pile pile = (Pile)group;
            GroupViewState state = pile.ViewState;
            switch (state)
            {
                case GroupViewState.Collapsed:
                    return "collapsed";
                case GroupViewState.Expanded:
                    return "expanded";
                case GroupViewState.Pile:
                    return "pile";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void PileSetViewState(int id, string state)
        {
            var group = Group.Find(id);
            if (!(group is Pile)) return;
            Pile pile = (Pile)group;
            QueueAction(
                () =>
                {
                    switch (state.ToLower())
                    {
                        case "collapsed":
                            pile.ViewState = GroupViewState.Collapsed;
                            break;
                        case "expanded":
                            pile.ViewState = GroupViewState.Expanded;
                            break;
                        case "pile":
                            pile.ViewState = GroupViewState.Pile;
                            break;
                        default:
                            Program.GameMess.Warning("Invalid view state type '{0}'", state);
                            break;
                    }
                });
        }

        public void GroupLookAt(int id, int value, bool isTop)
        {
            var g = (Pile)Group.Find(id);
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
        public int[] GroupViewers(int id)
        {
            return Group.Find(id).Viewers.Select(p => (int)p.Id).ToArray();
        }
        public void GroupAddViewer(int id, int pid)
        {
            Group group = Group.Find(id);
            Player player = Player.Find((byte)pid);
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
        public void GroupRemoveViewer(int id, int pid)
        {
            Group group = Group.Find(id);
            Player player = Player.Find((byte)pid);
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
        public int GroupController(int id)
        {
            return Group.Find(id).Controller.Id;
        }

        public bool IsTableBackgroundFlipped()
        {
            return Program.GameEngine.IsTableBackgroundFlipped;
        }

        public void SetTableBackgroundFlipped(bool isFlipped)
        {
            Program.Client.Rpc.IsTableBackgroundFlipped(isFlipped);
        }

        public void GroupSetController(int id, int player)
        {
            var g = Group.Find(id);
            var p = Player.Find((byte)player);

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
            return Program.GameEngine.Definition.AllProperties().Select(x => x.Name).ToArray();
        }

        public void CardSwitchTo(int id, string alternate)
        {
            var c = Card.Find(id);
            if (c == null) return;
            QueueAction(() => c.SwitchTo(Player.LocalPlayer, alternate, true));

        }

        public CardSize CardSize(int id)
        {
            var c = Card.Find(id);
            if (c == null) return null;
            return c.Size;
        }

        public int? RealHeight(int id)
        {
            var c = Card.Find(id);
            if (c == null) return null;
            return c.RealHeight;
        }

        public int? RealWidth(int id)
        {
            var c = Card.Find(id);
            if (c == null) return null;
            return c.RealWidth;
        }

        public string[] CardAlternates(int id)
        {
            var c = Card.Find(id);
            if (c == null) return new string[0];
            //if ((!c.FaceUp && !c.PeekingPlayers.Contains(Player.LocalPlayer)) || c.Type.Model == null) return new string[0];
            return c.Alternates();
        }

        public string CardAlternate(int id)
        {
            var c = Card.Find(id);
            if (c == null) return "";
            //if ((!c.FaceUp && !c.PeekingPlayers.Contains(Player.LocalPlayer)) || c.Type.Model == null) return "";
            return c.Alternate();
        }

        public string CardName(int id)
        {
            return Card.Find(id).Name;
        }

        public string CardModel(int id)
        //Why is this public? I would expect the model to be private - (V)_V
        // Ur dumb that's why.
        {
            Card c = Card.Find(id);
            if (c.Type.Model == null) return null;
            return c.Type.Model.Id.ToString();
        }

        public string CardSet(int id)
        {
            Card c = Card.Find(id);
            string set = c.Type.Model.GetSet().Name;
            return set;
        }

        public string CardSetId(int id)
        {
            Card c = Card.Find(id);
            string setId = c.Type.Model.SetId.ToString();
            return setId;
        }

        public object CardProperty(int id, string property)
        {
            Card c = Card.Find(id);
            property = property.ToLowerInvariant();
            var value = c.GetProperty(property, "", StringComparison.InvariantCultureIgnoreCase, c.Alternate());
            if (value is RichTextPropertyValue richText) return richText.ToString();
            return value;
        }

        public bool CardHasProperty(int id, string property, string alt)
        {
            Card c = Card.Find(id);
            if (alt == null)
                alt = c.Alternate();
            property = property.ToLowerInvariant();
            var value = c.GetProperty(property, null, StringComparison.InvariantCultureIgnoreCase, alt);
            return value != null;
        }

        public object CardAlternateProperty(int id, string alt, string property)
        {
            Card c = Card.Find(id);
            property = property.ToLowerInvariant();

            var value = c.GetProperty(property, "", StringComparison.InvariantCultureIgnoreCase, alt);
            if (value is RichTextPropertyValue richText) return richText.ToString();
            return value;
        }

        public int CardOwner(int id)
        {
            return Card.Find(id).Owner.Id;
        }

        public int CardController(int id)
        {
            return Card.Find(id).Controller.Id;
        }

        public void SetController(int id, int player)
        {
            Card c = Card.Find(id);
            Player p = Player.Find((byte)player);
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

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't flip up {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));

            QueueAction(() => card.FaceUp = value);
        }

        public int CardGetOrientation(int id)
        {
            return (int)Card.Find(id).Orientation;
        }

        public void CardSetOrientation(int id, int rot)
        {
            if (rot < 0 || rot > 3) throw new IndexOutOfRangeException("orientation must be between 0 and 3");
            Card card = Card.Find(id);

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't rotate {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));

            QueueAction(() => card.Orientation = (CardOrientation)rot);
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
            Color? value = color == null ? null : (Color?)ColorConverter.ConvertFromString(color);

            /*if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't highlight {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));
            */
            // Will add in checks or controls to handle/allow this. - DS
            QueueAction(() => card.HighlightColor = value);
        }

        public string CardGetFilter(int id)
        {
            Color? colorOrNull = Card.Find(id).FilterColor;
            if (colorOrNull == null) return null;
            Color color = colorOrNull.Value;
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public void CardSetFilter(int id, string color)
        {
            Card card = Card.Find(id);
            Color? value = color == null ? null : (Color?)ColorConverter.ConvertFromString(color);

            /*if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't highlight {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));
            */
            // Will add in checks or controls to handle/allow this. - DS
            QueueAction(() => card.FilterColor = value);
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

        public void CardMoveToTable(int cardId, double x, double y, bool forceFaceDown)
        {
            Card card = Card.Find(cardId);

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

        public void CardSelect(int id, bool selected)
        {
            Card c = Card.Find(id);
            // At the moment, only table support multiple selection
            QueueAction(() =>
            {
                if (c.Group is Table)
                {
                    if (selected)
                        Selection.Add(c);
                    else
                        Selection.Remove(c);
                }
                    
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
            if (idx < 0)
            {
                Program.GameMess.Warning("Cannot setIndex({0}), number is less than 0", idx);
                return;
            }
            Card card = Card.Find(CardId);

            if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set index of {1} to Table because they don't control {1}.", Player.LocalPlayer.Name, card.Name));

            if (card.Group != Program.GameEngine.Table && card.Group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set index of {1} in {2} because they don't control it.", Player.LocalPlayer.Name, card, card.Group));

            if (!TableOnly || (TableOnly && card.Group is Table))
            {
                QueueAction(
                    () =>
                    {
                        //Program.GameEngine.EventProxy.MuteEvents = true;
                        card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, idx, true);
                        //Program.GameEngine.EventProxy.MuteEvents = false;
                    });
            }
        }

        public void CardTarget(int id, bool active)
        {
            Card c = Card.Find(id);
            QueueAction(() =>
            {
                //Program.GameEngine.EventProxy.MuteEvents = true;
                if (active) c.Target(true);
                else c.Untarget(true);
                //Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public void CardPeek(int id)
        {
            Card c = Card.Find(id);
            QueueAction(() =>
            {
                c.Peek();
            });
        }

        public List<int> CardPeekers(int id)
        {
            Card c = Card.Find(id);
            return c.PeekingPlayers.Select(x => (int)x.Id).ToList();
        }

        public void CardTargetArrow(int id, int targetId, bool active)
        {
            Card c = Card.Find(id);
            Card target = Card.Find(targetId);
            QueueAction(() =>
            {
                //Program.GameEngine.EventProxy.MuteEvents = true;
                if (active) c.Target(target, true);
                else c.Untarget(true);
                //Program.GameEngine.EventProxy.MuteEvents = false;
            });
        }

        public int CardTargeted(int id)
        {
            Card c = Card.Find(id);
            return c.TargetedBy != null ? c.TargetedBy.Id : -1;
        }

        public Tuple<string, string>[] CardGetMarkers(int id)
        {
            return Card.Find(id).Markers.Select(m => Tuple.Create(m.Model.Name, m.Model.Id.ToString())).ToArray();
        }

        public int MarkerGetCount(int cardId, string markerName, string markerId)
        {
            Card card = Card.Find(cardId);
            Marker marker = card.FindMarker(markerId, markerName);
            return marker == null ? 0 : marker.Count;
        }

        public void MarkerSetCount(int cardId, int count, string markerName, string markerId)
        {
            if (count < 0) count = 0;
            Card card = Card.Find(cardId);
            Marker marker = card.FindMarker(markerId, markerName);
            int origCount = 0;

            /*if (card.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't set markers on {1} because they don't control it.", Player.LocalPlayer.Name, card.Name));
            */
            // Will add in checks or controls to handle/allow this. -- DS
            QueueAction(() =>
            {
                if (marker == null)
                {
                    GameMarker model = Program.GameEngine.GetMarkerModel(markerId);
                    model.Name = markerName;
                    //card.SetMarker(Player.LocalPlayer, guid, markerName, count);
                    Program.Client.Rpc.AddMarkerReq(card, markerId, markerName, (ushort)count, (ushort)origCount, true);
                    card.AddMarker(model, (ushort)count);
                }
                else
                {
                    origCount = marker.Count;
                    if (origCount < count)
                    {
                        Program.Client.Rpc.AddMarkerReq(card, markerId, markerName, (ushort)(count - origCount), (ushort)origCount, true);
                        card.AddMarker(marker.Model, (ushort)(count - origCount));
                    }
                    else if (origCount > count)
                    {
                        Program.Client.Rpc.RemoveMarkerReq(card, markerId, markerName, (ushort)(origCount - count), (ushort)origCount, true);
                        card.RemoveMarker(marker, (ushort)(origCount - count));
                    }
                }
            });
        }

        // TODO: Replace this hack with an actual delete function.
        public void CardDelete(int cardId)
        {
            Card c = Card.Find(cardId);
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

        public bool CardAnchored(int cardId)
        {
            var card = Card.Find(cardId);
            if (card == null)
                return false;

            return card.Anchored;
        }

        public void CardSetAnchored(int cardId, bool anchored)
        {
            var card = Card.Find(cardId);
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

        public void CardSetProperty(int cardId, string name, string val)
        {
            var card = Card.Find(cardId);
            if (card == null)
            {
                Program.GameMess.Warning("Card " + cardId + " doesn't exist.");
                return;
            }
            QueueAction(() =>
            {
                card.SetProperty(name, val);
            });
        }

        public void CardResetProperties(int cardId)
        {
            var card = Card.Find(cardId);
            if (card == null)
            {
                Program.GameMess.Warning("Card " + cardId + " doesn't exist.");
                return;
            }

            QueueAction(() =>
            {
                card.ResetProperties();
            });
        }

        #endregion Cards API

        #region Messages API

        public void Mute(bool muted)
        {
            ScriptJobBase job = ScriptEngine.CurrentJob;
            ScriptEngine.CurrentJob.Muted = muted ? job.id : 0;
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

        public string SaveFileDlg(string title, string defaultValue, string fileFilter)
        {
            return QueueAction<string>(() =>
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    return FileDlg(title, defaultValue, fileFilter, saveFileDialog);
                };
            });
        }

        public string OpenFileDlg(string title, string defaultValue, string fileFilter)
        {
            return QueueAction<string>(() =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    return FileDlg(title, defaultValue, fileFilter, openFileDialog);
                }
            });
        }

        private string FileDlg(string title, string defaultValue, string fileFilter, FileDialog fileDialog)
        {
            fileDialog.InitialDirectory = Config.Instance.DataDirectory;
            fileDialog.Filter = fileFilter;
            fileDialog.Title = title;
            fileDialog.RestoreDirectory = true;

            var filePath = string.Empty;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }
            return filePath;
        }

        //public Tuple<string, int> AskCard(string restriction)
        //{
        //    return QueueAction<Tuple<string, int>>(() =>
        //                                                  {
        //                                                      //fix MAINWINDOW bug
        //                                                      var dlg = new CardDlg(restriction) { Owner = WindowManager.PlayWindow };
        //                                                      if (!dlg.ShowDialog().GetValueOrDefault()) return null;
        //                                                      return Tuple.Create(dlg.SelectedCard.Id.ToString(),
        //                                                                          dlg.Quantity);
        //                                                  });
        //}

        public SelectMultiCardsDlg SelectMultiCard(List<int> idList, List<int> idList2, int? minValue, int? maxValue, string question, string title, string boxLabel, string boxLabel2)
        {
            return QueueAction<SelectMultiCardsDlg>(() =>
            {
                var dlg = new SelectMultiCardsDlg(idList, idList2, question, title, minValue, maxValue, boxLabel, boxLabel2) { Owner = WindowManager.PlayWindow };
                if (!dlg.ShowDialog().GetValueOrDefault()) return null;
                return dlg;
            });
        }

        public int? SelectCard(List<int> idList, string question, string title)
        {
            return QueueAction<int?>(() =>
            {
                var cardList = idList.Select(x => Card.Find(x)).ToList();
                var dlg = new SelectCardsDlg(cardList, question, title) { Owner = WindowManager.PlayWindow };
                if (!dlg.ShowDialog().GetValueOrDefault()) return null;
                return dlg.returnIndex;
            });
        }

        public Tuple<string, int> AskCard(Dictionary<string, List<string>> properties, string op, string title)
        {
            //this.AskCard(x => x.Where(y => y.Name = "a"));
            //default(DataNew.Entities.ICard).Properties.Where(x => x.Key.Name == "Rarity" && x.Value == "Token");
            return QueueAction<Tuple<string, int>>(() =>
            {
                //fix MAINWINDOW bug
                var dlg = new CardDlg(properties, op, title) { Owner = WindowManager.PlayWindow };
                if (!dlg.ShowDialog().GetValueOrDefault()) return null;
                return Tuple.Create(dlg.SelectedCard.Id.ToString(),
                                    dlg.Quantity);
            });
        }

        public List<string> QueryCard(Dictionary<string, List<string>> properties, bool match)
        {
            return QueueAction(() =>
            {
                var Cards = new List<string>();

                var query = Program.GameEngine.Definition.AllCards();
                foreach (var property in properties)
                {
                    var tempCardList = new List<DataNew.Entities.Card>();
                    foreach (var propertyValue in property.Value)
                    {
                        if (match)
                            tempCardList.AddRange(query
                                .Where(x => x.GetFullCardProperties()
                                .Any(y => y.Key.Name.ToLower() == property.Key.ToLower() && y.Value.ToString().ToLower() == propertyValue.ToLower())).ToList());
                        else
                            tempCardList.AddRange(query
                                .Where(x => x.GetFullCardProperties()
                                .Any(y => y.Key.Name.ToLower() == property.Key.ToLower() && y.Value.ToString().ToLower().Contains(propertyValue.ToLower()))).ToList());

                    }
                    query = tempCardList;
                }
                Cards = query.Select(x => x.Id.ToString()).ToList();

                return Cards.Distinct().ToList();
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

        public List<int> Create(string modelId, int groupId, int quantity)
        {
            var ret = new List<int>();

            Guid modelGuid;
            if (!Guid.TryParse(modelId, out modelGuid)) return ret;

            var model = Program.GameEngine.Definition.GetCardById(modelGuid);
            if (model == null) return ret;

            var group = Group.Find(groupId);
            if (group == null) return ret;

            if (group.Controller != Player.LocalPlayer)
                Program.GameMess.Warning(String.Format("{0} Can't create card in {1} because they don't control it.", Player.LocalPlayer.Name, group.Name));

            QueueAction(
                () =>
                {
                    var gt = new GameEngine.GrpTmp(group, group.Visibility, group.Viewers.ToList());
                    group.SetVisibility(false, false);


                    var ids = new int[quantity];
                    var keys = new Guid[quantity];
                    var sizes = new string[quantity];
                    for (int i = 0; i < quantity; ++i)
                    {
                        var card = model.ToPlayCard(Player.LocalPlayer);
                        //ulong key = (ulong)Crypto.PositiveRandom() << 32 | model.Id.Condense();
                        //int id = Program.GameEngine.GenerateCardId();
                        ids[i] = card.Id;
                        keys[i] = card.Type.Model.Id;
                        sizes[i] = card.Size.Name;
                        ret.Add(card.Id);
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
            // Comment for a test.
        }

        public List<int> CreateOnTable(string modelId, int x, int y, bool persist, int quantity, bool faceDown)
        {
            var result = new List<int>();

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
                    var ids = new int[quantity];
                    var models = new Guid[quantity];
                    int[] xs = new int[quantity], ys = new int[quantity];


                    //   if (Player.LocalPlayer.InvertedTable)
                    //   {
                    //       x -= Program.GameEngine.Definition.CardWidth;
                    //       y -= Program.GameEngine.Definition.CardHeight;
                    //   }
                    //   var offset = (int)(Math.Min(Program.GameEngine.Definition.CardWidth, Program.GameEngine.Definition.CardHeight) * 0.2);
                    //   if (Program.GameSettings.UseTwoSidedTable && TableControl.IsInInvertedZone(y))
                    //       offset = -offset;

                    for (int i = 0; i < quantity; ++i)
                    {
                        ulong key = ((ulong)Crypto.PositiveRandom()) << 32 | model.Id.Condense();
                        int id = model.GenerateCardId();

                        new CreateCard(Player.LocalPlayer, id, faceDown != true, model, x, y, !persist).Do();

                        ids[i] = id;
                        models[i] = model.Id;
                        xs[i] = x;
                        ys[i] = y;
                        result.Add(id);

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

        public void SetBoard(string name)
        {
            if (name == null)
            {
                QueueAction(() => Program.Client.Rpc.RemoveBoard(Player.LocalPlayer));
            }
            else if (GetBoardList().Contains(name))
            {
                QueueAction(() => Program.Client.Rpc.SetBoard(Player.LocalPlayer, name));
            }
            else
            {
                Program.GameMess.Warning("Cannot find game board with name `{0}`.", name);
            }
        }
        public string GetBoard()
        {
            return Program.GameEngine.GameBoard?.Name;
        }
        public string[] GetBoardList()
        {
            return Program.GameEngine.Definition.GameBoards.Keys.ToArray();
        }

        #endregion Special APIs

        #region GlobalVariables

        public void PlayerSetGlobalVariable(int id, string name, object value)
        {
            string val = String.Format("{0}", value);
            Player p = Player.Find((byte)id);
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

        public string PlayerGetGlobalVariable(int id, string name)
        {
            Player p = Player.Find((byte)id);
            if (p == null)
                return "";
            if (p.GlobalVariables.ContainsKey(name) == false)
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

        public void RemoteCall(int playerid, string func, string args = "")
        {
            //if (args == null) args = new object[0];

            //var argString = Program.GameEngine.ScriptEngine.FormatObject(args);

            //var rargs = args.ToArray();
            //var sb = new StringBuilder();
            //for (var i = 0; i < rargs.Length; i++)
            //{
            //    var isLast = i == rargs.Length - 1;
            //    var a = rargs[i];
            //    if (a is Array)
            //    {
            //        var arr = a as Array;
            //        sb.Append("[");
            //        var argStrings = new List<string>();
            //        foreach (var o in arr)
            //        {
            //            argStrings.Add(Program.GameEngine.ScriptEngine.FormatObject(o));
            //        }
            //        sb.Append(string.Join(",", argStrings));
            //        sb.Append("]");
            //    }
            //    else
            //        sb.Append(Program.GameEngine.ScriptEngine.FormatObject(a));

            //    if (!isLast) sb.Append(", ");
            //}

            var player = Player.Find((byte)playerid);
            using (CreateMute())
                Program.Client.Rpc.RemoteCall(player, func, args);
        }

        public Tuple<string, string, string> ChooseCardPackage()
        {
            return QueueAction(() =>
            {
                var dlg = new PackDlg();
                var result = dlg.GetPack();
                return dlg.DialogResult.GetValueOrDefault() ? new Tuple<string, string, string>(result.Set.Name, result.Name, result.Id.ToString()) : null;
            });
        }

        public List<string> GenerateCardsFromPackage(string packId)
        {
            Guid guid = Guid.Parse(packId);
            var pack = Program.GameEngine.Definition.GetPackById(guid);
            if (pack == null)
            {
                Program.GameMess.Warning("Pack is missing from the database. Pack is ignored.");
                return new List<string>();
            }
            var packContents = pack.GenerateContent().LimitedCards.Select(x => x.Id.ToString()).ToList();
            return packContents;
        }

        public void SwitchSides()
        {
            QueueAction(() => Player.LocalPlayer.InvertedTable = Player.LocalPlayer.InvertedTable == false);
        }

        public void ForceDisconnect()
        {
            if (Program.Client is ClientSocket clientSocket) {
                clientSocket.SeverConnectionAtTheKnee();
            }
        }
        public void ResetGame()
        {
            QueueAction(() => Program.Client.Rpc.ResetReq(false));
        }
        
        public void SoftResetGame()
        {
            QueueAction(() => Program.Client.Rpc.ResetReq(true));
        }

        public void ShowWinForm(System.Windows.Forms.Form form)
        {
            QueueAction(() =>
            {
                form.ShowDialog();
                return;
            });
        }

        public int[] RandomArray(int min, int max, int count) {
            return GetRandoms(min, max).Take(count).ToArray();
        }

        private static readonly Random _rnd = new Random();

        private IEnumerable<int> GetRandoms(int min, int max) {
            while (true) {
                yield return _rnd.Next(min, max + 1);
            }
        }

        public void ClearSelection()
        {
            QueueAction(() => Selection.Clear());
        }
    }
}