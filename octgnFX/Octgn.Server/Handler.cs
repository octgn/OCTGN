/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using Octgn.Library.Localization;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

namespace Octgn.Server
{
    public sealed class Handler
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool GameStarted {
            get => _context.Game.Status == Online.Hosting.HostedGameStatus.GameInProgress;
            private set {
                if (_context.Game.Status == Online.Hosting.HostedGameStatus.GameInProgress) throw new InvalidOperationException("Can only start game once.");

                _context.Game.DateStarted = DateTimeOffset.Now;
                _context.Game.Status = Online.Hosting.HostedGameStatus.GameInProgress;
                Log.Info($"{_context.Game.GameName}: Game marked started.");
            }
        }

        public bool AcceptingNewPlayers => !GameStarted;

        private readonly GameContext _context;

        public Handler(GameContext context) {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private Player _player;

        public void SetPlayer(Player player) {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public void Binary() {
            /* This never gets called. This message gets special treatment in the server. */
        }

        public void Error(string msg) {
            Debug.WriteLine(msg);
        }

        public void Start() {
            GameStarted = true;
            _context.Broadcaster.Start();
            // Just a precaution, shouldn't happen though.
            if (_context.State.Settings.AllowSpectators == false) {
                foreach (var p in _context.State.Players.Players) {
                    if (p.Settings.IsSpectator)
                        p.Rpc.Kick(L.D.ServerMessage__SpectatorsNotAllowed);
                }
            }
            if (_context.Config.IsLocal == false) {
                try {
                    var c = new ApiClient();
                    var req = new PutGameHistoryReq(_context.Config.ApiKey,
                        _context.Game.Id.ToString());
                    foreach (var p in _context.State.Players.Players.ToArray()) {
                        req.Usernames.Add(p.Nick);
                    }
                    c.CreateGameHistory(req);
                } catch (Exception e) {
                    Log.Error("Start Error creating Game History", e);
                }
            }
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators, bool allowCardList) {
            var player = _player;

            if (player.Id != Player.HOSTPLAYERID) return;

            _context.State.Settings.AllowSpectators = allowSpectators;
            _context.State.Settings.MuteSpectators = muteSpectators;
            _context.State.Settings.AllowCardList = allowCardList;
            _context.Game.Spectators = allowSpectators;

            // We can't change this after the game is started, so don't change it
            if (!GameStarted)
                _context.State.Settings.UseTwoSidedTable = twoSidedTable;

            _context.Broadcaster.Settings(_context.State.Settings.UseTwoSidedTable, allowSpectators, muteSpectators, allowCardList);
        }

        public void PlayerSettings(byte player, bool invertedTable, bool spectator) {
            if (this.GameStarted) return;
            Player p;
            // The player may have left the game concurrently
            p = _context.State.Players.GetPlayer(player);
            if (p == null) return;
            p.Settings = new PlayerSettings(invertedTable, spectator);
        }

        public void ResetReq(bool isSoft) {
            _context.Reset(_player.Id, isSoft);
        }

        public void ChatReq(string text) {
            if (_player.Settings.IsSpectator && _context.State.Settings.MuteSpectators) {
                _player.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _context.Broadcaster.Chat(_player.Id, text);
            if (_context.Config.IsLocal != false) return;
            var mess = new GameMessage();
            // don't send if we join our own room...that'd be annoying
            //if (_player.Nick.Equals(_context.Game.HostUserName, StringComparison.InvariantCultureIgnoreCase)) return;
            mess.Message = string.Format("{0} has left your game", _player.Nick);
            mess.Sent = DateTime.Now;
            mess.SessionId = _context.Game.Id;
            mess.Type = GameMessageType.Chat;
            //new Octgn.Site.Api.ApiClient().GameMessage(_context.ApiKey, mess);
        }

        public void PrintReq(string text) {
            if (_player.Settings.IsSpectator && _context.State.Settings.MuteSpectators) {
                _player.Rpc.Error(L.D.ServerMessage__SpectatorsMuted);
                return;
            }
            _context.Broadcaster.Print(_player.Id, text);
        }

        private Random rnd = new Random();
        public void RandomReq(int min, int max) {
            var pi = _player;
            var result = rnd.Next(min, max + 1);
            pi.Rpc.Random(result);
        }


        public void CounterReq(int counter, int value, bool isScriptChange) {
            _context.Broadcaster.Counter(_player.Id, counter, value, isScriptChange);
        }

        private bool ValidateHello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool spectator) {
            if (_context.State.Players.KickedPlayers.Contains(pkey)) {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsMuted);
                return false;
            }
            // One should say Hello only once
            if (_player.SaidHello) {
                ErrorAndCloseConnection(L.D.ServerMessage__SayHelloOnlyOnce);
                return false;
            }

            // Verify password
            if (_context.Game.HasPassword) {
                if (!password.Equals(_context.Game.Password)) {
                    ErrorAndCloseConnection(L.D.ServerMessage__IncorrectPassword);
                    return false;
                }
            }
            // Check if the versions are compatible
            if(clientVer.CompareTo(_context.Config.ServerVersion) < 0)
            {
                ErrorAndCloseConnection(string.Format(L.D.ServerMessage__IncompatibleOctgnClient_Format,
                                        _context.Config.ServerVersion, clientVer));
                return false;
            }

            // Check if the client wants to play the correct game
            if (lGameId != _context.Game.GameId) {
                Log.Warn($"User {nick} tried to say hello for the game id {lGameId}, but the game id {_context.Game.GameId} was expected.");
                ErrorAndCloseConnection(L.D.ServerMessage__InvalidGame_Format, _context.Game.GameName);
                return false;
            }
            // Check if the client's major game version matches ours
            var hostedGameVersion = Version.Parse(_context.Game.GameVersion);

            if (gameVer.Major != hostedGameVersion.Major) {
                ErrorAndCloseConnection(
                    L.D.ServerMessage__IncompatibleGameVersion_Format,
                    hostedGameVersion);
                return false;
            }
            return true;
        }

        private void ErrorAndCloseConnection(string message, params object[] args) {
            var str = string.Format(message, args);
            Log.Warn($"{nameof(ErrorAndCloseConnection)}: {str}");
            _player.Rpc.Kick(str);
        }

        public void Hello(string nick, string userId, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId,
                          Version gameVer, string password, bool playerIsSpectator) {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, playerIsSpectator)) return;
            if (playerIsSpectator && _context.Game.Spectators == false) {
                ErrorAndCloseConnection(L.D.ServerMessage__SpectatorsNotAllowed);
                return;
            }
            // Check if we accept new players
            if (!AcceptingNewPlayers && playerIsSpectator == false) {
                ErrorAndCloseConnection(L.D.ServerMessage__GameStartedNotAcceptingNewPlayers);
                return;
            }
            var software = client + " (" + clientVer + ')';

            var aPlayers = (short)_context.State.Players.Players.Count(x => !x.Settings.InvertedTable);
            var bPlayers = (short)_context.State.Players.Players.Count(x => x.Settings.InvertedTable);

            var invertedTable = (aPlayers > bPlayers) && !playerIsSpectator;

            _player.Setup(_context.State.NextPlayerId(), nick, userId, pkey, software, invertedTable, playerIsSpectator, _context.State.Players);

            _player.SaidHello = true;

            // Welcome newcomer and asign them their side
            _player.Rpc.Welcome(_player.Id, _context.Game.Id, _context.Game.Name, GameStarted);
            _player.Rpc.PlayerSettings(_player.Id, _player.Settings.InvertedTable, _player.Settings.IsSpectator);

            // Notify everybody of the newcomer
            _context.Broadcaster.NewPlayer(_player.Id, nick, userId, pkey, _player.Settings.InvertedTable, _player.Settings.IsSpectator);

            // Add everybody to the newcomer
            foreach (var player in _context.State.Players.Players.Where(x => x.Id != _player.Id))
                _player.Rpc.NewPlayer(player.Id, player.Nick, player.UserId, player.Pkey, player.Settings.InvertedTable, player.Settings.IsSpectator);

            // Notify the newcomer of table sides
            _player.Rpc.Settings(_context.State.Settings.UseTwoSidedTable, _context.State.Settings.AllowSpectators, _context.State.Settings.MuteSpectators, _context.State.Settings.AllowCardList);

            // Add it to our lists
            if (GameStarted) {
                Log.Info("Game already started, sending 'Start' message.");
                _player.Rpc.Start();
            } else {
                if (_context.Config.IsLocal != false) return;
                var mess = new GameMessage();
                // don't send if we join our own room...that'd be annoying
                if (userId.Equals(_context.Game.HostUser.Id, StringComparison.InvariantCultureIgnoreCase)) return;
                mess.Message = string.Format("{0} has joined your game", nick);
                mess.Sent = DateTime.Now;
                mess.SessionId = _context.Game.Id;
                mess.Type = GameMessageType.Event;
                new Octgn.Site.Api.ApiClient().GameMessage(_context.Config.ApiKey, mess);
            }
        }

        public void HelloAgain(byte pid, string nick, string userId, ulong pkey, string client, Version clientVer, Version octgnVer, Guid lGameId, Version gameVer, string password) {
            if (!ValidateHello(nick, pkey, client, clientVer, octgnVer, lGameId, gameVer, password, false)) return;
            // Make sure the pid is one that exists
            var oldPlayerInfo = _context.State.Players.GetPlayer(pid);
            if (oldPlayerInfo == null) {
                ErrorAndCloseConnection(L.D.ServerMessage__CanNotReconnectFirstTimeConnecting);
                return;
            }

            // Make sure the pkey matches the pkey for the pid
            if (oldPlayerInfo.Pkey != pkey) {
                ErrorAndCloseConnection(L.D.ServerMessage__PublicKeyDoesNotMatch);
                return;
            }
            var software = client + " (" + clientVer + ')';

            // Setupthe old PlayerInfo to use our socket, this will reactivate it.
            oldPlayerInfo.ResetSocket(_player);

            oldPlayerInfo.SaidHello = true;
            // welcome the player and assign them their side
            oldPlayerInfo.Rpc.Welcome(oldPlayerInfo.Id, _context.Game.Id, _context.Game.Name, true);
            // Notify everybody of the newcomer
            _context.Broadcaster.NewPlayer(oldPlayerInfo.Id, nick, userId, pkey, oldPlayerInfo.Settings.InvertedTable, oldPlayerInfo.Settings.IsSpectator);

            // Add everybody to the newcomer
            foreach (var player in _context.State.Players.Players.Where(x => x.Id != oldPlayerInfo.Id))
                oldPlayerInfo.Rpc.NewPlayer(player.Id, player.Nick, player.UserId, player.Pkey, player.Settings.InvertedTable, player.Settings.IsSpectator);

            // Notify the newcomer of some shared settings
            oldPlayerInfo.Rpc.Settings(_context.State.Settings.UseTwoSidedTable, _context.State.Settings.AllowSpectators, _context.State.Settings.MuteSpectators, _context.State.Settings.AllowCardList);
            foreach (var player in _context.State.Players.Players)
                oldPlayerInfo.Rpc.PlayerSettings(player.Id, player.Settings.InvertedTable, player.Settings.IsSpectator);

            Log.Info("HelloAgain, so sending 'Start'");
            oldPlayerInfo.Rpc.Start();
        }

        public void LoadDeck(int[] id, Guid[] type, int[] @group, string[] size, string sleeveString, bool limited) {
            short s = _player.Id;
            for (var i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);

            if (!_context.Config.IsLocal) {
                // Check if the user can even do this
                var p = _player;
                if (!p.IsSubbed) {
                    sleeveString = null;
                } if(!string.IsNullOrWhiteSpace(sleeveString)) {
                    if (sleeveString.StartsWith("custom")){
                        // Disable custom sleeves for now.
                        sleeveString = null;
                    }
                }
            }

            _context.Broadcaster.LoadDeck(id, type, group, size, sleeveString ?? string.Empty, limited);
        }

        public void CreateCard(int[] id, Guid[] type, string[] size, int @group) {
            short s = _player.Id;
            for (var i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _context.Broadcaster.CreateCard(id, type, size, group);
        }

        public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist) {
            short s = _player.Id;
            for (var i = 0; i < id.Length; i++)
                id[i] = s << 16 | (id[i] & 0xffff);
            _context.Broadcaster.CreateCardAt(id, modelId, x, y, faceUp, persist);
        }

        public void NextTurn(byte nextPlayer, bool setActive, bool force) {
            if (!force) {
                // find the first phase that a player has stopped
                var firstStop = _context.State.PhaseStops.Where(x => x.Item1 > _context.State.PhaseNumber).OrderBy(x => x.Item1).FirstOrDefault();
                if (firstStop != null) //if there's a phase stop set
                {
                    var stopPlayers = _context.State.PhaseStops.Where(x => x.Item1 == firstStop.Item1).Select(x => x.Item2);
                    _context.State.PhaseNumber = firstStop.Item1;
                    _context.Broadcaster.SetPhase(_context.State.PhaseNumber, stopPlayers.ToArray(), force);
                    return;
                }
                // check if a player has the end of turn stopped
                if (_context.State.TurnStopPlayers.Count > 0) {
                    var stopPlayerId = _context.State.TurnStopPlayers.First();
                    _context.State.TurnStopPlayers.Remove(stopPlayerId);
                    _context.Broadcaster.StopTurn(stopPlayerId);
                    return;
                }
            }
            _context.State.TurnNumber++;
            _context.State.PhaseNumber = 0;
            _context.Broadcaster.NextTurn(nextPlayer, setActive, force);
        }

        public void StopTurnReq(int lTurnNumber, bool stop) {
            if (lTurnNumber != _context.State.TurnNumber) return; // Message StopTurn crossed a NextTurn message
            var id = _player.Id;
            if (stop)
                _context.State.TurnStopPlayers.Add(id);
            else
                _context.State.TurnStopPlayers.Remove(id);
        }

        public void StopPhaseReq(byte phase, bool stop) {
            var tuple = new Tuple<byte, byte>(phase, _player.Id);
            if (stop) {
                if (!_context.State.PhaseStops.Contains(tuple))
                    _context.State.PhaseStops.Add(tuple);
            } else {
                if (_context.State.PhaseStops.Contains(tuple))
                    _context.State.PhaseStops.Remove(tuple);
            }
        }

        public void SetPhaseReq(byte phase, bool force) {
            if (force == false && phase > _context.State.PhaseNumber) {
                // find the first phase that a player has stopped
                var firstStop = _context.State.PhaseStops.Where(x => x.Item1 > _context.State.PhaseNumber).OrderBy(x => x.Item1).FirstOrDefault();
                if (firstStop != null && phase > firstStop.Item1) //if there's a phase stop set earlier than the desired phase
                {
                    var stopPlayers = _context.State.PhaseStops.Where(x => x.Item1 == firstStop.Item1).Select(x => x.Item2);
                    _context.State.PhaseNumber = firstStop.Item1;
                    _context.Broadcaster.SetPhase(_context.State.PhaseNumber, stopPlayers.ToArray(), force);
                    return;
                }
            }
            _context.State.PhaseNumber = phase;
            _context.Broadcaster.SetPhase(phase, new byte[0], force);
        }


        public void SetActivePlayer(byte player) {
            _context.State.TurnStopPlayers.Clear();
            _context.Broadcaster.SetActivePlayer(player);
        }

        public void ClearActivePlayer() {
            _context.State.TurnStopPlayers.Clear();
            _context.Broadcaster.ClearActivePlayer();
        }

        public void PlayerSetGlobalVariable(byte p, string name, string oldvalue, string value) {
            _context.Broadcaster.PlayerSetGlobalVariable(p, name, oldvalue, value);
        }

        public void SetGlobalVariable(string name, string oldvalue, string value) {
            _context.Broadcaster.SetGlobalVariable(name, oldvalue, value);
        }

        public void CardSwitchTo(byte uid, int c, string alternate) {
            _context.Broadcaster.CardSwitchTo(uid, c, alternate);
        }

        public void MoveCardReq(int[] id, int to, int[] idx, bool[] faceUp, bool isScriptMove) {
            _context.Broadcaster.MoveCard(_player.Id, id, to, idx, faceUp, isScriptMove);
        }

        public void MoveCardAtReq(int[] card, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp) {
            _context.Broadcaster.MoveCardAt(_player.Id, card, x, y, idx, faceUp, isScriptMove);
        }

        public void AddMarkerReq(int card, string id, string name, ushort count, ushort oldCount, bool isScriptChange) {
            _context.Broadcaster.AddMarker(_player.Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void RemoveMarkerReq(int card, string id, string name, ushort count, ushort oldCount, bool isScriptChange) {
            _context.Broadcaster.RemoveMarker(_player.Id, card, id, name, count, oldCount, isScriptChange);
        }

        public void TransferMarkerReq(int from, int to, string id, string name, ushort count, ushort oldCount, bool isScriptChange) {
            _context.Broadcaster.TransferMarker(_player.Id, from, to, id, name, count, oldCount, isScriptChange);
        }

        public void PeekReq(int card) {
            _context.Broadcaster.Peek(_player.Id, card);
        }

        public void UntargetReq(int card, bool isScriptChange) {
            _context.Broadcaster.Untarget(_player.Id, card, isScriptChange);
        }

        public void TargetReq(int card, bool isScriptChange) {
            _context.Broadcaster.Target(_player.Id, card, isScriptChange);
        }

        public void TargetArrowReq(int card, int otherCard, bool isScriptChange) {
            _context.Broadcaster.TargetArrow(_player.Id, card, otherCard, isScriptChange);
        }

        public void Highlight(int card, string color) {
            _context.Broadcaster.Highlight(card, color);
        }

        public void Filter(int card, string color) {
            _context.Broadcaster.Filter(card, color);
        }

        public void TurnReq(int card, bool up) {
            _context.Broadcaster.Turn(_player.Id, card, up);
        }

        public void RotateReq(int card, CardOrientation rot) {
            _context.Broadcaster.Rotate(_player.Id, card, rot);
        }

        public void Shuffled(byte player, int group, int[] card, short[] pos) {
            _context.Broadcaster.Shuffled(player, group, card, pos);
        }

        public void PassToReq(int id, byte player, bool requested) {
            _context.Broadcaster.PassTo(_player.Id, id, player, requested);
        }

        public void TakeFromReq(int id, byte fromPlayer) {
            _context.State.Players.GetPlayer(fromPlayer).Rpc.TakeFrom(id, _player.Id);
        }

        public void DontTakeReq(int id, byte toPlayer) {
            _context.State.Players.GetPlayer(toPlayer).Rpc.DontTake(id);
        }

        public void FreezeCardsVisibility(int group) {
            _context.Broadcaster.FreezeCardsVisibility(group);
        }

        public void GroupVisReq(int id, bool defined, bool visible) {
            _context.Broadcaster.GroupVis(_player.Id, id, defined, visible);
        }

        public void GroupVisAddReq(int gId, byte pId) {
            _context.Broadcaster.GroupVisAdd(_player.Id, gId, pId);
        }

        public void GroupVisRemoveReq(int gId, byte pId) {
            _context.Broadcaster.GroupVisRemove(_player.Id, gId, pId);
        }

        public void LookAtReq(int uid, int gId, bool look) {
            _context.Broadcaster.LookAt(_player.Id, uid, gId, look);
        }

        public void LookAtTopReq(int uid, int gId, int count, bool look) {
            _context.Broadcaster.LookAtTop(_player.Id, uid, gId, count, look);
        }

        public void LookAtBottomReq(int uid, int gId, int count, bool look) {
            _context.Broadcaster.LookAtBottom(_player.Id, uid, gId, count, look);
        }

        public void StartLimitedReq(Guid[] packs) {
            _context.Broadcaster.StartLimited(_player.Id, packs);
        }

        public void CancelLimitedReq() {
            _context.Broadcaster.CancelLimited(_player.Id);
        }

        public void AddPacksReq(Guid[] packs, bool selfOnly) {
            _context.Broadcaster.AddPacks(_player.Id, packs, selfOnly);
        }
        public void IsTableBackgroundFlipped(bool isFlipped) {
            _context.Broadcaster.IsTableBackgroundFlipped(isFlipped);
        }

        internal void Ping() {
        }

        public void PlaySound(byte player, string soundName) {
            _context.Broadcaster.PlaySound(player, soundName);
        }

        public void Ready(byte player) {
            _context.Broadcaster.Ready(player);
        }

        public void RemoteCall(byte player, string func, string args) {
            _context.State.Players.GetPlayer(player).Rpc.RemoteCall(_player.Id, func, args);
        }

        public void ShuffleDeprecated(int arg0, int[] ints) {
            _context.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void UnaliasGrpDeprecated(int arg0) {
            _context.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void UnaliasDeprecated(int[] arg0, ulong[] ulongs) {
            _context.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void CreateAliasDeprecated(int[] arg0, ulong[] ulongs) {
            _context.Broadcaster.Error(String.Format(L.D.ServerMessage__CallDepreciated, MethodInfo.GetCurrentMethod().Name));
        }

        public void GameState(byte player, string state) {
            _context.State.Players.GetPlayer(player).Rpc.GameState(_player.Id, state);
        }

        public void GameStateReq(byte toPlayer) {
            _context.State.Players.GetPlayer(toPlayer).Rpc.GameStateReq(_player.Id);
        }

        public void DeleteCard(int cardId, byte playerId) {
            _context.Broadcaster.DeleteCard(cardId, playerId);
        }

        public void Leave(byte player) {
            var info = _player;
            // If the client is not registered, do nothing
            if (info == null) return;

            info.Leave();
        }

        public void Boot(byte player, string reason) {
            var p = _player;
            var bplayer = _context.State.Players.GetPlayer(player);
            if (bplayer == null) {
                _context.Broadcaster.Error(string.Format(L.D.ServerMessage__CanNotBootPlayerDoesNotExist, p.Nick));
                return;
            }
            if (p.Id != 1) {
                _context.Broadcaster.Error(string.Format(L.D.ServerMessage__CanNotBootNotHost, p.Nick, bplayer.Nick));
                return;
            }
            bplayer.Kick(reason);
            _context.Broadcaster.Leave(bplayer.Id);
        }

        internal void AnchorCard(int card, byte player, bool anchor) {
            _context.Broadcaster.AnchorCard(card, player, anchor);
        }

        internal void SetCardProperty(int card, byte player, string name, string val, string valtype) {
            _context.Broadcaster.SetCardProperty(card, player, name, val, valtype);
        }

        public void ResetCardProperties(int card, byte player) {
            _context.Broadcaster.ResetCardProperties(card, player);
        }

        public void SetBoard(byte player, string name) {
            _context.Broadcaster.SetBoard(player, name);
        }
        public void RemoveBoard(byte player) {
            _context.Broadcaster.RemoveBoard(player);
        }

        public void SetPlayerColor(byte player, string colorHex) {
            _context.Broadcaster.SetPlayerColor(player, colorHex);
        }
    }
}