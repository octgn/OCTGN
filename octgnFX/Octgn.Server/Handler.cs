using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace Octgn.Server
{
    internal sealed class Handler
    {
        #region Statics

        private const string ServerName = "OCTGN.NET";
        private static Version ServerVersion = GetServerVersion();

        private static Version GetServerVersion()
        {
            Assembly asm = typeof(Server).Assembly;
            AssemblyProductAttribute at = (AssemblyProductAttribute)asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
            return asm.GetName().Version;
        }

        #endregion Statics

        #region Private fields

        private readonly XmlParser xmlParser;       // Parser for xml messages
        private readonly BinaryParser binParser;    // Parser for binary messages
        // List of connected clients, keyed by underlying socket
        private readonly Dictionary<TcpClient, PlayerInfo> clients = new Dictionary<TcpClient, PlayerInfo>();
        // List of connected clients, keyed by id
        private readonly Dictionary<byte, PlayerInfo> players = new Dictionary<byte, PlayerInfo>();
        private readonly Broadcaster broadcaster;   // Stub to broadcast messages
        private TcpClient sender = null;            // Socket on which current message was received
        private byte playerId = 1;                  // Next free player id
        private bool acceptPlayers = true;          // When false, no new players are accepted
        private int turnNumber = 0;                 // Turn number, used to validate TurnStop requests
        private Octgn.Data.GameSettings gameSettings = new Octgn.Data.GameSettings();
        private HashSet<byte> turnStopPlayers = new HashSet<byte>();
        private Server.Connection Connection;

        #endregion Private fields

        #region Internal methods

        internal int muted;
        private Guid gameId;
        private Version gameVersion;

        // C'tor
        internal Handler(Guid gameId, Version gameVersion)
        {
            this.gameId = gameId; this.gameVersion = gameVersion;
            // Init fields
            broadcaster = new Broadcaster(clients, this);
            xmlParser = new XmlParser(this);
            binParser = new BinaryParser(this);
        }

        // Show the management GUI
        //        internal void ShowGUI(System.Windows.Forms.Form parent)
        //        {
        //            ManagementForm cf = new ManagementForm(clients);
        //            cf.ShowDialog(parent);
        //        }

        // Handle an XML message
        internal void ReceiveMessage(string msg, TcpClient sender, Server.Connection con)
        {
            // Check if this is the first message received
            if (!clients.ContainsKey(sender))
            {
                // A new connection must always start with a <Hello> message.
                if (!msg.StartsWith("<Hello>", StringComparison.Ordinal))
                {
                    // Refuse the connection
                    sender.GetStream().Close(); return;
                }
            }
            // Set the sender field
            this.sender = sender;
            this.Connection = con;
            // Parse and handle the message
            xmlParser.Parse(msg);
        }

        // Handle a binary message
        internal void ReceiveMessage(byte[] data, TcpClient sender, Server.Connection con)
        {
            // Check if this is the first message received
            if (!clients.ContainsKey(sender))
            {
                // A new connection must always start with a <Hello> xml message, refuse the connection
                sender.GetStream().Close(); return;
            }
            // Set the sender field
            this.sender = sender;
            this.Connection = con;
            // Parse and handle the message
            binParser.Parse(data);
        }

        // Called when a client is unexpectedly disconnected
        internal void Disconnected(TcpClient client)
        {
            PlayerInfo info;
            // If the client is not registered, do nothing
            if (!clients.TryGetValue(client, out info)) return;
            // Remove the client from our lists
            clients.Remove(client);
            players.Remove(info.id);
            // Notify everybody that the player has left the game
            broadcaster.Leave(info.id);
        }

        #endregion Internal methods

        #region IRemoteCalls interface

        public void Binary()
        { /* This never gets called. This message gets special treatment in the server. */ }

        public void Error(string msg)
        { Debug.WriteLine(msg); }

        public void Start()
        {
            acceptPlayers = false;
            broadcaster.Start();
        }

        public void Settings(bool twoSidedTable)
        {
            gameSettings.UseTwoSidedTable = twoSidedTable;
            broadcaster.Settings(twoSidedTable);
        }

        public void PlayerSettings(byte player, bool invertedTable)
        {
            PlayerInfo p;
            // The player may have left the game concurently
            if (players.TryGetValue(player, out p))
            {
                p.invertedTable = invertedTable;
                broadcaster.PlayerSettings(player, invertedTable);
            }
        }

        public void ResetReq()
        {
            turnNumber = 0; turnStopPlayers.Clear();
            broadcaster.Reset(clients[sender].id);
        }

        public void ChatReq(string text)
        { broadcaster.Chat(clients[sender].id, text); }

        public void PrintReq(string text)
        { broadcaster.Print(clients[sender].id, text); }

        public void RandomReq(int id, int min, int max)
        { broadcaster.Random(clients[sender].id, id, min, max); }

        public void RandomAnswer1Req(int id, ulong value)
        { broadcaster.RandomAnswer1(clients[sender].id, id, value); }

        public void RandomAnswer2Req(int id, ulong value)
        { broadcaster.RandomAnswer2(clients[sender].id, id, value); }

        public void CounterReq(int counter, int value)
        { broadcaster.Counter(clients[sender].id, counter, value); }

        public void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVer)
        {
            // One should say Hello only once
            if (clients.ContainsKey(sender))
            {
                clients[sender].rpc.Error("[Hello]You may say hello only once.");
                return;
            }
            // Check if the versions are compatible
#if !DEBUG
            if(clientVer.Major != ServerVersion.Major || clientVer.Minor != ServerVersion.Minor)
            {
                XmlSenderStub rpc = new XmlSenderStub(sender, this);
                rpc.Error(string.Format("Incompatible versions. This server is accepting {0}.* clients only.", ServerVersion.ToString(2)));
                try { sender.Client.Close(); sender.Close(); }
                catch { }
                return;
            }
#endif
            // Check if we accept new players
            if (!acceptPlayers)
            {
                XmlSenderStub rpc = new XmlSenderStub(sender, this);
                rpc.Error("No more players are accepted in this game.");
                try { sender.Client.Close(); sender.Close(); }
                catch { }
                return;
            }
            // Check if the client wants to play the correct game
            if (gameId != this.gameId)
            {
                XmlSenderStub rpc = new XmlSenderStub(sender, this);
                rpc.Error(string.Format("Invalid game. This server is hosting another game (game id: {0}).", this.gameId));
                try { sender.Client.Close(); sender.Close(); }
                catch { }
                return;
            }
            // Check if the client's major game version matches ours
            if (gameVer.Major != this.gameVersion.Major)
            {
                XmlSenderStub rpc = new XmlSenderStub(sender, this);
                rpc.Error(string.Format("Incompatible game version. This server is hosting game version {0:3}.", this.gameVersion));
                try { sender.Client.Close(); sender.Close(); }
                catch { }
                return;
            }
            // Create the new endpoint
            IClientCalls senderRpc = new XmlSenderStub(sender, this);
            string software = client + " (" + clientVer.ToString() + ')';
            PlayerInfo pi = new PlayerInfo(playerId++, nick, pkey, senderRpc, software);
            // Check if one can switch to binary mode
            if (client == ServerName)
            {
                pi.rpc.Binary(); pi.rpc = senderRpc = new BinarySenderStub(sender, this);
                pi.binary = true;
            }
            // Notify everybody of the newcomer
            broadcaster.NewPlayer(pi.id, nick, pkey);
            // Add everybody to the newcomer
            foreach (PlayerInfo player in clients.Values)
                senderRpc.NewPlayer(player.id, player.nick, player.pkey);
            senderRpc.Welcome(pi.id);
            // Notify the newcomer of some shared settings
            senderRpc.Settings(gameSettings.UseTwoSidedTable);
            foreach (PlayerInfo player in players.Values.Where(p => p.invertedTable))
                senderRpc.PlayerSettings(player.id, true);
            // Add it to our lists
            clients.Add(sender, pi);
            players.Add(pi.id, pi);
            broadcaster.RefreshTypes();
        }

        public void LoadDeck(int[] id, ulong[] type, int[] group)
        {
            short playerId = clients[sender].id;
            for (int i = 0; i < id.Length; i++)
                id[i] = playerId << 16 | (id[i] & 0xffff);
            broadcaster.LoadDeck(id, type, group);
        }

        public void CreateCard(int[] id, ulong[] type, int group)
        {
            short playerId = clients[sender].id;
            for (int i = 0; i < id.Length; i++)
                id[i] = playerId << 16 | (id[i] & 0xffff);
            broadcaster.CreateCard(id, type, group);
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            short playerId = clients[sender].id;
            for (int i = 0; i < id.Length; i++)
                id[i] = playerId << 16 | (id[i] & 0xffff);
            broadcaster.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
        }

        public void CreateAlias(int[] id, ulong[] type)
        {
            short playerId = clients[sender].id;
            for (int i = 0; i < id.Length; i++)
                id[i] = playerId << 16 | (id[i] & 0xffff);
            broadcaster.CreateAlias(id, type);
        }

        public void NextTurn(byte nextPlayer)
        {
            if (turnStopPlayers.Count > 0)
            {
                byte stopPlayerId = turnStopPlayers.First();
                turnStopPlayers.Remove(stopPlayerId);
                broadcaster.StopTurn(stopPlayerId);
                return;
            }
            turnNumber++;
            broadcaster.NextTurn(nextPlayer);
        }

        public void PlayerSetGlobalVariable(byte p, string name, string value)
        {
            broadcaster.PlayerSetGlobalVariable(clients[sender].id, p, name, value);
        }
        public void SetGlobalVariable(string name, string value)
        {
            broadcaster.SetGlobalVariable(name, value);
        }
        public void StopTurnReq(int turnNumber, bool stop)
        {
            if (turnNumber != this.turnNumber) return;  // Message StopTurn crossed a NextTurn message
            var playerId = clients[sender].id;
            if (stop)
                turnStopPlayers.Add(playerId);
            else
                turnStopPlayers.Remove(playerId);
        }

        public void IsAlternateImage(int c, bool isAlternateImage)
        {
            broadcaster.IsAlternateImage(c, isAlternateImage);
        }

        public void MoveCardReq(int card, int to, int idx, bool faceUp)
        { broadcaster.MoveCard(clients[sender].id, card, to, idx, faceUp); }

        public void MoveCardAtReq(int card, int x, int y, int idx, bool faceUp)
        { broadcaster.MoveCardAt(clients[sender].id, card, x, y, idx, faceUp); }

        public void AddMarkerReq(int card, Guid id, string name, ushort count)
        { broadcaster.AddMarker(clients[sender].id, card, id, name, count); }

        public void RemoveMarkerReq(int card, Guid id, string name, ushort count)
        { broadcaster.RemoveMarker(clients[sender].id, card, id, name, count); }

        public void SetMarkerReq(int card, Guid id, string name, ushort count)
        { broadcaster.SetMarker(clients[sender].id, card, id, name, count); }

        public void TransferMarkerReq(int from, int to, Guid id, string name, ushort count)
        { broadcaster.TransferMarker(clients[sender].id, from, to, id, name, count); }

        public void NickReq(string nick)
        {
            PlayerInfo pi = clients[sender];
            pi.nick = nick;
            broadcaster.Nick(pi.id, nick);
        }

        public void Reveal(int card, ulong revealed, Guid guid)
        { broadcaster.Reveal(card, revealed, guid); }

        public void RevealToReq(byte sendTo, byte[] revealTo, int card, ulong[] encrypted)
        {
            if (encrypted.Length != 2 && encrypted.Length != 5)
                Debug.WriteLine("[RevealToReq] Invalid encrypted length.");
            players[sendTo].rpc.RevealTo(revealTo, card, encrypted);
        }

        public void PeekReq(int card)
        { broadcaster.Peek(clients[sender].id, card); }

        public void UntargetReq(int card)
        { broadcaster.Untarget(clients[sender].id, card); }

        public void TargetReq(int card)
        { broadcaster.Target(clients[sender].id, card); }

        public void TargetArrowReq(int card, int otherCard)
        { broadcaster.TargetArrow(clients[sender].id, card, otherCard); }

        public void Highlight(int card, string color)
        { broadcaster.Highlight(card, color); }

        public void TurnReq(int card, bool up)
        { broadcaster.Turn(clients[sender].id, card, up); }

        public void RotateReq(int card, CardOrientation rot)
        { broadcaster.Rotate(clients[sender].id, card, rot); }

        public void Shuffle(int group, int[] card)
        {
            // Special case: solo playing
            if (clients.Count == 1)
            {
                clients[sender].rpc.Shuffle(group, card);
                return;
            }
            // Normal case
            int nCards = card.Length / (clients.Count - 1);
            int from = 0, client = 1;
            int[] someCard = new int[nCards];
            foreach (KeyValuePair<TcpClient, PlayerInfo> kvp in clients)
            {
                if (kvp.Key == sender) continue;
                if (client < clients.Count - 1)
                {
                    if (nCards > 0)
                    {
                        Array.Copy(card, from, someCard, 0, nCards);
                        kvp.Value.rpc.Shuffle(group, someCard);
                        from += nCards;
                    }
                    client++;
                }
                else
                {
                    int rest = card.Length - from;
                    if (rest > 0)
                    {
                        someCard = new int[rest];
                        Array.Copy(card, from, someCard, 0, rest);
                        kvp.Value.rpc.Shuffle(group, someCard);
                    }
                    return;
                }
            }
        }

        public void Shuffled(int group, int[] card, short[] pos)
        { broadcaster.Shuffled(group, card, pos); }

        public void UnaliasGrp(int group)
        { broadcaster.UnaliasGrp(group); }

        public void Unalias(int[] card, ulong[] type)
        { broadcaster.Unalias(card, type); }

        public void PassToReq(int id, byte player, bool requested)
        { broadcaster.PassTo(clients[sender].id, id, player, requested); }

        public void TakeFromReq(int id, byte fromPlayer)
        { players[fromPlayer].rpc.TakeFrom(id, clients[sender].id); }

        public void DontTakeReq(int id, byte toPlayer)
        { players[toPlayer].rpc.DontTake(id); }

        public void FreezeCardsVisibility(int group)
        { broadcaster.FreezeCardsVisibility(group); }

        public void GroupVisReq(int id, bool defined, bool visible)
        { broadcaster.GroupVis(clients[sender].id, id, defined, visible); }

        public void GroupVisAddReq(int gId, byte pId)
        { broadcaster.GroupVisAdd(clients[sender].id, gId, pId); }

        public void GroupVisRemoveReq(int gId, byte pId)
        { broadcaster.GroupVisRemove(clients[sender].id, gId, pId); }

        public void LookAtReq(int uid, int gId, bool look)
        { broadcaster.LookAt(clients[sender].id, uid, gId, look); }

        public void LookAtTopReq(int uid, int gId, int count, bool look)
        { broadcaster.LookAtTop(clients[sender].id, uid, gId, count, look); }

        public void LookAtBottomReq(int uid, int gId, int count, bool look)
        { broadcaster.LookAtBottom(clients[sender].id, uid, gId, count, look); }

        public void StartLimitedReq(Guid[] packs)
        { broadcaster.StartLimited(clients[sender].id, packs); }

        public void CancelLimitedReq()
        { broadcaster.CancelLimited(clients[sender].id); }

        #endregion IRemoteCalls interface

        // This class contains high-level infos about connected clients
        internal sealed class PlayerInfo
        {
            internal string nick;                   // Player nick
            internal bool invertedTable;						// When using a two-sided table, indicates whether this player plays on the opposite side
            internal readonly string software;      // Connected software
            internal readonly ulong pkey;           // Player public cryptographic key
            internal IClientCalls rpc;              // Stub to send messages to the player
            internal readonly byte id;              // Player id
            internal bool binary = false;           // Send binary data ?
            internal bool spectates = false;        // Is a spectator rather than a player?

            // C'tor
            internal PlayerInfo(byte id, string nick, ulong pkey, IClientCalls rpc, string software)
            {
                // Init fields
                this.id = id; this.nick = nick; this.rpc = rpc;
                this.software = software; this.pkey = pkey;
            }
        }

        internal void PingReceived()
        {
            this.Connection.PingReceived();

        }
    }
}