using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Octgn.Data;
using Octgn.Play;
using Octgn.Play.Actions;
using Octgn.Play.Dialogs;

namespace Octgn.Networking
{
    internal sealed class Handler
    {
        private readonly BinaryParser _binParser;
        private readonly XmlParser _xmlParser;

        public Handler()
        {
            _xmlParser = new XmlParser(this);
            _binParser = new BinaryParser(this);
        }

        public void ReceiveMessage(string msg)
        {
            // Fix: because ReceiveMessage is called through the Dispatcher queue, we may be called
            // just after the Client has already been closed. In that case we should simply drop the message
            // (otherwise NRE may occur)
            if (Program.Client == null) return;

            try
            {
                _xmlParser.Parse(msg);
            }
            finally
            {
                if (Program.Client != null) Program.Client.Muted = 0;
            }
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
            finally
            {
                if (Program.Client != null) Program.Client.Muted = 0;
            }
        }

        public void Binary()
        {
            Program.Trace.TraceEvent(TraceEventType.Verbose, EventIds.NonGame, "Switched to binary protocol.");
            Program.Client.Binary();
        }

        public void Error(string msg)
        {
            Program.Trace.TraceEvent(TraceEventType.Error, EventIds.NonGame, "The server has returned an error: {0}",
                                     msg);
            Program.OnServerError(msg);
        }

        public void Start()
        {
            Program.ClientWindow.StartGame();
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
            Program.Game.Reset();
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                     "{0} resets the game.", player);
        }

        public void NextTurn(Player player)
        {
            Program.Game.TurnNumber++;
            Program.Game.TurnPlayer = player;
            Program.Game.StopTurn = false;
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Turn, "Turn {0}: {1}", Program.Game.TurnNumber,
                                     player);
        }

        public void StopTurn(Player player)
        {
            if (player == Player.LocalPlayer)
                Program.Game.StopTurn = false;
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                     "{0} wants to play before end of turn.", player);
        }

        public void Chat(Player player, string text)
        {
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Chat | EventIds.PlayerFlag(player),
                                     "<{0}> {1}", player, text);
        }

        public void Print(Player player, string text)
        {
            Program.Print(player, text);
        }

        public void Random(Player player, int id, int min, int max)
        {
            var req = new RandomRequest(player, id, min, max);
            Program.Game.RandomRequests.Add(req);
            req.Answer1();
        }

        public void RandomAnswer1(Player player, int id, ulong value)
        {
            RandomRequest req = Program.Game.FindRandomRequest(id);
            if (req == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                         "[RandomAnswer1] Random request not found.");
                return;
            }
            if (req.IsPhase1Completed())
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                         "[RandomAnswer1] Too many values received. One client is buggy or tries to cheat.");
                return;
            }
            req.AddAnswer1(player, value);
            if (req.IsPhase1Completed())
                req.Answer2();
        }

        public void RandomAnswer2(Player player, int id, ulong value)
        {
            RandomRequest req = Program.Game.FindRandomRequest(id);
            if (req == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                         "[RandomAnswer1] Random request not found.");
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

        public void Welcome(byte id)
        {
            Player.LocalPlayer.Id = id;
            Player.LocalPlayer.SetPlayerColor(id);
            Program.Client.StartPings();
        }

        public void NewPlayer(byte id, string nick, ulong pkey)
        {
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event, "{0} has joined the game.", nick);
            var player = new Player(Program.Game.Definition, nick, id, pkey);
            // Define the default table side if we are the host
            //TODO All references to player list should be hosted on server.
            //And all decisions based on players should be hosted there aswell.
            if (Program.IsHost)
                player.InvertedTable = (Player.AllExceptGlobal.Count() & 1) == 0;
        }

        /// <summary>
        ///   Loads a player deck.
        /// </summary>
        /// <param name="id"> An array containing the loaded CardIdentity ids. </param>
        /// <param name="type"> An array containing the corresponding CardModel guids (encrypted). </param>
        /// <param name="group"> An array indicating the group the cards must be loaded into. </param>
        public void LoadDeck(int[] id, ulong[] type, Group[] group)
        {
            if (id.Length != type.Length || id.Length != group.Length)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                         "[LoadDeck] Protocol violation: inconsistent arrays sizes.");
                return;
            }

            if (id.Length == 0) return; // Loading an empty deck --> do nothing

            Player who = Player.Find((byte) (id[0] >> 16));
            if (who == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[LoadDeck] Player not found.");
                return;
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(who),
                                     "{0} loads a deck.", who);
            CreateCard(id, type, group);
        }

        /// <summary>
        ///   Creates new Cards as well as the corresponding CardIdentities. The cards may be in different groups.
        /// </summary>
        /// <param name="id"> An array with the new CardIdentity ids. </param>
        /// <param name="type"> An array containing the corresponding CardModel guids (encrypted) </param>
        /// <param name="groups"> An array indicating the group the cards must be loaded into. </param>
        /// <seealso cref="CreateCard(int[], ulong[], Group)">for a more efficient way to insert cards inside one group.</seealso>
        private static void CreateCard(IList<int> id, IList<ulong> type, IList<Group> groups)
        {
            Player owner = Player.Find((byte) (id[0] >> 16));
            if (owner == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCard] Player not found.");
                return;
            }
            // Ignore cards created by oneself
            if (owner == Player.LocalPlayer) return;
            for (int i = 0; i < id.Count; i++)
            {
                var c = new Card(owner, id[i], type[i], Program.Game.Definition.CardDefinition, null, false);
                Group group = groups[i];
                group.AddAt(c, group.Count);
            }
        }

        /// <summary>
        ///   Creates new Cards as well as the corresponding CardIdentities. All cards are created in the same group.
        /// </summary>
        /// <param name="id"> An array with the new CardIdentity ids. </param>
        /// <param name="type"> An array containing the corresponding CardModel guids (encrypted) </param>
        /// <param name="group"> The group, in which the cards are added. </param>
        public void CreateCard(int[] id, ulong[] type, Group group)
        {
            Player owner = Player.Find((byte) (id[0] >> 16));
            if (owner == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCard] Player not found.");
                return;
            }
            // Ignore cards created by oneself
            if (owner == Player.LocalPlayer) return;
            for (int i = 0; i < id.Length; i++)
            {
                var c = new Card(owner, id[i], type[i], Program.Game.Definition.CardDefinition, null, false);
                group.AddAt(c, group.Count);
            }
        }

        public void SwitchWithAlternate(Card c)
        {
            c.SwitchWithAlternate();
        }
        public void IsAlternateImage(Card c, bool isAlternateImage)
        {
            c.IsAlternateImage = isAlternateImage;
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            if (id.Length == 0)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCardAt] Empty id parameter.");
                return;
            }
            if (id.Length != key.Length || id.Length != x.Length || id.Length != y.Length || id.Length != modelId.Length)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                         "[CreateCardAt] Inconsistent parameters length.");
                return;
            }
            Player owner = Player.Find((byte) (id[0] >> 16));
            if (owner == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[CreateCardAt] Player not found.");
                return;
            }
            Table table = Program.Game.Table;
            // Bring cards created by oneself to top, for z-order consistency
            if (owner == Player.LocalPlayer)
            {
                for (int i = id.Length - 1; i >= 0; --i)
                {
                    Card card = Card.Find(id[i]);
                    if (card == null)
                    {
                        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                                 "[CreateCardAt] Card not found.");
                        return;
                    }
                    table.SetCardIndex(card, table.Count + i - id.Length);
                }
            }
            else
            {
                for (int i = 0; i < id.Length; i++)
                    new CreateCard(owner, id[i], key[i], faceUp, faceUp ? Database.GetCardById(modelId[i]) : null, x[i],
                                   y[i], !persist).Do();
            }

            // Display log messages
            if (modelId.All(m => m == modelId[0]))
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(owner),
                                         "{0} creates {1} '{2}'", owner, modelId.Length,
                                         Database.GetCardById(modelId[0]));
            else
                foreach (Guid m in modelId)
                    Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(owner),
                                             "{0} creates a '{1}'", owner, Database.GetCardById(m));
        }

        /// <summary>
        ///   Create new CardIdentities, which hide aliases to other CardIdentities
        /// </summary>
        /// <param name="id"> An array containing the new CardIdentity ids </param>
        /// <param name="type"> An array with the aliased CardIdentity ids (encrypted) </param>
        public void CreateAlias(int[] id, ulong[] type)
        {
            var playerId = (byte) (id[0] >> 16);
            // Ignore cards created by oneself
            if (playerId == Player.LocalPlayer.Id) return;
            for (int i = 0; i < id.Length; i++)
            {
                if (type[i] == ulong.MaxValue) continue;
                // TODO: Why did we define then not use this?
                // var ci = new CardIdentity(id[i]) {Alias = true, Key = type[i]};
            }
        }

        public void Leave(Player player)
        {
            player.Delete();
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event, "{0} has left the game.", player);
        }

        public void MoveCard(Player player, Card card, Group to, int idx, bool faceUp)
        {
            // Ignore cards moved by the local player (already done, for responsiveness)
            if (player != Player.LocalPlayer)
                new MoveCard(player, card, to, idx, faceUp).Do();
            else
            {
                // Fix: cards may move quickly locally from one group to another one, before we get a chance
                // to execute this handler, during game actions scripts (e.g. Mulligan with one player -
                // shuffling happens locally). The result is that we are going to receive two messages,
                // one for the move to group A, then the move to group B; while the card already is in group B.
                // In this case, trying to set index inside group B with an index coming from the time the card
                // was in group A is just plain wrong and may crash depending on the index.
                if (card.Group == to)
                    card.SetIndex(idx);
                // This is done to preserve stack order consistency with other players (should be a noop most of the time)
            }
        }

        public void MoveCardAt(Player player, Card card, int x, int y, int idx, bool faceUp)
        {
            // Get the table control
            Table table = Program.Game.Table;
            // Because every player may manipulate the table at the same time, the index may be out of bound
            if (card.Group == table)
            {
                if (idx >= table.Count) idx = table.Count - 1;
            }
            else if (idx > table.Count) idx = table.Count;

            // Ignore cards moved by the local player (already done, for responsiveness)
            if (player == Player.LocalPlayer)
            {
                // See remark in MoveCard
                if (card.Group == table)
                    card.SetIndex(idx);
                // This is done to preserve stack order consistency with other players (should be a noop most of the time)
                return;
            }
            // Find the old position on the table, if any
            //bool onTable = card.Group == table;
            //double oldX = card.X, oldY = card.Y;
            // Do the move
            new MoveCard(player, card, x, y, idx, faceUp).Do();
        }

        public void SetMarker(Player player, Card card, Guid id, string name, ushort count)
        {
            // Always perform this call (even if player == LocalPlayer) for consistency as markers aren't an exclusive resource
            card.SetMarker(player, id, name, count);
        }

        public void AddMarker(Player player, Card card, Guid id, string name, ushort count)
        {
            MarkerModel model = Program.Game.GetMarkerModel(id);
            var defaultMarkerModel = model as DefaultMarkerModel;
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
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame,
                                             "Inconsistent state. Marker not found on card.");
                    return;
                }
                if (marker.Count < count)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame,
                                             "Inconsistent state. Missing markers to remove");

                card.RemoveMarker(marker, count);
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                     "{0} removes {1} {2} marker(s) from {3}", player, count, name, card);
        }

        public void TransferMarker(Player player, Card from, Card to, Guid id, string name, ushort count)
        {
            // Ignore markers moved by oneself (already moved for responsiveness issues)
            if (player != Player.LocalPlayer)
            {
                Marker marker = from.FindMarker(id, name);
                if (marker == null)
                {
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame,
                                             "Inconsistent state. Marker not found on card.");
                    return;
                }
                if (marker.Count < count)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame,
                                             "Inconsistent state. Missing markers to remove");

                from.RemoveMarker(marker, count);
                to.AddMarker(marker.Model, count);
            }
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                     "{0} moves {1} {2} marker(s) from {3} to {4}", player, count, name, from, to);
        }

        public void Nick(Player player, string nick)
        {
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event, "{0} is now known as {1}.", player,
                                     nick);
            player.Name = nick;
        }

        /// <summary>
        ///   Reveal one card's identity
        /// </summary>
        /// <param name="card"> The card, whose identity is revealed </param>
        /// <param name="revealed"> Either the salted CardIdentity id (in the case of an alias), or the salted, condensed Card GUID. </param>
        /// <param name="guid"> If the revealed identity is a model, the non-condensed CardModel guid. Otherwise this parameter should be Guid.Empty. </param>
        public void Reveal(Card card, ulong revealed, Guid guid)
        {
            // Save old id
            CardIdentity oldType = card.Type;
            // Check if the card is rightfully revealed
            if (!card.Type.Revealing)
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                         "Someone tries to reveal a card which is not visible to everybody.");
            // Check if we can trust other clients
            if (!card.Type.MySecret)
            {
                if (guid != Guid.Empty && (uint) revealed != guid.Condense())
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                             "[Reveal] Alias and id aren't the same. One client is buggy or tries to cheat.");
                if (Crypto.ModExp(revealed) != card.Type.Key)
                    Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                             "[Reveal] Card identity doesn't match. One client is buggy or tries to cheat.");
            }
            else
                card.Type.MySecret = false;
            // Reveal an alias
            if (guid == Guid.Empty)
            {
                // Find the new type
                CardIdentity newId = CardIdentity.Find((int) revealed);
                // HACK: it is unclear to me how the CardIdentity could not be found and newId ends up null
                // see this bug report: https://octgn.16bugs.com/projects/3602/bugs/192070
                // for now I'm just doing nothing (supposing that it means the type was already revealed).
                if (newId == null)
                {
                    card.Reveal();
                    return;
                }
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
                card.SetModel(Database.GetCardById(guid));
                // Raise a notification
                oldType.OnRevealed(oldType);
            }
        }

        public void RevealTo(Player[] players, Card card, ulong[] encrypted)
        {
            CardIdentity oldType = card.Type;
            ulong alias = 0;
            Guid id = Guid.Empty;

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

            if (!players.All(p => (card.Group.Visibility == GroupVisibility.Custom && card.Group.Viewers.Contains(p)) ||
                                  card.PlayersLooking.Contains(p) || card.PeekingPlayers.Contains(p)))
                Program.TraceWarning(
                    "[RevealTo] Revealing a card to a player, who isn't allowed to see it. This indicates a bug or cheating.");

            // If it's an alias, we must revealed it to the final recipient
            bool sendToMyself = true;
            if (alias != 0)
            {
                sendToMyself = false;
                CardIdentity ci = CardIdentity.Find((int) alias);
                if (ci == null)
                {
                    Program.TraceWarning("[RevealTo] Identity not found.");
                    return;
                }

                // If the revealed type is an alias, pass it to the one who owns it to continue the RevealTo chain.
                if (ci.Alias)
                {
                    Player p = Player.Find((byte) (ci.Key >> 16));
                    Program.Client.Rpc.RevealToReq(p, players, card, Crypto.Encrypt(ci.Key, p.PublicKey));
                }
                    // Else revealed the card model to the ones, who must see it
                else
                {
                    var pArray = new Player[1];
                    foreach (Player p in players)
                        if (p != Player.LocalPlayer)
                        {
                            pArray[0] = p;
                            Program.Client.Rpc.RevealToReq(p, pArray, card, Crypto.Encrypt(ci.Model.Id, p.PublicKey));
                        }
                        else
                        {
                            sendToMyself = true;
                            id = ci.Model.Id;
                        }
                }
            }
            // Else it's a type and we are the final recipients
            if (!sendToMyself) return;
            if (card.Type.Model == null)
                card.SetModel(Database.GetCardById(id));
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
                // TODO: Better indicate which card is being peeked
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
        {
            card.SetHighlight(color);
        }

        public void Turn(Player player, Card card, bool up)
        {
            // Ignore the card we turned ourselves
            if (player == Player.LocalPlayer)
            {
                card.MayBeConsideredFaceUp = false; // see comment on mayBeConsideredFaceUp
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

        /// <summary>
        ///   Part of a shuffle process.
        /// </summary>
        /// <param name="group"> The group being shuffled. </param>
        /// <param name="card"> An array containing the CardIdentity ids to shuffle. </param>
        public void Shuffle(Group group, int[] card)
        {
            // Array to hold the new aliases (sent to CreateAlias)
            var aliases = new ulong[card.Length];
            // Intialize the group shuffle
            group.FilledShuffleSlots = 0;
            group.HasReceivedFirstShuffledMessage = false;
            group.MyShufflePos = new short[card.Length];
            // Check if we received enough cards
            if (card.Length < group.Count/(Player.Count - 1))
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event, "[Shuffle] Too few cards received.");
            // Do the shuffling
            var rnd = new Random();
            for (int i = card.Length - 1; i >= 0; i--)
            {
                int r = rnd.Next(i);
                int tc = card[r];
                card[r] = card[i];
                // Create a new alias, if the card is not face up
                CardIdentity ci = CardIdentity.Find(tc);
                if (group.FindByCardIdentity(ci) != null)
                {
                    card[i] = tc;
                    aliases[i] = ulong.MaxValue;
                    ci.Visible = true;
                }
                else
                {
                    ci = new CardIdentity(Program.Game.GenerateCardId());
                    ci.MySecret = ci.Alias = true;
                    ci.Key = ((ulong) Crypto.PositiveRandom()) << 32 | (uint) tc;
                    card[i] = ci.Id;
                    aliases[i] = Crypto.ModExp(ci.Key);
                    ci.Visible = false;
                }
                // Give a random position to the card
                // TODO: I don't think this shuffling algorithm generates all possibilities equiprobably
                group.MyShufflePos[i] = (short) Crypto.Random(group.Count);
            }
            // Send the results
            Program.Client.Rpc.CreateAlias(card, aliases);
            Program.Client.Rpc.Shuffled(group, card, group.MyShufflePos);
        }

        public void Shuffled(Group group, int[] card, short[] pos)
        {
            // Check the args
            if (card.Length != pos.Length)
            {
                Program.TraceWarning("[Shuffled] Cards and positions lengths don't match.");
                return;
            }
            group.FilledShuffleSlots += card.Length;
            if (group.FilledShuffleSlots > group.Count)
            {
                Program.TraceWarning("[Shuffled] Too many card positions received.");
                return;
            }
            // If it's the first packet we receive for this shuffle, clear all Types
            if (!group.HasReceivedFirstShuffledMessage)
                foreach (Card c in group) c.Type = null;
            group.HasReceivedFirstShuffledMessage = true;
            // Check that the server didn't change our positions
            if (card[0] >> 16 == Player.LocalPlayer.Id && group.MyShufflePos != null)
            {
                if (pos.Where((t, i) => t != @group.MyShufflePos[i]).Any())
                {
                    Program.TraceWarning("[Shuffled] The server has changed the order of the cards.");
                }
                group.MyShufflePos = null;
            }
            // Insert the cards
            for (int j = 0; j < card.Length; j++)
            {
                // Get the wished position
                int i = pos[j];
                // Get the card
                CardIdentity ci = CardIdentity.Find(card[j]);
                if (ci == null)
                {
                    Program.TraceWarning("[Shuffled] Card not found.");
                    continue;
                }
                // Check if the slot is free, otherwise choose the first free one
                if (group[i].Type != null) i = group.FindNextFreeSlot(i);
                // Set the type
                group[i].Type = ci;
                group[i].SetVisibility(ci.Visible ? GroupVisibility.Everybody : GroupVisibility.Nobody, null);
            }
            if (group.FilledShuffleSlots == group.Count)
                group.OnShuffled();
        }

        /// <summary>
        ///   Completely remove all aliases from a group, e.g. before performing a shuffle.
        /// </summary>
        /// <param name="group"> The group to remove all aliases from. </param>
        public void UnaliasGrp(Group group)
        {
            // Get the group
            var g = group as Pile;
            if (g == null)
            {
                Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, "[UnaliasGrp] Group is not a pile.");
                return;
            }
            // Collect aliases which we p
            var cards = new List<int>(g.Count);
            var types = new List<ulong>(g.Count);
            bool hasAlias = false;
            foreach (Card t in g)
            {
                CardIdentity ci = t.Type;
                if (!ci.Alias) continue;
                hasAlias = true;
                if (!ci.MySecret) continue;
                cards.Add(t.Id);
                types.Add(ci.Key);
            }
            // Unalias cards that we know (if any)
            if (cards.Count > 0)
                Program.Client.Rpc.Unalias(cards.ToArray(), types.ToArray());
            // If there are no alias, we may be ready to shuffle
            if (!hasAlias && g.WantToShuffle)
            {
                g.DoShuffle();
                return;
            }
            // Mark the group for shuffling
            g.PreparingShuffle = true;
            // Notify the user
            Program.TracePlayerEvent(group.Owner, "{0} is being prepared for shuffle.", g);
            // Check for null because the chat can currently be muted (e.g. during a Mulligan scripted action)
            if (Program.LastChatTrace != null)
                g.ShuffledTrace += (new ShuffleTraceChatHandler {Line = Program.LastChatTrace}).ReplaceText;
        }

        /// <summary>
        ///   Unalias some Cards, e.g. before a shuffle
        /// </summary>
        /// <param name="card"> An array containing the Card ids to unalias. </param>
        /// <param name="type"> An array containing the corresponding revealed CardIdentity ids. </param>
        public void Unalias(int[] card, ulong[] type)
        {
            if (card.Length != type.Length)
            {
                Program.TraceWarning("[Unalias] Card and type lengths don't match.");
                return;
            }
            Pile g = null;
            var cards = new List<int>(card.Length);
            var types = new List<ulong>(card.Length);
            for (int i = 0; i < card.Length; i++)
            {
                Card c = Card.Find(card[i]);
                if (c == null)
                {
                    Program.TraceWarning("[Unalias] Card not found.");
                    continue;
                }
                if (g == null) g = c.Group as Pile;
                else if (g != c.Group)
                {
                    Program.TraceWarning("[Unalias] Not all cards belong to the same group!");
                    continue;
                }
                // Check nobody cheated
                if (!c.Type.MySecret)
                {
                    if (c.Type.Key != Crypto.ModExp(type[i]))
                        Program.TraceWarning("[Unalias] Card identity doesn't match.");
                }
                // Substitue the card's identity
                CardIdentity ci = CardIdentity.Find((int) type[i]);
                if (ci == null)
                {
                    Program.TraceWarning("[Unalias] Card identity not found.");
                    continue;
                }
                CardIdentity.Delete(c.Type.Id);
                c.Type = ci;
                // Propagate unaliasing
                if (ci.Alias && ci.MySecret)
                    cards.Add(c.Id);
                types.Add(ci.Key);
            }
            if (cards.Count > 0)
                Program.Client.Rpc.Unalias(cards.ToArray(), types.ToArray());
            if (g != null && !g.PreparingShuffle)
            {
                Program.TraceWarning("[Unalias] Cards revealed are not in a group prepared for shuffle.");
                return;
            }
            // If all cards are now revealed, one can proceed to shuffling
            if (g == null || !g.WantToShuffle) return;
            bool done = false;
            for (int i = 0; !done && i < g.Count; i++)
                done = g[i].Type.Alias;
            if (!done)
                g.DoShuffle();
        }

        public void PassTo(Player who, ControllableObject obj, Player player, bool requested)
        {
            // Ignore message that we sent in the first place
            if (who != Player.LocalPlayer)
                obj.PassControlTo(player, who, false, requested);
        }

        public void TakeFrom(ControllableObject obj, Player to)
        {
            obj.TakingControl(to);
        }

        public void DontTake(ControllableObject obj)
        {
            obj.DontTakeError();
        }

        public void FreezeCardsVisibility(Group group)
        {
            foreach (Card c in group.Cards) c.SetOverrideGroupVisibility(true);
        }

        public void GroupVis(Player player, Group group, bool defined, bool visible)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.SetVisibility(defined ? (bool?) visible : null, false);
            if (defined)
                Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                         visible ? "{0} shows {1} to everybody." : "{0} shows {1} to nobody.", player,
                                         group);
        }

        public void GroupVisAdd(Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.AddViewer(whom, false);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                     "{0} shows {1} to {2}.", player, group, whom);
        }

        public void GroupVisRemove(Player player, Group group, Player whom)
        {
            // Ignore messages sent by myself
            if (player != Player.LocalPlayer)
                group.RemoveViewer(whom, false);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player),
                                     "{0} hides {1} from {2}.", player, group, whom);
        }

        public void LookAt(Player player, int uid, Group group, bool look)
        {
            if (look)
            {
                if (group.Visibility != GroupVisibility.Everybody)
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
                {
                    Program.TraceWarning("[LookAtTop] Protocol violation: unknown unique id received.");
                    return;
                }
                if (group.Visibility != GroupVisibility.Everybody)
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
                IEnumerable<Card> cards = group.Take(count);
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
                {
                    Program.TraceWarning("[LookAtTop] Protocol violation: unknown unique id received.");
                    return;
                }
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
                IEnumerable<Card> cards = group.Skip(skipCount);
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
                {
                    Program.TraceWarning("[LookAtTop] Protocol violation: unknown unique id received.");
                    return;
                }
                foreach (Card c in group.LookedAt[uid])
                    c.PlayersLooking.Remove(player);
                Program.TracePlayerEvent(player, "{0} stops looking at {1} bottom {2} cards.", player, group, count);
                group.LookedAt.Remove(uid);
            }
        }

        public void StartLimited(Player player, Guid[] packs)
        {
            Program.TracePlayerEvent(player, "{0} starts a limited game.", player);
            var wnd = new PickCardsDialog();
            wnd.OpenPacks(packs);
            //fix MAINWINDOW bug
            Program.PlayWindow.ShowBackstage(wnd);
        }

        public void CancelLimited(Player player)
        {
            Program.TracePlayerEvent(player, "{0} cancels out of the limited game.", player);
        }

        public void PlayerSetGlobalVariable(Player fromp, Player p, string name, string value)
        {
            if (fromp.Id != p.Id) return;
            if (p.GlobalVariables.ContainsKey(name))
                p.GlobalVariables[name] = value;
            else
                p.GlobalVariables.Add(name, value);
        }

        public void SetGlobalVariable(string name, string value)
        {
            if (Program.Game.GlobalVariables.ContainsKey(name))
                Program.Game.GlobalVariables[name] = value;
            else
                Program.Game.GlobalVariables.Add(name, value);
        }
    }
}