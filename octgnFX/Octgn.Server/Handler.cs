/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
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
            var asm = typeof(Server).Assembly;
            //var at = (AssemblyProductAttribute) asm.GetCustomAttributes(typeof (AssemblyProductAttribute), false)[0]; //unused
            return asm.GetName().Version;
        }

        #endregion Statics

        #region Private fields

        private readonly BinaryParser _binParser; // Parser for Binary messages
        // List of connected clients, keyed by underlying socket
        private readonly GameSettings _gameSettings = new GameSettings();
        private readonly HashSet<byte> _turnStopPlayers = new HashSet<byte>();
        private readonly HashSet<Tuple<byte, byte>> _phaseStops = new HashSet<Tuple<byte, byte>>();
        private ServerSocket _sender;
        private byte _playerId = 1; // Next free player id
        private int _turnNumber; // Turn number, used to validate TurnStop requests
        private byte _phaseNumber;

        #endregion Private fields

        private int _gameStarted = 0;
        public bool GameStarted {
            get => _gameStarted == 1;
            private set {
                if (Interlocked.CompareExchange(ref _gameStarted, 1, 0) != 0) throw new InvalidOperationException("Can only start game once.");
                _state.Game.DateStarted = DateTimeOffset.Now;
                _state.Game.Status = Online.Hosting.HostedGameStatus.GameInProgress;
                Log.Info($"{_state.Game.GameName}: Game marked started.");
            }
        }

        public bool AcceptingNewPlayers => !GameStarted;

        #region Internal methods

        private readonly Guid _gameId;
        private readonly Version _gameVersion;
        private readonly string _password;
        internal int muted;

        private readonly State _state;

        // C'tor
        public Handler(State state)
        {
            _state = state;
            _gameSettings.AllowSpectators = _state.Game.Spectators;
            _gameId = _state.Game.GameId;
            _gameVersion = Version.Parse(_state.Game.GameVersion);
            _password = _state.Game.Password;
            // Init fields
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
            if (!_state.SaidHello(con))
            {
                var acceptableMessages = new byte[]
                    {
                        1, // Error
                        4, // Hello
                        5, // HelloAgain
                        90, // Ping
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
                    var pi = _state.GetClient(con);
                    pi.Kick(false, L.D.ServerMessage__FailedToSendHelloMessage);
                    _state.RemoveClient(pi);
                    return;
                }
            }
            // Set the lSender field
            _sender = con;
            _sender.OnPingReceived();
            // Parse and handle the message
            _binParser.Parse(data);
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
            GameStarted = true;
            _state.Broadcaster.Start();
            // Just a precaution, shouldn't happen though.
            if (_gameSettings.AllowSpectators == false)
            {
                foreach (var p in _state.Players)
                {
                    if (p.IsSpectator)
                        p.Kick(false, L.D.ServerMessage__SpectatorsNotAllowed);
                }
            }
            if (_state.IsLocal == false)
            {
                try
                {
                    var c = new ApiClient();
                    var req = new PutGameHistoryReq(_state.ApiKey,
                        _state.Game.Id.ToString());
                    foreach (var p in _state.Players.ToArray())
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
            if (_state.GetPlayer(_sender).Id == 1)
            {
                if (this.GameStarted)
                {
                    _gameSettings.AllowSpectators = allowSpectators;
                    _gameSettings.MuteSpectators = muteSpectators;
                    _state.Game.Spectators = allowSpectators;
                    _state.Broadcaster.Settings(_gameSettings.UseTwoSidedTable, allowSpectators, muteSpectators);
                }
                else
                {
                    _gameSettings.UseTwoSidedTable = twoSidedTable;
                    _gameSettings.AllowSpectators = allowSpectators;
                    _gameSettings.MuteSpectators = muteSpectators;
                    _state.Game.Spectators = allowSpectators;
                    _state.Broadcaster.Settings(twoSidedTable, allowSpectators, muteSpectators);
                }
            }
        }

        public void PlayerSettings(byte player, bool invertedTable, bool spectator)
        {
            if (this.GameStarted) return;
            PlayerInfo p;
            // The player may have left the game concurrently
            p = _state.Players.FirstOrDefault(x => x.Id == player);
            if (p == null) return;
            if (p.InvertedTable != invertedTable || p.IsSpectator != spectator)
            {
                p.InvertedTable = invertedTable;
                p.IsSpectator = spectator;
                _state.Broadcaster.PlayerSettings(player, invertedTable, spectator);
            }
        }

        public void ResetReq()
        {
            _turnNumber = 0;
            _phaseNumber = 0;
            _turnStopPlayers.Clear();
            _phaseStops.Clear();
            _state.Broadcaster.Reset(_state.GetPlayer(_sender).Id);
        }

        public void ChatReq(string text)
        {
            var player = _state.GetPlayer(_sender);
            if (player.IsSpectator && _gameSettings.MuteSpectators)
            {
                player.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _state.Broadcaster.Chat(player.Id, text);
            if (_state.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            //if (player.Nick.Equals(_state.Game.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
            mess.Message = string.Format("{0} has left your game", player.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = _state.Game.Id;
            mess.Type = GameMessageType.Chat;
            //new Octgn.Site.Api.ApiClient().GameMessage(_state.ApiKey, mess);
        }

        public void PrintReq(string text)
        {
            var player = _state.GetPlayer(_sender);
            if (player.IsSpectator && _gameSettings.MuteSpectators)
            {
                player.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _state.Broadcaster.Print(player.Id, text);
        }

		private Random rnd = new Random();
        public void RandomReq(int min, int max)
        {
            var pi = _state.GetClient(_sender);
            var result = rnd.Next(min, max + 1);
			pi.Rpc.Random(result);
        }


        public void CounterReq(int counter, int value, bool isScriptChange)
        {
            _state.Broadcaster.Counter(_state.GetPlayer(_sender).Id, counter, value, isScriptChange);
        }

        private bool ValidateHello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool spectator)
        {
            if (_state.KickedPlayers.Contains(pkey))
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsMuted);
                return false;
            }
            // One should say Hello only once
            if (_state.SaidHello(_sender))
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
                Log.Warn($"User {nick} tried to say hello for the game id {lGameId}, but the game id {_gameId} was expected.");
                ErrorAndCloseConnection(L.D.ServerMessage__InvalidGame_Format, _state.Game.GameName);
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
            var str = string.Format(message, args);
            Log.Warn($"{nameof(ErrorAndCloseConnection)}: {str}");
            var pi = _state.GetClient(_sender);
            pi.Kick(false, str);
            _state.RemoveClient(pi);
        }

        public void Hello(string nick, string userId, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool playerIsSpectator)
        {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, playerIsSpectator)) return;
            if (playerIsSpectator && _state.Game.Spectators == false)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsNotAllowed);
                return;
            }
            // Check if we accept new players
            if (!AcceptingNewPlayers && playerIsSpectator == false)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__GameStartedNotAcceptingNewPlayers);
                return;
            }
            _state.HasSomeoneJoined = true;
            // Create the new endpoint
            IClientCalls senderRpc = new BinarySenderStub(_sender, this);
            var software = client + " (" + clientVer + ')';
            var pi = _state.GetClient(_sender);
            pi.Setup(_playerId++, nick, userId, pkey, senderRpc, software, playerIsSpectator);
            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                pi.Rpc.Binary();
                pi.Rpc = senderRpc = new BinarySenderStub(_sender, this);
                pi.Binary = true;
            }
            // decide players side of table; before saying hello so new player not included
            var aPlayers = (short)_state.Players.Count(x => !x.InvertedTable);
            var bPlayers = (short)_state.Players.Count(x => x.InvertedTable);
            if (aPlayers > bPlayers) pi.InvertedTable = true;
            if (playerIsSpectator)
                pi.InvertedTable = false;

            pi.SaidHello = true;
            // Welcome newcomer and asign them their side
            senderRpc.Welcome(pi.Id, _state.Game.Id, GameStarted);
            senderRpc.PlayerSettings(pi.Id, pi.InvertedTable, pi.IsSpectator);
            // Notify everybody of the newcomer
            _state.Broadcaster.NewPlayer(pi.Id, nick, userId, pkey, pi.InvertedTable, playerIsSpectator);
            // Add everybody to the newcomer
            foreach (var player in _state.Players.Where(x => x.Id != pi.Id))
                senderRpc.NewPlayer(player.Id, player.Nick, player.UserId, player.Pkey, player.InvertedTable, player.IsSpectator);
            // Notify the newcomer of table sides
            senderRpc.Settings(_gameSettings.UseTwoSidedTable, _gameSettings.AllowSpectators, _gameSettings.MuteSpectators);
            // Add it to our lists
            _state.Broadcaster.RefreshTypes();
            if (GameStarted)
            {
                Log.Info("Game already started, sending 'Start' message.");
                senderRpc.Start();
            }
            else
            {
                if (_state.IsLocal != false) return;
                var mess = new GameMessage();
                // don't send if we join our own room...that'd be annoying
                if (userId.Equals(_state.Game.HostUser.Id, StringComparison.InvariantCultureIgnoreCase)) return;
                mess.Message = string.Format("{0} has joined your game", nick);
                mess.Sent = DateTime.Now;
                mess.SessionId = _state.Game.Id;
                mess.Type = GameMessageType.Event;
                new Octgn.Site.Api.ApiClient().GameMessage(_state.ApiKey, mess);
            }
        }

        public void HelloAgain(byte pid, string nick, string userId, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId, Version gameVer, string password)
        {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, false)) return;
            // Make sure the pid is one that exists
            var pi = _state.GetPlayer(pid);
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

            var software = client + " (" + clientVer + ')';

            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                pi.Rpc.Binary();
                pi.Rpc = senderRpc = new BinarySenderStub(_sender, this);
                pi.Binary = true;
            }
            pi.SaidHello = true;
            // welcome the player and assign them their side
            senderRpc.Welcome(pi.Id, _state.Game.Id, true);
            senderRpc.PlayerSettings(pi.Id, pi.InvertedTable, pi.IsSpectator);
            // Notify everybody of the newcomer
            _state.Broadcaster.NewPlayer(pi.Id, nick, userId, pkey, pi.InvertedTable, pi.IsSpectator);
            // Add everybody to the newcomer
            foreach (var player in _state.Players.Where(x => x.Id != pi.Id))
                senderRpc.NewPlayer(player.Id, player.Nick, player.UserId, player.Pkey, player.InvertedTable, player.IsSpectator);
            // Notify the newcomer of some shared settings
            senderRpc.Settings(_gameSettings.UseTwoSidedTable, _gameSettings.AllowSpectators, _gameSettings.MuteSpectators);
            foreach (var player in _state.Players)
                senderRpc.PlayerSettings(player.Id, player.InvertedTable, player.IsSpectator);
            // Add it to our lists
            pi.Connected = true;
            pi.ResetSocket(_sender);
            pi.Connected = true;
            _state.UpdateDcPlayer(pi.Nick, false);
            _state.Broadcaster.RefreshTypes();

            Log.Info("HelloAgain, so sending 'Start'");
            senderRpc.Start();
        }

        public void LoadDeck(int[] id, Guid[] type, int[] @group, string[] size, string sleeveString, bool limited)        {
            short s = _state.GetPlayer(_sender).Id;
            for (var i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);

            var sstring = "";
            try
            {
                var split = sleeveString.Split(new char[1] { '\t' }, 2);
                if (split.Length == 2)
                {
                    if (int.TryParse(split[0], out var sid)) {
                        if (Uri.TryCreate(split[1], UriKind.Absolute, out var url)) {
                            if (_state.IsLocal == false) {
                                // Check if the user can even do this
                                var p = _state.GetPlayer(_sender);
                                var c = new ApiClient();
                                var resp = c.CanUseSleeve(p.Nick, sid);
                                if (resp.Authorized) {
                                    sstring = split[1];
                                }
                            } else {
                                sstring = split[1];
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (_state.IsLocal)
                    Log.Warn("LoadDeck", e);
                else
                    Log.Error("LoadDeck", e);
            }
            _state.Broadcaster.LoadDeck(id, type, group, size, sstring, limited);
        }

        public void CreateCard(int[] id, Guid[] type, string[] size, int @group)        {
            short s = _state.GetPlayer(_sender).Id;
            for (var i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _state.Broadcaster.CreateCard(id, type, size, group);
        }

        public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            short s = _state.GetPlayer(_sender).Id;
            for (var i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _state.Broadcaster.CreateCardAt(id, modelId, x, y, faceUp, persist);
        }

        public void NextTurn(byte nextPlayer, bool setActive, bool force)
        {
            if (!force)
            {
                // find the first phase that a player has stopped
                var firstStop = _phaseStops.Where(x => x.Item1 > _phaseNumber).OrderBy(x => x.Item1).FirstOrDefault();
                if (firstStop != null) //if there's a phase stop set
                {
                    var stopPlayers = _phaseStops.Where(x => x.Item1 == firstStop.Item1).Select(x => x.Item2);
                    _phaseNumber = firstStop.Item1;
                    _state.Broadcaster.SetPhase(_phaseNumber, stopPlayers.ToArray(), force);
                    return;
                }
                // check if a player has the end of turn stopped
                if (_turnStopPlayers.Count > 0)
                {
                    var stopPlayerId = _turnStopPlayers.First();
                    _turnStopPlayers.Remove(stopPlayerId);
                    _state.Broadcaster.StopTurn(stopPlayerId);
                    return;
                }
            }
            _turnNumber++;
            _phaseNumber = 0;
            _state.Broadcaster.NextTurn(nextPlayer, setActive, force);
        }

        public void StopTurnReq(int lTurnNumber, bool stop)
        {
            if (lTurnNumber != _turnNumber) return; // Message StopTurn crossed a NextTurn message
            var id = _state.GetPlayer(_sender).Id;
            if (stop)
                _turnStopPlayers.Add(id);
            else
                _turnStopPlayers.Remove(id);
        }

        public void StopPhaseReq(byte phase, bool stop)
        {
            var tuple = new Tuple<byte, byte>(phase, _state.GetPlayer(_sender).Id);
            if (stop)
            {
                if (!_phaseStops.Contains(tuple))
                    _phaseStops.Add(tuple);
            }
            else
            {
                if (_phaseStops.Contains(tuple))
                    _phaseStops.Remove(tuple);
            }
        }

        public void SetPhaseReq(byte phase, bool force)
        {
            if (force == false && phase > _phaseNumber)
            {
                // find the first phase that a player has stopped
                var firstStop = _phaseStops.Where(x => x.Item1 > _phaseNumber).OrderBy(x => x.Item1).FirstOrDefault();
                if (firstStop != null && phase > firstStop.Item1) //if there's a phase stop set earlier than the desired phase
                {
                    var stopPlayers = _phaseStops.Where(x => x.Item1 == firstStop.Item1).Select(x => x.Item2);
                    _phaseNumber = firstStop.Item1;
                    _state.Broadcaster.SetPhase(_phaseNumber, stopPlayers.ToArray(), force);
                    return;
                }
            }
            _phaseNumber = phase;
            _state.Broadcaster.SetPhase(phase, new byte[0], force);
        }


        public void SetActivePlayer(byte player)
        {
            _turnStopPlayers.Clear();
            _state.Broadcaster.SetActivePlayer(player);
        }

        public void ClearActivePlayer()
        {
            _turnStopPlayers.Clear();
            _state.Broadcaster.ClearActivePlayer();
        }

        public void PlayerSetGlobalVariable(byte p, string name, string oldvalue, string value)
        {
            _state.Broadcaster.PlayerSetGlobalVariable(p, name, oldvalue, value);
        }

        public void SetGlobalVariable(string name, string oldvalue, string value)
        {
            _state.Broadcaster.SetGlobalVariable(name, oldvalue, value);
        }

        public void CardSwitchTo(byte uid, int c, string alternate)
        {
            _state.Broadcaster.CardSwitchTo(uid, c, alternate);
        }

        public void MoveCardReq(int[] id, int to, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _state.Broadcaster.MoveCard(_state.GetPlayer(_sender).Id, id, to, idx, faceUp, isScriptMove);
        }

        public void MoveCardAtReq(int[] card, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            _state.Broadcaster.MoveCardAt(_state.GetPlayer(_sender).Id, card, x, y, idx, faceUp, isScriptMove);
        }

        public void AddMarkerReq(int card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _state.Broadcaster.AddMarker(_state.GetPlayer(_sender).Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void RemoveMarkerReq(int card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _state.Broadcaster.RemoveMarker(_state.GetPlayer(_sender).Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void TransferMarkerReq(int from, int to, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _state.Broadcaster.TransferMarker(_state.GetPlayer(_sender).Id, from, to, id, name, count, oldCount, isScriptChange);
        }

        public void NickReq(string nick)
        {
            var pi = _state.GetPlayer(_sender);
            pi.Nick = nick;
            _state.Broadcaster.Nick(pi.Id, nick);
        }

        public void PeekReq(int card)
        {
            _state.Broadcaster.Peek(_state.GetPlayer(_sender).Id, card);
        }

        public void UntargetReq(int card, bool isScriptChange)
        {
            _state.Broadcaster.Untarget(_state.GetPlayer(_sender).Id, card, isScriptChange);
        }

        public void TargetReq(int card, bool isScriptChange)
        {
            _state.Broadcaster.Target(_state.GetPlayer(_sender).Id, card, isScriptChange);
        }

        public void TargetArrowReq(int card, int otherCard, bool isScriptChange)
        {
            _state.Broadcaster.TargetArrow(_state.GetPlayer(_sender).Id, card, otherCard, isScriptChange);
        }

        public void Highlight(int card, string color)
        {
            _state.Broadcaster.Highlight(card, color);
        }

        public void Filter(int card, string color)
        {
            _state.Broadcaster.Filter(card, color);
        }

        public void TurnReq(int card, bool up)
        {
            _state.Broadcaster.Turn(_state.GetPlayer(_sender).Id, card, up);
        }

        public void RotateReq(int card, CardOrientation rot)
        {
            _state.Broadcaster.Rotate(_state.GetPlayer(_sender).Id, card, rot);
        }

        public void Shuffled(byte player, int group, int[] card, short[] pos)
        {
            _state.Broadcaster.Shuffled(player, group, card, pos);
        }

        public void PassToReq(int id, byte player, bool requested)
        {
            _state.Broadcaster.PassTo(_state.GetPlayer(_sender).Id, id, player, requested);
        }

        public void TakeFromReq(int id, byte fromPlayer)
        {
            _state.GetPlayer(fromPlayer).Rpc.TakeFrom(id, _state.GetPlayer(_sender).Id);
        }

        public void DontTakeReq(int id, byte toPlayer)
        {
            _state.GetPlayer(toPlayer).Rpc.DontTake(id);
        }

        public void FreezeCardsVisibility(int group)
        {
            _state.Broadcaster.FreezeCardsVisibility(group);
        }

        public void GroupVisReq(int id, bool defined, bool visible)
        {
            _state.Broadcaster.GroupVis(_state.GetPlayer(_sender).Id, id, defined, visible);
        }

        public void GroupVisAddReq(int gId, byte pId)
        {
            _state.Broadcaster.GroupVisAdd(_state.GetPlayer(_sender).Id, gId, pId);
        }

        public void GroupVisRemoveReq(int gId, byte pId)
        {
            _state.Broadcaster.GroupVisRemove(_state.GetPlayer(_sender).Id, gId, pId);
        }

        public void LookAtReq(int uid, int gId, bool look)
        {
            _state.Broadcaster.LookAt(_state.GetPlayer(_sender).Id, uid, gId, look);
        }

        public void LookAtTopReq(int uid, int gId, int count, bool look)
        {
            _state.Broadcaster.LookAtTop(_state.GetPlayer(_sender).Id, uid, gId, count, look);
        }

        public void LookAtBottomReq(int uid, int gId, int count, bool look)
        {
            _state.Broadcaster.LookAtBottom(_state.GetPlayer(_sender).Id, uid, gId, count, look);
        }

        public void StartLimitedReq(Guid[] packs)
        {
            _state.Broadcaster.StartLimited(_state.GetPlayer(_sender).Id, packs);
        }

        public void CancelLimitedReq()
        {
            _state.Broadcaster.CancelLimited(_state.GetPlayer(_sender).Id);
        }

        public void AddPacksReq(Guid[] packs, bool selfOnly)
        {
            _state.Broadcaster.AddPacks(_state.GetPlayer(_sender).Id, packs, selfOnly);
        }
        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            _state.Broadcaster.IsTableBackgroundFlipped(isFlipped);
        }

        #endregion IRemoteCalls interface

        // This class contains high-level infos about connected clients

        internal void Ping()
        {
            _sender.OnPingReceived();
        }

        public void PlaySound(byte player, string soundName)
        {
            _state.Broadcaster.PlaySound(player, soundName);
        }

        public void Ready(byte player)
        {
            _state.Broadcaster.Ready(player);
        }

        public void RemoteCall(byte player, string func, string args)
        {
            _state.GetPlayer(player).Rpc.RemoteCall(_state.GetPlayer(_sender).Id, func, args);
        }

        public void ShuffleDeprecated(int arg0, int[] ints)
        {
            _state.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void UnaliasGrpDeprecated(int arg0)
        {
            _state.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void UnaliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            _state.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void CreateAliasDeprecated(int[] arg0, ulong[] ulongs)
        {
            _state.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void GameState(byte player, string state)
        {
            _state.GetPlayer(player).Rpc.GameState(_state.GetPlayer(_sender).Id, state);
        }

        public void GameStateReq(byte toPlayer)
        {
            _state.GetPlayer(toPlayer).Rpc.GameStateReq(_state.GetPlayer(_sender).Id);
        }

        public void DeleteCard(int cardId, byte playerId)
        {
            _state.Broadcaster.DeleteCard(cardId, playerId);
        }

        public void Leave(byte player)
        {
            var info = _state.GetPlayer(_sender);
            // If the client is not registered, do nothing
            if (info == null) return;
            _state.RemoveClient(info);
            info.Connected = false;
            // Notify everybody that the player has left the game
            _state.Broadcaster.Leave(info.Id);
            if (_state.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            if (info.UserId.Equals(_state.Game.HostUser.Id, StringComparison.InvariantCultureIgnoreCase)) return;
            mess.Message = string.Format("{0} has left your game", info.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = _state.Game.Id;
            mess.Type = GameMessageType.Event;
            new Octgn.Site.Api.ApiClient().GameMessage(_state.ApiKey, mess);
        }

        public void Boot(byte player, string reason)
        {
            var p = _state.GetPlayer(_sender);
            var bplayer = _state.GetPlayer(player);
            if (bplayer == null)
            {
                _state.Broadcaster.Error(string.Format(L.D.ServerMessage__CanNotBootPlayerDoesNotExist, p.Nick));
                return;
            }
            if (p.Id != 1)
            {
                _state.Broadcaster.Error(string.Format(L.D.ServerMessage__CanNotBootNotHost, p.Nick, bplayer.Nick));
                return;
            }
            _state.AddKickedPlayer(bplayer);
            bplayer.Kick(false, reason);
            _state.Broadcaster.Leave(bplayer.Id);
        }

        internal void AnchorCard(int card, byte player, bool anchor)
        {
            _state.Broadcaster.AnchorCard(card, player, anchor);
        }

        internal void SetCardProperty(int card, byte player, string name, string val, string valtype)
        {
            _state.Broadcaster.SetCardProperty(card, player, name, val, valtype);
        }

        public void ResetCardProperties(int card, byte player)
        {
            _state.Broadcaster.ResetCardProperties(card, player);
        }

        public void SetBoard(string name)
        {
            _state.Broadcaster.SetBoard(name);
        }

	    public void SetPlayerColor(byte player, string colorHex)
	    {
		    _state.Broadcaster.SetPlayerColor(player, colorHex);
	    }
    }
}
