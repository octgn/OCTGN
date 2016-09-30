/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Microsoft.AspNet.SignalR;
using System;

namespace Octgn.Server.Signalr
{
    public class HubRpc : IServerToClientCalls
    {
        private readonly dynamic _rpc;
        public HubRpc(dynamic rpc) {
            _rpc = rpc;
        }

        // This region is automatically generated from CallGenerator.tt
        // Do not modify anything in here...
        #region IServerToClientCalls

        public void Error(string msg)
        {
            _rpc.Error(msg);
        }

        public void Kick(string reason)
        {
            _rpc.Kick(reason);
        }

        public void Welcome(uint id, int gameId, bool waitForGameState)
        {
            _rpc.Welcome(id, gameId, waitForGameState);
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            _rpc.Settings(twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(uint playerId, bool invertedTable, bool spectator)
        {
            _rpc.PlayerSettings(playerId, invertedTable, spectator);
        }

        public void NewPlayer(uint id, string nick, long pkey, bool tableSide, bool spectator)
        {
            _rpc.NewPlayer(id, nick, pkey, tableSide, spectator);
        }

        public void Leave(uint player)
        {
            _rpc.Leave(player);
        }

        public void Nick(uint player, string nick)
        {
            _rpc.Nick(player, nick);
        }

        public void Start()
        {
            _rpc.Start();
        }

        public void Reset(uint player)
        {
            _rpc.Reset(player);
        }

        public void NextTurn(uint nextPlayer)
        {
            _rpc.NextTurn(nextPlayer);
        }

        public void StopTurn(uint player)
        {
            _rpc.StopTurn(player);
        }

        public void Chat(uint player, string text)
        {
            _rpc.Chat(player, text);
        }

        public void Print(uint player, string text)
        {
            _rpc.Print(player, text);
        }

        public void Random(int result)
        {
            _rpc.Random(result);
        }

        public void Counter(uint player, ulong counter, int value, bool isScriptChange)
        {
            _rpc.Counter(player, counter, value, isScriptChange);
        }

        public void LoadDeck(uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited)
        {
            _rpc.LoadDeck(player, id, type, group, size, sleeve, limited);
        }

        public void CreateCard(uint player, ulong[] id, Guid[] type, string[] size, ulong group)
        {
            _rpc.CreateCard(player, id, type, size, group);
        }

        public void CreateCardAt(uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            _rpc.CreateCardAt(player, id, modelId, x, y, faceUp, persist);
        }

        public void MoveCard(uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _rpc.MoveCard(player, id, group, idx, faceUp, isScriptMove);
        }

        public void MoveCardAt(uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _rpc.MoveCardAt(player, id, x, y, idx, faceUp, isScriptMove);
        }

        public void Peek(uint player, ulong card)
        {
            _rpc.Peek(player, card);
        }

        public void Untarget(uint player, ulong card, bool isScriptChange)
        {
            _rpc.Untarget(player, card, isScriptChange);
        }

        public void Target(uint player, ulong card, bool isScriptChange)
        {
            _rpc.Target(player, card, isScriptChange);
        }

        public void TargetArrow(uint player, ulong card, ulong otherCard, bool isScriptChange)
        {
            _rpc.TargetArrow(player, card, otherCard, isScriptChange);
        }

        public void Highlight(ulong card, string color)
        {
            _rpc.Highlight(card, color);
        }

        public void Turn(uint player, ulong card, bool up)
        {
            _rpc.Turn(player, card, up);
        }

        public void Rotate(uint player, ulong card, CardOrientation rot)
        {
            _rpc.Rotate(player, card, rot);
        }

        public void Shuffled(uint player, ulong group, ulong[] card, short[] pos)
        {
            _rpc.Shuffled(player, group, card, pos);
        }

        public void AddMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.AddMarker(player, card, id, name, count, origCount, isScriptChange);
        }

        public void RemoveMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.RemoveMarker(player, card, id, name, count, origCount, isScriptChange);
        }

        public void TransferMarker(uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.TransferMarker(player, from, to, id, name, count, origCount, isScriptChange);
        }

        public void PassTo(uint player, ulong id, uint to, bool requested)
        {
            _rpc.PassTo(player, id, to, requested);
        }

        public void TakeFrom(ulong id, uint to)
        {
            _rpc.TakeFrom(id, to);
        }

        public void DontTake(ulong id)
        {
            _rpc.DontTake(id);
        }

        public void FreezeCardsVisibility(ulong group)
        {
            _rpc.FreezeCardsVisibility(group);
        }

        public void GroupVis(uint player, ulong group, bool defined, bool visible)
        {
            _rpc.GroupVis(player, group, defined, visible);
        }

        public void GroupVisAdd(uint player, ulong group, uint who)
        {
            _rpc.GroupVisAdd(player, group, who);
        }

        public void GroupVisRemove(uint player, ulong group, uint who)
        {
            _rpc.GroupVisRemove(player, group, who);
        }

        public void LookAt(uint player, uint uid, ulong group, bool look)
        {
            _rpc.LookAt(player, uid, group, look);
        }

        public void LookAtTop(uint player, uint uid, ulong group, int count, bool look)
        {
            _rpc.LookAtTop(player, uid, group, count, look);
        }

        public void LookAtBottom(uint player, uint uid, ulong group, int count, bool look)
        {
            _rpc.LookAtBottom(player, uid, group, count, look);
        }

        public void StartLimited(uint player, Guid[] packs)
        {
            _rpc.StartLimited(player, packs);
        }

        public void CancelLimited(uint player)
        {
            _rpc.CancelLimited(player);
        }

        public void CardSwitchTo(uint player, ulong card, string alternate)
        {
            _rpc.CardSwitchTo(player, card, alternate);
        }

        public void PlayerSetGlobalVariable(uint player, string name, string oldval, string val)
        {
            _rpc.PlayerSetGlobalVariable(player, name, oldval, val);
        }

        public void SetGlobalVariable(string name, string oldval, string val)
        {
            _rpc.SetGlobalVariable(name, oldval, val);
        }

        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            _rpc.IsTableBackgroundFlipped(isFlipped);
        }

        public void PlaySound(uint player, string name)
        {
            _rpc.PlaySound(player, name);
        }

        public void Ready(uint player)
        {
            _rpc.Ready(player);
        }

        public void PlayerState(uint player, byte state)
        {
            _rpc.PlayerState(player, state);
        }

        public void RemoteCall(uint player, string function, string args)
        {
            _rpc.RemoteCall(player, function, args);
        }

        public void GameStateReq(uint player)
        {
            _rpc.GameStateReq(player);
        }

        public void GameState(uint toPlayer, string state)
        {
            _rpc.GameState(toPlayer, state);
        }

        public void DeleteCard(ulong card, uint player)
        {
            _rpc.DeleteCard(card, player);
        }

        public void PlayerDisconnect(uint player)
        {
            _rpc.PlayerDisconnect(player);
        }

        public void AddPacks(uint player, Guid[] packs, bool selfOnly)
        {
            _rpc.AddPacks(player, packs, selfOnly);
        }

        public void AnchorCard(ulong id, uint player, bool anchor)
        {
            _rpc.AnchorCard(id, player, anchor);
        }

        public void SetCardProperty(ulong id, uint player, string name, string val, string valtype)
        {
            _rpc.SetCardProperty(id, player, name, val, valtype);
        }

        public void ResetCardProperties(ulong id, uint player)
        {
            _rpc.ResetCardProperties(id, player);
        }

        public void Filter(ulong card, string color)
        {
            _rpc.Filter(card, color);
        }

        public void SetBoard(string name)
        {
            _rpc.SetBoard(name);
        }

        public void SetPlayerColor(uint player, string color)
        {
            _rpc.SetPlayerColor(player, color);
        }

        public void SetPhase(byte phase, byte nextPhase)
        {
            _rpc.SetPhase(phase, nextPhase);
        }

        public void StopPhase(uint player, byte phase)
        {
            _rpc.StopPhase(player, phase);
        }

		#endregion IServerToClientCalls
    }
}
