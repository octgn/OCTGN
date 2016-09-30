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

        public void NickReq(string nick)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.NickReq(nick);
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

        public void LookAtReq(uint uid, ulong group, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtReq(uid, group, look);
            }
        }

        public void LookAtTopReq(uint uid, ulong group, int count, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtTopReq(uid, group, count, look);
            }
        }

        public void LookAtBottomReq(uint uid, ulong group, int count, bool look)
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.LookAtBottomReq(uid, group, count, look);
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

        public void SwitchWithAlternate()
        {
            using (var context = new RequestContext(_gameRepo, _settings, _broadcaster)) {
                context.Initialize(Context, this.Clients.Caller).Wait();
                if (!_handler.InitializeRequest(context)) return;
                _handler.SwitchWithAlternate();
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
