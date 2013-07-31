using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using Octgn.Data;

namespace Octgn.Server
{
    internal sealed class Handler
    {
        #region Statics

        private const string ServerName = "OCTGN.NET";
        private static readonly Version ServerVersion = GetServerVersion(); //unused

        private static Version GetServerVersion()
        {
            Assembly asm = typeof (Server).Assembly;
            //var at = (AssemblyProductAttribute) asm.GetCustomAttributes(typeof (AssemblyProductAttribute), false)[0]; //unused
            return asm.GetName().Version;
        }

        #endregion Statics

        #region Private fields

        private readonly BinaryParser _binParser; // Parser for Binary messages
        // List of connected clients, keyed by underlying socket
        private readonly Broadcaster _broadcaster; // Stub to broadcast messages
        private readonly Dictionary<TcpClient, PlayerInfo> _clients = new Dictionary<TcpClient, PlayerInfo>();
        private readonly GameSettings _gameSettings = new GameSettings();
        private readonly Dictionary<byte, PlayerInfo> _players = new Dictionary<byte, PlayerInfo>();
        private readonly HashSet<byte> _turnStopPlayers = new HashSet<byte>();
        private bool _acceptPlayers = true; // When false, no new players are accepted
        private Server.Connection _connection;
        private byte _playerId = 1; // Next free player id
        private TcpClient _sender; // Socket on which current message was received
        private int _turnNumber; // Turn number, used to validate TurnStop requests

        #endregion Private fields

        public Dictionary<TcpClient, PlayerInfo> Players
        {
            get { return _clients; }
        }

        public bool GameStarted { get; private set; }

        #region Internal methods

        private readonly Guid _gameId;
        private readonly Version _gameVersion;
        private readonly string _password;
        internal int muted;

        // C'tor
        internal Handler(Guid gameId, Version gameVersion, string password)
        {
            GameStarted = false;
            _gameId = gameId;
            _gameVersion = gameVersion;
            _password = password;
            // Init fields
            _broadcaster = new Broadcaster(_clients, this);
            _binParser = new BinaryParser(this);
        }

        // Show the management GUI
        //        internal void ShowGUI(System.Windows.Forms.Form parent)
        //        {
        //            ManagementForm cf = new ManagementForm(clients);
        //            cf.ShowDialog(parent);
        //        }

        // Handle a Binary message
        internal void ReceiveMessage(byte[] data, TcpClient lSender, Server.Connection con)
        {
            // Check if this is the first message received
            if (!_clients.ContainsKey(lSender))
            {
                // A new connection must always start with a hello message, refuse the connection
                if (data[4] != (byte)2)
                {
                    lSender.GetStream().Close();
                    return;
                }
            }
            // Set the lSender field
            _sender = lSender;
            _connection = con;
            // Parse and handle the message
            _binParser.Parse(data);
        }

        // Called when a client is unexpectedly disconnected
        internal void Disconnected(TcpClient client)
        {
            PlayerInfo info;
            // If the client is not registered, do nothing
            if (!_clients.TryGetValue(client, out info)) return;
            // Remove the client from our lists
            _clients.Remove(client);
            _players.Remove(info.Id);
            // Notify everybody that the player has left the game
            _broadcaster.Leave(info.Id);
        }

        #endregion Internal methods

        #region IRemoteCalls interface

        public void Binary()
        {
            /* This never gets called. This message gets special treatment in the server. */
        }

        public void Error(string msg)
        {
            Debug.WriteLine(msg);
        }

        public void Start()
        {
            _acceptPlayers = false;
            _broadcaster.Start();
            GameStarted = true;
        }

        public void Settings(bool twoSidedTable)
        {
            _gameSettings.UseTwoSidedTable = twoSidedTable;
            _broadcaster.Settings(twoSidedTable);
        }

        public void PlayerSettings(byte player, bool invertedTable)
        {
            PlayerInfo p;
            // The player may have left the game concurently
            if (!_players.TryGetValue(player, out p)) return;
            p.InvertedTable = invertedTable;
            _broadcaster.PlayerSettings(player, invertedTable);
        }

        public void ResetReq()
        {
            _turnNumber = 0;
            _turnStopPlayers.Clear();
            _broadcaster.Reset(_clients[_sender].Id);
        }

        public void ChatReq(string text)
        {
            _broadcaster.Chat(_clients[_sender].Id, text);
        }

        public void PrintReq(string text)
        {
            _broadcaster.Print(_clients[_sender].Id, text);
        }

        public void RandomReq(int id, int min, int max)
        {
            _broadcaster.Random(_clients[_sender].Id, id, min, max);
        }

        public void RandomAnswer1Req(int id, ulong value)
        {
            _broadcaster.RandomAnswer1(_clients[_sender].Id, id, value);
        }

        public void RandomAnswer2Req(int id, ulong value)
        {
            _broadcaster.RandomAnswer2(_clients[_sender].Id, id, value);
        }

        public void CounterReq(int counter, int value)
        {
            _broadcaster.Counter(_clients[_sender].Id, counter, value);
        }

        public void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password)
        {
            // One should say Hello only once
            if (_clients.ContainsKey(_sender))
            {
                _clients[_sender].Rpc.Error("[Hello]You may say hello only once.");
                return;
            }

            // Verify password
            if (!string.IsNullOrWhiteSpace(_password))
            {
                if (!password.Equals(_password))
                {
                    var rpc = new BinarySenderStub(_sender, this);
                    rpc.Error("The password you entered was incorrect.");
                    try
                    {
                        _sender.Client.Close();
                        _sender.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        if (Debugger.IsAttached) Debugger.Break();
                    }
                    return;
                }
            }

            // Check if the versions are compatible
#if !DEBUG
            if(clientVer.CompareTo(ServerVersion) < 0)
            //if ((clientVer.Major != ServerVersion.Major || clientVer.Minor != ServerVersion.Minor))
            {
                var rpc = new BinarySenderStub(_sender, this);
                rpc.Error(string.Format("Incompatible versions. This server is accepting {0}.* clients only.",
                                        ServerVersion.ToString(2)));
                try
                {
                    _sender.Client.Close();
                    _sender.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                return;
            }
#endif
            // Check if we accept new players
            if (!_acceptPlayers)
            {
                var rpc = new BinarySenderStub(_sender, this);
                rpc.Error("No more players are accepted in this game.");
                try
                {
                    _sender.Client.Close();
                    _sender.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                return;
            }
            // Check if the client wants to play the correct game
            if (lGameId != _gameId)
            {
                var rpc = new BinarySenderStub(_sender, this);
                rpc.Error(string.Format("Invalid game. This server is hosting another game (game id: {0}).", _gameId));
                try
                {
                    _sender.Client.Close();
                    _sender.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                return;
            }
            // Check if the client's major game version matches ours
            if (gameVer.Major != _gameVersion.Major)
            {
                var rpc = new BinarySenderStub(_sender, this);
                rpc.Error(string.Format("Incompatible game version. This server is hosting game version {0}.",_gameVersion));
                try
                {
                    _sender.Client.Close();
                    _sender.Close();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                return;
            }
            // Create the new endpoint
            IClientCalls senderRpc = new BinarySenderStub(_sender, this);
            string software = client + " (" + clientVer + ')';
            var pi = new PlayerInfo(_playerId++, nick, pkey, senderRpc, software);
            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                pi.Rpc.Binary();
                pi.Rpc = senderRpc = new BinarySenderStub(_sender, this);
                pi.Binary = true;
            }
            // Notify everybody of the newcomer
            _broadcaster.NewPlayer(pi.Id, nick, pkey);
            // Add everybody to the newcomer
            foreach (PlayerInfo player in _clients.Values)
                senderRpc.NewPlayer(player.Id, player.Nick, player.Pkey);
            senderRpc.Welcome(pi.Id);
            // Notify the newcomer of some shared settings
            senderRpc.Settings(_gameSettings.UseTwoSidedTable);
            foreach (PlayerInfo player in _players.Values.Where(p => p.InvertedTable))
                senderRpc.PlayerSettings(player.Id, true);
            // Add it to our lists
            _clients.Add(_sender, pi);
            _players.Add(pi.Id, pi);
            _broadcaster.RefreshTypes();
        }

        public void LoadDeck(int[] id, ulong[] type, int[] group)
        {
            short s = _clients[_sender].Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.LoadDeck(id, type, group);
        }

        public void CreateCard(int[] id, ulong[] type, int group)
        {
            short s = _clients[_sender].Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateCard(id, type, group);
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            short s = _clients[_sender].Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
        }

        public void CreateAlias(int[] id, ulong[] type)
        {
            short s = _clients[_sender].Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateAlias(id, type);
        }

        public void NextTurn(byte nextPlayer)
        {
            if (_turnStopPlayers.Count > 0)
            {
                byte stopPlayerId = _turnStopPlayers.First();
                _turnStopPlayers.Remove(stopPlayerId);
                _broadcaster.StopTurn(stopPlayerId);
                return;
            }
            _turnNumber++;
            _broadcaster.NextTurn(nextPlayer);
        }

        public void PlayerSetGlobalVariable(byte p, string name, string value)
        {
            _broadcaster.PlayerSetGlobalVariable( p, name, value);
        }

        public void SetGlobalVariable(string name, string value)
        {
            _broadcaster.SetGlobalVariable(name, value);
        }

        public void StopTurnReq(int lTurnNumber, bool stop)
        {
            if (lTurnNumber != _turnNumber) return; // Message StopTurn crossed a NextTurn message
            byte id = _clients[_sender].Id;
            if (stop)
                _turnStopPlayers.Add(id);
            else
                _turnStopPlayers.Remove(id);
        }

        public void CardSwitchTo(byte uid, int c, string alternate)
        {
            _broadcaster.CardSwitchTo(uid,c,alternate);
        }

        public void MoveCardReq(int card, int to, int idx, bool faceUp)
        {
            _broadcaster.MoveCard(_clients[_sender].Id, card, to, idx, faceUp);
        }

        public void MoveCardAtReq(int card, int x, int y, int idx, bool faceUp)
        {
            _broadcaster.MoveCardAt(_clients[_sender].Id, card, x, y, idx, faceUp);
        }

        public void AddMarkerReq(int card, Guid id, string name, ushort count)
        {
            _broadcaster.AddMarker(_clients[_sender].Id, card, id, name, count);
        }

        public void RemoveMarkerReq(int card, Guid id, string name, ushort count)
        {
            _broadcaster.RemoveMarker(_clients[_sender].Id, card, id, name, count);
        }

        public void SetMarkerReq(int card, Guid id, string name, ushort count)
        {
            _broadcaster.SetMarker(_clients[_sender].Id, card, id, name, count);
        }

        public void TransferMarkerReq(int from, int to, Guid id, string name, ushort count)
        {
            _broadcaster.TransferMarker(_clients[_sender].Id, from, to, id, name, count);
        }

        public void NickReq(string nick)
        {
            PlayerInfo pi = _clients[_sender];
            pi.Nick = nick;
            _broadcaster.Nick(pi.Id, nick);
        }

        public void Reveal(int card, ulong revealed, Guid guid)
        {
            _broadcaster.Reveal(card, revealed, guid);
        }

        public void RevealToReq(byte sendTo, byte[] revealTo, int card, ulong[] encrypted)
        {
            if (encrypted.Length != 2 && encrypted.Length != 5)
                Debug.WriteLine("[RevealToReq] Invalid encrypted length.");
            _players[sendTo].Rpc.RevealTo(revealTo, card, encrypted);
        }

        public void PeekReq(int card)
        {
            _broadcaster.Peek(_clients[_sender].Id, card);
        }

        public void UntargetReq(int card)
        {
            _broadcaster.Untarget(_clients[_sender].Id, card);
        }

        public void TargetReq(int card)
        {
            _broadcaster.Target(_clients[_sender].Id, card);
        }

        public void TargetArrowReq(int card, int otherCard)
        {
            _broadcaster.TargetArrow(_clients[_sender].Id, card, otherCard);
        }

        public void Highlight(int card, string color)
        {
            _broadcaster.Highlight(card, color);
        }

        public void TurnReq(int card, bool up)
        {
            _broadcaster.Turn(_clients[_sender].Id, card, up);
        }

        public void RotateReq(int card, CardOrientation rot)
        {
            _broadcaster.Rotate(_clients[_sender].Id, card, rot);
        }

        public void Shuffle(int group, int[] card)
        {
            // Special case: solo playing
            if (_clients.Count == 1)
            {
                _clients[_sender].Rpc.Shuffle(group, card);
                return;
            }
            // Normal case
            int nCards = card.Length/(_clients.Count - 1);
            int from = 0, client = 1;
            var someCard = new int[nCards];
            foreach (KeyValuePair<TcpClient, PlayerInfo> kvp in _clients.Where(kvp => kvp.Key != _sender))
            {
                if (client < _clients.Count - 1)
                {
                    if (nCards > 0)
                    {
                        Array.Copy(card, @from, someCard, 0, nCards);
                        kvp.Value.Rpc.Shuffle(@group, someCard);
                        @from += nCards;
                    }
                    client++;
                }
                else
                {
                    int rest = card.Length - @from;
                    if (rest > 0)
                    {
                        someCard = new int[rest];
                        Array.Copy(card, @from, someCard, 0, rest);
                        kvp.Value.Rpc.Shuffle(@group, someCard);
                    }
                    return;
                }
            }
        }

        public void Shuffled(int group, int[] card, short[] pos)
        {
            _broadcaster.Shuffled(group, card, pos);
        }

        public void UnaliasGrp(int group)
        {
            _broadcaster.UnaliasGrp(group);
        }

        public void Unalias(int[] card, ulong[] type)
        {
            _broadcaster.Unalias(card, type);
        }

        public void PassToReq(int id, byte player, bool requested)
        {
            _broadcaster.PassTo(_clients[_sender].Id, id, player, requested);
        }

        public void TakeFromReq(int id, byte fromPlayer)
        {
            _players[fromPlayer].Rpc.TakeFrom(id, _clients[_sender].Id);
        }

        public void DontTakeReq(int id, byte toPlayer)
        {
            _players[toPlayer].Rpc.DontTake(id);
        }

        public void FreezeCardsVisibility(int group)
        {
            _broadcaster.FreezeCardsVisibility(group);
        }

        public void GroupVisReq(int id, bool defined, bool visible)
        {
            _broadcaster.GroupVis(_clients[_sender].Id, id, defined, visible);
        }

        public void GroupVisAddReq(int gId, byte pId)
        {
            _broadcaster.GroupVisAdd(_clients[_sender].Id, gId, pId);
        }

        public void GroupVisRemoveReq(int gId, byte pId)
        {
            _broadcaster.GroupVisRemove(_clients[_sender].Id, gId, pId);
        }

        public void LookAtReq(int uid, int gId, bool look)
        {
            _broadcaster.LookAt(_clients[_sender].Id, uid, gId, look);
        }

        public void LookAtTopReq(int uid, int gId, int count, bool look)
        {
            _broadcaster.LookAtTop(_clients[_sender].Id, uid, gId, count, look);
        }

        public void LookAtBottomReq(int uid, int gId, int count, bool look)
        {
            _broadcaster.LookAtBottom(_clients[_sender].Id, uid, gId, count, look);
        }

        public void StartLimitedReq(Guid[] packs)
        {
            _broadcaster.StartLimited(_clients[_sender].Id, packs);
        }

        public void CancelLimitedReq()
        {
            _broadcaster.CancelLimited(_clients[_sender].Id);
        }

        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            _broadcaster.IsTableBackgroundFlipped(isFlipped);
        }

        #endregion IRemoteCalls interface

        // This class contains high-level infos about connected clients

        internal void Ping()
        {
            _connection.PingReceived();
        }

        #region Nested type: PlayerInfo

        internal sealed class PlayerInfo
        {
            internal readonly byte Id; // Player id
            internal readonly ulong Pkey; // Player public cryptographic key
            internal readonly string Software; // Connected software
            internal bool Binary; // Send Binary data ?

            internal bool InvertedTable;
            // When using a two-sided table, indicates whether this player plays on the opposite side

            internal string Nick; // Player nick
            internal IClientCalls Rpc; // Stub to send messages to the player

            // internal bool spectates; // Is a spectator rather than a player?  Not even used

            // C'tor
            internal PlayerInfo(byte id, string nick, ulong pkey, IClientCalls rpc, string software)
            {
                // Init fields
                Id = id;
                Nick = nick;
                Rpc = rpc;
                Software = software;
                Pkey = pkey;
            }
        }

        #endregion

        public void PlaySound(byte player, string soundName)
        {
            _broadcaster.PlaySound(player,soundName);
        }

        public void Ready(byte player)
        {
            _broadcaster.Ready(player);
        }
    }
}