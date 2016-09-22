/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using Microsoft.AspNet.SignalR;
using Octgn.Server.Data;

namespace Octgn.Server.Signalr
{
    public class GameHub : Hub, IRemoteCalls
    {
        private RequestHandler _handler;
        private IOctgnServerSettings _settings;
        private IGameRepository _gameRepo;
        private readonly IClientCalls _broadcaster;
        public GameHub(RequestHandler handler, IGameRepository gameRepo, IOctgnServerSettings settings) {
            _handler = handler;
            _settings = settings;
            _gameRepo = gameRepo;
            _broadcaster = new HubBroadcaster(this);
        }

        #region IRemoteCalls

        public void AddMarkerReq(int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            // TODO I'm guessing most of this context boiler plate shit could be handled
            //     some other way, like pre/post call or something.
            using (var context = new RequestContext(this.Context, _gameRepo, _settings, _broadcaster)) {
                if (!_handler.InitializeRequest(context)) return;
                _handler.AddMarkerReq(card, id, name, count, origCount, isScriptChange);
            }
        }

        public void AddPacksReq(Guid[] packs, bool selfOnly) {
            throw new NotImplementedException();
        }

        public void Boot(ulong player, string reason) {
            throw new NotImplementedException();
        }

        public void CancelLimitedReq() {
            throw new NotImplementedException();
        }

        public void ChatReq(string text) {
            throw new NotImplementedException();
        }

        public void CounterReq(int counter, int value, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void DontTakeReq(int id, ulong to) {
            throw new NotImplementedException();
        }

        public void GroupVisAddReq(int group, ulong who) {
            throw new NotImplementedException();
        }

        public void GroupVisRemoveReq(int group, ulong who) {
            throw new NotImplementedException();
        }

        public void GroupVisReq(int group, bool defined, bool visible) {
            throw new NotImplementedException();
        }

        public void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator) {
            throw new NotImplementedException();
        }

        public void HelloAgain(ulong pid, string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password) {
            throw new NotImplementedException();
        }

        public void LookAtBottomReq(int uid, int group, int count, bool look) {
            throw new NotImplementedException();
        }

        public void LookAtReq(int uid, int group, bool look) {
            throw new NotImplementedException();
        }

        public void LookAtTopReq(int uid, int group, int count, bool look) {
            throw new NotImplementedException();
        }

        public void MoveCardAtReq(int[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp) {
            throw new NotImplementedException();
        }

        public void MoveCardReq(int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove) {
            throw new NotImplementedException();
        }

        public void NickReq(string nick) {
            throw new NotImplementedException();
        }

        public void PassToReq(int id, ulong to, bool requested) {
            throw new NotImplementedException();
        }

        public void PeekReq(int card) {
            throw new NotImplementedException();
        }

        public void PrintReq(string text) {
            throw new NotImplementedException();
        }

        public void RandomReq(int min, int max) {
            throw new NotImplementedException();
        }

        public void RemoveMarkerReq(int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void ResetReq() {
            throw new NotImplementedException();
        }

        public void RotateReq(int card, CardOrientation rot) {
            throw new NotImplementedException();
        }

        public void StartLimitedReq(Guid[] packs) {
            throw new NotImplementedException();
        }

        public void StopPhaseReq(int turnNumber, byte phase, bool stop) {
            throw new NotImplementedException();
        }

        public void StopTurnReq(int turnNumber, bool stop) {
            throw new NotImplementedException();
        }

        public void SwitchWithAlternate() {
            throw new NotImplementedException();
        }

        public void TakeFromReq(int id, ulong from) {
            throw new NotImplementedException();
        }

        public void TargetArrowReq(int card, int otherCard, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TargetReq(int card, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TransferMarkerReq(int from, int to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TurnReq(int card, bool up) {
            throw new NotImplementedException();
        }

        public void UntargetReq(int card, bool isScriptChange) {
            throw new NotImplementedException();
        }

        #endregion IRemoteCalls
    }
}
