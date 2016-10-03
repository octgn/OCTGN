/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using Microsoft.AspNet.SignalR;

namespace Octgn.Server
{
	public interface IServerToClientCalls
	{
		void Error(uint sender, string msg);
		void Kick(uint sender, string reason);
		void Welcome(uint sender, uint id, uint gameId, bool waitForGameState);
		void Settings(uint sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(uint sender, uint playerId, bool invertedTable, bool spectator);
		void NewPlayer(uint sender, uint id, string nick, long pkey, bool tableSide, bool spectator);
		void Leave(uint sender, uint player);
		void Nick(uint sender, uint player, string nick);
		void Start(uint sender);
		void Reset(uint sender, uint player);
		void NextTurn(uint sender, uint nextPlayer);
		void StopTurn(uint sender, uint player);
		void Chat(uint sender, uint player, string text);
		void Print(uint sender, uint player, string text);
		void Random(uint sender, int result);
		void Counter(uint sender, uint player, ulong counter, int value, bool isScriptChange);
		void LoadDeck(uint sender, uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited);
		void CreateCard(uint sender, uint player, ulong[] id, Guid[] type, string[] size, ulong group);
		void CreateCardAt(uint sender, uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void MoveCard(uint sender, uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAt(uint sender, uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove);
		void Peek(uint sender, uint player, ulong card);
		void Untarget(uint sender, uint player, ulong card, bool isScriptChange);
		void Target(uint sender, uint player, ulong card, bool isScriptChange);
		void TargetArrow(uint sender, uint player, ulong card, ulong otherCard, bool isScriptChange);
		void Highlight(uint sender, ulong card, string color);
		void Turn(uint sender, uint player, ulong card, bool up);
		void Rotate(uint sender, uint player, ulong card, CardOrientation rot);
		void Shuffled(uint sender, uint player, ulong group, ulong[] card, short[] pos);
		void AddMarker(uint sender, uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarker(uint sender, uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarker(uint sender, uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassTo(uint sender, uint player, ulong id, uint to, bool requested);
		void TakeFrom(uint sender, ulong id, uint to);
		void DontTake(uint sender, ulong id);
		void FreezeCardsVisibility(uint sender, ulong group);
		void GroupVis(uint sender, uint player, ulong group, bool defined, bool visible);
		void GroupVisAdd(uint sender, uint player, ulong group, uint who);
		void GroupVisRemove(uint sender, uint player, ulong group, uint who);
		void LookAt(uint sender, uint player, uint uniqueid, ulong group, bool look);
		void LookAtTop(uint sender, uint player, uint uniqueid, ulong group, int count, bool look);
		void LookAtBottom(uint sender, uint player, uint uniqueid, ulong group, int count, bool look);
		void StartLimited(uint sender, uint player, Guid[] packs);
		void CancelLimited(uint sender, uint player);
		void CardSwitchTo(uint sender, uint player, ulong card, string alternate);
		void PlayerSetGlobalVariable(uint sender, uint player, string name, string oldval, string val);
		void SetGlobalVariable(uint sender, string name, string oldval, string val);
		void IsTableBackgroundFlipped(uint sender, bool isFlipped);
		void PlaySound(uint sender, uint player, string name);
		void Ready(uint sender, uint player);
		void PlayerState(uint sender, uint player, byte state);
		void RemoteCall(uint sender, uint player, string function, string args);
		void GameStateReq(uint sender, uint player);
		void GameState(uint sender, uint toPlayer, string state);
		void DeleteCard(uint sender, ulong card, uint player);
		void PlayerDisconnect(uint sender, uint player);
		void AddPacks(uint sender, uint player, Guid[] packs, bool selfOnly);
		void AnchorCard(uint sender, ulong id, uint player, bool anchor);
		void SetCardProperty(uint sender, ulong id, uint player, string name, string val, string valtype);
		void ResetCardProperties(uint sender, ulong id, uint player);
		void Filter(uint sender, ulong card, string color);
		void SetBoard(uint sender, string name);
		void SetPlayerColor(uint sender, uint player, string color);
		void SetPhase(uint sender, byte phase, byte nextPhase);
		void StopPhase(uint sender, uint player, byte phase);
	}
}
