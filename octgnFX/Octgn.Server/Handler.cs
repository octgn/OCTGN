/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using log4net;
using Octgn.Data;
using Octgn.Library.Localization;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

namespace Octgn.Server
{
    public sealed class Handler
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Statics

        private const string ServerName = "OCTGN.NET";
        private static readonly Version ServerVersion = GetServerVersion(); //unused

        private static Version GetServerVersion()
        {
            Assembly asm = typeof(Server).Assembly;
            //var at = (AssemblyProductAttribute) asm.GetCustomAttributes(typeof (AssemblyProductAttribute), false)[0]; //unused
            return asm.GetName().Version;
        }

        #endregion Statics

        #region Private fields

        private readonly BinaryParser _binParser; // Parser for Binary messages
        // List of connected clients, keyed by underlying socket
        private readonly Broadcaster _broadcaster; // Stub to broadcast messages
        private readonly GameSettings _gameSettings = new GameSettings();
        private readonly HashSet<byte> _turnStopPlayers = new HashSet<byte>();
        private bool _acceptPlayers = true; // When false, no new players are accepted
        private ServerSocket _sender;
        private byte _playerId = 1; // Next free player id
        private int _turnNumber; // Turn number, used to validate TurnStop requests

        #endregion Private fields

        public bool GameStarted { get; private set; }

        #region Internal methods

        private readonly Guid _gameId;
        private readonly Version _gameVersion;
        private readonly string _password;
        internal int muted;

        private bool _gameStarted;

        // C'tor
        internal Handler()
        {
            _gameSettings.AllowSpectators = State.Instance.Engine.Game.Spectators;
            GameStarted = false;
            _gameId = State.Instance.Engine.Game.GameId;
            _gameVersion = State.Instance.Engine.Game.GameVersion;
            _password = State.Instance.Engine.Game.Password;
            // Init fields
            _broadcaster = new Broadcaster(this);
            _binParser = new BinaryParser(this);
        }

        internal void SetupHandler(ServerSocket con)
        {
            // Set the lSender field
            _sender = con;
        }

        internal void ReceiveMessage(byte[] data, ServerSocket con)
        {
            //Debug.WriteLine("[Message] {0}", data[4]);
            // Check if this is the first message received
            if (!State.Instance.SaidHello(con))
            {
                var acceptableMessages = new byte[]
                    {
                        1, // Error
                        4, // Hello
                        5, // HelloAgain
                        92, // Ping
                    };
                //TODO Maybe we shouldn't kill the connection here
                //     Basically, if someone dc's it's possible that
                //     a network call gets sent up on accident before HelloAgain,
                //     which effectivly kills the game.
                //     Maybe need a flag on the player saying they at least said
                //     hello once.
                // A new connection must always start with a hello message, refuse the connection
                if (acceptableMessages.Contains(data[4]) == false)
                {
                    var pi = State.Instance.GetClient(con);
                    pi.Kick(false, L.D.ServerMessage__FailedToSendHelloMessage);
                    State.Instance.RemoveClient(pi);
                    return;
                }
            }
            // Set the lSender field
            _sender = con;
            _sender.OnPingReceived();
            // Parse and handle the message
            _binParser.Parse(data);
        }

        // Called when a client is unexpectedly disconnected
        //internal void Disconnected(ServerSocket client)
        //{
        //    PlayerInfo info;
        //    // If the client is not registered, do nothing
        //    if (!_clients.TryGetValue(client, out info)) return;
        //    info.Connected = false;
        //    //_clients.Remove(client);
        //    //_players.Remove(info.Id);
        //    // Notify everybody that the player has left the game
        //    info.Connected = false;
        //    info.TimeDisconnected = DateTime.Now;
        //    _broadcaster.PlayerDisconnect(info.Id);
        //}

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
            _gameStarted = true;
            _broadcaster.Start();
            GameStarted = true;
            State.Instance.Handler.GameStarted = true;
            // Just a precaution, shouldn't happen though.
            if (_gameSettings.AllowSpectators == false)
            {
                foreach (var p in State.Instance.Players)
                {
                    if (p.IsSpectator)
                        p.Kick(false, L.D.ServerMessage__SpectatorsNotAllowed);
                }
            }
            if (State.Instance.Engine.IsLocal == false)
            {
                try
                {
                    var c = new ApiClient();
                    var req = new PutGameHistoryReq(State.Instance.Engine.ApiKey,
                        State.Instance.Engine.Game.Id.ToString());
                    foreach (var p in State.Instance.Players.ToArray())
                    {
                        req.Usernames.Add(p.Nick);
                    }
                    c.CreateGameHistory(req);
                }
                catch (Exception e)
                {
                    Log.Error("Start Error creating Game History", e);
                }
            }
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            if (State.Instance.GetPlayer(_sender).Id == 1)
            {
                if (this.GameStarted)
                {
                    _gameSettings.AllowSpectators = allowSpectators;
                    _gameSettings.MuteSpectators = muteSpectators;
                    State.Instance.Engine.Game.Spectators = allowSpectators;
                    _broadcaster.Settings(_gameSettings.UseTwoSidedTable, allowSpectators, muteSpectators);
                }
                else
                {
                    _gameSettings.UseTwoSidedTable = twoSidedTable;
                    _gameSettings.AllowSpectators = allowSpectators;
                    _gameSettings.MuteSpectators = muteSpectators;
                    State.Instance.Engine.Game.Spectators = allowSpectators;
                    _broadcaster.Settings(twoSidedTable, allowSpectators, muteSpectators);
                }
            }
        }

        public void PlayerSettings(byte player, bool invertedTable, bool spectator)
        {
            if (this.GameStarted) return;
            PlayerInfo p;
            // The player may have left the game concurrently
            p = State.Instance.Players.FirstOrDefault(x => x.Id == player);
            if (p == null) return;
            if (p.InvertedTable != invertedTable || p.IsSpectator != spectator)
            {
                p.InvertedTable = invertedTable;
                p.IsSpectator = spectator;
                _broadcaster.PlayerSettings(player, invertedTable, spectator);
            }
        }

        public void ResetReq()
        {
            _turnNumber = 0;
            _turnStopPlayers.Clear();
            _broadcaster.Reset(State.Instance.GetPlayer(_sender).Id);
        }

        public void ChatReq(string text)
        {
            var player = State.Instance.GetPlayer(_sender);
            if (player.IsSpectator && _gameSettings.MuteSpectators)
            {
                player.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _broadcaster.Chat(player.Id, text);
            if (State.Instance.Engine.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            //if (player.Nick.Equals(State.Instance.Engine.Game.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
            mess.Message = string.Format("{0} has left your game", player.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = State.Instance.Engine.Game.Id;
            mess.Type = GameMessageType.Chat;
            //new Octgn.Site.Api.ApiClient().GameMessage(State.Instance.Engine.ApiKey, mess);
        }

        public void PrintReq(string text)
        {
            var player = State.Instance.GetPlayer(_sender);
            if (player.IsSpectator && _gameSettings.MuteSpectators)
            {
                player.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _broadcaster.Print(player.Id, text);
        }

		private Random rnd = new Random();
        public void RandomReq(int min, int max)
        {
            var pi = State.Instance.GetClient(_sender);
            var result = rnd.Next(min, max + 1);
			pi.Rpc.Random(result);
            //_broadcaster.Random(min, max);
        }


        public void CounterReq(int counter, int value)
        {
            _broadcaster.Counter(State.Instance.GetPlayer(_sender).Id, counter, value);
        }

        private bool ValidateHello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool spectator)
        {
            if (State.Instance.KickedPlayers.Contains(pkey))
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsMuted);
                return false;
            }
            // One should say Hello only once
            if (State.Instance.SaidHello(_sender))
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SayHelloOnlyOnce);
                return false;
            }

            // Verify password
            if (!string.IsNullOrWhiteSpace(_password))
            {
                if (!password.Equals(_password))
                {
                    ErrorAndCloseConnection(L.D.ServerMessage__IncorrectPassword);
                    return false;
                }
            }
            // Check if the versions are compatible
#if(!DEBUG)
            if(clientVer.CompareTo(ServerVersion) < 0)
            //if ((clientVer.Major != ServerVersion.Major || clientVer.Minor != ServerVersion.Minor))
            {
                ErrorAndCloseConnection(string.Format(L.D.ServerMessage__IncompatibleOctgnClient_Format,
                                        ServerVersion, clientVer));
                return false;
            }
#endif
            // Check if the client wants to play the correct game
            if (lGameId != _gameId)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__InvalidGame_Format, State.Instance.Engine.Game.GameName);
                return false;
            }
            // Check if the client's major game version matches ours
            if (gameVer.Major != _gameVersion.Major)
            {
                ErrorAndCloseConnection(
                    L.D.ServerMessage__IncompatibleGameVersion_Format,
                    _gameVersion);
                return false;
            }
            return true;
        }

        private void ErrorAndCloseConnection(string message, params object[] args)
        {
            var pi = State.Instance.GetClient(_sender);
            pi.Kick(false, message, args);
            State.Instance.RemoveClient(pi);
        }

        public void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool spectator)
        {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, spectator)) return;
            if (spectator && State.Instance.Engine.Game.Spectators == false)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsNotAllowed);
                return;
            }
            // Check if we accept new players
            if (!_acceptPlayers && spectator == false)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__GameStartedNotAcceptingNewPlayers);
                return;
            }
            State.Instance.HasSomeoneJoined = true;
            // Create the new endpoint
            IClientCalls senderRpc = new BinarySenderStub(_sender, this);
            string software = client + " (" + clientVer + ')';
            var pi = State.Instance.GetClient(_sender);
            pi.Setup(_playerId++, nick, pkey, senderRpc, software, spectator);
            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                pi.Rpc.Binary();
                pi.Rpc = senderRpc = new BinarySenderStub(_sender, this);
                pi.Binary = true;
            }
            // decide players side of table; before saying hello so new player not included
            short aPlayers = (short)State.Instance.Players.Count(x => !x.InvertedTable);
            short bPlayers = (short)State.Instance.Players.Count(x => x.InvertedTable);
            if (aPlayers > bPlayers) pi.InvertedTable = true;

            pi.SaidHello = true;
            // Welcome newcomer and asign them their side 
            senderRpc.Welcome(pi.Id, State.Instance.Engine.Game.Id, _gameStarted);
            senderRpc.PlayerSettings(pi.Id, pi.InvertedTable, pi.IsSpectator);
            // Notify everybody of the newcomer
            _broadcaster.NewPlayer(pi.Id, nick, pkey, pi.InvertedTable, spectator);
            // Add everybody to the newcomer
            foreach (PlayerInfo player in State.Instance.Players.Where(x => x.Id != pi.Id))
                senderRpc.NewPlayer(player.Id, player.Nick, player.Pkey, player.InvertedTable, player.IsSpectator);
            // Notify the newcomer of table sides
            senderRpc.Settings(_gameSettings.UseTwoSidedTable, _gameSettings.AllowSpectators, _gameSettings.MuteSpectators);
            // Add it to our lists
            _broadcaster.RefreshTypes();
            if (_gameStarted)
            {
                senderRpc.Start();
            }
            else
            {
                if (State.Instance.Engine.IsLocal != false) return;
                var mess = new GameMessage();
                // don't send if we join our own room...that'd be annoying
                if (nick.Equals(State.Instance.Engine.Game.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
                mess.Message = string.Format("{0} has joined your game", nick);
                mess.Sent = DateTime.Now;
                mess.SessionId = State.Instance.Engine.Game.Id;
                mess.Type = GameMessageType.Event;
                new Octgn.Site.Api.ApiClient().GameMessage(State.Instance.Engine.ApiKey, mess);
            }
        }

        public void HelloAgain(byte pid, string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId, Version gameVer, string password)
        {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, false)) return;
            // Make sure the pid is one that exists
            var pi = State.Instance.GetPlayer(pid);
            if (pi == null)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__CanNotReconnectFirstTimeConnecting);
                return;
            }

            // Make sure the pkey matches the pkey for the pid
            if (pi.Pkey != pkey)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__PublicKeyDoesNotMatch);
                return;
            }
            // Create the new endpoint
            IClientCalls senderRpc = new BinarySenderStub(_sender, this);
            pi.Rpc = senderRpc;

            string software = client + " (" + clientVer + ')';

            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                pi.Rpc.Binary();
                pi.Rpc = senderRpc = new BinarySenderStub(_sender, this);
                pi.Binary = true;
            }
            pi.SaidHello = true;
            // welcome the player and assign them their side
            senderRpc.Welcome(pi.Id, State.Instance.Engine.Game.Id, true);
            senderRpc.PlayerSettings(pi.Id, pi.InvertedTable, pi.IsSpectator);
            // Notify everybody of the newcomer
            _broadcaster.NewPlayer(pi.Id, nick, pkey, pi.InvertedTable, pi.IsSpectator);
            // Add everybody to the newcomer
            foreach (PlayerInfo player in State.Instance.Players.Where(x => x.Id != pi.Id))
                senderRpc.NewPlayer(player.Id, player.Nick, player.Pkey, player.InvertedTable, player.IsSpectator);
            // Notify the newcomer of some shared settings
            senderRpc.Settings(_gameSettings.UseTwoSidedTable, _gameSettings.AllowSpectators, _gameSettings.MuteSpectators);
            foreach (PlayerInfo player in State.Instance.Players)
                senderRpc.PlayerSettings(player.Id, player.InvertedTable, pi.IsSpectator);
            // Add it to our lists
            pi.Connected = true;
            pi.ResetSocket(_sender);
            pi.Connected = true;
            State.Instance.UpdateDcPlayer(pi.Nick, false);
            _broadcaster.RefreshTypes();
            senderRpc.Start();
        }

        public void LoadDeck(int[] id, Guid[] type, int[] @group, string[] size, string sleeveString)        {
            short s = State.Instance.GetPlayer(_sender).Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);

            var sstring = "";
            try
            {
                var split = sleeveString.Split(new char[1] { '\t' }, 2);
                if (split.Length == 2)
                {
                    var sid = 0;
                    if (int.TryParse(split[0], out sid))
                    {
                        Uri url = null;
                        if (Uri.TryCreate(split[1], UriKind.Absolute, out url))
                        {
                            if (State.Instance.Engine.IsLocal == false)
                            {
                                // Check if the user can even do this
                                var p = State.Instance.GetPlayer(_sender);
                                var c = new ApiClient();
                                var resp = c.CanUseSleeve(p.Nick, sid);
                                if (resp.Authorized)
                                {
                                    sstring = split[1];
                                }
                            }
                            else
                            {
                                sstring = split[1];
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (State.Instance.Engine.IsLocal)
                    Log.Warn("LoadDeck", e);
                else
                    Log.Error("LoadDeck", e);
            }
            _broadcaster.LoadDeck(id, type, group, size, sstring);
        }

        public void CreateCard(int[] id, Guid[] type, string[] size, int @group)        {
            short s = State.Instance.GetPlayer(_sender).Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateCard(id, type, size, group);
        }

        public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            short s = State.Instance.GetPlayer(_sender).Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateCardAt(id, modelId, x, y, faceUp, persist);
        }

        //public void CreateAlias(int[] id, ulong[] type)
        //{
        //    short s = _clients[_sender].Id;
        //    for (int i = 0; i < id.Length; i++)
        //        id[i] = s << 16 | (id[i] & 0xffff);
        //    _broadcaster.CreateAlias(id, type);
        //}

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

        public void PlayerSetGlobalVariable(byte p, string name, string oldvalue, string value)
        {
            _broadcaster.PlayerSetGlobalVariable(p, name, oldvalue, value);
        }

        public void SetGlobalVariable(string name, string oldvalue, string value)
        {
            _broadcaster.SetGlobalVariable(name, oldvalue, value);
        }

        public void StopTurnReq(int lTurnNumber, bool stop)
        {
            if (lTurnNumber != _turnNumber) return; // Message StopTurn crossed a NextTurn message
            byte id = State.Instance.GetPlayer(_sender).Id;
            if (stop)
                _turnStopPlayers.Add(id);
            else
                _turnStopPlayers.Remove(id);
        }

        public void CardSwitchTo(byte uid, int c, string alternate)
        {
            _broadcaster.CardSwitchTo(uid, c, alternate);
        }

        public void MoveCardReq(int[] id, int to, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _broadcaster.MoveCard(State.Instance.GetPlayer(_sender).Id, id, to, idx, faceUp, isScriptMove);
        }

        public void MoveCardAtReq(int[] card, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            _broadcaster.MoveCardAt(State.Instance.GetPlayer(_sender).Id, card, x, y, idx, faceUp, isScriptMove);
        }

        public void AddMarkerReq(int card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _broadcaster.AddMarker(State.Instance.GetPlayer(_sender).Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void RemoveMarkerReq(int card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _broadcaster.RemoveMarker(State.Instance.GetPlayer(_sender).Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void TransferMarkerReq(int from, int to, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _broadcaster.TransferMarker(State.Instance.GetPlayer(_sender).Id, from, to, id, name, count, oldCount, isScriptChange);
        }

        public void NickReq(string nick)
        {
            PlayerInfo pi = State.Instance.GetPlayer(_sender);
            pi.Nick = nick;
            _broadcaster.Nick(pi.Id, nick);
        }

        public void PeekReq(int card)
        {
            _broadcaster.Peek(State.Instance.GetPlayer(_sender).Id, card);
        }

        public void UntargetReq(int card)
        {
            _broadcaster.Untarget(State.Instance.GetPlayer(_sender).Id, card);
        }

        public void TargetReq(int card)
        {
            _broadcaster.Target(State.Instance.GetPlayer(_sender).Id, card);
        }

        public void TargetArrowReq(int card, int otherCard)
        {
            _broadcaster.TargetArrow(State.Instance.GetPlayer(_sender).Id, card, otherCard);
        }

        public void Highlight(int card, string color)
        {
            _broadcaster.Highlight(card, color);
        }

        public void TurnReq(int card, bool up)
        {
            _broadcaster.Turn(State.Instance.GetPlayer(_sender).Id, card, up);
        }

        public void RotateReq(int card, CardOrientation rot)
        {
            _broadcaster.Rotate(State.Instance.GetPlayer(_sender).Id, card, rot);
        }

        //public void Shuffle(int group, int[] card)
        //{
        //    // Special case: solo playing
        //    if (_clients.Count == 1)
        //    {
        //        _clients[_sender].Rpc.Shuffle(group, card);
        //        return;
        //    }
        //    // Normal case
        //    int nCards = card.Length/(_clients.Count - 1);
        //    int from = 0, client = 1;
        //    var someCard = new int[nCards];
        //    foreach (KeyValuePair<TcpClient, PlayerInfo> kvp in _clients.Where(kvp => kvp.Key != _sender))
        //    {
        //        if (client < _clients.Count - 1)
        //        {
        //            if (nCards > 0)
        //            {
        //                Array.Copy(card, @from, someCard, 0, nCards);
        //                kvp.Value.Rpc.Shuffle(@group, someCard);
        //                @from += nCards;
        //            }
        //            client++;
        //        }
        //        else
        //        {
        //            int rest = card.Length - @from;
        //            if (rest > 0)
        //            {
        //                someCard = new int[rest];
        //                Array.Copy(card, @from, someCard, 0, rest);
        //                kvp.Value.Rpc.Shuffle(@group, someCard);
        //            }
        //            return;
        //        }
        //    }
        //}

        public void Shuffled(byte player, int group, int[] card, short[] pos)
        {
            _broadcaster.Shuffled(player, group, card, pos);
        }

        //public void UnaliasGrp(int group)
        //{
        //    _broadcaster.UnaliasGrp(group);
        //}

        //public void Unalias(int[] card, ulong[] type)
        //{
        //    _broadcaster.Unalias(card, type);
        //}

        public void PassToReq(int id, byte player, bool requested)
        {
            _broadcaster.PassTo(State.Instance.GetPlayer(_sender).Id, id, player, requested);
        }

        public void TakeFromReq(int id, byte fromPlayer)
        {
            State.Instance.GetPlayer(fromPlayer).Rpc.TakeFrom(id, State.Instance.GetPlayer(_sender).Id);
        }

        public void DontTakeReq(int id, byte toPlayer)
        {
            State.Instance.GetPlayer(toPlayer).Rpc.DontTake(id);
        }

        public void FreezeCardsVisibility(int group)
        {
            _broadcaster.FreezeCardsVisibility(group);
        }

        public void GroupVisReq(int id, bool defined, bool visible)
        {
            _broadcaster.GroupVis(State.Instance.GetPlayer(_sender).Id, id, defined, visible);
        }

        public void GroupVisAddReq(int gId, byte pId)
        {
            _broadcaster.GroupVisAdd(State.Instance.GetPlayer(_sender).Id, gId, pId);
        }

        public void GroupVisRemoveReq(int gId, byte pId)
        {
            _broadcaster.GroupVisRemove(State.Instance.GetPlayer(_sender).Id, gId, pId);
        }

        public void LookAtReq(int uid, int gId, bool look)
        {
            _broadcaster.LookAt(State.Instance.GetPlayer(_sender).Id, uid, gId, look);
        }

        public void LookAtTopReq(int uid, int gId, int count, bool look)
        {
            _broadcaster.LookAtTop(State.Instance.GetPlayer(_sender).Id, uid, gId, count, look);
        }

        public void LookAtBottomReq(int uid, int gId, int count, bool look)
        {
            _broadcaster.LookAtBottom(State.Instance.GetPlayer(_sender).Id, uid, gId, count, look);
        }

        public void StartLimitedReq(Guid[] packs)
        {
            _broadcaster.StartLimited(State.Instance.GetPlayer(_sender).Id, packs);
        }

        public void CancelLimitedReq()
        {
            _broadcaster.CancelLimited(State.Instance.GetPlayer(_sender).Id);
        }

        public void AddPacksReq(Guid[] packs, bool selfOnly)
        {
            _broadcaster.AddPacks(State.Instance.GetPlayer(_sender).Id, packs, selfOnly);
        }
        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            _broadcaster.IsTableBackgroundFlipped(isFlipped);
        }

        #endregion IRemoteCalls interface

        // This class contains high-level infos about connected clients

        internal void Ping()
        {
            _sender.OnPingReceived();
        }

        public void PlaySound(byte player, string soundName)
        {
            _broadcaster.PlaySound(player, soundName);
        }

        public void Ready(byte player)
        {
            _broadcaster.Ready(player);
        }

        public void RemoteCall(byte player, string func, string args)
        {
            State.Instance.GetPlayer(player).Rpc.RemoteCall(State.Instance.GetPlayer(_sender).Id, func, args);
        }

        public void ShuffleDeprecated(int arg0, int[] ints)
        {
            _broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void UnaliasGrpDeprecated(int arg0)
        {
            _broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void UnaliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            _broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void CreateAliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            _broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void GameState(byte player, string state)
        {
            State.Instance.GetPlayer(player).Rpc.GameState(State.Instance.GetPlayer(_sender).Id, state);
        }

        public void GameStateReq(byte toPlayer)
        {
            State.Instance.GetPlayer(toPlayer).Rpc.GameStateReq(State.Instance.GetPlayer(_sender).Id);
        }

        public void DeleteCard(int cardId, byte playerId)
        {
            _broadcaster.DeleteCard(cardId, playerId);
        }

        public void Leave(byte player)
        {
            PlayerInfo info = State.Instance.GetPlayer(_sender);
            // If the client is not registered, do nothing
            if (info == null) return;
            State.Instance.RemoveClient(info);
            info.Connected = false;
            // Notify everybody that the player has left the game
            _broadcaster.Leave(info.Id);
            if (State.Instance.Engine.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            if (info.Nick.Equals(State.Instance.Engine.Game.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
            mess.Message = string.Format("{0} has left your game", info.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = State.Instance.Engine.Game.Id;
            mess.Type = GameMessageType.Event;
            new Octgn.Site.Api.ApiClient().GameMessage(State.Instance.Engine.ApiKey, mess);
        }

        public void Boot(byte player, string reason)
        {
            var p = State.Instance.GetPlayer(_sender);
            var bplayer = State.Instance.GetPlayer(player);
            if (bplayer == null)
            {
                _broadcaster.Error(string.Format(L.D.ServerMessage__CanNotBootPlayerDoesNotExist, p.Nick));
                return;
            }
            if (p.Id != 1)
            {
                _broadcaster.Error(string.Format(L.D.ServerMessage__CanNotBootNotHost, p.Nick, bplayer.Nick));
                return;
            }
            State.Instance.AddKickedPlayer(bplayer);
            bplayer.Kick(false, reason);
            _broadcaster.Leave(bplayer.Id);
        }

        internal void AnchorCard(int card, byte player, bool anchor)
        {
            _broadcaster.AnchorCard(card, player, anchor);
        }
    }
}