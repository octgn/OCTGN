/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Octgn.Data;
using Octgn.Library.Localization;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;
using Octgn.Online.Library.Models;
using System.Runtime.CompilerServices;

namespace Octgn.Server
{
    public sealed class RequestHandler : IRemoteCalls
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [ThreadStatic]
        private static RequestContext Context;

        private static readonly bool IsDebug;
        private const string ServerName = "OCTGN.NET";
        private static readonly Version ServerVersion = GetServerVersion(); //unused
        private static Version GetServerVersion()
        {
            Assembly asm = typeof(Server).Assembly;
            return asm.GetName().Version;
        }

        private State _state;

        private readonly Broadcaster _broadcaster; // Stub to broadcast messages
        private readonly GameSettings _gameSettings;
        private readonly IHostedGameState _gameState;
        internal int muted;

        static RequestHandler() {
#if (DEBUG)
            IsDebug = true;
#endif
        }

        // C'tor
        public RequestHandler(GameSettings gameSettings, IHostedGameState game, State state)
        {
            _state = state;
            _gameSettings = gameSettings;
            _gameState = game;
            // Init fields
            _broadcaster = new Broadcaster(this);
        }

        public bool InitializeRequest(RequestContext context, [CallerMemberName] string request = null) {
            Context = context;
            // Check if this is the first message received
            if (!Context.Sender.SaidHello)
            {
                var acceptableMessages = new string[]
                    {
                        nameof(IRemoteCalls.Hello),
                        nameof(IRemoteCalls.HelloAgain),
                    };
                //TODO Maybe we shouldn't kill the connection here
                //     Basically, if someone dc's it's possible that
                //     a network call gets sent up on accident before HelloAgain,
                //     which effectivly kills the game.
                //     Maybe need a flag on the player saying they at least said
                //     hello once.

                // A new connection must always start with a hello message, send error message back
                if (!acceptableMessages.Contains(request))
                {
                    Context.Sender.Rpc.Error(L.D.ServerMessage__FailedToSendHelloMessage);
                    return false;
                }
            }
            Context.Sender.Socket.OnPingReceived();
            return true;
        }

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
            _state.Engine.SetStatus(Online.Library.Enums.EnumHostedGameStatus.GameStarted);
            _gameState.AcceptingPlayers = false;
            _broadcaster.Start();
            // Just a precaution, shouldn't happen though.
            if (_gameSettings.AllowSpectators == false)
            {
                foreach (var p in _state.Players)
                {
                    if (p.IsSpectator)
                        p.Kick(false, L.D.ServerMessage__SpectatorsNotAllowed);
                }
            }
            if (_state.Engine.IsLocal == false)
            {
                try
                {
                    var c = new ApiClient();
                    var req = new PutGameHistoryReq(_state.Engine.ApiKey,
                        _gameState.Id.ToString());
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
            if (Context.Sender.Id != 1) return;

            if (_gameState.Status == Online.Library.Enums.EnumHostedGameStatus.GameStarted)
                twoSidedTable = _gameSettings.UseTwoSidedTable; // Can't change this after the game started.
            else _gameSettings.UseTwoSidedTable = twoSidedTable;

            _gameSettings.AllowSpectators = allowSpectators;
            _gameSettings.MuteSpectators = muteSpectators;
            _gameState.Spectators = allowSpectators;
            _broadcaster.Settings(twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(byte player, bool invertedTable, bool spectator)
        {
            if (_gameState.Status == Online.Library.Enums.EnumHostedGameStatus.GameStarted) return;
            PlayerInfo p;
            // The player may have left the game concurrently
            p = _state.Players.FirstOrDefault(x => x.Id == player);
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
            _gameState.CurrentTurnNumber = 0;
            _gameState.TurnStopPlayers.Clear();
            _gameState.PhaseStopPlayers.Clear();
            _broadcaster.Reset(Context.Sender.Id);
        }

        public void ChatReq(string text)
        {
            if (Context.Sender.IsSpectator && _gameSettings.MuteSpectators)
            {
                Context.Sender.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _broadcaster.Chat(Context.Sender.Id, text);
            if (_state.Engine.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            mess.Message = string.Format("{0} has left your game", Context.Sender.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = _gameState.Id;
            mess.Type = GameMessageType.Chat;
        }

        public void PrintReq(string text)
        {
            if (Context.Sender.IsSpectator && _gameSettings.MuteSpectators)
            {
                Context.Sender.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _broadcaster.Print(Context.Sender.Id, text);
        }

		private Random rnd = new Random();
        public void RandomReq(int min, int max)
        {
            var result = rnd.Next(min, max + 1);
            Context.Sender.Rpc.Random(result);
        }


        public void CounterReq(int counter, int value, bool isScriptChange)
        {
            _broadcaster.Counter(Context.Sender.Id, counter, value, isScriptChange);
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
            if (_state.SaidHello(Context.Sender.Socket))
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SayHelloOnlyOnce);
                return false;
            }

            // Verify password
            if (!string.IsNullOrWhiteSpace(_gameState.Password))
            {
                if (!password.Equals(_gameState.Password))
                {
                    ErrorAndCloseConnection(L.D.ServerMessage__IncorrectPassword);
                    return false;
                }
            }
            // Check if the versions are compatible
            if (!IsDebug) {
                if (clientVer.CompareTo(ServerVersion) < 0)
                //if ((clientVer.Major != ServerVersion.Major || clientVer.Minor != ServerVersion.Minor))
                {
                    ErrorAndCloseConnection(string.Format(L.D.ServerMessage__IncompatibleOctgnClient_Format,
                                            ServerVersion, clientVer));
                    return false;
                }
            }
            // Check if the client wants to play the correct game
            if (lGameId != _gameState.GameId)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__InvalidGame_Format, _gameState.GameName);
                return false;
            }
            // Check if the client's major game version matches ours
            if (gameVer.Major != _gameState.GameVersion.Major)
            {
                ErrorAndCloseConnection(
                    L.D.ServerMessage__IncompatibleGameVersion_Format,
                    _gameState.GameVersion);
                return false;
            }
            return true;
        }

        private void ErrorAndCloseConnection(string message, params object[] args)
        {
            Context.Sender.Kick(false, message, args);
            _state.RemoveClient(Context.Sender);
        }

        public void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool spectator)
        {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, spectator)) return;
            if (spectator && _gameState.Spectators == false)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsNotAllowed);
                return;
            }
            // Check if we accept new players
            if (!_gameState.AcceptingPlayers && spectator == false)
            {
                ErrorAndCloseConnection(L.D.ServerMessage__GameStartedNotAcceptingNewPlayers);
                return;
            }
            _state.HasSomeoneJoined = true;
            // Create the new endpoint
            IClientCalls senderRpc = new BinarySenderStub(Context.Sender.Socket, this);
            string software = client + " (" + clientVer + ')';
            Context.Sender.Setup(_gameState.NextPlayerId++, nick, pkey, senderRpc, software, spectator);
            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                Context.Sender.Rpc.Binary();
                Context.Sender.Rpc = senderRpc = new BinarySenderStub(Context.Sender.Socket, this);
                Context.Sender.Binary = true;
            }
            // decide players side of table; before saying hello so new player not included
            short aPlayers = (short)_state.Players.Count(x => !x.InvertedTable);
            short bPlayers = (short)_state.Players.Count(x => x.InvertedTable);
            if (aPlayers > bPlayers) Context.Sender.InvertedTable = true;
            if (spectator)
                Context.Sender.InvertedTable = false;

            Context.Sender.SaidHello = true;
            // Welcome newcomer and asign them their side
            senderRpc.Welcome(Context.Sender.Id, _gameState.Id, _gameState.Status == Online.Library.Enums.EnumHostedGameStatus.GameStarted);
            senderRpc.PlayerSettings(Context.Sender.Id, Context.Sender.InvertedTable, Context.Sender.IsSpectator);
            // Notify everybody of the newcomer
            _broadcaster.NewPlayer(Context.Sender.Id, nick, pkey, Context.Sender.InvertedTable, spectator);
            // Add everybody to the newcomer
            foreach (PlayerInfo player in _state.Players.Where(x => x.Id != Context.Sender.Id))
                senderRpc.NewPlayer(player.Id, player.Nick, player.Pkey, player.InvertedTable, player.IsSpectator);
            // Notify the newcomer of table sides
            senderRpc.Settings(_gameSettings.UseTwoSidedTable, _gameSettings.AllowSpectators, _gameSettings.MuteSpectators);
            // Add it to our lists
            _broadcaster.RefreshTypes();
            if (_gameState.Status == Online.Library.Enums.EnumHostedGameStatus.GameStarted)
            {
                senderRpc.Start();
            }
            else
            {
                if (_state.Engine.IsLocal != false) return;
                var mess = new GameMessage();
                // don't send if we join our own room...that'd be annoying
                if (nick.Equals(_gameState.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
                mess.Message = string.Format("{0} has joined your game", nick);
                mess.Sent = DateTime.Now;
                mess.SessionId = _gameState.Id;
                mess.Type = GameMessageType.Event;
                new Octgn.Site.Api.ApiClient().GameMessage(_state.Engine.ApiKey, mess);
            }
        }

        public void HelloAgain(byte pid, string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId, Version gameVer, string password)
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
            IClientCalls senderRpc = new BinarySenderStub(Context.Sender.Socket, this);
            pi.Rpc = senderRpc;

            string software = client + " (" + clientVer + ')';

            // Check if one can switch to Binary mode
            if (client == ServerName)
            {
                pi.Rpc.Binary();
                pi.Rpc = senderRpc = new BinarySenderStub(Context.Sender.Socket, this);
                pi.Binary = true;
            }
            pi.SaidHello = true;
            // welcome the player and assign them their side
            senderRpc.Welcome(pi.Id, _gameState.Id, true);
            senderRpc.PlayerSettings(pi.Id, pi.InvertedTable, pi.IsSpectator);
            // Notify everybody of the newcomer
            _broadcaster.NewPlayer(pi.Id, nick, pkey, pi.InvertedTable, pi.IsSpectator);
            // Add everybody to the newcomer
            foreach (PlayerInfo player in _state.Players.Where(x => x.Id != pi.Id))
                senderRpc.NewPlayer(player.Id, player.Nick, player.Pkey, player.InvertedTable, player.IsSpectator);
            // Notify the newcomer of some shared settings
            senderRpc.Settings(_gameSettings.UseTwoSidedTable, _gameSettings.AllowSpectators, _gameSettings.MuteSpectators);
            foreach (PlayerInfo player in _state.Players)
                senderRpc.PlayerSettings(player.Id, player.InvertedTable, player.IsSpectator);
            // Add it to our lists
            pi.Connected = true;
            pi.ResetSocket(Context.Sender.Socket);
            pi.Connected = true;
            _state.UpdateDcPlayer(pi.Nick, false);
            _broadcaster.RefreshTypes();
            senderRpc.Start();
        }

        public void LoadDeck(int[] id, Guid[] type, int[] @group, string[] size, string sleeveString, bool limited)        {
            short s = Context.Sender.Id;
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
                            if (_state.Engine.IsLocal == false)
                            {
                                // Check if the user can even do this
                                var c = new ApiClient();
                                var resp = c.CanUseSleeve(Context.Sender.Nick, sid);
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
                if (_state.Engine.IsLocal)
                    Log.Warn("LoadDeck", e);
                else
                    Log.Error("LoadDeck", e);
            }
            _broadcaster.LoadDeck(id, type, group, size, sstring, limited);
        }

        public void CreateCard(int[] id, Guid[] type, string[] size, int @group)        {
            short s = Context.Sender.Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateCard(id, type, size, group);
        }

        public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            short s = Context.Sender.Id;
            for (int i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _broadcaster.CreateCardAt(id, modelId, x, y, faceUp, persist);
        }

        public void NextTurn(byte nextPlayer)
        {
            if (_gameState.TurnStopPlayers.Count > 0)
            {
                byte stopPlayerId = _gameState.TurnStopPlayers.First();
                _gameState.TurnStopPlayers.Remove(stopPlayerId);
                _broadcaster.StopTurn(stopPlayerId);
                return;
            }
            _gameState.CurrentTurnNumber++;
            _gameState.PhaseStopPlayers.Clear();
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
            if (lTurnNumber != _gameState.CurrentTurnNumber) return; // Message StopTurn crossed a NextTurn message
            byte id = Context.Sender.Id;
            if (stop)
                _gameState.TurnStopPlayers.Add(id);
            else
                _gameState.TurnStopPlayers.Remove(id);
        }

        public void CardSwitchTo(byte uid, int c, string alternate)
        {
            _broadcaster.CardSwitchTo(uid, c, alternate);
        }

        public void MoveCardReq(int[] id, int to, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _broadcaster.MoveCard(Context.Sender.Id, id, to, idx, faceUp, isScriptMove);
        }

        public void MoveCardAtReq(int[] card, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            _broadcaster.MoveCardAt(Context.Sender.Id, card, x, y, idx, faceUp, isScriptMove);
        }

        public void AddMarkerReq(int card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _broadcaster.AddMarker(Context.Sender.Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void RemoveMarkerReq(int card, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _broadcaster.RemoveMarker(Context.Sender.Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void TransferMarkerReq(int from, int to, Guid id, string name, ushort count, ushort oldCount, bool isScriptChange)
        {
            _broadcaster.TransferMarker(Context.Sender.Id, from, to, id, name, count, oldCount, isScriptChange);
        }

        public void NickReq(string nick)
        {
            Context.Sender.Nick = nick;
            _broadcaster.Nick(Context.Sender.Id, nick);
        }

        public void PeekReq(int card)
        {
            _broadcaster.Peek(Context.Sender.Id, card);
        }

        public void UntargetReq(int card, bool isScriptChange)
        {
            _broadcaster.Untarget(Context.Sender.Id, card, isScriptChange);
        }

        public void TargetReq(int card, bool isScriptChange)
        {
            _broadcaster.Target(Context.Sender.Id, card, isScriptChange);
        }

        public void TargetArrowReq(int card, int otherCard, bool isScriptChange)
        {
            _broadcaster.TargetArrow(Context.Sender.Id, card, otherCard, isScriptChange);
        }

        public void Highlight(int card, string color)
        {
            _broadcaster.Highlight(card, color);
        }

        public void Filter(int card, string color)
        {
            _broadcaster.Filter(card, color);
        }

        public void TurnReq(int card, bool up)
        {
            _broadcaster.Turn(Context.Sender.Id, card, up);
        }

        public void RotateReq(int card, CardOrientation rot)
        {
            _broadcaster.Rotate(Context.Sender.Id, card, rot);
        }

        public void Shuffled(byte player, int group, int[] card, short[] pos)
        {
            _broadcaster.Shuffled(player, group, card, pos);
        }

        public void PassToReq(int id, byte player, bool requested)
        {
            _broadcaster.PassTo(Context.Sender.Id, id, player, requested);
        }

        public void TakeFromReq(int id, byte fromPlayer)
        {
            _state.GetPlayer(fromPlayer).Rpc.TakeFrom(id, Context.Sender.Id);
        }

        public void DontTakeReq(int id, byte toPlayer)
        {
            _state.GetPlayer(toPlayer).Rpc.DontTake(id);
        }

        public void FreezeCardsVisibility(int group)
        {
            _broadcaster.FreezeCardsVisibility(group);
        }

        public void GroupVisReq(int id, bool defined, bool visible)
        {
            _broadcaster.GroupVis(Context.Sender.Id, id, defined, visible);
        }

        public void GroupVisAddReq(int gId, byte pId)
        {
            _broadcaster.GroupVisAdd(Context.Sender.Id, gId, pId);
        }

        public void GroupVisRemoveReq(int gId, byte pId)
        {
            _broadcaster.GroupVisRemove(Context.Sender.Id, gId, pId);
        }

        public void LookAtReq(int uid, int gId, bool look)
        {
            _broadcaster.LookAt(Context.Sender.Id, uid, gId, look);
        }

        public void LookAtTopReq(int uid, int gId, int count, bool look)
        {
            _broadcaster.LookAtTop(Context.Sender.Id, uid, gId, count, look);
        }

        public void LookAtBottomReq(int uid, int gId, int count, bool look)
        {
            _broadcaster.LookAtBottom(Context.Sender.Id, uid, gId, count, look);
        }

        public void StartLimitedReq(Guid[] packs)
        {
            _broadcaster.StartLimited(Context.Sender.Id, packs);
        }

        public void CancelLimitedReq()
        {
            _broadcaster.CancelLimited(Context.Sender.Id);
        }

        public void AddPacksReq(Guid[] packs, bool selfOnly)
        {
            _broadcaster.AddPacks(Context.Sender.Id, packs, selfOnly);
        }
        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            _broadcaster.IsTableBackgroundFlipped(isFlipped);
        }

        #endregion IRemoteCalls interface

        // This class contains high-level infos about connected clients

        internal void Ping()
        {
            Context.Sender.Socket.OnPingReceived();
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
            _state.GetPlayer(player).Rpc.RemoteCall(Context.Sender.Id, func, args);
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
            _state.GetPlayer(player).Rpc.GameState(Context.Sender.Id, state);
        }

        public void GameStateReq(byte toPlayer)
        {
            _state.GetPlayer(toPlayer).Rpc.GameStateReq(Context.Sender.Id);
        }

        public void DeleteCard(int cardId, byte playerId)
        {
            _broadcaster.DeleteCard(cardId, playerId);
        }

        public void Leave(byte player)
        {
            PlayerInfo info = Context.Sender;
            // If the client is not registered, do nothing
            if (info == null) return;
            _state.RemoveClient(info);
            info.Connected = false;
            // Notify everybody that the player has left the game
            _broadcaster.Leave(info.Id);
            if (_state.Engine.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            if (info.Nick.Equals(_gameState.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
            mess.Message = string.Format("{0} has left your game", info.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = _gameState.Id;
            mess.Type = GameMessageType.Event;
            new Octgn.Site.Api.ApiClient().GameMessage(_state.Engine.ApiKey, mess);
        }

        public void Boot(byte player, string reason)
        {
            var p = Context.Sender;
            var bplayer = _state.GetPlayer(player);
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
            _state.AddKickedPlayer(bplayer);
            bplayer.Kick(false, reason);
            _broadcaster.Leave(bplayer.Id);
        }

        internal void AnchorCard(int card, byte player, bool anchor)
        {
            _broadcaster.AnchorCard(card, player, anchor);
        }

        internal void SetCardProperty(int card, byte player, string name, string val, string valtype)
        {
            _broadcaster.SetCardProperty(card, player, name, val, valtype);
        }

        public void ResetCardProperties(int card, byte player)
        {
            _broadcaster.ResetCardProperties(card, player);
        }

        public void SetBoard(string name)
        {
            _broadcaster.SetBoard(name);
        }

	    public void SetPlayerColor(byte player, string colorHex)
	    {
		    _broadcaster.SetPlayerColor(player, colorHex);
	    }

        public void SetPhase(byte phase, byte nextPhase)
        {
            var stopPlayers = _gameState.PhaseStopPlayers.Where(x => x.Item2 == phase).ToList();
            if (stopPlayers.Count > 0)
            {
                var stopPlayer = stopPlayers.First();
                _gameState.PhaseStopPlayers.Remove(stopPlayer);
                _broadcaster.StopPhase(stopPlayer.Item1, stopPlayer.Item2);
                return;
            }
            _broadcaster.SetPhase(phase, nextPhase);
        }

        public void StopPhaseReq(int lTurnNumber, byte phase, bool stop)
        {
            if (lTurnNumber != _gameState.CurrentTurnNumber) return; // Message StopTurn crossed a NextTurn message
            var tuple = new Tuple<byte, byte>(Context.Sender.Id, phase);
            if (stop)
                if (!_gameState.PhaseStopPlayers.Contains(tuple)) _gameState.PhaseStopPlayers.Add(tuple);
            else
                if (_gameState.PhaseStopPlayers.Contains(tuple)) _gameState.PhaseStopPlayers.Remove(tuple);
        }

        public void SwitchWithAlternate() {
            throw new NotImplementedException();
        }
    }
}