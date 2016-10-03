/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Utils;

namespace Octgn.Networking
{
    using System.IO;
    using System.Reflection;
    using System.Text;

    using log4net;

    using Newtonsoft.Json;

    using Octgn.Core.DataExtensionMethods;
    using System.Windows.Media;

    using Octgn.Core.Play;
    using Octgn.Core.Util;
    using Octgn.Play.State;

    using Card = Octgn.Play.Card;
    using Counter = Octgn.Play.Counter;
    using Group = Octgn.Play.Group;
    using Marker = Octgn.Play.Marker;
    using Player = Octgn.Play.Player;
    using Phase = Octgn.Play.Phase;
    using Microsoft.AspNet.SignalR.Client;

    internal sealed class Handler : HandlerBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Handler()
        {
        }
        protected override void Error(Player sender, string msg)
        {
            //Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "The server has returned an error: {0}", msg);
            Program.GameMess.Warning("The server has returned an error: {0}", msg);
        }

        protected override void Kick(Player sender, string reason)
        {
            //Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "You have been kicked: {0}", reason);
            Program.GameMess.Warning("You have been kicked: {0}", reason);
            Program.InPreGame = false;
            Program.Client.ForceDisconnect();
        }

        protected override void Start(Player sender)
        {
            Log.Debug("Start");
            Program.InPreGame = false;
            //Program.StartGame();
            if (WindowManager.PlayWindow != null)
            {
                WindowManager.PlayWindow.PreGameLobby.Start(false);
            }
            if (Program.GameEngine.WaitForGameState)
            {
                Log.Debug("Start WaitForGameState");
                foreach (var p in Player.AllExceptGlobal)
                {
                    if (p == Player.LocalPlayer)
                    {
                        Log.DebugFormat("Start Skipping {0}", p.Name);
                        continue;
                    }
                    Log.DebugFormat("Start Sending Request to {0}", p.Name);
                    Program.Client.Rpc.GameStateReq(p);
                }
            }
        }

        protected override void Settings(Player sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            // The host is the driver for this flag and should ignore notifications,
            // otherwise there might be a loop if the server takes more time to dispatch this message
            // than the user to click again on the checkbox.
            if (!Program.IsHost)
            {
                Program.GameSettings.UseTwoSidedTable = twoSidedTable;
                Program.GameSettings.AllowSpectators = allowSpectators;
                Program.GameSettings.MuteSpectators = muteSpectators;
            }
        }

        protected override void PlayerSettings(Player sender, Player player, bool invertedTable, bool spectator)
        {
            if (sender == Player.LocalPlayer) return;
            player.UpdateSettings(invertedTable, spectator);
            Player.RefreshSpectators();
        }

        protected override void Reset(Player sender, Player player)
        {
            Program.GameEngine.Reset();
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} resets the game.", player);
            Program.GameMess.System("{0} reset the game", player);
        }

        protected override void SetPhase(Player sender, byte phase, byte nextPhase)
        {
            var lastPhase = Program.GameEngine.CurrentPhase;
            var newPhase = Phase.Find(nextPhase);
            Program.GameEngine.CurrentPhase = newPhase;
            Program.GameMess.Phase(Program.GameEngine.TurnPlayer, newPhase.Name);
            if (lastPhase == null)
                Program.GameEngine.EventProxy.OnPhasePassed_3_1_0_2(null, 0);
            else
                Program.GameEngine.EventProxy.OnPhasePassed_3_1_0_2(lastPhase.Name, lastPhase.Id);
        }

        protected override void StopPhase(Player sender, Player player, byte phase)
        {
            Phase thisPhase = Phase.Find(phase);
            if (player == Player.LocalPlayer)
                thisPhase.Hold = false;
            Program.GameMess.System("{0} wants to play before end of {1}", player, thisPhase.Name);
            Program.GameEngine.EventProxy.OnPhasePaused_3_1_0_2(player);
        }

        protected override void NextTurn(Player sender, Player player)
        {
            Program.GameEngine.TurnNumber++;
            var lastPlayer = Program.GameEngine.TurnPlayer;
            Program.GameEngine.TurnPlayer = player;
            Program.GameEngine.StopTurn = false;
            Program.GameEngine.AllPhases.Select(x => x.Hold = false).ToList();
            Program.GameEngine.CurrentPhase = null;
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Turn, "Turn {0}: {1}", Program.GameEngine.TurnNumber, player);
            //Program.GameMess.System("Turn {0}: {1}", Program.GameEngine.TurnNumber, player);
            Program.GameMess.Turn(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurn_3_1_0_0(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurn_3_1_0_1(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurnPassed_3_1_0_2(lastPlayer);
        }

        protected override void StopTurn(Player sender, Player player)
        {
            if (player == Player.LocalPlayer)
                Program.GameEngine.StopTurn = false;
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} wants to play before end of turn.", player);
            Program.GameMess.System("{0} wants to play before end of turn", player);
            Program.GameEngine.EventProxy.OnEndTurn_3_1_0_0(player);
            Program.GameEngine.EventProxy.OnEndTurn_3_1_0_1(player);
            Program.GameEngine.EventProxy.OnTurnPaused_3_1_0_2(player);
        }

        protected override void SetBoard(Player sender, string name)
        {
            Program.GameEngine.ChangeGameBoard(name);
        }

        protected override void Chat(Player sender, Player player, string text)
        {
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Chat | EventIds.PlayerFlag(player), "<{0}> {1}", player, text);
            Program.GameMess.Chat(player, text);
        }

        protected override void Print(Player sender, Player player, string text)
        {
            // skip for local player, handled when called for consistency
            if (player == Player.LocalPlayer) return;
            Program.Print(player, text);
        }

        protected override void Random(Player sender, int result)
        {
            Program.GameEngine.ScriptApi.RandomResult(result);

        }

        protected override void Counter(Player sender, Player player, Counter counter, int value, bool isScriptChange)
        {
            counter.SetValue(value, player, false, isScriptChange);
        }

        protected override void Welcome(Player sender, Guid id, Guid gameId, bool waitForGameState)
        {
            Program.InPreGame = true;
            Player.LocalPlayer.Id = id;
            Player.FireLocalPlayerWelcomed();
            Program.GameEngine.Id = gameId;
            Program.GameEngine.WaitForGameState = waitForGameState;
        }

        protected override void NewPlayer(Player sender, Guid id, string nick, long pkey, bool invertedTable, bool spectator)
        {
            var p = Player.FindIncludingSpectators(id);
            if (p == null)
            {
                var player = new Player(Program.GameEngine.Definition, nick, id, (ulong)pkey, spectator, false);
                Program.GameMess.System("{0} has joined the game", player);
                player.UpdateSettings(invertedTable, spectator);
                if (Program.InPreGame == false)
                {
                    GameStateReq(sender, player);
                    if (player.Spectator == false)
                    {
                        Program.GameEngine.EventProxy.OnPlayerConnect_3_1_0_1(player);
                        Program.GameEngine.EventProxy.OnPlayerConnected_3_1_0_2(player);
                    }
                }
                else
                {
                    if (Octgn.Core.Prefs.SoundOption == Core.Prefs.SoundType.DingDong)
                        Sounds.PlaySound(Properties.Resources.userjoinsroom, false);
                    else if (Octgn.Core.Prefs.SoundOption == Core.Prefs.SoundType.KnockKnock)
                        Sounds.PlaySound(Properties.Resources.knockknock, false);
                }
            }
            else
            {
                if (p.Spectator == false && Program.InPreGame == false)
                {
                    Program.GameEngine.EventProxy.OnPlayerConnect_3_1_0_1(p);
                    Program.GameEngine.EventProxy.OnPlayerConnected_3_1_0_2(p);
                }
            }
        }

        /// <summary>Loads a player deck.</summary>
        /// <param name="id">An array containing the loaded CardIdentity ids.</param>
        /// <param name="type">An array containing the corresponding CardModel guids (encrypted).</param>
        /// <param name="group">An array indicating the group the cards must be loaded into.</param>
        protected override void LoadDeck(Player sender, Player who, Guid[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited)
        {
            if (id.Length != type.Length || id.Length != group.Length)
            {
                //Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                Program.GameMess.Warning("[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                return;
            }

            if (id.Length == 0) return;   // Loading an empty deck --> do nothing

            if (who == null)
            {
                //Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Player not found.");
                Program.GameMess.Warning("[LoadDeck] Player not found.");
                return;
            }
            if (limited) Program.GameMess.System("{0} loads a limited deck.", who);
            else Program.GameMess.System("{0} loads a deck.", who);
            CreateCard(who, id, type, group, size, sleeve);
            Log.Info("LoadDeck Starting Task to Fire Event");
            Program.GameEngine.EventProxy.OnLoadDeck_3_1_0_0(who, @group.Distinct().ToArray());
            Program.GameEngine.EventProxy.OnLoadDeck_3_1_0_1(who, @group.Distinct().ToArray());
            Program.GameEngine.EventProxy.OnDeckLoaded_3_1_0_2(who, @group.Distinct().ToArray());
        }

        /// <summary>Creates new Cards as well as the corresponding CardIdentities. The cards may be in different groups.</summary>
        /// <param name="id">An array with the new CardIdentity ids.</param>
        /// <param name="type">An array containing the corresponding CardModel guids (encrypted)</param>
        /// <param name="groups">An array indicating the group the cards must be loaded into.</param>
        /// <seealso cref="CreateCard(int[], ulong[], Group)"> for a more efficient way to insert cards inside one group.</seealso>
        private static void CreateCard(Player who, IList<Guid> id, IList<Guid> type, IList<Group> groups, IList<string> sizes, string sleeveUrl = "")
        {
            // Ignore cards created by oneself
            if (who == Player.LocalPlayer) return;
            for (int i = 0; i < id.Count; i++)
            {
                Group group = groups[i];
                Player owner = group.Owner;
                if (owner == null)
                {
                    //Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCard] Player not found.");
                    Program.GameMess.Warning("[CreateCard] Player not found.");
                    continue;
                }

                Card c = new Card(owner, id[i], Program.GameEngine.Definition.GetCardById(type[i]), false, sizes[i]); c.SetSleeve(sleeveUrl); group.AddAt(c, group.Count);
            }
        }

        /// <summary>Creates new Cards as well as the corresponding CardIdentities. All cards are created in the same group.</summary>
        /// <param name="id">An array with the new CardIdentity ids.</param>
        /// <param name="type">An array containing the corresponding CardModel guids (encrypted)</param>
        /// <param name="group">The group, in which the cards are added.</param>
        /// <seealso cref="CreateCard(int[], ulong[], Group[])"> to add cards to several groups</seealso>
        protected override void CreateCard(Player sender, Player who, Guid[] id, Guid[] type, string[] size, Group group)
        {
            if (who == Player.LocalPlayer) return;
            for (int i = 0; i < id.Length; i++)
            {
                Player owner = group.Owner;
                if (owner == null)
                {
                    Program.GameMess.Warning("[CreateCard] Player not found.");
                    return;
                }
                //var c = new Card(owner,id[0], type[0], Program.Game.Definition.CardDefinition, null, false);
                var c = Card.Find(id[0]);

                Program.GameMess.PlayerEvent(owner, "{0} creates {1} {2} in {3}'s {4}", owner.Name, id.Length, c == null ? "card" : (object)c, group.Owner.Name, group.Name);
                // Ignore cards created by oneself

                //Card c = new Card(owner, id[i], type[i], Program.Game.Definition.CardDefinition, null, false);
                //group.AddAt(c, group.Count);
                var card = new Card(owner, id[i], Program.GameEngine.Definition.GetCardById(type[i]), false, size[i]); group.AddAt(card, group.Count);
            }
        }

        /// <summary>Creates new cards on the table, as well as the corresponding CardIdentities.</summary>
        /// <param name="id">An array with the new CardIdentity ids</param>
        /// <param name="modelId"> </param>
        /// <param name="x">The x position of the cards on the table.</param>
        /// <param name="y">The y position of the cards on the table.</param>
        /// <param name="faceUp">Whether the cards are face up or not.</param>
        /// <param name="key"> </param>
        /// <param name="persist"> </param>
        protected override void CreateCardAt(Player sender, Player player, Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            if (id.Length == 0)
            {
                Program.GameMess.Warning("[CreateCardAt] Empty id parameter.");
                return;
            }
            if (id.Length != x.Length || id.Length != y.Length || id.Length != modelId.Length)
            {
                Program.GameMess.Warning("[CreateCardAt] Inconsistent parameters length.");
                return;
            }
            if (player == null)
            {
                Program.GameMess.Warning("[CreateCardAt] Player not found.");
                return;
            }
            var table = Program.GameEngine.Table;
            // Bring cards created by oneself to top, for z-order consistency
            if (player == Player.LocalPlayer)
            {
                for (int i = id.Length - 1; i >= 0; --i)
                {
                    var card = Card.Find(id[i]);
                    if (card == null)
                    {
                        Program.GameMess.Warning("[CreateCardAt] Card not found.");
                        return;
                    }
                    table.SetCardIndex(card, table.Count + i - id.Length);
                }
            }
            else
            {
                for (int i = 0; i < id.Length; i++)
                    new CreateCard(player, id[i], faceUp, Program.GameEngine.Definition.GetCardById(modelId[i]), x[i], y[i], !persist).Do();
            }

            // Display log messages
            try
            {
                if (modelId.All(m => m == modelId[0]))
                    Program.GameMess.PlayerEvent(player, "creates {1} '{2}'", player, modelId.Length, player == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(modelId[0]).Name : "card");
                else
                    foreach (Guid m in modelId)
                        Program.GameMess.PlayerEvent(player, "{0} creates a '{1}'", player, player == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(m).Name : "card");

            }
            catch (Exception)
            {
                // TODO - [FIX THIS SHIT] - A null reference exception happens on the first trace event. - Kelly Elton - 3/24/2013
                // This should be cleaered up, this is only a temp fix. - Kelly Elton - 3/24/2013
            }
        }

        protected override void Leave(Player sender, Player player)
        {
            Program.GameMess.System("{0} has closed their game window left the game. They did not crash or lose connection, they left on purpose.", player);
            Program.GameEngine.EventProxy.OnPlayerLeaveGame_3_1_0_1(player);
            Program.GameEngine.EventProxy.OnPlayerQuit_3_1_0_2(player);
            player.Delete();
            if (Program.IsHost && Program.InPreGame)
            {
                Sounds.PlaySound(Properties.Resources.doorclose);
            }
        }

        protected override void MoveCard(Player sender, Player player, Guid[] card, Group to, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            // Ignore cards moved by the local player (already done, for responsiveness)
            var cards = card.Select(Card.Find).Where(x=>x != null).ToArray();
            if (player != Player.LocalPlayer)
                new MoveCards(player, cards, to, idx, faceUp, isScriptMove).Do();
        }

        protected override void MoveCardAt(Player sender, Player player, Guid[] card, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            // Get the table control
            Table table = Program.GameEngine.Table;
            var cards = card.Select(Card.Find).Where(x1=>x1 != null).ToArray();
            // Because every player may manipulate the table at the same time, the index may be out of bound
            if (cards[0].Group == table)
            {
                for (int index = 0; index < idx.Length; index++)
                {
                    if (idx[index] >= table.Count) idx[index] = table.Count - 1;
                }
            }
            else
            {
                for (int index = 0; index < idx.Length; index++)
                {
                    if (idx[index] > table.Count) idx[index] = table.Count;
                }
            }

            // Ignore cards moved by the local player (already done, for responsiveness)
            if (player == Player.LocalPlayer) return;
            // Find the old position on the table, if any
            //bool onTable = card.Group == table;
            //double oldX = card.X, oldY = card.Y;
            // Do the move
            new MoveCards(player, cards, x, y, idx, faceUp, isScriptMove).Do();
        }

        protected override void AddMarker(Player sender, Player player, Card card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            DataNew.Entities.Marker model = Program.GameEngine.GetMarkerModel(id);
            model.Name = name;
            Marker marker = card.FindMarker(id, name);
            if (player != Player.LocalPlayer)
            {
                if (marker == null && oldCount != 0)
                {
                    Program.GameMess.Warning("Inconsistent state. Cannot create a marker when that marker already exists.");
                    return;
                }
                if (marker != null && oldCount != marker.Count)
                {
                    Program.GameMess.Warning("Inconsistent state.  Marker count invalid.");
                    return;
                }
                card.AddMarker(model, count);

            }
            if (count != 0)
            {
                int newCount = oldCount + count;
                Program.GameMess.PlayerEvent(player, "adds {0} {1} marker(s) on {2}", count, model.Name, card);
                if (isScriptChange == false)
                {
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(card, model.ModelString(), oldCount, newCount, isScriptChange);
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(card, model.ModelString(), oldCount, newCount, isScriptChange);
                }
                Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, model.Name, model.Id.ToString(), oldCount, isScriptChange);
            }
        }

        protected override void RemoveMarker(Player sender, Player player, Card card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            Marker marker = card.FindMarker(id, name);
            if (player != Player.LocalPlayer)
            {
                if (marker == null)
                {
                    Program.GameMess.Warning("Inconsistent state. Marker not found on card.");
                    return;
                }
                if (marker.Count != oldCount)
                    Program.GameMess.Warning("Inconsistent state. Missing markers to remove");
            }
            if (count != 0)
            {
                int newCount = oldCount - count;
                if (player != Player.LocalPlayer)
                {
                    card.RemoveMarker(marker, count);
                }
                Program.GameMess.PlayerEvent(player, "removes {0} {1} marker(s) from {2}", count, name, card);
                if (player == Player.LocalPlayer && marker == null)
                {
                    StringBuilder markerString = new StringBuilder();
                    markerString.AppendFormat("('{0}','{1}')", name, id);
                    if (isScriptChange == false)
                    {
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(card, markerString.ToString(), oldCount, newCount, isScriptChange);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(card, markerString.ToString(), oldCount, newCount, isScriptChange);
                    }
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, name, id.ToString(), oldCount, isScriptChange);
                }
                else
                {
                    if (isScriptChange == false)
                    {
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(card, marker.Model.ModelString(), oldCount, newCount, isScriptChange);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(card, marker.Model.ModelString(), oldCount, newCount, isScriptChange);
                    }
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, name, id.ToString(), oldCount, isScriptChange);
                }

            }

        }

        protected override void TransferMarker(Player sender, Player player, Card from, Card to, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            Marker marker = from.FindMarker(id, name);
            if (player == null)
            {
                Program.GameMess.Warning("Inconsistent state. Cannot transfer marker to unknown player.");
                return;
            }
            if (player != Player.LocalPlayer)
            {
                if (marker == null)
                {
                    Program.GameMess.Warning("Inconsistent state. Marker not found on card.");
                    return;
                }
                if (marker.Count != oldCount)
                    Program.GameMess.Warning("Inconsistent state. Missing markers to remove");
            }
            Marker newMarker = to.FindMarker(id, name);
            int toOldCount = 0;
            if (newMarker != null)
                toOldCount = newMarker.Count - 1;
            int fromNewCount = oldCount - count;
            int toNewCount = toOldCount + count;
            if (player != Player.LocalPlayer)
            {
                from.RemoveMarker(marker, count);
                to.AddMarker(marker.Model, count);
            }
            Program.GameMess.PlayerEvent(player, "moves {0} {1} marker(s) from {2} to {3}", count, name, from, to);
            if (marker == null)
            {
                marker = from.FindRemovedMarker(id, name);
            }
            if (marker != null)
            {
                if (isScriptChange == false)
                {
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(
                        from,
                        marker.Model.ModelString(),
                        oldCount,
                        fromNewCount,
                        isScriptChange);
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(
                        to,
                        marker.Model.ModelString(),
                        toOldCount,
                        toNewCount,
                        isScriptChange);
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(
                        from,
                        marker.Model.ModelString(),
                        oldCount,
                        fromNewCount,
                        isScriptChange);
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(
                        to,
                        marker.Model.ModelString(),
                        toOldCount,
                        toNewCount,
                        isScriptChange);
                }
                Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(
                    from,
                    marker.Model.Name,
                    marker.Model.Id.ToString(),
                    oldCount,
                    isScriptChange);
                Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(
                    to,
                    marker.Model.Name,
                    marker.Model.Id.ToString(),
                    toOldCount,
                    isScriptChange);
            }
        }

        protected override void Nick(Player sender, Player player, string nick)
        {
            Program.GameMess.System("{0} is now known as {1}.", player, nick);
            player.Name = nick;
        }

        protected override void Peek(Player sender, Player player, Card card)
        {
            if (!card.PeekingPlayers.Contains(player))
                card.PeekingPlayers.Add(player);
            if (player != Player.LocalPlayer)
            {
                Program.GameMess.PlayerEvent(player, "peeks at a card ({0}).", card.Group is Table ? "on table" : "in " + card.Group.FullName);
            }
        }

        protected override void Untarget(Player sender, Player player, Card card, bool isScriptChange)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, null, false, isScriptChange).Do();
        }

        protected override void Target(Player sender, Player player, Card card, bool isScriptChange)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, null, true, isScriptChange).Do();
        }

        protected override void TargetArrow(Player sender, Player player, Card card, Card otherCard, bool isScriptChange)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, otherCard, true, isScriptChange).Do();
        }

        protected override void Highlight(Player sender, Card card, Color? color)
        { card.SetHighlight(color); }

        protected override void Filter(Player sender, Card card, Color? color)
        { card.SetFilter(color); }

        protected override void Turn(Player sender, Player player, Card card, bool up)
        {
            // Ignore the card we turned ourselves
            if (player == Player.LocalPlayer)
            {
                card.MayBeConsideredFaceUp = false;     // see comment on mayBeConsideredFaceUp
                return;
            }
            new Turn(player, card, up).Do();
        }

        protected override void Rotate(Player sender, Player player, Card card, CardOrientation rot)
        {
            // Ignore the moves we made ourselves
            if (player == Player.LocalPlayer)
                return;
            new Rotate(player, card, rot).Do();
        }

        protected override void Shuffled(Player sender, Player player, Group group, Guid[] card, short[] pos)
        {
            if (player == Player.LocalPlayer) return;
            ((Pile)group).DoShuffle(card, pos);
        }

        protected override void PassTo(Player sender, Player who, ControllableObject obj, Player player, bool requested)
        {
            // Ignore message that we sent in the first place
            if (who != Player.LocalPlayer)
                obj.PassControlTo(player, who, false, requested);
            if (obj is Card)
               Program.GameEngine.EventProxy.OnCardControllerChanged_3_1_0_2((Card)obj, who, player);
        }

        protected override void TakeFrom(Player sender, ControllableObject obj, Player to)
        { obj.TakingControl(to); }

        protected override void DontTake(Player sender, ControllableObject obj)
        { obj.DontTakeError(); }

        protected override void FreezeCardsVisibility(Player sender, Group group)
        {
            foreach (Card c in group.Cards) c.SetOverrideGroupVisibility(true);
        }

        protected override void GroupVis(Player sender, Player player, Group group, bool defined, bool visible)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.SetVisibility(defined ? (bool?)visible : null, false);
            if (defined)
                Program.GameMess.PlayerEvent(player, visible ? "shows {0} to everybody." : "shows {0} to nobody.", group);
        }

        protected override void GroupVisAdd(Player sender, Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.AddViewer(whom, false);
            Program.GameMess.PlayerEvent(player, "shows {0} to {1}.", group, whom);
        }

        protected override void GroupVisRemove(Player sender, Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.RemoveViewer(whom, false);
            Program.GameMess.PlayerEvent(player, "hides {0} from {1}.", group, whom);
        }

        protected override void LookAt(Player sender, Player player, Guid uniqueid, Group group, bool look)
        {
            if (look)
            {
                if (group.Visibility != DataNew.Entities.GroupVisibility.Everybody)
                    foreach (Card c in group)
                    {
                        c.PlayersLooking.Add(player);
                    }
                group.LookedAt.Add(uniqueid, group.ToList());
                Program.GameMess.PlayerEvent(player, "looks at {0}.", group);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uniqueid))
                { Program.GameMess.Warning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                if (group.Visibility != DataNew.Entities.GroupVisibility.Everybody)
                {
                    foreach (Card c in group.LookedAt[uniqueid])
                        c.PlayersLooking.Remove(player);
                }
                group.LookedAt.Remove(uniqueid);
                Program.GameMess.PlayerEvent(player, "stops looking at {0}.", group);
            }
        }

        protected override void LookAtTop(Player sender, Player player, Guid uniqueid, Group group, int count, bool look)
        {
            if (look)
            {
                var cards = group.Take(count);
                foreach (Card c in cards)
                {
                    c.PlayersLooking.Add(player);
                }
                group.LookedAt.Add(uniqueid, cards.ToList());
                Program.GameMess.PlayerEvent(player, "looks at {0} top {1} cards.", group, count);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uniqueid))
                { Program.GameMess.Warning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                foreach (Card c in group.LookedAt[uniqueid])
                    c.PlayersLooking.Remove(player);
                Program.GameMess.PlayerEvent(player, "stops looking at {0} top {1} cards.", group, count);
                group.LookedAt.Remove(uniqueid);
            }
        }

        protected override void LookAtBottom(Player sender, Player player, Guid uniqueid, Group group, int count, bool look)
        {
            if (look)
            {
                int skipCount = Math.Max(0, group.Count - count);
                var cards = group.Skip(skipCount);
                foreach (Card c in cards)
                {
                    c.PlayersLooking.Add(player);
                }
                group.LookedAt.Add(uniqueid, cards.ToList());
                Program.GameMess.PlayerEvent(player, "looks at {0} bottom {1} cards.", group, count);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uniqueid))
                { Program.GameMess.Warning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                foreach (Card c in group.LookedAt[uniqueid])
                    c.PlayersLooking.Remove(player);
                Program.GameMess.PlayerEvent(player, "stops looking at {0} bottom {1} cards.", group, count);
                group.LookedAt.Remove(uniqueid);
            }
        }

        protected override void StartLimited(Player sender, Player player, Guid[] packs)
        {
            Program.GameMess.System("{0} starts a limited game.", player);
            var wnd = new Play.Dialogs.PickCardsDialog();
            WindowManager.PlayWindow.ShowBackstage(wnd);
            wnd.OpenPacks(packs);
        }

        protected override void AddPacks(Player sender, Player player, Guid[] packs, bool selfOnly)
        {
            var wnd = (Play.Dialogs.PickCardsDialog)WindowManager.PlayWindow.backstage.Child;
            string packNames = wnd.PackNames(packs);
            if (packNames == "") return;
            if (selfOnly && player != Player.LocalPlayer)
            {
                Program.GameMess.System("{0} added {1} to their pool.", player, packNames);
            }
            else if (selfOnly && player == Player.LocalPlayer)
            {
                Program.GameMess.System("{0} added {1} to their pool.", player, packNames);
                wnd.OpenPacks(packs);
            }
            else
            {
                Program.GameMess.System("{0} added {1} to the limited game for all players.", player, packNames);
                wnd.OpenPacks(packs);
            }
        }

        protected override void CancelLimited(Player sender, Player player)
        {
            Program.GameMess.System("{0} cancels out of the limited game.", player);
        }

        protected override void PlayerSetGlobalVariable(Player sender, Player p, string name, string oldValue, string value)
        {
            if (p.GlobalVariables.ContainsKey(name))
            {
                p.GlobalVariables[name] = value;
            }
            else
            {
                p.GlobalVariables.Add(name, value);
            }
            if (p != Player.LocalPlayer)
            {
                Program.GameEngine.EventProxy.OnPlayerGlobalVariableChanged_3_1_0_0(p, name, oldValue, value);
                Program.GameEngine.EventProxy.OnPlayerGlobalVariableChanged_3_1_0_1(p, name, oldValue, value);
            }
            Program.GameEngine.EventProxy.OnPlayerGlobalVariableChanged_3_1_0_2(p, name, oldValue, value);
        }

        protected override void SetGlobalVariable(Player sender, string name, string oldValue, string value)
        {
            if (Program.GameEngine.GlobalVariables.ContainsKey(name))
            {
                Program.GameEngine.GlobalVariables[name] = value;
            }
            else
            {
                Program.GameEngine.GlobalVariables.Add(name, value);
            }
            Program.GameEngine.EventProxy.OnGlobalVariableChanged_3_1_0_0(name, oldValue, value);
            Program.GameEngine.EventProxy.OnGlobalVariableChanged_3_1_0_1(name, oldValue, value);
            Program.GameEngine.EventProxy.OnGlobalVariableChanged_3_1_0_2(name, oldValue, value);

        }

        protected override void IsTableBackgroundFlipped(Player sender, bool isFlipped)
        {
            Program.GameEngine.IsTableBackgroundFlipped = isFlipped;
        }

        protected override void CardSwitchTo(Player sender, Player player, Card card, string alternate)
        {
            if (player.Id != Player.LocalPlayer.Id)
                card.SwitchTo(player, alternate);
        }

        protected override void PlaySound(Player sender, Player player, string name)
        {
            if (player.Id != Player.LocalPlayer.Id) Program.GameEngine.PlaySoundReq(player, name);
        }

        protected override void Ready(Player sender, Player player)
        {
            player.Ready = true;
            Program.GameMess.System("{0} is ready", player);
            if (player.Spectator)
                return;
            if (player.WaitingOnPlayers == false)
            {
                Program.GameMess.System("Unlocking game");
                if (Program.GameEngine.TableLoaded == false)
                {
                    Program.GameEngine.TableLoaded = true;

                    Program.GameEngine.EventProxy.OnTableLoad_3_1_0_0();
                    Program.GameEngine.EventProxy.OnTableLoad_3_1_0_1();
                    Program.GameEngine.EventProxy.OnTableLoaded_3_1_0_2();

                    Program.GameEngine.EventProxy.OnGameStart_3_1_0_0();
                    Program.GameEngine.EventProxy.OnGameStart_3_1_0_1();
                    Program.GameEngine.EventProxy.OnGameStarted_3_1_0_2();
                }
            }
        }

        protected override void PlayerState(Player sender, Player player, byte b)
        {
            player.State = (PlayerState)b;
        }

        protected override void RemoteCall(Player sender, Player fromplayer, string func, string args)
        {
            Program.GameMess.PlayerEvent(fromplayer, "executes {0}", func);
            Program.GameEngine.ExecuteRemoteCall(fromplayer, func, args);
        }

        protected override void GameState(Player sender, Player fromPlayer, string strstate)
        {
            Log.DebugFormat("GameState From {0}", fromPlayer);
            var state = JsonConvert.DeserializeObject<GameSaveState>(strstate);

            state.Load(Program.GameEngine, fromPlayer);

            Program.GameMess.System("{0} sent game state ", fromPlayer.Name);
            Program.GameEngine.GotGameState(fromPlayer);
        }

        protected override void GameStateReq(Player sender, Player fromPlayer)
        {
            Log.DebugFormat("GameStateReq From {0}", fromPlayer);
            try
            {
                var ps = new GameSaveState().Create(Program.GameEngine, fromPlayer);

                var str = JsonConvert.SerializeObject(ps, Formatting.None);

                Program.Client.Rpc.GameState(fromPlayer, str);
            }
            catch (Exception e)
            {
                Log.Error("GameStateReq Error", e);
            }
        }

        protected override void DeleteCard(Player sender, Card card, Player player)
        {
            Program.GameMess.PlayerEvent(player, "deletes {0}", card);
            if (player != Player.LocalPlayer)
                card.Group.Remove(card);
        }

        protected override void PlayerDisconnect(Player sender, Player player)
        {
            Program.GameMess.System("{0} disconnected, please wait. If they do not reconnect within 1 minute they will be booted.", player);
            player.Ready = false;
        }

        protected override void AnchorCard(Player sender, Card card, Player player, bool anchor)
        {
            var astring = anchor ? "anchored" : "unanchored";
            Program.GameMess.PlayerEvent(player, "{0} {1}", astring, card);
            if (Player.LocalPlayer == player)
                return;
            card.SetAnchored(true, anchor);
        }

        protected override void SetCardProperty(Player sender, Card card, Player player, string name, string val, string valtype)
        {
            if (player == Player.LocalPlayer) return;
            //var vtype = Type.GetType(valtype);
            //var objval = JsonConvert.DeserializeObject(val, vtype);
            card.SetProperty(name, val, false);
        }

        protected override void ResetCardProperties(Player sender, Card card, Player player)
        {
            if (player == Player.LocalPlayer) return;
            card.ResetProperties(false);
        }

        protected override void SetPlayerColor(Player sender, Player player, string colorHex)
	    {
			player.SetPlayerColor(colorHex);
	    }
    }
}