/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Microsoft.AspNet.SignalR;
using Octgn.Server.Data;

namespace Octgn.Server.Signalr
{
    public class GameHub : Hub, IClientToServerCalls
    {
        private RequestHandler _handler;
        private IOctgnServerSettings _settings;
        private IGameRepository _gameRepo;
        private IServerToClientCalls _broadcaster;
        public GameHub(RequestHandler handler, IGameRepository gameRepo, IOctgnServerSettings settings) {
            _handler = handler;
            _settings = settings;
            _gameRepo = gameRepo;
            _broadcaster = new HubBroadcaster(this);
        }

        // This region is automatically generated from CallGenerator.tt
        // Do not modify anything in here...
        #region IClientToServerCalls

        public void Error(string msg)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Error(msg);
            }
        }

        public void Boot(uint player, string reason)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Boot(player, reason);
            }
        }

        public void Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Hello(nick, pkey, client, clientVer, octgnVer, gameId, gameVersion, password, spectator);
            }
        }

        public void HelloAgain(uint pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.HelloAgain(pid, nick, pkey, client, clientVer, octgnVer, gameId, gameVersion, password);
            }
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Settings(twoSidedTable, allowSpectators, muteSpectators);
            }
        }

        public void PlayerSettings(uint playerId, bool invertedTable, bool spectator)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PlayerSettings(playerId, invertedTable, spectator);
            }
        }

        public void Leave(uint player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Leave(player);
            }
        }

        public void NickReq(string nick)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.NickReq(nick);
            }
        }

        public void Start()
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Start();
            }
        }

        public void ResetReq()
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.ResetReq();
            }
        }

        public void NextTurn(uint nextPlayer)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.NextTurn(nextPlayer);
            }
        }

        public void StopTurnReq(int turnNumber, bool stop)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.StopTurnReq(turnNumber, stop);
            }
        }

        public void ChatReq(string text)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.ChatReq(text);
            }
        }

        public void PrintReq(string text)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PrintReq(text);
            }
        }

        public void RandomReq(int min, int max)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RandomReq(min, max);
            }
        }

        public void CounterReq(ulong counter, int value, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CounterReq(counter, value, isScriptChange);
            }
        }

        public void LoadDeck(ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LoadDeck(id, type, group, size, sleeve, limited);
            }
        }

        public void CreateCard(ulong[] id, Guid[] type, string[] size, ulong group)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CreateCard(id, type, size, group);
            }
        }

        public void CreateCardAt(ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CreateCardAt(id, modelId, x, y, faceUp, persist);
            }
        }

        public void MoveCardReq(ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.MoveCardReq(id, group, idx, faceUp, isScriptMove);
            }
        }

        public void MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.MoveCardAtReq(id, x, y, idx, isScriptMove, faceUp);
            }
        }

        public void PeekReq(ulong card)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PeekReq(card);
            }
        }

        public void UntargetReq(ulong card, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.UntargetReq(card, isScriptChange);
            }
        }

        public void TargetReq(ulong card, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TargetReq(card, isScriptChange);
            }
        }

        public void TargetArrowReq(ulong card, ulong otherCard, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TargetArrowReq(card, otherCard, isScriptChange);
            }
        }

        public void Highlight(ulong card, string color)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Highlight(card, color);
            }
        }

        public void TurnReq(ulong card, bool up)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TurnReq(card, up);
            }
        }

        public void RotateReq(ulong card, CardOrientation rot)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RotateReq(card, rot);
            }
        }

        public void Shuffled(uint player, ulong group, ulong[] card, short[] pos)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Shuffled(player, group, card, pos);
            }
        }

        public void AddMarkerReq(ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.AddMarkerReq(card, id, name, count, origCount, isScriptChange);
            }
        }

        public void RemoveMarkerReq(ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RemoveMarkerReq(card, id, name, count, origCount, isScriptChange);
            }
        }

        public void TransferMarkerReq(ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TransferMarkerReq(from, to, id, name, count, origCount, isScriptChange);
            }
        }

        public void PassToReq(ulong id, uint to, bool requested)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PassToReq(id, to, requested);
            }
        }

        public void TakeFromReq(ulong id, uint from)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TakeFromReq(id, from);
            }
        }

        public void DontTakeReq(ulong id, uint to)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.DontTakeReq(id, to);
            }
        }

        public void FreezeCardsVisibility(ulong group)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.FreezeCardsVisibility(group);
            }
        }

        public void GroupVisReq(ulong group, bool defined, bool visible)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GroupVisReq(group, defined, visible);
            }
        }

        public void GroupVisAddReq(ulong group, uint who)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GroupVisAddReq(group, who);
            }
        }

        public void GroupVisRemoveReq(ulong group, uint who)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GroupVisRemoveReq(group, who);
            }
        }

        public void LookAtReq(uint uniqueid, ulong group, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtReq(uniqueid, group, look);
            }
        }

        public void LookAtTopReq(uint uniqueid, ulong group, int count, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtTopReq(uniqueid, group, count, look);
            }
        }

        public void LookAtBottomReq(uint uniqueid, ulong group, int count, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtBottomReq(uniqueid, group, count, look);
            }
        }

        public void StartLimitedReq(Guid[] packs)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.StartLimitedReq(packs);
            }
        }

        public void CancelLimitedReq()
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CancelLimitedReq();
            }
        }

        public void CardSwitchTo(uint player, ulong card, string alternate)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CardSwitchTo(player, card, alternate);
            }
        }

        public void PlayerSetGlobalVariable(uint player, string name, string oldval, string val)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PlayerSetGlobalVariable(player, name, oldval, val);
            }
        }

        public void SetGlobalVariable(string name, string oldval, string val)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SetGlobalVariable(name, oldval, val);
            }
        }

        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.IsTableBackgroundFlipped(isFlipped);
            }
        }

        public void PlaySound(uint player, string name)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PlaySound(player, name);
            }
        }

        public void Ready(uint player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Ready(player);
            }
        }

        public void RemoteCall(uint player, string function, string args)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RemoteCall(player, function, args);
            }
        }

        public void GameStateReq(uint player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GameStateReq(player);
            }
        }

        public void GameState(uint toPlayer, string state)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GameState(toPlayer, state);
            }
        }

        public void DeleteCard(ulong card, uint player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.DeleteCard(card, player);
            }
        }

        public void AddPacksReq(Guid[] packs, bool selfOnly)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.AddPacksReq(packs, selfOnly);
            }
        }

        public void AnchorCard(ulong id, uint player, bool anchor)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.AnchorCard(id, player, anchor);
            }
        }

        public void SetCardProperty(ulong id, uint player, string name, string val, string valtype)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SetCardProperty(id, player, name, val, valtype);
            }
        }

        public void ResetCardProperties(ulong id, uint player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.ResetCardProperties(id, player);
            }
        }

        public void Filter(ulong card, string color)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Filter(card, color);
            }
        }

        public void SetBoard(string name)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SetBoard(name);
            }
        }

        public void SetPlayerColor(uint player, string color)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SetPlayerColor(player, color);
            }
        }

        public void SetPhase(byte phase, byte nextPhase)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SetPhase(phase, nextPhase);
            }
        }

        public void StopPhaseReq(int turnNumber, byte phase, bool stop)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.StopPhaseReq(turnNumber, phase, stop);
            }
        }

		#endregion IClientToServerCalls

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (!disposing) return;

            _broadcaster = null;
            _gameRepo = null;
            _settings = null;
            _handler = null;
        }
    }
}
