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

        public void Error(uint sender, string msg)
        {
            _rpc.Error(sender, msg);
        }

        public void Kick(uint sender, string reason)
        {
            _rpc.Kick(sender, reason);
        }

        public void Welcome(uint sender, uint id, int gameId, bool waitForGameState)
        {
            _rpc.Welcome(sender, id, gameId, waitForGameState);
        }

        public void Settings(uint sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            _rpc.Settings(sender, twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(uint sender, uint playerId, bool invertedTable, bool spectator)
        {
            _rpc.PlayerSettings(sender, playerId, invertedTable, spectator);
        }

        public void NewPlayer(uint sender, uint id, string nick, long pkey, bool tableSide, bool spectator)
        {
            _rpc.NewPlayer(sender, id, nick, pkey, tableSide, spectator);
        }

        public void Leave(uint sender, uint player)
        {
            _rpc.Leave(sender, player);
        }

        public void Nick(uint sender, uint player, string nick)
        {
            _rpc.Nick(sender, player, nick);
        }

        public void Start(uint sender)
        {
            _rpc.Start(sender);
        }

        public void Reset(uint sender, uint player)
        {
            _rpc.Reset(sender, player);
        }

        public void NextTurn(uint sender, uint nextPlayer)
        {
            _rpc.NextTurn(sender, nextPlayer);
        }

        public void StopTurn(uint sender, uint player)
        {
            _rpc.StopTurn(sender, player);
        }

        public void Chat(uint sender, uint player, string text)
        {
            _rpc.Chat(sender, player, text);
        }

        public void Print(uint sender, uint player, string text)
        {
            _rpc.Print(sender, player, text);
        }

        public void Random(uint sender, int result)
        {
            _rpc.Random(sender, result);
        }

        public void Counter(uint sender, uint player, ulong counter, int value, bool isScriptChange)
        {
            _rpc.Counter(sender, player, counter, value, isScriptChange);
        }

        public void LoadDeck(uint sender, uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited)
        {
            _rpc.LoadDeck(sender, player, id, type, group, size, sleeve, limited);
        }

        public void CreateCard(uint sender, uint player, ulong[] id, Guid[] type, string[] size, ulong group)
        {
            _rpc.CreateCard(sender, player, id, type, size, group);
        }

        public void CreateCardAt(uint sender, uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            _rpc.CreateCardAt(sender, player, id, modelId, x, y, faceUp, persist);
        }

        public void MoveCard(uint sender, uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _rpc.MoveCard(sender, player, id, group, idx, faceUp, isScriptMove);
        }

        public void MoveCardAt(uint sender, uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _rpc.MoveCardAt(sender, player, id, x, y, idx, faceUp, isScriptMove);
        }

        public void Peek(uint sender, uint player, ulong card)
        {
            _rpc.Peek(sender, player, card);
        }

        public void Untarget(uint sender, uint player, ulong card, bool isScriptChange)
        {
            _rpc.Untarget(sender, player, card, isScriptChange);
        }

        public void Target(uint sender, uint player, ulong card, bool isScriptChange)
        {
            _rpc.Target(sender, player, card, isScriptChange);
        }

        public void TargetArrow(uint sender, uint player, ulong card, ulong otherCard, bool isScriptChange)
        {
            _rpc.TargetArrow(sender, player, card, otherCard, isScriptChange);
        }

        public void Highlight(uint sender, ulong card, string color)
        {
            _rpc.Highlight(sender, card, color);
        }

        public void Turn(uint sender, uint player, ulong card, bool up)
        {
            _rpc.Turn(sender, player, card, up);
        }

        public void Rotate(uint sender, uint player, ulong card, CardOrientation rot)
        {
            _rpc.Rotate(sender, player, card, rot);
        }

        public void Shuffled(uint sender, uint player, ulong group, ulong[] card, short[] pos)
        {
            _rpc.Shuffled(sender, player, group, card, pos);
        }

        public void AddMarker(uint sender, uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.AddMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void RemoveMarker(uint sender, uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.RemoveMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void TransferMarker(uint sender, uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.TransferMarker(sender, player, from, to, id, name, count, origCount, isScriptChange);
        }

        public void PassTo(uint sender, uint player, ulong id, uint to, bool requested)
        {
            _rpc.PassTo(sender, player, id, to, requested);
        }

        public void TakeFrom(uint sender, ulong id, uint to)
        {
            _rpc.TakeFrom(sender, id, to);
        }

        public void DontTake(uint sender, ulong id)
        {
            _rpc.DontTake(sender, id);
        }

        public void FreezeCardsVisibility(uint sender, ulong group)
        {
            _rpc.FreezeCardsVisibility(sender, group);
        }

        public void GroupVis(uint sender, uint player, ulong group, bool defined, bool visible)
        {
            _rpc.GroupVis(sender, player, group, defined, visible);
        }

        public void GroupVisAdd(uint sender, uint player, ulong group, uint who)
        {
            _rpc.GroupVisAdd(sender, player, group, who);
        }

        public void GroupVisRemove(uint sender, uint player, ulong group, uint who)
        {
            _rpc.GroupVisRemove(sender, player, group, who);
        }

        public void LookAt(uint sender, uint player, uint uid, ulong group, bool look)
        {
            _rpc.LookAt(sender, player, uid, group, look);
        }

        public void LookAtTop(uint sender, uint player, uint uid, ulong group, int count, bool look)
        {
            _rpc.LookAtTop(sender, player, uid, group, count, look);
        }

        public void LookAtBottom(uint sender, uint player, uint uid, ulong group, int count, bool look)
        {
            _rpc.LookAtBottom(sender, player, uid, group, count, look);
        }

        public void StartLimited(uint sender, uint player, Guid[] packs)
        {
            _rpc.StartLimited(sender, player, packs);
        }

        public void CancelLimited(uint sender, uint player)
        {
            _rpc.CancelLimited(sender, player);
        }

        public void CardSwitchTo(uint sender, uint player, ulong card, string alternate)
        {
            _rpc.CardSwitchTo(sender, player, card, alternate);
        }

        public void PlayerSetGlobalVariable(uint sender, uint player, string name, string oldval, string val)
        {
            _rpc.PlayerSetGlobalVariable(sender, player, name, oldval, val);
        }

        public void SetGlobalVariable(uint sender, string name, string oldval, string val)
        {
            _rpc.SetGlobalVariable(sender, name, oldval, val);
        }

        public void IsTableBackgroundFlipped(uint sender, bool isFlipped)
        {
            _rpc.IsTableBackgroundFlipped(sender, isFlipped);
        }

        public void PlaySound(uint sender, uint player, string name)
        {
            _rpc.PlaySound(sender, player, name);
        }

        public void Ready(uint sender, uint player)
        {
            _rpc.Ready(sender, player);
        }

        public void PlayerState(uint sender, uint player, byte state)
        {
            _rpc.PlayerState(sender, player, state);
        }

        public void RemoteCall(uint sender, uint player, string function, string args)
        {
            _rpc.RemoteCall(sender, player, function, args);
        }

        public void GameStateReq(uint sender, uint player)
        {
            _rpc.GameStateReq(sender, player);
        }

        public void GameState(uint sender, uint toPlayer, string state)
        {
            _rpc.GameState(sender, toPlayer, state);
        }

        public void DeleteCard(uint sender, ulong card, uint player)
        {
            _rpc.DeleteCard(sender, card, player);
        }

        public void PlayerDisconnect(uint sender, uint player)
        {
            _rpc.PlayerDisconnect(sender, player);
        }

        public void AddPacks(uint sender, uint player, Guid[] packs, bool selfOnly)
        {
            _rpc.AddPacks(sender, player, packs, selfOnly);
        }

        public void AnchorCard(uint sender, ulong id, uint player, bool anchor)
        {
            _rpc.AnchorCard(sender, id, player, anchor);
        }

        public void SetCardProperty(uint sender, ulong id, uint player, string name, string val, string valtype)
        {
            _rpc.SetCardProperty(sender, id, player, name, val, valtype);
        }

        public void ResetCardProperties(uint sender, ulong id, uint player)
        {
            _rpc.ResetCardProperties(sender, id, player);
        }

        public void Filter(uint sender, ulong card, string color)
        {
            _rpc.Filter(sender, card, color);
        }

        public void SetBoard(uint sender, string name)
        {
            _rpc.SetBoard(sender, name);
        }

        public void SetPlayerColor(uint sender, uint player, string color)
        {
            _rpc.SetPlayerColor(sender, player, color);
        }

        public void SetPhase(uint sender, byte phase, byte nextPhase)
        {
            _rpc.SetPhase(sender, phase, nextPhase);
        }

        public void StopPhase(uint sender, uint player, byte phase)
        {
            _rpc.StopPhase(sender, player, phase);
        }

		#endregion IServerToClientCalls
    }
}
