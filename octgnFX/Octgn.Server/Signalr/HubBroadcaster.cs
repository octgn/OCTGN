/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Microsoft.AspNet.SignalR;
using System;
using System.Dynamic;

namespace Octgn.Server.Signalr
{
    internal class HubBroadcaster : IServerToClientCalls
    {
        private readonly Hub _hub;
        public HubBroadcaster(Hub hub) {
            _hub = hub;
        }

        // This region is automatically generated from CallGenerator.tt
        // Do not modify anything in here...
        #region IServerToClientCalls

        public void Error(Guid sender, string msg)
        {
            _hub.Clients.All.Error(sender, msg);
        }

        public void Kick(Guid sender, string reason)
        {
            _hub.Clients.All.Kick(sender, reason);
        }

        public void Welcome(Guid sender, Guid id, Guid gameId, bool waitForGameState)
        {
            _hub.Clients.All.Welcome(sender, id, gameId, waitForGameState);
        }

        public void Settings(Guid sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            _hub.Clients.All.Settings(sender, twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(Guid sender, Guid playerId, bool invertedTable, bool spectator)
        {
            _hub.Clients.All.PlayerSettings(sender, playerId, invertedTable, spectator);
        }

        public void NewPlayer(Guid sender, Guid id, string nick, long pkey, bool tableSide, bool spectator)
        {
            _hub.Clients.All.NewPlayer(sender, id, nick, pkey, tableSide, spectator);
        }

        public void Leave(Guid sender, Guid player)
        {
            _hub.Clients.All.Leave(sender, player);
        }

        public void Nick(Guid sender, Guid player, string nick)
        {
            _hub.Clients.All.Nick(sender, player, nick);
        }

        public void Start(Guid sender)
        {
            _hub.Clients.All.Start(sender);
        }

        public void Reset(Guid sender, Guid player)
        {
            _hub.Clients.All.Reset(sender, player);
        }

        public void NextTurn(Guid sender, Guid nextPlayer)
        {
            _hub.Clients.All.NextTurn(sender, nextPlayer);
        }

        public void StopTurn(Guid sender, Guid player)
        {
            _hub.Clients.All.StopTurn(sender, player);
        }

        public void Chat(Guid sender, Guid player, string text)
        {
            _hub.Clients.All.Chat(sender, player, text);
        }

        public void Print(Guid sender, Guid player, string text)
        {
            _hub.Clients.All.Print(sender, player, text);
        }

        public void Random(Guid sender, int result)
        {
            _hub.Clients.All.Random(sender, result);
        }

        public void Counter(Guid sender, Guid player, Guid counter, int value, bool isScriptChange)
        {
            _hub.Clients.All.Counter(sender, player, counter, value, isScriptChange);
        }

        public void LoadDeck(Guid sender, Guid player, Guid[] id, Guid[] type, Guid[] group, string[] size, string sleeve, bool limited)
        {
            _hub.Clients.All.LoadDeck(sender, player, id, type, group, size, sleeve, limited);
        }

        public void CreateCard(Guid sender, Guid player, Guid[] id, Guid[] type, string[] size, Guid group)
        {
            _hub.Clients.All.CreateCard(sender, player, id, type, size, group);
        }

        public void CreateCardAt(Guid sender, Guid player, Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            _hub.Clients.All.CreateCardAt(sender, player, id, modelId, x, y, faceUp, persist);
        }

        public void MoveCard(Guid sender, Guid player, Guid[] id, Guid group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _hub.Clients.All.MoveCard(sender, player, id, group, idx, faceUp, isScriptMove);
        }

        public void MoveCardAt(Guid sender, Guid player, Guid[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _hub.Clients.All.MoveCardAt(sender, player, id, x, y, idx, faceUp, isScriptMove);
        }

        public void Peek(Guid sender, Guid player, Guid card)
        {
            _hub.Clients.All.Peek(sender, player, card);
        }

        public void Untarget(Guid sender, Guid player, Guid card, bool isScriptChange)
        {
            _hub.Clients.All.Untarget(sender, player, card, isScriptChange);
        }

        public void Target(Guid sender, Guid player, Guid card, bool isScriptChange)
        {
            _hub.Clients.All.Target(sender, player, card, isScriptChange);
        }

        public void TargetArrow(Guid sender, Guid player, Guid card, Guid otherCard, bool isScriptChange)
        {
            _hub.Clients.All.TargetArrow(sender, player, card, otherCard, isScriptChange);
        }

        public void Highlight(Guid sender, Guid card, string color)
        {
            _hub.Clients.All.Highlight(sender, card, color);
        }

        public void Turn(Guid sender, Guid player, Guid card, bool up)
        {
            _hub.Clients.All.Turn(sender, player, card, up);
        }

        public void Rotate(Guid sender, Guid player, Guid card, CardOrientation rot)
        {
            _hub.Clients.All.Rotate(sender, player, card, rot);
        }

        public void Shuffled(Guid sender, Guid player, Guid group, Guid[] card, short[] pos)
        {
            _hub.Clients.All.Shuffled(sender, player, group, card, pos);
        }

        public void AddMarker(Guid sender, Guid player, Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.AddMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void RemoveMarker(Guid sender, Guid player, Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.RemoveMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void TransferMarker(Guid sender, Guid player, Guid from, Guid to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.TransferMarker(sender, player, from, to, id, name, count, origCount, isScriptChange);
        }

        public void PassTo(Guid sender, Guid player, Guid id, Guid to, bool requested)
        {
            _hub.Clients.All.PassTo(sender, player, id, to, requested);
        }

        public void TakeFrom(Guid sender, Guid id, Guid to)
        {
            _hub.Clients.All.TakeFrom(sender, id, to);
        }

        public void DontTake(Guid sender, Guid id)
        {
            _hub.Clients.All.DontTake(sender, id);
        }

        public void FreezeCardsVisibility(Guid sender, Guid group)
        {
            _hub.Clients.All.FreezeCardsVisibility(sender, group);
        }

        public void GroupVis(Guid sender, Guid player, Guid group, bool defined, bool visible)
        {
            _hub.Clients.All.GroupVis(sender, player, group, defined, visible);
        }

        public void GroupVisAdd(Guid sender, Guid player, Guid group, Guid who)
        {
            _hub.Clients.All.GroupVisAdd(sender, player, group, who);
        }

        public void GroupVisRemove(Guid sender, Guid player, Guid group, Guid who)
        {
            _hub.Clients.All.GroupVisRemove(sender, player, group, who);
        }

        public void LookAt(Guid sender, Guid player, Guid uniqueid, Guid group, bool look)
        {
            _hub.Clients.All.LookAt(sender, player, uniqueid, group, look);
        }

        public void LookAtTop(Guid sender, Guid player, Guid uniqueid, Guid group, int count, bool look)
        {
            _hub.Clients.All.LookAtTop(sender, player, uniqueid, group, count, look);
        }

        public void LookAtBottom(Guid sender, Guid player, Guid uniqueid, Guid group, int count, bool look)
        {
            _hub.Clients.All.LookAtBottom(sender, player, uniqueid, group, count, look);
        }

        public void StartLimited(Guid sender, Guid player, Guid[] packs)
        {
            _hub.Clients.All.StartLimited(sender, player, packs);
        }

        public void CancelLimited(Guid sender, Guid player)
        {
            _hub.Clients.All.CancelLimited(sender, player);
        }

        public void CardSwitchTo(Guid sender, Guid player, Guid card, string alternate)
        {
            _hub.Clients.All.CardSwitchTo(sender, player, card, alternate);
        }

        public void PlayerSetGlobalVariable(Guid sender, Guid player, string name, string oldval, string val)
        {
            _hub.Clients.All.PlayerSetGlobalVariable(sender, player, name, oldval, val);
        }

        public void SetGlobalVariable(Guid sender, string name, string oldval, string val)
        {
            _hub.Clients.All.SetGlobalVariable(sender, name, oldval, val);
        }

        public void IsTableBackgroundFlipped(Guid sender, bool isFlipped)
        {
            _hub.Clients.All.IsTableBackgroundFlipped(sender, isFlipped);
        }

        public void PlaySound(Guid sender, Guid player, string name)
        {
            _hub.Clients.All.PlaySound(sender, player, name);
        }

        public void Ready(Guid sender, Guid player)
        {
            _hub.Clients.All.Ready(sender, player);
        }

        public void PlayerState(Guid sender, Guid player, byte state)
        {
            _hub.Clients.All.PlayerState(sender, player, state);
        }

        public void RemoteCall(Guid sender, Guid player, string function, string args)
        {
            _hub.Clients.All.RemoteCall(sender, player, function, args);
        }

        public void GameStateReq(Guid sender, Guid player)
        {
            _hub.Clients.All.GameStateReq(sender, player);
        }

        public void GameState(Guid sender, Guid toPlayer, string state)
        {
            _hub.Clients.All.GameState(sender, toPlayer, state);
        }

        public void DeleteCard(Guid sender, Guid card, Guid player)
        {
            _hub.Clients.All.DeleteCard(sender, card, player);
        }

        public void PlayerDisconnect(Guid sender, Guid player)
        {
            _hub.Clients.All.PlayerDisconnect(sender, player);
        }

        public void AddPacks(Guid sender, Guid player, Guid[] packs, bool selfOnly)
        {
            _hub.Clients.All.AddPacks(sender, player, packs, selfOnly);
        }

        public void AnchorCard(Guid sender, Guid id, Guid player, bool anchor)
        {
            _hub.Clients.All.AnchorCard(sender, id, player, anchor);
        }

        public void SetCardProperty(Guid sender, Guid id, Guid player, string name, string val, string valtype)
        {
            _hub.Clients.All.SetCardProperty(sender, id, player, name, val, valtype);
        }

        public void ResetCardProperties(Guid sender, Guid id, Guid player)
        {
            _hub.Clients.All.ResetCardProperties(sender, id, player);
        }

        public void Filter(Guid sender, Guid card, string color)
        {
            _hub.Clients.All.Filter(sender, card, color);
        }

        public void SetBoard(Guid sender, string name)
        {
            _hub.Clients.All.SetBoard(sender, name);
        }

        public void SetPlayerColor(Guid sender, Guid player, string color)
        {
            _hub.Clients.All.SetPlayerColor(sender, player, color);
        }

        public void SetPhase(Guid sender, byte phase, byte nextPhase)
        {
            _hub.Clients.All.SetPhase(sender, phase, nextPhase);
        }

        public void StopPhase(Guid sender, Guid player, byte phase)
        {
            _hub.Clients.All.StopPhase(sender, player, phase);
        }

		#endregion IServerToClientCalls
    }
}
