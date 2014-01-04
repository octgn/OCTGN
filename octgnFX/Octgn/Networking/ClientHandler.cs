using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Utils;

namespace Octgn.Networking
{
    using System.Reflection;

    using log4net;

    using Newtonsoft.Json;

    using Octgn.Core.DataExtensionMethods;
    using System.Windows.Media;

    using Octgn.Core.Play;
    using Octgn.Core.Util;
    using Octgn.Play.State;

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
            { _binParser.Parse(data); }
            finally
            { if (Program.Client != null) Program.Client.Muted = 0; }
        }

        public void Binary()
        {
            Program.Trace.TraceEvent(TraceEventType.Verbose, EventIds.NonGame, "Switched to binary protocol.");
            //Program.Client.Binary();
        }

        public void Error(string msg)
        {
            Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "The server has returned an error: {0}", msg);
        }

        public void Kick(string reason)
        {
            Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "You have been kicked: {0}", reason);
            Program.InPreGame = false;
			Program.Client.ForceDisconnect();
        }

        public void Start()
        {
			Log.Debug("Start");
            Program.InPreGame = false;
            Program.StartGame();
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

        public void Settings(bool twoSidedTable)
        {
            // The host is the driver for this flag and should ignore notifications,
            // otherwise there might be a loop if the server takes more time to dispatch this message
            // than the user to click again on the checkbox.
            if (!Program.IsHost)
                Program.GameSettings.UseTwoSidedTable = twoSidedTable;
        }

        public void PlayerSettings(Player player, bool invertedTable)
        {
            player.InvertedTable = invertedTable;
        }

        public void Reset(Player player)
        {
            Program.GameEngine.Reset();
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} resets the game.", player);
        }

        public void NextTurn(Player player)
        {
            Program.GameEngine.TurnNumber++;
            Program.GameEngine.TurnPlayer = player;
            Program.GameEngine.StopTurn = false;
            Program.GameEngine.EventProxy.OnTurn(player, Program.GameEngine.TurnNumber);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Turn, "Turn {0}: {1}", Program.GameEngine.TurnNumber, player);
        }

        public void StopTurn(Player player)
        {
            if (player == Player.LocalPlayer)
                Program.GameEngine.StopTurn = false;
            Program.GameEngine.EventProxy.OnEndTurn(player);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} wants to play before end of turn.", player);
        }

        public void Chat(Player player, string text)
        {
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Chat | EventIds.PlayerFlag(player), "<{0}> {1}", player, text);
        }

        public void Print(Player player, string text)
        {
            // skip for local player, handled when called for consistency
            if (player == Player.LocalPlayer) return;
            Program.Print(player, text);
        }

        public void Random(Player player, int id, int min, int max)
        {
            var req = new RandomRequest(player, id, min, max);
            Program.GameEngine.RandomRequests.Add(req);
            req.Answer1();
        }

        public void RandomAnswer1(Player player, int id, ulong value)
        {
            var req = Program.GameEngine.FindRandomRequest(id);
            if (req == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[RandomAnswer1] Random request not found.");
                return;
            }
            if (req.IsPhase1Completed())
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[RandomAnswer1] Too many values received. One client is buggy or tries to cheat.");
                return;
            }
            req.AddAnswer1(player, value);
            if (req.IsPhase1Completed())
                req.Answer2();
        }

        public void RandomAnswer2(Player player, int id, ulong value)
        {
            var req = Program.GameEngine.FindRandomRequest(id);
            if (req == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[RandomAnswer1] Random request not found.");
                return;
            }
            req.AddAnswer2(player, value);
            if (req.IsPhase2Completed())
                req.Complete();
        }

        public void Counter(Player player, Counter counter, int value)
        {
            counter.SetValue(value, player, false);
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

        public void NewPlayer(byte id, string nick, ulong pkey, bool invertedTable)
        {
            var p = Player.Find(id);
            if (p == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event, "{0} has joined the game.", nick);
                System.Windows.Application.Current.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            var player = new Player(Program.GameEngine.Definition, nick, id, pkey);
                            player.InvertedTable = invertedTable;
                            if (Program.IsHost)
                            {
                                Sounds.PlaySound(Properties.Resources.knockknock, false);
                            }
                        }));
            }
        }

        /// <summary>Loads a player deck.</summary>
        /// <param name="id">An array containing the loaded CardIdentity ids.</param>
        /// <param name="type">An array containing the corresponding CardModel guids (encrypted).</param>
        /// <param name="group">An array indicating the group the cards must be loaded into.</param>
        public void LoadDeck(int[] id, ulong[] type, Group[] group)
        {
            if (id.Length != type.Length || id.Length != group.Length)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                return;
            }

            if (id.Length == 0) return;   // Loading an empty deck --> do nothing

            Player who = Player.Find((byte)(id[0] >> 16));
            if (who == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Player not found.");
                return;
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who), "{0} loads a deck.", who);
            CreateCard(id, type, group);
            Log.Info("LoadDeck Starting Task to Fire Event");
            Task.Factory.StartNew(() =>
            {
                Log.Info("LoadDeck Factory Started to Fire Event");
                Thread.Sleep(1000);
                Log.Info("LoadDeck Firing Event");
                try
                {
                    Program.Dispatcher.Invoke(new Action(()=>Program.GameEngine.EventProxy.OnLoadDeck(who, @group.Distinct().ToArray())));
                    Log.Info("LoadDeck Finished firing event.");
                }
                catch (Exception e)
                {
                    Log.Error("LoadDeck Error Firing Event", e);
                }
            });
        }

        /// <summary>Creates new Cards as well as the corresponding CardIdentities. The cards may be in different groups.</summary>
        /// <param name="id">An array with the new CardIdentity ids.</param>
        /// <param name="type">An array containing the corresponding CardModel guids (encrypted)</param>
        /// <param name="groups">An array indicating the group the cards must be loaded into.</param>
        /// <seealso cref="CreateCard(int[], ulong[], Group)"> for a more efficient way to insert cards inside one group.</seealso>
        private static void CreateCard(IList<int> id, IList<ulong> type, IList<Group> groups)
        {
            if (Player.Find((byte)(id[0] >> 16)) == Player.LocalPlayer) return;
            for (int i = 0; i < id.Count; i++)
            {
                Group group = groups[i];
                Player owner = group.Owner;
                if (owner == null)
                {
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCard] Player not found.");
                    continue;
                }
                // Ignore cards created by oneself

                Card c = new Card(owner, id[i], type[i], null, false);
                group.AddAt(c, group.Count);
            }
        }

        /// <summary>Creates new Cards as well as the corresponding CardIdentities. All cards are created in the same group.</summary>
        /// <param name="id">An array with the new CardIdentity ids.</param>
        /// <param name="type">An array containing the corresponding CardModel guids (encrypted)</param>
        /// <param name="group">The group, in which the cards are added.</param>
        /// <seealso cref="CreateCard(int[], ulong[], Group[])"> to add cards to several groups</seealso>
        public void CreateCard(int[] id, ulong[] type, Group group)
        {
            if (Player.Find((byte)(id[0] >> 16)) == Player.LocalPlayer) return;
            for (int i = 0; i < id.Length; i++)
            {
                Player owner = group.Owner;
                if (owner == null)
                {
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCard] Player not found.");
                    return;
                }
                //var c = new Card(owner,id[0], type[0], Program.Game.Definition.CardDefinition, null, false);
                var c = Card.Find(id[0]);

                Program.TracePlayerEvent(owner, "{0} creates {1} {2} in {3}'s {4}", owner.Name, id.Length, c == null ? "card" : c.Name, group.Owner.Name, group.Name);
                // Ignore cards created by oneself

                //Card c = new Card(owner, id[i], type[i], Program.Game.Definition.CardDefinition, null, false);
                //group.AddAt(c, group.Count);
                var card = new Card(owner, id[i], type[i], null, false);
                group.AddAt(card, group.Count);
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
        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            if (id.Length == 0)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCardAt] Empty id parameter.");
                return;
            }
            if (id.Length != key.Length || id.Length != x.Length || id.Length != y.Length || id.Length != modelId.Length)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCardAt] Inconsistent parameters length.");
                return;
            }
            Player owner = Player.Find((byte)(id[0] >> 16));
            if (owner == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCardAt] Player not found.");
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
                        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCardAt] Card not found.");
                        return;
                    }
                    table.SetCardIndex(card, table.Count + i - id.Length);
                }
            }
            else
            {
                for (int i = 0; i < id.Length; i++)
                    new CreateCard(owner, id[i], key[i], faceUp, Program.GameEngine.Definition.GetCardById(modelId[i]), x[i], y[i], !persist).Do();
            }

            // Display log messages
            try
            {
                if (modelId.All(m => m == modelId[0]))
                    Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(owner), "{0} creates {1} '{2}'", owner, modelId.Length, owner == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(modelId[0]).Name : "card");
                else
                    foreach (Guid m in modelId)
                        Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(owner), "{0} creates a '{1}'", owner, owner == Player.LocalPlayer || faceUp ? Program.GameEngine.Definition.GetCardById(m).Name : "card");

            }
            catch (Exception e)
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
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event, "{0} has left the game.", player);
            player.Delete();
            if (Program.IsHost)
            {
                Sounds.PlaySound(Properties.Resources.doorclose);
            }
        }

        public void MoveCard(Player player, Card card, Group to, int idx, bool faceUp, bool isScriptMove)
        {
            // Ignore cards moved by the local player (already done, for responsiveness)
            if (player != Player.LocalPlayer)
                new MoveCard(player, card, to, idx, faceUp, isScriptMove).Do();
            else
            {
                // Fix: cards may move quickly locally from one group to another one, before we get a chance
                // to execute this handler, during game actions scripts (e.g. Mulligan with one player -
                // shuffling happens locally). The result is that we are going to receive two messages,
                // one for the move to group A, then the move to group B; while the card already is in group B.
                // In this case, trying to set index inside group B with an index coming from the time the card
                // was in group A is just plain wrong and may crash depending on the index.
                if (card.Group == to)
                    card.SetIndex(idx); // This is done to preserve stack order consistency with other players (should be a noop most of the time)
            }
        }

        public void MoveCardAt(Player player, Card card, int x, int y, int idx, bool faceUp, bool isScriptMove)
        {
            // Get the table control
            Table table = Program.GameEngine.Table;
            // Because every player may manipulate the table at the same time, the index may be out of bound
            if (card.Group == table)
            { if (idx >= table.Count) idx = table.Count - 1; }
            else
                if (idx > table.Count) idx = table.Count;

            // Ignore cards moved by the local player (already done, for responsiveness)
            if (player == Player.LocalPlayer)
            {
                // See remark in MoveCard
                if (card.Group == table)
                    card.SetIndex(idx); // This is done to preserve stack order consistency with other players (should be a noop most of the time)
                return;
            }
            // Find the old position on the table, if any
            //bool onTable = card.Group == table;
            //double oldX = card.X, oldY = card.Y;
            // Do the move
            new MoveCard(player, card, x, y, idx, faceUp, isScriptMove).Do();
        }

        public void AddMarker(Player player, Card card, Guid id, string name, ushort count)
        {
            DataNew.Entities.Marker model = Program.GameEngine.GetMarkerModel(id);
            DefaultMarkerModel defaultMarkerModel = model as DefaultMarkerModel;
            if (defaultMarkerModel != null)
                (defaultMarkerModel).SetName(name);
            // Ignore markers created by oneself (already created for responsiveness issues)
            if (player != Player.LocalPlayer)
                card.AddMarker(model, count);
            if (count != 0)
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                  "{0} adds {1} {2} marker(s) on {3}", player, count, model, card);
        }

        public void RemoveMarker(Player player, Card card, Guid id, string name, ushort count)
        {
            // Ignore markers removed by oneself (already removed for responsiveness issues)
            if (player != Player.LocalPlayer)
            {
                Marker marker = card.FindMarker(id, name);
                if (marker == null)
                {
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, "Inconsistent state. Marker not found on card.");
                    return;
                }
                if (marker.Count < count)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, "Inconsistent state. Missing markers to remove");

                card.RemoveMarker(marker, count);
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} removes {1} {2} marker(s) from {3}", player, count, name, card);
        }

        public void TransferMarker(Player player, Card from, Card to, Guid id, string name, ushort count)
        {
            // Ignore markers moved by oneself (already moved for responsiveness issues)
            if (player != Player.LocalPlayer)
            {
                Marker marker = from.FindMarker(id, name);
                if (marker == null)
                {
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, "Inconsistent state. Marker not found on card.");
                    return;
                }
                if (marker.Count < count)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, "Inconsistent state. Missing markers to remove");

                from.RemoveMarker(marker, count);
                to.AddMarker(marker.Model, count);
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} moves {1} {2} marker(s) from {3} to {4}", player, count, name, from, to);
        }

        public void Nick(Player player, string nick)
        {
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event, "{0} is now known as {1}.", player, nick);
            player.Name = nick;
        }

        /// <summary>Reveal one card's identity</summary>
        /// <param name="card">The card, whose identity is revealed</param>
        /// <param name="revealed">Either the salted CardIdentity id (in the case of an alias), or the salted, condensed Card GUID.</param>
        /// <param name="guid"> </param>
        public void Reveal(Card card, ulong revealed, Guid guid)
        {
            // Save old id
            CardIdentity oldType = card.Type;
            // Check if the card is rightfully revealed
            if (!card.Type.Revealing)
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "Someone tries to reveal a card which is not visible to everybody.");
            // Check if we can trust other clients
            if (!card.Type.MySecret)
            {
                if (guid != Guid.Empty && (uint)revealed != guid.Condense())
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[Reveal] Alias and id aren't the same. One client is buggy or tries to cheat.");
                if (Crypto.ModExp(revealed) != card.Type.Key)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[Reveal] Card identity doesn't match. One client is buggy or tries to cheat.");
            }
            else
                card.Type.MySecret = false;
            // Reveal an alias
            if (guid == Guid.Empty)
            {
                // Find the new type
                CardIdentity newId = CardIdentity.Find((int)revealed);
                // HACK: it is unclear to me how the CardIdentity could not be found and newId ends up null
                // see this bug report: https://octgn.16bugs.com/projects/3602/bugs/192070
                // for now I'm just doing nothing (supposing that it means the type was already revealed).
                if (newId == null) { card.Reveal(); return; }
                // Possibly copy the model, if it was known and isn't anymore
                // (possible when the alias has beeen locally revealed)
                if (newId.Model == null) newId.Model = card.Type.Model;
                // Set the new type
                card.Type = newId;
                // Delete the old identity
                CardIdentity.Delete(oldType.Id);
                // Possibly reveal the alias further
                card.Reveal();
                // Raise a notification
                oldType.OnRevealed(newId);
            }
            // Reveal a card's type
            else if (card.Type.Model == null)
            {
                card.SetModel(Program.GameEngine.Definition.GetCardById(guid));
                // Raise a notification
                oldType.OnRevealed(oldType);
            }
        }

        /// <summary>Reveal a card's identity to one player only.</summary>
        /// <param name="players"> </param>
        /// <param name="card">The card, whose identity is revealed.</param>
        /// <param name="encrypted">Either a ulong[2] containing an encrypted aliased CardIdentity id. Or a ulong[5] containing an encrypted CardModel guid.</param>
        public void RevealTo(Player[] players, Card card, ulong[] encrypted)
        {
            var oldType = card.Type;
            ulong alias = 0;
            Guid id = Guid.Empty;
            players = players.Where(x => x != null).ToArray();
            switch (encrypted.Length)
            {
                case 2:
                    alias = Crypto.Decrypt(encrypted);
                    break;
                case 5:
                    id = Crypto.DecryptGuid(encrypted);
                    break;
                default:
                    Program.TraceWarning("[RevealTo] Invalid data received.");
                    return;
            }

            if (!players.All(p => (card.Group.Visibility == DataNew.Entities.GroupVisibility.Custom && card.Group.Viewers.Contains(p)) ||
                                  card.PlayersLooking.Contains(p) || card.PeekingPlayers.Contains(p)))
                Program.TraceWarning("[RevealTo] Revealing a card to a player, who isn't allowed to see it. This indicates a bug or cheating.");

            // If it's an alias, we must revealed it to the final recipient
            bool sendToMyself = true;
            if (alias != 0)
            {
                sendToMyself = false;
                CardIdentity ci = CardIdentity.Find((int)alias);
                if (ci == null)
                { Program.TraceWarning("[RevealTo] Identity not found."); return; }

                // If the revealed type is an alias, pass it to the one who owns it to continue the RevealTo chain.
                //if (ci.Alias)
                //{
                //    Player p = Player.Find((byte)(ci.Key >> 16));
                //    Program.Client.Rpc.RevealToReq(p, players, card, Crypto.Encrypt(ci.Key, p.PublicKey));
                //}
                // Else revealed the card model to the ones, who must see it
                //else
                //{
                    Player[] pArray = new Player[1];
                    foreach (Player p in players)
                        if (p != Player.LocalPlayer)
                        {
                            pArray[0] = p;
                            Program.Client.Rpc.RevealToReq(p, pArray, card, Crypto.EncryptGuid(ci.Model.Id, p.PublicKey));
                        }
                        else
                        {
                            sendToMyself = true;
                            id = ci.Model.Id;
                        }
                //}
            }
            // Else it's a type and we are the final recipients
            if (!sendToMyself) return;
            if (card.Type.Model == null)
                card.SetModel(Program.GameEngine.Definition.GetCardById(id));
            // Raise a notification
            oldType.OnRevealed(card.Type);
        }

        public void Peek(Player player, Card card)
        {
            if (!card.PeekingPlayers.Contains(player))
                card.PeekingPlayers.Add(player);
            card.RevealTo(Enumerable.Repeat(player, 1));
            if (player != Player.LocalPlayer)
            {
                Program.TracePlayerEvent(player, "{0} peeks at a card ({1}).", player,
                  card.Group is Table ? "on table" : "in " + card.Group.FullName);
            }
        }

        public void Untarget(Player player, Card card)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, null, false).Do();
        }

        public void Target(Player player, Card card)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, null, true).Do();
        }

        public void TargetArrow(Player player, Card card, Card otherCard)
        {
            // Ignore the card we targeted ourselves
            if (player == Player.LocalPlayer) return;
            new Target(player, card, otherCard, true).Do();
        }

        public void Highlight(Card card, Color? color)
        { card.SetHighlight(color); }

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
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), visible ? "{0} shows {1} to everybody." : "{0} shows {1} to nobody.", player, group);
        }

        public void GroupVisAdd(Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.AddViewer(whom, false);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} shows {1} to {2}.", player, group, whom);
        }

        public void GroupVisRemove(Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.RemoveViewer(whom, false);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), "{0} hides {1} from {2}.", player, group, whom);
        }

        public void LookAt(Player player, int uid, Group group, bool look)
        {
            if (look)
            {
                if (group.Visibility != DataNew.Entities.GroupVisibility.Everybody)
                    foreach (Card c in group)
                    {
                        c.PlayersLooking.Add(player);
                        c.RevealTo(Enumerable.Repeat(player, 1));
                    }
                group.LookedAt.Add(uid, group.ToList());
                Program.TracePlayerEvent(player, "{0} looks at {1}.", player, group);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uid))
                { Program.TraceWarning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                if (group.Visibility != DataNew.Entities.GroupVisibility.Everybody)
                {
                    foreach (Card c in group.LookedAt[uid])
                        c.PlayersLooking.Remove(player);
                }
                group.LookedAt.Remove(uid);
                Program.TracePlayerEvent(player, "{0} stops looking at {1}.", player, group);
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
                    c.RevealTo(Enumerable.Repeat(player, 1));
                }
                group.LookedAt.Add(uid, cards.ToList());
                Program.TracePlayerEvent(player, "{0} looks at {1} top {2} cards.", player, group, count);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uid))
                { Program.TraceWarning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                foreach (Card c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.TracePlayerEvent(player, "{0} stops looking at {1} top {2} cards.", player, group, count);
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
                    c.RevealTo(Enumerable.Repeat(player, 1));
                }
                group.LookedAt.Add(uid, cards.ToList());
                Program.TracePlayerEvent(player, "{0} looks at {1} bottom {2} cards.", player, group, count);
            }
            else
            {
                if (!group.LookedAt.ContainsKey(uid))
                { Program.TraceWarning("[LookAtTop] Protocol violation: unknown unique id received."); return; }
                foreach (Card c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.TracePlayerEvent(player, "{0} stops looking at {1} bottom {2} cards.", player, group, count);
                group.LookedAt.Remove(uid);
            }
        }

        public void StartLimited(Player player, Guid[] packs)
        {
            Program.TracePlayerEvent(player, "{0} starts a limited game.", player);
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
                Program.TracePlayerEvent(player, "{0} added {1} to their pool.", player, packNames);
            }
            else if (selfOnly && player == Player.LocalPlayer)
            {
                Program.TracePlayerEvent(player, "{0} added {1} to their pool.", player, packNames);
                wnd.OpenPacks(packs);
            }
            else
            {
                Program.TracePlayerEvent(player, "{0} added {1} to the limited game for all players.", player, packNames);
                wnd.OpenPacks(packs);
            }
        }

        public void CancelLimited(Player player)
        {
            Program.TracePlayerEvent(player, "{0} cancels out of the limited game.", player);
        }

        public void PlayerSetGlobalVariable(Player p, string name, string value)
        {
            string oldValue = null;
            if (p.GlobalVariables.ContainsKey(name))
            {
                oldValue = p.GlobalVariables[name];
                p.GlobalVariables[name] = value;
            }
            else
            {
                p.GlobalVariables.Add(name, value);
            }
            Program.GameEngine.EventProxy.OnPlayerGlobalVariableChanged(p, name, oldValue, value);
        }

        public void SetGlobalVariable(string name, string value)
        {
            string oldValue = null;
            if (Program.GameEngine.GlobalVariables.ContainsKey(name))
            {
                oldValue = Program.GameEngine.GlobalVariables[name];
                Program.GameEngine.GlobalVariables[name] = value;
            }
            else
            {
                Program.GameEngine.GlobalVariables.Add(name, value);
            }
            Program.GameEngine.EventProxy.OnGlobalVariableChanged(name, oldValue, value);

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
            Program.TracePlayerEvent(player, "{0} is ready", player.Name);
            if (player.WaitingOnPlayers == false)
            {
                Program.TracePlayerEvent(Player.LocalPlayer, "Unlocking game");
                Program.GameEngine.EventProxy.OnTableLoad();
                Program.GameEngine.EventProxy.OnGameStart();
            }
        }

        public void PlayerState(Player player, byte b)
        {
            player.State = (PlayerState)b;
        }

        public void RemoteCall(Player fromplayer, string func, string args)
        {
            Program.TracePlayerEvent(fromplayer, "{0} executes {1}", fromplayer, func);
            Program.GameEngine.ExecuteRemoteCall(fromplayer,func,args);
        }

        public void CreateAliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            Program.TraceWarning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void ShuffleDeprecated(Group arg0, int[] ints)
        {
            Program.TraceWarning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void UnaliasGrpDeprecated(Group arg0)
        {
            Program.TraceWarning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void UnaliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            Program.TraceWarning("[" + MethodInfo.GetCurrentMethod().Name + "] is deprecated");
        }

        public void GameState(Player fromPlayer, string strstate)
        {
            Log.DebugFormat("GameState From {0}", fromPlayer);
            var state = JsonConvert.DeserializeObject<GameSaveState>(strstate);

            state.Load(Program.GameEngine, fromPlayer);

            Program.TracePlayerEvent(fromPlayer, "{0} sent game state ", fromPlayer.Name);
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
            Program.TracePlayerEvent(player, "{0} deletes {1}", player.Name, card.Name);
            if (player != Player.LocalPlayer)
                card.Group.Remove(card);
        }

        public void PlayerDisconnect(Player player)
        {
            Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "{0} disconnected, please wait. If they do not reconnect within 1 minute they will be booted.", player);
            player.Ready = false;
        }
    }
}