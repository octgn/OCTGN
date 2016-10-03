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

        public void Boot(Guid player, string reason)
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

        public void HelloAgain(Guid pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password)
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

        public void PlayerSettings(Guid playerId, bool invertedTable, bool spectator)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PlayerSettings(playerId, invertedTable, spectator);
            }
        }

        public void Leave(Guid player)
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

        public void NextTurn(Guid nextPlayer)
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

        public void CounterReq(Guid counter, int value, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CounterReq(counter, value, isScriptChange);
            }
        }

        public void LoadDeck(Guid[] id, Guid[] type, Guid[] group, string[] size, string sleeve, bool limited)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LoadDeck(id, type, group, size, sleeve, limited);
            }
        }

        public void CreateCard(Guid[] id, Guid[] type, string[] size, Guid group)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CreateCard(id, type, size, group);
            }
        }

        public void CreateCardAt(Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CreateCardAt(id, modelId, x, y, faceUp, persist);
            }
        }

        public void MoveCardReq(Guid[] id, Guid group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.MoveCardReq(id, group, idx, faceUp, isScriptMove);
            }
        }

        public void MoveCardAtReq(Guid[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.MoveCardAtReq(id, x, y, idx, isScriptMove, faceUp);
            }
        }

        public void PeekReq(Guid card)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PeekReq(card);
            }
        }

        public void UntargetReq(Guid card, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.UntargetReq(card, isScriptChange);
            }
        }

        public void TargetReq(Guid card, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TargetReq(card, isScriptChange);
            }
        }

        public void TargetArrowReq(Guid card, Guid otherCard, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TargetArrowReq(card, otherCard, isScriptChange);
            }
        }

        public void Highlight(Guid card, string color)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Highlight(card, color);
            }
        }

        public void TurnReq(Guid card, bool up)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TurnReq(card, up);
            }
        }

        public void RotateReq(Guid card, CardOrientation rot)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RotateReq(card, rot);
            }
        }

        public void Shuffled(Guid player, Guid group, Guid[] card, short[] pos)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Shuffled(player, group, card, pos);
            }
        }

        public void AddMarkerReq(Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.AddMarkerReq(card, id, name, count, origCount, isScriptChange);
            }
        }

        public void RemoveMarkerReq(Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RemoveMarkerReq(card, id, name, count, origCount, isScriptChange);
            }
        }

        public void TransferMarkerReq(Guid from, Guid to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TransferMarkerReq(from, to, id, name, count, origCount, isScriptChange);
            }
        }

        public void PassToReq(Guid id, Guid to, bool requested)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PassToReq(id, to, requested);
            }
        }

        public void TakeFromReq(Guid id, Guid from)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.TakeFromReq(id, from);
            }
        }

        public void DontTakeReq(Guid id, Guid to)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.DontTakeReq(id, to);
            }
        }

        public void FreezeCardsVisibility(Guid group)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.FreezeCardsVisibility(group);
            }
        }

        public void GroupVisReq(Guid group, bool defined, bool visible)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GroupVisReq(group, defined, visible);
            }
        }

        public void GroupVisAddReq(Guid group, Guid who)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GroupVisAddReq(group, who);
            }
        }

        public void GroupVisRemoveReq(Guid group, Guid who)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GroupVisRemoveReq(group, who);
            }
        }

        public void LookAtReq(Guid uniqueid, Guid group, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtReq(uniqueid, group, look);
            }
        }

        public void LookAtTopReq(Guid uniqueid, Guid group, int count, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtTopReq(uniqueid, group, count, look);
            }
        }

        public void LookAtBottomReq(Guid uniqueid, Guid group, int count, bool look)
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

        public void CardSwitchTo(Guid player, Guid card, string alternate)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.CardSwitchTo(player, card, alternate);
            }
        }

        public void PlayerSetGlobalVariable(Guid player, string name, string oldval, string val)
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

        public void PlaySound(Guid player, string name)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.PlaySound(player, name);
            }
        }

        public void Ready(Guid player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.Ready(player);
            }
        }

        public void RemoteCall(Guid player, string function, string args)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.RemoteCall(player, function, args);
            }
        }

        public void GameStateReq(Guid player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GameStateReq(player);
            }
        }

        public void GameState(Guid toPlayer, string state)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.GameState(toPlayer, state);
            }
        }

        public void DeleteCard(Guid card, Guid player)
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

        public void AnchorCard(Guid id, Guid player, bool anchor)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.AnchorCard(id, player, anchor);
            }
        }

        public void SetCardProperty(Guid id, Guid player, string name, string val, string valtype)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SetCardProperty(id, player, name, val, valtype);
            }
        }

        public void ResetCardProperties(Guid id, Guid player)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.ResetCardProperties(id, player);
            }
        }

        public void Filter(Guid card, string color)
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

        public void SetPlayerColor(Guid player, string color)
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
