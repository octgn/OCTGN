/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Microsoft.AspNet.SignalR;
using System;
using System.Dynamic;

namespace Octgn.Server.Signalr
{
    internal class HubBroadcaster : IClientCalls
    {
        private readonly Hub _hub;
        public HubBroadcaster(Hub hub) {
            _hub = hub;
        }

        // This region is automatically generated from CallGenerator.tt
        // Do not modify anything in here...
        #region IClientCalls

        public void Error(string msg)
        {
            _hub.Clients.All.Error(msg);
        }

        public void Kick(string reason)
        {
            _hub.Clients.All.Kick(reason);
        }

        public void Welcome(uint id, Guid gameSessionId, bool waitForGameState)
        {
            _hub.Clients.All.Welcome(id, gameSessionId, waitForGameState);
        }

        public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
        {
            _hub.Clients.All.Settings(twoSidedTable, allowSpectators, muteSpectators);
        }

        public void PlayerSettings(uint playerId, bool invertedTable, bool spectator)
        {
            _hub.Clients.All.PlayerSettings(playerId, invertedTable, spectator);
        }

        public void NewPlayer(uint id, string nick, ulong pkey, bool tableSide, bool spectator)
        {
            _hub.Clients.All.NewPlayer(id, nick, pkey, tableSide, spectator);
        }

        public void Leave(uint player)
        {
            _hub.Clients.All.Leave(player);
        }

        public void Nick(uint player, string nick)
        {
            _hub.Clients.All.Nick(player, nick);
        }

        public void Start()
        {
            _hub.Clients.All.Start();
        }

        public void Reset(uint player)
        {
            _hub.Clients.All.Reset(player);
        }

        public void NextTurn(uint nextPlayer)
        {
            _hub.Clients.All.NextTurn(nextPlayer);
        }

        public void StopTurn(uint player)
        {
            _hub.Clients.All.StopTurn(player);
        }

        public void Chat(uint player, string text)
        {
            _hub.Clients.All.Chat(player, text);
        }

        public void Print(uint player, string text)
        {
            _hub.Clients.All.Print(player, text);
        }

        public void Random(int result)
        {
            _hub.Clients.All.Random(result);
        }

        public void Counter(uint player, ulong counter, int value, bool isScriptChange)
        {
            _hub.Clients.All.Counter(player, counter, value, isScriptChange);
        }

        public void LoadDeck(uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited)
        {
            _hub.Clients.All.LoadDeck(player, id, type, group, size, sleeve, limited);
        }

        public void CreateCard(uint player, ulong[] id, Guid[] type, string[] size, ulong group)
        {
            _hub.Clients.All.CreateCard(player, id, type, size, group);
        }

        public void CreateCardAt(uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            _hub.Clients.All.CreateCardAt(player, id, modelId, x, y, faceUp, persist);
        }

        public void MoveCard(uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _hub.Clients.All.MoveCard(player, id, group, idx, faceUp, isScriptMove);
        }

        public void MoveCardAt(uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
        {
            _hub.Clients.All.MoveCardAt(player, id, x, y, idx, faceUp, isScriptMove);
        }

        public void Peek(uint player, ulong card)
        {
            _hub.Clients.All.Peek(player, card);
        }

        public void Untarget(uint player, ulong card, bool isScriptChange)
        {
            _hub.Clients.All.Untarget(player, card, isScriptChange);
        }

        public void Target(uint player, ulong card, bool isScriptChange)
        {
            _hub.Clients.All.Target(player, card, isScriptChange);
        }

        public void TargetArrow(uint player, ulong card, ulong otherCard, bool isScriptChange)
        {
            _hub.Clients.All.TargetArrow(player, card, otherCard, isScriptChange);
        }

        public void Highlight(ulong card, string color)
        {
            _hub.Clients.All.Highlight(card, color);
        }

        public void Turn(uint player, ulong card, bool up)
        {
            _hub.Clients.All.Turn(player, card, up);
        }

        public void Rotate(uint player, ulong card, CardOrientation rot)
        {
            _hub.Clients.All.Rotate(player, card, rot);
        }

        public void Shuffled(uint player, ulong group, ulong[] card, short[] pos)
        {
            _hub.Clients.All.Shuffled(player, group, card, pos);
        }

        public void AddMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.AddMarker(player, card, id, name, count, origCount, isScriptChange);
        }

        public void RemoveMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.RemoveMarker(player, card, id, name, count, origCount, isScriptChange);
        }

        public void TransferMarker(uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
        {
            _hub.Clients.All.TransferMarker(player, from, to, id, name, count, origCount, isScriptChange);
        }

        public void PassTo(uint player, ulong id, uint to, bool requested)
        {
            _hub.Clients.All.PassTo(player, id, to, requested);
        }

        public void TakeFrom(ulong id, uint to)
        {
            _hub.Clients.All.TakeFrom(id, to);
        }

        public void DontTake(ulong id)
        {
            _hub.Clients.All.DontTake(id);
        }

        public void FreezeCardsVisibility(ulong group)
        {
            _hub.Clients.All.FreezeCardsVisibility(group);
        }

        public void GroupVis(uint player, ulong group, bool defined, bool visible)
        {
            _hub.Clients.All.GroupVis(player, group, defined, visible);
        }

        public void GroupVisAdd(uint player, ulong group, uint who)
        {
            _hub.Clients.All.GroupVisAdd(player, group, who);
        }

        public void GroupVisRemove(uint player, ulong group, uint who)
        {
            _hub.Clients.All.GroupVisRemove(player, group, who);
        }

        public void LookAt(uint player, uint uid, ulong group, bool look)
        {
            _hub.Clients.All.LookAt(player, uid, group, look);
        }

        public void LookAtTop(uint player, uint uid, ulong group, int count, bool look)
        {
            _hub.Clients.All.LookAtTop(player, uid, group, count, look);
        }

        public void LookAtBottom(uint player, uint uid, ulong group, int count, bool look)
        {
            _hub.Clients.All.LookAtBottom(player, uid, group, count, look);
        }

        public void StartLimited(uint player, Guid[] packs)
        {
            _hub.Clients.All.StartLimited(player, packs);
        }

        public void CancelLimited(uint player)
        {
            _hub.Clients.All.CancelLimited(player);
        }

        public void CardSwitchTo(uint player, ulong card, string alternate)
        {
            _hub.Clients.All.CardSwitchTo(player, card, alternate);
        }

        public void PlayerSetGlobalVariable(uint player, string name, string oldval, string val)
        {
            _hub.Clients.All.PlayerSetGlobalVariable(player, name, oldval, val);
        }

        public void SetGlobalVariable(string name, string oldval, string val)
        {
            _hub.Clients.All.SetGlobalVariable(name, oldval, val);
        }

        public void Ping()
        {
            _hub.Clients.All.Ping();
        }

        public void IsTableBackgroundFlipped(bool isFlipped)
        {
            _hub.Clients.All.IsTableBackgroundFlipped(isFlipped);
        }

        public void PlaySound(uint player, string name)
        {
            _hub.Clients.All.PlaySound(player, name);
        }

        public void Ready(uint player)
        {
            _hub.Clients.All.Ready(player);
        }

        public void PlayerState(uint player, byte state)
        {
            _hub.Clients.All.PlayerState(player, state);
        }

        public void RemoteCall(uint player, string function, string args)
        {
            _hub.Clients.All.RemoteCall(player, function, args);
        }

        public void GameStateReq(uint player)
        {
            _hub.Clients.All.GameStateReq(player);
        }

        public void GameState(uint toPlayer, string state)
        {
            _hub.Clients.All.GameState(toPlayer, state);
        }

        public void DeleteCard(ulong card, uint player)
        {
            _hub.Clients.All.DeleteCard(card, player);
        }

        public void PlayerDisconnect(uint player)
        {
            _hub.Clients.All.PlayerDisconnect(player);
        }

        public void AddPacks(uint player, Guid[] packs, bool selfOnly)
        {
            _hub.Clients.All.AddPacks(player, packs, selfOnly);
        }

        public void AnchorCard(ulong id, uint player, bool anchor)
        {
            _hub.Clients.All.AnchorCard(id, player, anchor);
        }

        public void SetCardProperty(ulong id, uint player, string name, string val, string valtype)
        {
            _hub.Clients.All.SetCardProperty(id, player, name, val, valtype);
        }

        public void ResetCardProperties(ulong id, uint player)
        {
            _hub.Clients.All.ResetCardProperties(id, player);
        }

        public void Filter(ulong card, string color)
        {
            _hub.Clients.All.Filter(card, color);
        }

        public void SetBoard(string name)
        {
            _hub.Clients.All.SetBoard(name);
        }

        public void SetPlayerColor(uint player, string color)
        {
            _hub.Clients.All.SetPlayerColor(player, color);
        }

        public void SetPhase(byte phase, byte nextPhase)
        {
            _hub.Clients.All.SetPhase(phase, nextPhase);
        }

        public void StopPhase(uint player, byte phase)
        {
            _hub.Clients.All.StopPhase(player, phase);
        }

		#endregion IClientCalls
    }
}
