/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Utils;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Octgn.Core.DataExtensionMethods;
using System.Windows.Media;
using Octgn.Core.Play;
using Octgn.Play.State;
using Card = Octgn.Play.Card;
using Counter = Octgn.Play.Counter;
using Group = Octgn.Play.Group;
using Player = Octgn.Play.Player;
using Phase = Octgn.Play.Phase;

namespace Octgn.Networking
{
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
        }

        public void Error(string msg)
        {
            Program.GameMess.Warning("The server has returned an error: {0}", msg);
        }

        public void Kick(string reason)
        {
            Program.GameMess.Warning("You have been kicked: {0}", reason);
            Program.InPreGame = false;
            Program.Client.ForceDisconnect();
        }

        public void Start()
        {
            Log.Debug("Start");
            Program.InPreGame = false;
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
            Program.GameMess.System("{0} reset the game", player);
        }

        public void NextTurn(Player player, bool setActive, bool force)
        {
            var lastPlayer = Program.GameEngine.TurnPlayer;
            var lastTurn = Program.GameEngine.TurnNumber;
            Program.GameEngine.TurnNumber++;
            Program.GameEngine.TurnPlayer = (setActive) ? player : null;
            Program.GameEngine.StopTurn = false;
            Program.GameEngine.CurrentPhase = null;
            Program.GameMess.Turn(Program.GameEngine.TurnPlayer, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurn_3_1_0_0(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurn_3_1_0_1(player, Program.GameEngine.TurnNumber);
            Program.GameEngine.EventProxy.OnTurnPassed_3_1_0_2(lastPlayer, lastTurn, force);
        }

        public void StopTurn(Player player)
        {
            if (player == Player.LocalPlayer)
                Program.GameEngine.StopTurn = false;
            Program.GameMess.System("{0} wants to play before end of turn.", player);
            Program.GameEngine.EventProxy.OnEndTurn_3_1_0_0(player);
            Program.GameEngine.EventProxy.OnEndTurn_3_1_0_1(player);
            Program.GameEngine.EventProxy.OnTurnPaused_3_1_0_2(player);
        }

        public void SetPhase(byte phase, Player[] players, bool force)
        {
            var currentPhase = Program.GameEngine.CurrentPhase;
            var newPhase = Phase.Find(phase);
            Program.GameEngine.CurrentPhase = newPhase;
            Program.GameMess.Phase(Program.GameEngine.TurnPlayer, newPhase.Name);
            if (players.Length > 0 && !players.Contains(Program.GameEngine.TurnPlayer)) //alert if a non-active player has a stop set on the phase
            {
                Program.GameMess.System("A player has a stop set on {0}.", newPhase.Name);
            }

            if (currentPhase == null)
                Program.GameEngine.EventProxy.OnPhasePassed_3_1_0_2(null, 0, force);
            else
                Program.GameEngine.EventProxy.OnPhasePassed_3_1_0_2(currentPhase.Name, currentPhase.Id, force);
        }

        public void SetActivePlayer(Player player)
        {
            var lastPlayer = Program.GameEngine.TurnPlayer;
            Program.GameEngine.TurnPlayer = player;
            Program.GameEngine.StopTurn = false;
            Program.GameEngine.EventProxy.OnTurnPassed_3_1_0_2(lastPlayer, Program.GameEngine.TurnNumber, false);
        }

        public void ClearActivePlayer()
        {
            var lastPlayer = Program.GameEngine.TurnPlayer;
            Program.GameEngine.TurnPlayer = null;
            Program.GameEngine.StopTurn = false;
            Program.GameEngine.EventProxy.OnTurnPassed_3_1_0_2(lastPlayer, Program.GameEngine.TurnNumber, false);
        }

        public void SetBoard(string name)
        {
            Program.GameEngine.ChangeGameBoard(name);
        }

        public void Chat(Player player, string text)
        {
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

        public void NewPlayer(byte id, string nick, string userId, ulong pkey, bool invertedTable, bool spectator)
        {
            var p = Player.FindIncludingSpectators(id);
            if (p == null)
            {
                var player = new Player(Program.GameEngine.Definition, nick, userId, id, pkey, spectator, false);
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
        public void LoadDeck(int[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited)
        {
            if (id.Length != type.Length || id.Length != group.Length)
            {
                Program.GameMess.Warning("[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                return;
            }

            if (id.Length == 0) return;   // Loading an empty deck --> do nothing

            var who = Player.Find((byte)(id[0] >> 16));
            if (who == null)
            {
                Program.GameMess.Warning("[LoadDeck] Player not found.");
                return;
            }
            if (limited) Program.GameMess.System("{0} loads a limited deck.", who);
            else Program.GameMess.System("{0} loads a deck.", who);
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
            for (var i = 0; i < id.Count; i++)
            {
                var group = groups[i];
                var owner = group.Owner;
                if (owner == null)
                {
                    Program.GameMess.Warning("[CreateCard] Player not found.");
                    continue;
                }

                var c = new Card(owner, id[i], Program.GameEngine.Definition.GetCardById(type[i]), false, sizes[i]); c.SetSleeve(sleeveUrl); group.AddAt(c, group.Count);
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
            for (var i = 0; i < id.Length; i++)
            {
                var owner = group.Owner;
                if (owner == null)
                {
                    Program.GameMess.Warning("[CreateCard] Player not found.");
                    return;
                }
                var c = Card.Find(id[0]);

                Program.GameMess.PlayerEvent(owner, "{0} creates {1} {2} in {3}'s {4}", owner.Name, id.Length, c == null ? "card" : (object)c, group.Owner.Name, group.Name);
                // Ignore cards created by oneself

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
            var owner = Player.Find((byte)(id[0] >> 16));
            if (owner == null)
            {
                Program.GameMess.Warning("[CreateCardAt] Player not found.");
                return;
            }
            var table = Program.GameEngine.Table;
            // Bring cards created by oneself to top, for z-order consistency
            if (owner == Player.LocalPlayer)
            {
                for (var i = id.Length - 1; i >= 0; --i)
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
                for (var i = 0; i < id.Length; i++)
                    new CreateCard(owner, id[i], faceUp, Program.GameEngine.Definition.GetCardById(modelId[i]), x[i], y[i], !persist).Do();
            }

            if (modelId.All(m => m == modelId[0]))
                Program.GameMess.PlayerEvent(owner, "creates {1} '{2}'", owner, modelId.Length, owner == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(modelId[0]).Name : "card");
            else
                foreach (var m in modelId)
                    Program.GameMess.PlayerEvent(owner, "{0} creates a '{1}'", owner, owner == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(m).Name : "card");
        }

        public void Leave(Player player)
        {
            Program.GameMess.System("{0} has closed their game window left the game. They did not crash or lose connection, they left on purpose.", player);
            if( !Program.InPreGame ) {
                Program.GameEngine.EventProxy.OnPlayerLeaveGame_3_1_0_1( player );
                Program.GameEngine.EventProxy.OnPlayerQuit_3_1_0_2( player );
            }
            player.Delete();
            if (Program.IsHost && Program.InPreGame)
            {
                Sounds.PlaySound(Properties.Resources.doorclose);
            }
        }

        public void MoveCard(Player player, int[] card, Group to, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            // Ignore cards moved by the local player (already done, for responsiveness)
            var cards = card.Select(Card.Find).Where(x=>x != null).ToArray();
            if (player != Player.LocalPlayer)
                new MoveCards(player, cards, to, idx, faceUp, isScriptMove).Do();
        }

        public void MoveCardAt(Player player, int[] cards, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            // Get the table control
            var table = Program.GameEngine.Table;

            var playCards = cards
                .Select( cardId => {
                    var playCard = Card.Find( cardId );
                    if( playCard == null ) {
                        Program.GameMess.Warning( "Inconsistent state. Player {0} tried to move a card that does not exist.", player );
                        Program.GameMess.GameDebug( "Missing Card ID={0}", cardId );
                    }
                    return playCard;
                } )
                .Where( playCard => playCard != null )
                .ToArray();

            if( playCards.Length == 0 ) return;

            // Because every player may manipulate the table at the same time, the index may be out of bound
            if ( playCards[0].Group == table)
            {
                for (var index = 0; index < idx.Length; index++)
                {
                    if (idx[index] >= table.Count) idx[index] = table.Count - 1;
                }
            }
            else
            {
                for (var index = 0; index < idx.Length; index++)
                {
                    if (idx[index] > table.Count) idx[index] = table.Count;
                }
            }

            // Ignore cards moved by the local player (already done, for responsiveness)
            if (player == Player.LocalPlayer) return;
            // Find the old position on the table, if any
            // Do the move
            new MoveCards( player, playCards, x, y, idx, faceUp, isScriptMove).Do();
        }

        public void AddMarker(Player player, Card card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            var model = Program.GameEngine.GetMarkerModel(id);
            model.Name = name;
            var marker = card.FindMarker(id, name);
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
                var newCount = oldCount + count;
                Program.GameMess.PlayerEvent(player, "adds {0} {1} marker(s) on {2}", count, model.Name, card);
                if (isScriptChange == false)
                {
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_0(card, model.ModelString(), oldCount, newCount, isScriptChange);
                    Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_1(card, model.ModelString(), oldCount, newCount, isScriptChange);
                }
                Program.GameEngine.EventProxy.OnMarkerChanged_3_1_0_2(card, model.Name, model.Id.ToString(), oldCount, isScriptChange);
            }
        }

        public void RemoveMarker(Player player, Card card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            var marker = card.FindMarker(id, name);
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
                var newCount = oldCount - count;
                if (player != Player.LocalPlayer)
                {
                    card.RemoveMarker(marker, count);
                }
                Program.GameMess.PlayerEvent(player, "removes {0} {1} marker(s) from {2}", count, name, card);
                if (player == Player.LocalPlayer && marker == null)
                {
                    var markerString = new StringBuilder();
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

        public void TransferMarker(Player player, Card from, Card to, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            var marker = from.FindMarker(id, name);
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
            var newMarker = to.FindMarker(id, name);
            var toOldCount = 0;
            if (newMarker != null)
                toOldCount = newMarker.Count - 1;
            var fromNewCount = oldCount - count;
            var toNewCount = toOldCount + count;
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

        public void Shuffled(Player player, Group group, int[] card, short[] pos)
        {
            if (player == Player.LocalPlayer) return;
            ((Pile)group).DoShuffle(card, pos);
        }

        public void PassTo(Player who, ControllableObject obj, Player player, bool requested)
        {
            // Ignore message that we sent in the first place
            if (who != Player.LocalPlayer)
                obj.PassControlTo(player, who, false, requested);
            if (obj is Card)
               Program.GameEngine.EventProxy.OnCardControllerChanged_3_1_0_2((Card)obj, who, player);
        }

        public void TakeFrom(ControllableObject obj, Player to)
        { obj.TakingControl(to); }

        public void DontTake(ControllableObject obj)
        { obj.DontTakeError(); }

        public void FreezeCardsVisibility(Group group)
        {
            foreach (var c in group.Cards) c.SetOverrideGroupVisibility(true);
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
                    foreach (var c in group)
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
                    foreach (var c in group.LookedAt[uid])
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
                foreach (var c in cards)
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
                foreach (var c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.GameMess.PlayerEvent(player, "stops looking at {0} top {1} cards.", group, count);
                group.LookedAt.Remove(uid);
            }
        }

        public void LookAtBottom(Player player, int uid, Group group, int count, bool look)
        {
            if (look)
            {
                var skipCount = Math.Max(0, group.Count - count);
                var cards = group.Skip(skipCount);
                foreach (var c in cards)
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

                foreach (var c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.GameMess.PlayerEvent(player, "stops looking at {0} bottom {1} cards.", group, count);
                group.LookedAt.Remove(uid);
            }
        }

        public void StartLimited(Player player, Guid[] packs)
        {
            Program.GameMess.System("{0} starts a limited game.", player);
            if (Player.LocalPlayer.Spectator == false)
            {
                var wnd = new Play.Dialogs.PickCardsDialog();
                WindowManager.PlayWindow.ShowBackstage(wnd);
                wnd.OpenPacks(packs);
            }
        }

        public void AddPacks(Player player, Guid[] packs, bool selfOnly)
        {
            var wnd = (Play.Dialogs.PickCardsDialog)WindowManager.PlayWindow.backstage.Child;
            var packNames = wnd.PackNames(packs);
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
            }
            Program.GameEngine.EventProxy.OnPlayerGlobalVariableChanged_3_1_0_2(p, name, oldValue, value);
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
            card.SetProperty(name, val, false);
        }

        public void ResetCardProperties(Card card, Player player)
        {
            if (player == Player.LocalPlayer) return;
            card.ResetProperties(false);
        }

	    public void SetPlayerColor(Player player, string colorHex)
	    {
			player.SetPlayerColor(colorHex);
	    }
    }
}