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

    internal sealed class Handler
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private readonly BinaryParser _binParser;

        public Handler()
        {
            _binParser = new BinaryParser(this);
        }

        public void ReceiveMessage(byte[] data)
        {
            // Fix: because ReceiveMessage is called through the Dispatcher queue, we may be called
            // just after the Client has already been closed. In that case we should simply drop the message
            // (otherwise NRE may occur)
            if (Program.Client == null) return;

            try
            {
                _binParser.Parse(data);
            }
            catch (EndOfStreamException e)
            {
                Log.Warn("ReceiveMessage Error", e);
            }
            finally
            {
                if (Program.Client != null) Program.Client.Muted = 0;
            }
        }

        public void Binary()
        {
            //Program.Trace.TraceEvent(TraceEventType.Verbose, EventIds.NonGame, "Switched to binary protocol.");
            //Program.Client.Binary();
        }

        public void Error(string msg)
        {
            //Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "The server has returned an error: {0}", msg);
            Program.GameMess.Warning("The server has returned an error: {0}", msg);
        }

        public void Kick(string reason)
        {
            //Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "You have been kicked: {0}", reason);
            Program.GameMess.Warning("You have been kicked: {0}", reason);
            Program.InPreGame = false;
            Program.Client.ForceDisconnect();
        }

        public void Start()
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

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
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

        public void PlayerSettings(Player player, bool invertedTable, bool spectator)
        {
            player.UpdateSettings(invertedTable, spectator);
            Player.RefreshSpectators();
        }

        public void Reset(Player player)
        {
            Program.GameEngine.Reset();
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} resets the game.", player);
            Program.GameMess.System("{0} reset the game", player);
        }

        public void NextTurn(Player player)
        {
            Program.GameEngine.TurnNumber++;
            var lastPlayer = Program.GameEngine.TurnPlayer;
            Program.GameEngine.TurnPlayer = player;
            Program.GameEngine.StopTurn = false;
            Program.GameEngine.EventProxy.OnTurn_3_1_0_0(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurn_3_1_0_1(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurnPassed_3_1_0_2(lastPlayer);
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Turn, "Turn {0}: {1}", Program.GameEngine.TurnNumber, player);
            //Program.GameMess.System("Turn {0}: {1}", Program.GameEngine.TurnNumber, player);
            Program.GameMess.Turn(player, Program.GameEngine.TurnNumber);
        }

        public void StopTurn(Player player)
        {
            if (player == Player.LocalPlayer)
                Program.GameEngine.StopTurn = false;
            Program.GameEngine.EventProxy.OnEndTurn_3_1_0_0(player);
            Program.GameEngine.EventProxy.OnEndTurn_3_1_0_1(player);
            Program.GameEngine.EventProxy.OnTurnPaused_3_1_0_2(player);
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} wants to play before end of turn.", player);
            Program.GameMess.System("{0} wants to play before end of turn", player);
        }

        public void Chat(Player player, string text)
        {
            //Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Chat | EventIds.PlayerFlag(player), "<{0}> {1}", player, text);
            Program.GameMess.Chat(player, text);
        }

        public void Print(Player player, string text)
        {
            // skip for local player, handled when called for consistency
            if (player == Player.LocalPlayer) return;
            Program.Print(player, text);
        }

        public void Random(int result)
        {
            Program.GameEngine.ScriptApi.RandomResult(result);

        }

        public void Counter(Player player, Counter counter, int value, bool isScriptChange)
        {
            counter.SetValue(value, player, false, isScriptChange);
        }

        public void Welcome(byte id, Guid gameSessionId, bool waitForGameState)
        {
            Program.InPreGame = true;
            Player.LocalPlayer.Id = id;
            Program.Client.StartPings();
            Player.FireLocalPlayerWelcomed();
            Program.GameEngine.SessionId = gameSessionId;
            Program.GameEngine.WaitForGameState = waitForGameState;
        }

        public void NewPlayer(byte id, string nick, ulong pkey, bool invertedTable, bool spectator)
        {
            var p = Player.FindIncludingSpectators(id);
            if (p == null)
            {
                var player = new Player(Program.GameEngine.Definition, nick, id, pkey, spectator, false);
                Program.GameMess.System("{0} has joined the game", player);
                player.UpdateSettings(invertedTable, spectator);
                if (Program.InPreGame == false)
                {
                    GameStateReq(player);
                    if (player.Spectator == false)
                    {
                        Program.GameEngine.EventProxy.OnPlayerConnect_3_1_0_1(player);
                        Program.GameEngine.EventProxy.OnPlayerConnected_3_1_0_2(player);
                    }
                }
                else
                {
                    Sounds.PlaySound(Properties.Resources.userjoinsroom, false);

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
        public void LoadDeck(int[] id, Guid[] type, Group[] group, string[] size, string sleeve)
        {
            if (id.Length != type.Length || id.Length != group.Length)
            {
                //Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                Program.GameMess.Warning("[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                return;
            }

            if (id.Length == 0) return;   // Loading an empty deck --> do nothing

            Player who = Player.Find((byte)(id[0] >> 16));
            if (who == null)
            {
                //Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Player not found.");
                Program.GameMess.Warning("[LoadDeck] Player not found.");
                return;
            }
            Program.GameMess.System("{0} loads a deck", who);
            CreateCard(id, type, group, size, sleeve);
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
        private static void CreateCard(IList<int> id, IList<Guid> type, IList<Group> groups, IList<string> sizes, string sleeveUrl = "")
        {
            // Ignore cards created by oneself
            if (Player.Find((byte)(id[0] >> 16)) == Player.LocalPlayer) return;
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
        public void CreateCard(int[] id, Guid[] type, string[] size, Group group)
        {
            if (Player.Find((byte)(id[0] >> 16)) == Player.LocalPlayer) return;
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
        public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
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
            Player owner = Player.Find((byte)(id[0] >> 16));
            if (owner == null)
            {
                Program.GameMess.Warning("[CreateCardAt] Player not found.");
                return;
            }
            var table = Program.GameEngine.Table;
            // Bring cards created by oneself to top, for z-order consistency
            if (owner == Player.LocalPlayer)
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
                    new CreateCard(owner, id[i], faceUp, Program.GameEngine.Definition.GetCardById(modelId[i]), x[i], y[i], !persist).Do();
            }

            // Display log messages
            try
            {
                if (modelId.All(m => m == modelId[0]))
                    Program.GameMess.PlayerEvent(owner, "creates {1} '{2}'", owner, modelId.Length, owner == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(modelId[0]).Name : "card");
                else
                    foreach (Guid m in modelId)
                        Program.GameMess.PlayerEvent(owner, "{0} creates a '{1}'", owner, owner == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(m).Name : "card");

            }
            catch (Exception)
            {
                // TODO - [FIX THIS SHIT] - A null reference exception happens on the first trace event. - Kelly Elton - 3/24/2013
                // This should be cleaered up, this is only a temp fix. - Kelly Elton - 3/24/2013
            }
        }

        /// <summary>Create new CardIdentities, which hide aliases to other CardIdentities</summary>
        /// <param name="id">An array containing the new CardIdentity ids</param>
        /// <param name="type">An array with the aliased CardIdentity ids (encrypted)</param>
        //public void CreateAlias(int[] id, ulong[] type)
        //{
        //    byte playerId = (byte)(id[0] >> 16);
        //    // Ignore cards created by oneself
        //    if (playerId == Player.LocalPlayer.Id) return;
        //    for (int i = 0; i < id.Length; i++)
        //    {
        //        if (type[i] == ulong.MaxValue) continue;
        //        CardIdentity ci = new CardIdentity(id[i]) { Alias = true, Key = type[i] };
        //    }
        //}

        public void Leave(Player player)
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

        public void MoveCard(Player player, int[] card, Group to, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            // Ignore cards moved by the local player (already done, for responsiveness)
            var cards = card.Select(Card.Find).ToArray();
            if (player != Player.LocalPlayer)
                new MoveCards(player, cards, to, idx, faceUp, isScriptMove).Do();
        }

        public void MoveCardAt(Player player, int[] card, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            // Get the table control
            Table table = Program.GameEngine.Table;
            var cards = card.Select(Card.Find).ToArray();
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

        public void AddMarker(Player player, Card card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            DataNew.Entities.Marker model = Program.GameEngine.GetMarkerModel(id);
            DefaultMarkerModel defaultMarkerModel = model as DefaultMarkerModel;
            if (defaultMarkerModel != null)
                (defaultMarkerModel).SetName(name);
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
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, model.ModelString(), oldCount);
                }
            }
        }

        public void RemoveMarker(Player player, Card card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
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
                if (isScriptChange == false)
                {
                    if (player == Player.LocalPlayer && marker == null)
                    {
                        StringBuilder markerString = new StringBuilder();
                        markerString.AppendFormat("('{0}','{1}')", name, id);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(card, markerString.ToString(), oldCount, newCount, isScriptChange);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(card, markerString.ToString(), oldCount, newCount, isScriptChange);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, markerString.ToString(), oldCount);
                    }
                    else
                    {
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(card, marker.Model.ModelString(), oldCount, newCount, isScriptChange);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(card, marker.Model.ModelString(), oldCount, newCount, isScriptChange);
                        Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, marker.Model.ModelString(), oldCount);
                    }
                }

            }

        }

        public void TransferMarker(Player player, Card from, Card to, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
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
            if (isScriptChange == false && marker != null)
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
                Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(
                    from,
                    marker.Model.ModelString(),
                    oldCount);
                Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(
                    to,
                    marker.Model.ModelString(),
                    toOldCount);
            }
        }

        public void Nick(Player player, string nick)
        {
            Program.GameMess.System("{0} is now known as {1}.", player, nick);
            player.Name = nick;
        }

        public void Peek(Player player, Card card)
        {
            if (!card.PeekingPlayers.Contains(player))
                card.PeekingPlayers.Add(player);
            if (player != Player.LocalPlayer)
            {
                Program.GameMess.PlayerEvent(player, "peeks at a card ({0}).", card.Group is Table ? "on table" : "in " + card.Group.FullName);
            }
        }

        public void Untarget(Player player, Card card, bool isScriptChange)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, null, false, isScriptChange).Do();
        }

        public void Target(Player player, Card card, bool isScriptChange)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, null, true, isScriptChange).Do();
        }

        public void TargetArrow(Player player, Card card, Card otherCard, bool isScriptChange)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, otherCard, true, isScriptChange).Do();
        }

        public void Highlight(Card card, Color? color)
        { card.SetHighlight(color); }

        public void Filter(Card card, Color? color)
        { card.SetFilter(color); }

        public void Turn(Player player, Card card, bool up)
        {
            // Ignore the card we turned ourselves
            if (player == Player.LocalPlayer)
            {
                card.MayBeConsideredFaceUp = false;     // see comment on mayBeConsideredFaceUp
                return;
            }
            new Turn(player, card, up).Do();
        }

        public void Rotate(Player player, Card card, CardOrientation rot)
        {
            // Ignore the moves we made ourselves
            if (player == Player.LocalPlayer)
                return;
            new Rotate(player, card, rot).Do();
        }

        /// <summary>Part of a shuffle process.</summary>
        /// <param name="group">The group being shuffled.</param>
        /// <param name="card">An array containing the CardIdentity ids to shuffle.</param>
        //public void Shuffle(Group group, int[] card)
        //{
        //    // Array to hold the new aliases (sent to CreateAlias)
        //    ulong[] aliases = new ulong[card.Length];
        //    // Intialize the group shuffle
        //    group.FilledShuffleSlots = 0;
        //    group.HasReceivedFirstShuffledMessage = false;
        //    group.MyShufflePos = new short[card.Length];
        //    // Check if we received enough cards
        //    if (Player.Count - 1 <= 0) return;
        //    if (card.Length < group.Count / (Player.Count - 1))
        //        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[Shuffle] Too few cards received.");
        //    // Do the shuffling
        //    var rnd = new CryptoRandom();
        //    for (int i = card.Length - 1; i >= 0; i--)
        //    {
        //        int r = rnd.Next(i + 1);
        //        int tc = card[r];
        //        card[r] = card[i];
        //        // Create a new alias, if the card is not face up
        //        CardIdentity ci = CardIdentity.Find(tc);
        //        if (group.FindByCardIdentity(ci) != null)
        //        {
        //            card[i] = tc; aliases[i] = ulong.MaxValue;
        //            ci.Visible = true;
        //        }
        //        else
        //        {
        //            ci = new CardIdentity(ExtensionMethods.GenerateCardId());
        //            ci.MySecret = ci.Alias = true;
        //            ci.Key = ((ulong)Crypto.PositiveRandom()) << 32 | (uint)tc;
        //            card[i] = ci.Id; aliases[i] = Crypto.ModExp(ci.Key);
        //            ci.Visible = false;
        //        }
        //        // Give a random position to the card
        //        group.MyShufflePos[i] = (short)Crypto.Random(group.Count);
        //    }
        //    // Send the results
        //    Program.Client.Rpc.CreateAlias(card, aliases);
        //    Program.Client.Rpc.Shuffled(group, card, group.MyShufflePos);
        //}

        public void Shuffled(Player player, Group group, int[] card, short[] pos)
        {
            if (player == Player.LocalPlayer) return;
            ((Pile)group).DoShuffle(card, pos);
        }

        /// <summary>Completely remove all aliases from a group, e.g. before performing a shuffle.</summary>
        /// <param name="group">The group to remove all aliases from.</param>
        //public void UnaliasGrp(Group group)
        //{
        //    // Get the group
        //    Pile g = group as Pile;
        //    if (g == null)
        //    { Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, "[UnaliasGrp] Group is not a pile."); return; }
        //    // Collect aliases which we p
        //    List<int> cards = new List<int>(g.Count);
        //    List<ulong> types = new List<ulong>(g.Count);
        //    bool hasAlias = false;
        //    foreach (Card t in g)
        //    {
        //        CardIdentity ci = t.Type;
        //        if (ci == null) continue; //Hack, should this ever be null? Sometimes it happens for whatever reason.
        //        if (!ci.Alias) continue;
        //        hasAlias = true;
        //        if (ci.MySecret)
        //        { cards.Add(t.Id); types.Add(ci.Key); }
        //    }
        //    // Unalias cards that we know (if any)
        //    if (cards.Count > 0)
        //        Program.Client.Rpc.Unalias(cards.ToArray(), types.ToArray());
        //    // If there are no alias, we may be ready to shuffle
        //    if (!hasAlias && g.WantToShuffle)
        //    { g.DoShuffle(); return; }
        //    // Mark the group for shuffling
        //    g.PreparingShuffle = true;
        //    // Notify the user
        //    Program.TracePlayerEvent(group.Owner, "{0} is being prepared for shuffle.", g);
        //    // Check for null because the chat can currently be muted (e.g. during a Mulligan scripted action)
        //    if (Program.LastChatTrace != null)
        //        g.ShuffledTrace += (new ShuffleTraceChatHandler { Line = Program.LastChatTrace }).ReplaceText;
        //}

        /// <summary>Unalias some Cards, e.g. before a shuffle</summary>
        /// <param name="card">An array containing the Card ids to unalias.</param>
        /// <param name="type">An array containing the corresponding revealed CardIdentity ids.</param>
        //public void Unalias(int[] card, ulong[] type)
        //{
        //    if (card.Length != type.Length)
        //    { Program.TraceWarning("[Unalias] Card and type lengths don't match."); return; }
        //    Pile g = null;
        //    List<int> cards = new List<int>(card.Length);
        //    List<ulong> types = new List<ulong>(card.Length);
        //    for (int i = 0; i < card.Length; i++)
        //    {
        //        Card c = Card.Find(card[i]);
        //        if (c == null)
        //        { Program.TraceWarning("[Unalias] Card not found."); continue; }
        //        if (g == null) g = c.Group as Pile;
        //        else if (g != c.Group)
        //        { Program.TraceWarning("[Unalias] Not all cards belong to the same group!"); continue; }
        //        // Check nobody cheated
        //        if (!c.Type.MySecret)
        //        {
        //            if (c.Type.Key != Crypto.ModExp(type[i]))
        //                Program.TraceWarning("[Unalias] Card identity doesn't match.");
        //        }
        //        // Substitue the card's identity
        //        CardIdentity ci = CardIdentity.Find((int)type[i]);
        //        if (ci == null)
        //        { Program.TraceWarning("[Unalias] Card identity not found."); continue; }
        //        CardIdentity.Delete(c.Type.Id); c.Type = ci;
        //        // Propagate unaliasing
        //        if (ci.Alias && ci.MySecret)
        //            cards.Add(c.Id); types.Add(ci.Key);
        //    }
        //    if (cards.Count > 0)
        //        Program.Client.Rpc.Unalias(cards.ToArray(), types.ToArray());
        //    if (g == null) return;
        //    if (!g.PreparingShuffle)
        //    { Program.TraceWarning("[Unalias] Cards revealed are not in a group prepared for shuffle."); return; }
        //    // If all cards are now revealed, one can proceed to shuffling
        //    if (!g.WantToShuffle) return;
        //    bool done = false;
        //    for (int i = 0; !done && i < g.Count; i++)
        //        done = g[i].Type.Alias;
        //    if (!done)
        //        g.DoShuffle();
        //}

        public void PassTo(Player who, ControllableObject obj, Player player, bool requested)
        {
            // Ignore message that we sent in the first place
            if (who != Player.LocalPlayer)
                obj.PassControlTo(player, who, false, requested);
        }

        public void TakeFrom(ControllableObject obj, Player to)
        { obj.TakingControl(to); }

        public void DontTake(ControllableObject obj)
        { obj.DontTakeError(); }

        public void FreezeCardsVisibility(Group group)
        {
            foreach (Card c in group.Cards) c.SetOverrideGroupVisibility(true);
        }

        public void GroupVis(Player player, Group group, bool defined, bool visible)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.SetVisibility(defined ? (bool?)visible : null, false);
            if (defined)
                Program.GameMess.PlayerEvent(player, visible ? "shows {0} to everybody." : "shows {0} to nobody.", group);
        }

        public void GroupVisAdd(Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.AddViewer(whom, false);
            Program.GameMess.PlayerEvent(player, "shows {0} to {1}.", group, whom);
        }

        public void GroupVisRemove(Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.RemoveViewer(whom, false);
            Program.GameMess.PlayerEvent(player, "hides {0} from {1}.", group, whom);
        }

        public void LookAt(Player player, int uid, Group group, bool look)
        {
            if (look)
            {
                if (group.Visibility != DataNew.Entities.GroupVisibility.Everybody)
                    foreach (Card c in group)
                    {
                        c.PlayersLooking.Add(player);
                    }
                group.LookedAt.Add(uid, group.ToList());
                Program.GameMess.PlayerEvent(player, "looks at {0}.", group);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uid))
                { Program.GameMess.Warning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                if (group.Visibility != DataNew.Entities.GroupVisibility.Everybody)
                {
                    foreach (Card c in group.LookedAt[uid])
                        c.PlayersLooking.Remove(player);
                }
                group.LookedAt.Remove(uid);
                Program.GameMess.PlayerEvent(player, "stops looking at {0}.", group);
            }
        }

        public void LookAtTop(Player player, int uid, Group group, int count, bool look)
        {
            if (look)
            {
                var cards = group.Take(count);
                foreach (Card c in cards)
                {
                    c.PlayersLooking.Add(player);
                }
                group.LookedAt.Add(uid, cards.ToList());
                Program.GameMess.PlayerEvent(player, "looks at {0} top {1} cards.", group, count);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uid))
                { Program.GameMess.Warning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                foreach (Card c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.GameMess.PlayerEvent(player, "stops looking at {0} top {1} cards.", group, count);
                group.LookedAt.Remove(uid);
            }
        }

        public void LookAtBottom(Player player, int uid, Group group, int count, bool look)
        {
            if (look)
            {
                int skipCount = Math.Max(0, group.Count - count);
                var cards = group.Skip(skipCount);
                foreach (Card c in cards)
                {
                    c.PlayersLooking.Add(player);
                }
                group.LookedAt.Add(uid, cards.ToList());
                Program.GameMess.PlayerEvent(player, "looks at {0} bottom {1} cards.", group, count);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uid))
                { Program.GameMess.Warning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                foreach (Card c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.GameMess.PlayerEvent(player, "stops looking at {0} bottom {1} cards.", group, count);
                group.LookedAt.Remove(uid);
            }
        }

        public void StartLimited(Player player, Guid[] packs)
        {
            Program.GameMess.System("{0} starts a limited game.", player);
            var wnd = new Play.Dialogs.PickCardsDialog();
            WindowManager.PlayWindow.ShowBackstage(wnd);
            wnd.OpenPacks(packs);
        }

        public void AddPacks(Player player, Guid[] packs, bool selfOnly)
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

        public void CancelLimited(Player player)
        {
            Program.GameMess.System("{0} cancels out of the limited game.", player);
        }

        public void PlayerSetGlobalVariable(Player p, string name, string oldValue, string value)
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
                Program.GameEngine.EventProxy.OnPlayerGlobalVariableChanged_3_1_0_2(p, name, oldValue, value);
            }
        }

        public void SetGlobalVariable(string name, string oldValue, string value)
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

        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            Program.GameEngine.IsTableBackgroundFlipped = isFlipped;
        }

        public void CardSwitchTo(Player player, Card card, string alternate)
        {
            if (player.Id != Player.LocalPlayer.Id)
                card.SwitchTo(player, alternate);
        }

        public void Ping()
        {

        }

        public void PlaySound(Player player, string name)
        {
            if (player.Id != Player.LocalPlayer.Id) Program.GameEngine.PlaySoundReq(player, name);
        }

        public void Ready(Player player)
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

        public void PlayerState(Player player, byte b)
        {
            player.State = (PlayerState)b;
        }

        public void RemoteCall(Player fromplayer, string func, string args)
        {
            Program.GameMess.PlayerEvent(fromplayer, "executes {0}", func);
            Program.GameEngine.ExecuteRemoteCall(fromplayer, func, args);
        }

        public void CreateAliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            Program.GameMess.Warning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void ShuffleDeprecated(Group arg0, int[] ints)
        {
            Program.GameMess.Warning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void UnaliasGrpDeprecated(Group arg0)
        {
            Program.GameMess.Warning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void UnaliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            Program.GameMess.Warning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void GameState(Player fromPlayer, string strstate)
        {
            Log.DebugFormat("GameState From {0}", fromPlayer);
            var state = JsonConvert.DeserializeObject<GameSaveState>(strstate);

            state.Load(Program.GameEngine, fromPlayer);

            Program.GameMess.System("{0} sent game state ", fromPlayer.Name);
            Program.GameEngine.GotGameState(fromPlayer);
        }

        public void GameStateReq(Player fromPlayer)
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

        public void DeleteCard(Card card, Player player)
        {
            Program.GameMess.PlayerEvent(player, "deletes {0}", card);
            if (player != Player.LocalPlayer)
                card.Group.Remove(card);
        }

        public void PlayerDisconnect(Player player)
        {
            Program.GameMess.System("{0} disconnected, please wait. If they do not reconnect within 1 minute they will be booted.", player);
            player.Ready = false;
        }

        public void AnchorCard(Card card, Player player, bool anchor)
        {
            var astring = anchor ? "anchored" : "unanchored";
            Program.GameMess.PlayerEvent(player, "{0} {1}", astring, card);
            if (Player.LocalPlayer == player)
                return;
            card.SetAnchored(true, anchor);
        }

        public void SetCardProperty(Card card, Player player, string name, string val, string valtype)
        {
            if (player == Player.LocalPlayer) return;
            //var vtype = Type.GetType(valtype);
            //var objval = JsonConvert.DeserializeObject(val, vtype);
            card.SetProperty(name, val, false);
        }

        public void ResetCardProperties(Card card, Player player)
        {
            if (player == Player.LocalPlayer) return;
            card.ResetProperties(false);
        }
    }
}