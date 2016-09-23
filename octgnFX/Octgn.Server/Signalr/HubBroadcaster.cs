/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Microsoft.AspNet.SignalR;
using System;

namespace Octgn.Server.Signalr
{
    internal class HubBroadcaster : IClientCalls
    {
        private readonly Hub _hub;
        public HubBroadcaster(Hub hub) {
            _hub = hub;
        }

        public void AddMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void AddPacks(uint player, Guid[] packs, bool selfOnly) {
            throw new NotImplementedException();
        }

        public void AnchorCard(ulong id, uint player, bool anchor) {
            throw new NotImplementedException();
        }

        public void CancelLimited(uint player) {
            throw new NotImplementedException();
        }

        public void CardSwitchTo(uint player, ulong card, string alternate) {
            throw new NotImplementedException();
        }

        public void Chat(uint player, string text) {
            throw new NotImplementedException();
        }

        public void Counter(uint player, ulong counter, int value, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void CreateCard(uint player, ulong[] id, Guid[] type, string[] size, ulong group) {
            throw new NotImplementedException();
        }

        public void CreateCardAt(uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist) {
            throw new NotImplementedException();
        }

        public void DeleteCard(ulong card, uint player) {
            throw new NotImplementedException();
        }

        public void DontTake(ulong id) {
            throw new NotImplementedException();
        }

        public void Error(string msg) {
            throw new NotImplementedException();
        }

        public void Filter(ulong card, string color) {
            throw new NotImplementedException();
        }

        public void FreezeCardsVisibility(ulong group) {
            throw new NotImplementedException();
        }

        public void GameState(uint toPlayer, string state) {
            throw new NotImplementedException();
        }

        public void GameStateReq(uint player) {
            throw new NotImplementedException();
        }

        public void GroupVis(uint player, ulong group, bool defined, bool visible) {
            throw new NotImplementedException();
        }

        public void GroupVisAdd(uint player, ulong group, uint who) {
            throw new NotImplementedException();
        }

        public void GroupVisRemove(uint player, ulong group, uint who) {
            throw new NotImplementedException();
        }

        public void Highlight(ulong card, string color) {
            throw new NotImplementedException();
        }

        public void IsTableBackgroundFlipped(bool isFlipped) {
            throw new NotImplementedException();
        }

        public void Kick(string reason) {
            throw new NotImplementedException();
        }

        public void Leave(uint player) {
            throw new NotImplementedException();
        }

        public void LoadDeck(uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited) {
            throw new NotImplementedException();
        }

        public void LookAt(uint player, uint uid, ulong group, bool look) {
            throw new NotImplementedException();
        }

        public void LookAtBottom(uint player, uint uid, ulong group, int count, bool look) {
            throw new NotImplementedException();
        }

        public void LookAtTop(uint player, uint uid, ulong group, int count, bool look) {
            throw new NotImplementedException();
        }

        public void MoveCard(uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove) {
            throw new NotImplementedException();
        }

        public void MoveCardAt(uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove) {
            throw new NotImplementedException();
        }

        public void NewPlayer(uint id, string nick, ulong pkey, bool tableSide, bool spectator) {
            throw new NotImplementedException();
        }

        public void NextTurn(uint nextPlayer) {
            throw new NotImplementedException();
        }

        public void Nick(uint player, string nick) {
            throw new NotImplementedException();
        }

        public void PassTo(uint player, ulong id, uint to, bool requested) {
            throw new NotImplementedException();
        }

        public void Peek(uint player, ulong card) {
            throw new NotImplementedException();
        }

        public void Ping() {
            throw new NotImplementedException();
        }

        public void PlayerDisconnect(uint player) {
            throw new NotImplementedException();
        }

        public void PlayerSetGlobalVariable(uint player, string name, string oldval, string val) {
            throw new NotImplementedException();
        }

        public void PlayerSettings(uint playerId, bool invertedTable, bool spectator) {
            throw new NotImplementedException();
        }

        public void PlayerState(uint player, byte state) {
            throw new NotImplementedException();
        }

        public void PlaySound(uint player, string name) {
            throw new NotImplementedException();
        }

        public void Print(uint player, string text) {
            throw new NotImplementedException();
        }

        public void Random(int result) {
            throw new NotImplementedException();
        }

        public void Ready(uint player) {
            throw new NotImplementedException();
        }

        public void RemoteCall(uint player, string function, string args) {
            throw new NotImplementedException();
        }

        public void RemoveMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void Reset(uint player) {
            throw new NotImplementedException();
        }

        public void ResetCardProperties(ulong id, uint player) {
            throw new NotImplementedException();
        }

        public void Rotate(uint player, ulong card, CardOrientation rot) {
            throw new NotImplementedException();
        }

        public void SetBoard(string name) {
            throw new NotImplementedException();
        }

        public void SetCardProperty(ulong id, uint player, string name, string val, string valtype) {
            throw new NotImplementedException();
        }

        public void SetGlobalVariable(string name, string oldval, string val) {
            throw new NotImplementedException();
        }

        public void SetPhase(byte phase, byte nextPhase) {
            throw new NotImplementedException();
        }

        public void SetPlayerColor(uint player, string color) {
            throw new NotImplementedException();
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators) {
            throw new NotImplementedException();
        }

        public void Shuffled(uint player, ulong group, ulong[] card, short[] pos) {
            throw new NotImplementedException();
        }

        public void Start() {
            throw new NotImplementedException();
        }

        public void StartLimited(uint player, Guid[] packs) {
            throw new NotImplementedException();
        }

        public void StopPhase(uint player, byte phase) {
            throw new NotImplementedException();
        }

        public void StopTurn(uint player) {
            throw new NotImplementedException();
        }

        public void TakeFrom(ulong id, uint to) {
            throw new NotImplementedException();
        }

        public void Target(uint player, ulong card, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TargetArrow(uint player, ulong card, ulong otherCard, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TransferMarker(uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void Turn(uint player, ulong card, bool up) {
            throw new NotImplementedException();
        }

        public void Untarget(uint player, ulong card, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void Welcome(uint id, Guid gameSessionId, bool waitForGameState) {
            throw new NotImplementedException();
        }
    }
}
