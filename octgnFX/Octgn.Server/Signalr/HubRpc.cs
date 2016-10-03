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

        public void Error(Guid sender, string msg)
        {
            _rpc.Error(sender, msg);
        }

        public void Kick(Guid sender, string reason)
        {
            _rpc.Kick(sender, reason);
        }

        public void Welcome(Guid sender, Guid id, Guid gameId, bool waitForGameState)
        {
            _rpc.Welcome(sender, id, gameId, waitForGameState);
        }

        public void Settings(Guid sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            _rpc.Settings(sender, twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(Guid sender, Guid playerId, bool invertedTable, bool spectator)
        {
            _rpc.PlayerSettings(sender, playerId, invertedTable, spectator);
        }

        public void NewPlayer(Guid sender, Guid id, string nick, long pkey, bool tableSide, bool spectator)
        {
            _rpc.NewPlayer(sender, id, nick, pkey, tableSide, spectator);
        }

        public void Leave(Guid sender, Guid player)
        {
            _rpc.Leave(sender, player);
        }

        public void Nick(Guid sender, Guid player, string nick)
        {
            _rpc.Nick(sender, player, nick);
        }

        public void Start(Guid sender)
        {
            _rpc.Start(sender);
        }

        public void Reset(Guid sender, Guid player)
        {
            _rpc.Reset(sender, player);
        }

        public void NextTurn(Guid sender, Guid nextPlayer)
        {
            _rpc.NextTurn(sender, nextPlayer);
        }

        public void StopTurn(Guid sender, Guid player)
        {
            _rpc.StopTurn(sender, player);
        }

        public void Chat(Guid sender, Guid player, string text)
        {
            _rpc.Chat(sender, player, text);
        }

        public void Print(Guid sender, Guid player, string text)
        {
            _rpc.Print(sender, player, text);
        }

        public void Random(Guid sender, int result)
        {
            _rpc.Random(sender, result);
        }

        public void Counter(Guid sender, Guid player, Guid counter, int value, bool isScriptChange)
        {
            _rpc.Counter(sender, player, counter, value, isScriptChange);
        }

        public void LoadDeck(Guid sender, Guid player, Guid[] id, Guid[] type, Guid[] group, string[] size, string sleeve, bool limited)
        {
            _rpc.LoadDeck(sender, player, id, type, group, size, sleeve, limited);
        }

        public void CreateCard(Guid sender, Guid player, Guid[] id, Guid[] type, string[] size, Guid group)
        {
            _rpc.CreateCard(sender, player, id, type, size, group);
        }

        public void CreateCardAt(Guid sender, Guid player, Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            _rpc.CreateCardAt(sender, player, id, modelId, x, y, faceUp, persist);
        }

        public void MoveCard(Guid sender, Guid player, Guid[] id, Guid group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _rpc.MoveCard(sender, player, id, group, idx, faceUp, isScriptMove);
        }

        public void MoveCardAt(Guid sender, Guid player, Guid[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _rpc.MoveCardAt(sender, player, id, x, y, idx, faceUp, isScriptMove);
        }

        public void Peek(Guid sender, Guid player, Guid card)
        {
            _rpc.Peek(sender, player, card);
        }

        public void Untarget(Guid sender, Guid player, Guid card, bool isScriptChange)
        {
            _rpc.Untarget(sender, player, card, isScriptChange);
        }

        public void Target(Guid sender, Guid player, Guid card, bool isScriptChange)
        {
            _rpc.Target(sender, player, card, isScriptChange);
        }

        public void TargetArrow(Guid sender, Guid player, Guid card, Guid otherCard, bool isScriptChange)
        {
            _rpc.TargetArrow(sender, player, card, otherCard, isScriptChange);
        }

        public void Highlight(Guid sender, Guid card, string color)
        {
            _rpc.Highlight(sender, card, color);
        }

        public void Turn(Guid sender, Guid player, Guid card, bool up)
        {
            _rpc.Turn(sender, player, card, up);
        }

        public void Rotate(Guid sender, Guid player, Guid card, CardOrientation rot)
        {
            _rpc.Rotate(sender, player, card, rot);
        }

        public void Shuffled(Guid sender, Guid player, Guid group, Guid[] card, short[] pos)
        {
            _rpc.Shuffled(sender, player, group, card, pos);
        }

        public void AddMarker(Guid sender, Guid player, Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.AddMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void RemoveMarker(Guid sender, Guid player, Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.RemoveMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void TransferMarker(Guid sender, Guid player, Guid from, Guid to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _rpc.TransferMarker(sender, player, from, to, id, name, count, origCount, isScriptChange);
        }

        public void PassTo(Guid sender, Guid player, Guid id, Guid to, bool requested)
        {
            _rpc.PassTo(sender, player, id, to, requested);
        }

        public void TakeFrom(Guid sender, Guid id, Guid to)
        {
            _rpc.TakeFrom(sender, id, to);
        }

        public void DontTake(Guid sender, Guid id)
        {
            _rpc.DontTake(sender, id);
        }

        public void FreezeCardsVisibility(Guid sender, Guid group)
        {
            _rpc.FreezeCardsVisibility(sender, group);
        }

        public void GroupVis(Guid sender, Guid player, Guid group, bool defined, bool visible)
        {
            _rpc.GroupVis(sender, player, group, defined, visible);
        }

        public void GroupVisAdd(Guid sender, Guid player, Guid group, Guid who)
        {
            _rpc.GroupVisAdd(sender, player, group, who);
        }

        public void GroupVisRemove(Guid sender, Guid player, Guid group, Guid who)
        {
            _rpc.GroupVisRemove(sender, player, group, who);
        }

        public void LookAt(Guid sender, Guid player, Guid uniqueid, Guid group, bool look)
        {
            _rpc.LookAt(sender, player, uniqueid, group, look);
        }

        public void LookAtTop(Guid sender, Guid player, Guid uniqueid, Guid group, int count, bool look)
        {
            _rpc.LookAtTop(sender, player, uniqueid, group, count, look);
        }

        public void LookAtBottom(Guid sender, Guid player, Guid uniqueid, Guid group, int count, bool look)
        {
            _rpc.LookAtBottom(sender, player, uniqueid, group, count, look);
        }

        public void StartLimited(Guid sender, Guid player, Guid[] packs)
        {
            _rpc.StartLimited(sender, player, packs);
        }

        public void CancelLimited(Guid sender, Guid player)
        {
            _rpc.CancelLimited(sender, player);
        }

        public void CardSwitchTo(Guid sender, Guid player, Guid card, string alternate)
        {
            _rpc.CardSwitchTo(sender, player, card, alternate);
        }

        public void PlayerSetGlobalVariable(Guid sender, Guid player, string name, string oldval, string val)
        {
            _rpc.PlayerSetGlobalVariable(sender, player, name, oldval, val);
        }

        public void SetGlobalVariable(Guid sender, string name, string oldval, string val)
        {
            _rpc.SetGlobalVariable(sender, name, oldval, val);
        }

        public void IsTableBackgroundFlipped(Guid sender, bool isFlipped)
        {
            _rpc.IsTableBackgroundFlipped(sender, isFlipped);
        }

        public void PlaySound(Guid sender, Guid player, string name)
        {
            _rpc.PlaySound(sender, player, name);
        }

        public void Ready(Guid sender, Guid player)
        {
            _rpc.Ready(sender, player);
        }

        public void PlayerState(Guid sender, Guid player, byte state)
        {
            _rpc.PlayerState(sender, player, state);
        }

        public void RemoteCall(Guid sender, Guid player, string function, string args)
        {
            _rpc.RemoteCall(sender, player, function, args);
        }

        public void GameStateReq(Guid sender, Guid player)
        {
            _rpc.GameStateReq(sender, player);
        }

        public void GameState(Guid sender, Guid toPlayer, string state)
        {
            _rpc.GameState(sender, toPlayer, state);
        }

        public void DeleteCard(Guid sender, Guid card, Guid player)
        {
            _rpc.DeleteCard(sender, card, player);
        }

        public void PlayerDisconnect(Guid sender, Guid player)
        {
            _rpc.PlayerDisconnect(sender, player);
        }

        public void AddPacks(Guid sender, Guid player, Guid[] packs, bool selfOnly)
        {
            _rpc.AddPacks(sender, player, packs, selfOnly);
        }

        public void AnchorCard(Guid sender, Guid id, Guid player, bool anchor)
        {
            _rpc.AnchorCard(sender, id, player, anchor);
        }

        public void SetCardProperty(Guid sender, Guid id, Guid player, string name, string val, string valtype)
        {
            _rpc.SetCardProperty(sender, id, player, name, val, valtype);
        }

        public void ResetCardProperties(Guid sender, Guid id, Guid player)
        {
            _rpc.ResetCardProperties(sender, id, player);
        }

        public void Filter(Guid sender, Guid card, string color)
        {
            _rpc.Filter(sender, card, color);
        }

        public void SetBoard(Guid sender, string name)
        {
            _rpc.SetBoard(sender, name);
        }

        public void SetPlayerColor(Guid sender, Guid player, string color)
        {
            _rpc.SetPlayerColor(sender, player, color);
        }

        public void SetPhase(Guid sender, byte phase, byte nextPhase)
        {
            _rpc.SetPhase(sender, phase, nextPhase);
        }

        public void StopPhase(Guid sender, Guid player, byte phase)
        {
            _rpc.StopPhase(sender, player, phase);
        }

		#endregion IServerToClientCalls
    }
}
