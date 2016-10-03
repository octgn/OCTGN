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

        public void Error(uint sender, string msg)
        {
            _hub.Clients.All.Error(sender, msg);
        }

        public void Kick(uint sender, string reason)
        {
            _hub.Clients.All.Kick(sender, reason);
        }

        public void Welcome(uint sender, uint id, uint gameId, bool waitForGameState)
        {
            _hub.Clients.All.Welcome(sender, id, gameId, waitForGameState);
        }

        public void Settings(uint sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            _hub.Clients.All.Settings(sender, twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(uint sender, uint playerId, bool invertedTable, bool spectator)
        {
            _hub.Clients.All.PlayerSettings(sender, playerId, invertedTable, spectator);
        }

        public void NewPlayer(uint sender, uint id, string nick, long pkey, bool tableSide, bool spectator)
        {
            _hub.Clients.All.NewPlayer(sender, id, nick, pkey, tableSide, spectator);
        }

        public void Leave(uint sender, uint player)
        {
            _hub.Clients.All.Leave(sender, player);
        }

        public void Nick(uint sender, uint player, string nick)
        {
            _hub.Clients.All.Nick(sender, player, nick);
        }

        public void Start(uint sender)
        {
            _hub.Clients.All.Start(sender);
        }

        public void Reset(uint sender, uint player)
        {
            _hub.Clients.All.Reset(sender, player);
        }

        public void NextTurn(uint sender, uint nextPlayer)
        {
            _hub.Clients.All.NextTurn(sender, nextPlayer);
        }

        public void StopTurn(uint sender, uint player)
        {
            _hub.Clients.All.StopTurn(sender, player);
        }

        public void Chat(uint sender, uint player, string text)
        {
            _hub.Clients.All.Chat(sender, player, text);
        }

        public void Print(uint sender, uint player, string text)
        {
            _hub.Clients.All.Print(sender, player, text);
        }

        public void Random(uint sender, int result)
        {
            _hub.Clients.All.Random(sender, result);
        }

        public void Counter(uint sender, uint player, ulong counter, int value, bool isScriptChange)
        {
            _hub.Clients.All.Counter(sender, player, counter, value, isScriptChange);
        }

        public void LoadDeck(uint sender, uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited)
        {
            _hub.Clients.All.LoadDeck(sender, player, id, type, group, size, sleeve, limited);
        }

        public void CreateCard(uint sender, uint player, ulong[] id, Guid[] type, string[] size, ulong group)
        {
            _hub.Clients.All.CreateCard(sender, player, id, type, size, group);
        }

        public void CreateCardAt(uint sender, uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            _hub.Clients.All.CreateCardAt(sender, player, id, modelId, x, y, faceUp, persist);
        }

        public void MoveCard(uint sender, uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _hub.Clients.All.MoveCard(sender, player, id, group, idx, faceUp, isScriptMove);
        }

        public void MoveCardAt(uint sender, uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _hub.Clients.All.MoveCardAt(sender, player, id, x, y, idx, faceUp, isScriptMove);
        }

        public void Peek(uint sender, uint player, ulong card)
        {
            _hub.Clients.All.Peek(sender, player, card);
        }

        public void Untarget(uint sender, uint player, ulong card, bool isScriptChange)
        {
            _hub.Clients.All.Untarget(sender, player, card, isScriptChange);
        }

        public void Target(uint sender, uint player, ulong card, bool isScriptChange)
        {
            _hub.Clients.All.Target(sender, player, card, isScriptChange);
        }

        public void TargetArrow(uint sender, uint player, ulong card, ulong otherCard, bool isScriptChange)
        {
            _hub.Clients.All.TargetArrow(sender, player, card, otherCard, isScriptChange);
        }

        public void Highlight(uint sender, ulong card, string color)
        {
            _hub.Clients.All.Highlight(sender, card, color);
        }

        public void Turn(uint sender, uint player, ulong card, bool up)
        {
            _hub.Clients.All.Turn(sender, player, card, up);
        }

        public void Rotate(uint sender, uint player, ulong card, CardOrientation rot)
        {
            _hub.Clients.All.Rotate(sender, player, card, rot);
        }

        public void Shuffled(uint sender, uint player, ulong group, ulong[] card, short[] pos)
        {
            _hub.Clients.All.Shuffled(sender, player, group, card, pos);
        }

        public void AddMarker(uint sender, uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.AddMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void RemoveMarker(uint sender, uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.RemoveMarker(sender, player, card, id, name, count, origCount, isScriptChange);
        }

        public void TransferMarker(uint sender, uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.TransferMarker(sender, player, from, to, id, name, count, origCount, isScriptChange);
        }

        public void PassTo(uint sender, uint player, ulong id, uint to, bool requested)
        {
            _hub.Clients.All.PassTo(sender, player, id, to, requested);
        }

        public void TakeFrom(uint sender, ulong id, uint to)
        {
            _hub.Clients.All.TakeFrom(sender, id, to);
        }

        public void DontTake(uint sender, ulong id)
        {
            _hub.Clients.All.DontTake(sender, id);
        }

        public void FreezeCardsVisibility(uint sender, ulong group)
        {
            _hub.Clients.All.FreezeCardsVisibility(sender, group);
        }

        public void GroupVis(uint sender, uint player, ulong group, bool defined, bool visible)
        {
            _hub.Clients.All.GroupVis(sender, player, group, defined, visible);
        }

        public void GroupVisAdd(uint sender, uint player, ulong group, uint who)
        {
            _hub.Clients.All.GroupVisAdd(sender, player, group, who);
        }

        public void GroupVisRemove(uint sender, uint player, ulong group, uint who)
        {
            _hub.Clients.All.GroupVisRemove(sender, player, group, who);
        }

        public void LookAt(uint sender, uint player, uint uniqueid, ulong group, bool look)
        {
            _hub.Clients.All.LookAt(sender, player, uniqueid, group, look);
        }

        public void LookAtTop(uint sender, uint player, uint uniqueid, ulong group, int count, bool look)
        {
            _hub.Clients.All.LookAtTop(sender, player, uniqueid, group, count, look);
        }

        public void LookAtBottom(uint sender, uint player, uint uniqueid, ulong group, int count, bool look)
        {
            _hub.Clients.All.LookAtBottom(sender, player, uniqueid, group, count, look);
        }

        public void StartLimited(uint sender, uint player, Guid[] packs)
        {
            _hub.Clients.All.StartLimited(sender, player, packs);
        }

        public void CancelLimited(uint sender, uint player)
        {
            _hub.Clients.All.CancelLimited(sender, player);
        }

        public void CardSwitchTo(uint sender, uint player, ulong card, string alternate)
        {
            _hub.Clients.All.CardSwitchTo(sender, player, card, alternate);
        }

        public void PlayerSetGlobalVariable(uint sender, uint player, string name, string oldval, string val)
        {
            _hub.Clients.All.PlayerSetGlobalVariable(sender, player, name, oldval, val);
        }

        public void SetGlobalVariable(uint sender, string name, string oldval, string val)
        {
            _hub.Clients.All.SetGlobalVariable(sender, name, oldval, val);
        }

        public void IsTableBackgroundFlipped(uint sender, bool isFlipped)
        {
            _hub.Clients.All.IsTableBackgroundFlipped(sender, isFlipped);
        }

        public void PlaySound(uint sender, uint player, string name)
        {
            _hub.Clients.All.PlaySound(sender, player, name);
        }

        public void Ready(uint sender, uint player)
        {
            _hub.Clients.All.Ready(sender, player);
        }

        public void PlayerState(uint sender, uint player, byte state)
        {
            _hub.Clients.All.PlayerState(sender, player, state);
        }

        public void RemoteCall(uint sender, uint player, string function, string args)
        {
            _hub.Clients.All.RemoteCall(sender, player, function, args);
        }

        public void GameStateReq(uint sender, uint player)
        {
            _hub.Clients.All.GameStateReq(sender, player);
        }

        public void GameState(uint sender, uint toPlayer, string state)
        {
            _hub.Clients.All.GameState(sender, toPlayer, state);
        }

        public void DeleteCard(uint sender, ulong card, uint player)
        {
            _hub.Clients.All.DeleteCard(sender, card, player);
        }

        public void PlayerDisconnect(uint sender, uint player)
        {
            _hub.Clients.All.PlayerDisconnect(sender, player);
        }

        public void AddPacks(uint sender, uint player, Guid[] packs, bool selfOnly)
        {
            _hub.Clients.All.AddPacks(sender, player, packs, selfOnly);
        }

        public void AnchorCard(uint sender, ulong id, uint player, bool anchor)
        {
            _hub.Clients.All.AnchorCard(sender, id, player, anchor);
        }

        public void SetCardProperty(uint sender, ulong id, uint player, string name, string val, string valtype)
        {
            _hub.Clients.All.SetCardProperty(sender, id, player, name, val, valtype);
        }

        public void ResetCardProperties(uint sender, ulong id, uint player)
        {
            _hub.Clients.All.ResetCardProperties(sender, id, player);
        }

        public void Filter(uint sender, ulong card, string color)
        {
            _hub.Clients.All.Filter(sender, card, color);
        }

        public void SetBoard(uint sender, string name)
        {
            _hub.Clients.All.SetBoard(sender, name);
        }

        public void SetPlayerColor(uint sender, uint player, string color)
        {
            _hub.Clients.All.SetPlayerColor(sender, player, color);
        }

        public void SetPhase(uint sender, byte phase, byte nextPhase)
        {
            _hub.Clients.All.SetPhase(sender, phase, nextPhase);
        }

        public void StopPhase(uint sender, uint player, byte phase)
        {
            _hub.Clients.All.StopPhase(sender, player, phase);
        }

		#endregion IServerToClientCalls
    }
}
