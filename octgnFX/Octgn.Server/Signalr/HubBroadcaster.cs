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

        public void AddMarker(ulong player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void AddPacks(ulong player, Guid[] packs, bool selfOnly) {
            throw new NotImplementedException();
        }

        public void AnchorCard(int id, ulong player, bool anchor) {
            throw new NotImplementedException();
        }

        public void CancelLimited(ulong player) {
            throw new NotImplementedException();
        }

        public void CardSwitchTo(ulong player, int card, string alternate) {
            throw new NotImplementedException();
        }

        public void Chat(ulong player, string text) {
            throw new NotImplementedException();
        }

        public void Counter(ulong player, int counter, int value, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void CreateAliasDeprecated(int[] id, ulong[] type) {
            throw new NotImplementedException();
        }

        public void CreateCard(int[] id, Guid[] type, string[] size, int group) {
            throw new NotImplementedException();
        }

        public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist) {
            throw new NotImplementedException();
        }

        public void DeleteCard(int card, ulong player) {
            throw new NotImplementedException();
        }

        public void DontTake(int id) {
            throw new NotImplementedException();
        }

        public void Error(string msg) {
            throw new NotImplementedException();
        }

        public void Filter(int card, string color) {
            throw new NotImplementedException();
        }

        public void FreezeCardsVisibility(int group) {
            throw new NotImplementedException();
        }

        public void GameState(ulong toPlayer, string state) {
            throw new NotImplementedException();
        }

        public void GameStateReq(ulong player) {
            throw new NotImplementedException();
        }

        public void GroupVis(ulong player, int group, bool defined, bool visible) {
            throw new NotImplementedException();
        }

        public void GroupVisAdd(ulong player, int group, ulong who) {
            throw new NotImplementedException();
        }

        public void GroupVisRemove(ulong player, int group, ulong who) {
            throw new NotImplementedException();
        }

        public void Highlight(int card, string color) {
            throw new NotImplementedException();
        }

        public void IsTableBackgroundFlipped(bool isFlipped) {
            throw new NotImplementedException();
        }

        public void Kick(string reason) {
            throw new NotImplementedException();
        }

        public void Leave(ulong player) {
            throw new NotImplementedException();
        }

        public void LoadDeck(int[] id, Guid[] type, int[] group, string[] size, string sleeve, bool limited) {
            throw new NotImplementedException();
        }

        public void LookAt(ulong player, int uid, int group, bool look) {
            throw new NotImplementedException();
        }

        public void LookAtBottom(ulong player, int uid, int group, int count, bool look) {
            throw new NotImplementedException();
        }

        public void LookAtTop(ulong player, int uid, int group, int count, bool look) {
            throw new NotImplementedException();
        }

        public void MoveCard(ulong player, int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove) {
            throw new NotImplementedException();
        }

        public void MoveCardAt(ulong player, int[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove) {
            throw new NotImplementedException();
        }

        public void NewPlayer(ulong id, string nick, ulong pkey, bool tableSide, bool spectator) {
            throw new NotImplementedException();
        }

        public void NextTurn(ulong nextPlayer) {
            throw new NotImplementedException();
        }

        public void Nick(ulong player, string nick) {
            throw new NotImplementedException();
        }

        public void PassTo(ulong player, int id, ulong to, bool requested) {
            throw new NotImplementedException();
        }

        public void Peek(ulong player, int card) {
            throw new NotImplementedException();
        }

        public void Ping() {
            throw new NotImplementedException();
        }

        public void PlayerDisconnect(ulong player) {
            throw new NotImplementedException();
        }

        public void PlayerSetGlobalVariable(ulong player, string name, string oldval, string val) {
            throw new NotImplementedException();
        }

        public void PlayerSettings(ulong playerId, bool invertedTable, bool spectator) {
            throw new NotImplementedException();
        }

        public void PlayerState(ulong player, byte state) {
            throw new NotImplementedException();
        }

        public void PlaySound(ulong player, string name) {
            throw new NotImplementedException();
        }

        public void Print(ulong player, string text) {
            throw new NotImplementedException();
        }

        public void Random(int result) {
            throw new NotImplementedException();
        }

        public void Ready(ulong player) {
            throw new NotImplementedException();
        }

        public void RemoteCall(ulong player, string function, string args) {
            throw new NotImplementedException();
        }

        public void RemoveMarker(ulong player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void Reset(ulong player) {
            throw new NotImplementedException();
        }

        public void ResetCardProperties(int id, ulong player) {
            throw new NotImplementedException();
        }

        public void Rotate(ulong player, int card, CardOrientation rot) {
            throw new NotImplementedException();
        }

        public void SetBoard(string name) {
            throw new NotImplementedException();
        }

        public void SetCardProperty(int id, ulong player, string name, string val, string valtype) {
            throw new NotImplementedException();
        }

        public void SetGlobalVariable(string name, string oldval, string val) {
            throw new NotImplementedException();
        }

        public void SetPhase(byte phase, byte nextPhase) {
            throw new NotImplementedException();
        }

        public void SetPlayerColor(ulong player, string color) {
            throw new NotImplementedException();
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators) {
            throw new NotImplementedException();
        }

        public void Shuffled(ulong player, int group, int[] card, short[] pos) {
            throw new NotImplementedException();
        }

        public void ShuffleDeprecated(int group, int[] card) {
            throw new NotImplementedException();
        }

        public void Start() {
            throw new NotImplementedException();
        }

        public void StartLimited(ulong player, Guid[] packs) {
            throw new NotImplementedException();
        }

        public void StopPhase(ulong player, byte phase) {
            throw new NotImplementedException();
        }

        public void StopTurn(ulong player) {
            throw new NotImplementedException();
        }

        public void TakeFrom(int id, ulong to) {
            throw new NotImplementedException();
        }

        public void Target(ulong player, int card, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TargetArrow(ulong player, int card, int otherCard, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void TransferMarker(ulong player, int from, int to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void Turn(ulong player, int card, bool up) {
            throw new NotImplementedException();
        }

        public void UnaliasDeprecated(int[] card, ulong[] type) {
            throw new NotImplementedException();
        }

        public void UnaliasGrpDeprecated(int group) {
            throw new NotImplementedException();
        }

        public void Untarget(ulong player, int card, bool isScriptChange) {
            throw new NotImplementedException();
        }

        public void Welcome(ulong id, Guid gameSessionId, bool waitForGameState) {
            throw new NotImplementedException();
        }
    }
}
