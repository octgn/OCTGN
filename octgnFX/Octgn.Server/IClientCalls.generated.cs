/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;

namespace Octgn.Server
{
	public interface IClientCalls
	{
		void Error(string msg);
		void Kick(string reason);
		void Welcome(uint id, Guid gameSessionId, bool waitForGameState);
		void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(uint playerId, bool invertedTable, bool spectator);
		void NewPlayer(uint id, string nick, ulong pkey, bool tableSide, bool spectator);
		void Leave(uint player);
		void Nick(uint player, string nick);
		void Start();
		void Reset(uint player);
		void NextTurn(uint nextPlayer);
		void StopTurn(uint player);
		void Chat(uint player, string text);
		void Print(uint player, string text);
		void Random(int result);
		void Counter(uint player, ulong counter, int value, bool isScriptChange);
		void LoadDeck(uint player, ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited);
		void CreateCard(uint player, ulong[] id, Guid[] type, string[] size, ulong group);
		void CreateCardAt(uint player, ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void MoveCard(uint player, ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAt(uint player, ulong[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove);
		void Peek(uint player, ulong card);
		void Untarget(uint player, ulong card, bool isScriptChange);
		void Target(uint player, ulong card, bool isScriptChange);
		void TargetArrow(uint player, ulong card, ulong otherCard, bool isScriptChange);
		void Highlight(ulong card, string color);
		void Turn(uint player, ulong card, bool up);
		void Rotate(uint player, ulong card, CardOrientation rot);
		void Shuffled(uint player, ulong group, ulong[] card, short[] pos);
		void AddMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarker(uint player, ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarker(uint player, ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassTo(uint player, ulong id, uint to, bool requested);
		void TakeFrom(ulong id, uint to);
		void DontTake(ulong id);
		void FreezeCardsVisibility(ulong group);
		void GroupVis(uint player, ulong group, bool defined, bool visible);
		void GroupVisAdd(uint player, ulong group, uint who);
		void GroupVisRemove(uint player, ulong group, uint who);
		void LookAt(uint player, uint uid, ulong group, bool look);
		void LookAtTop(uint player, uint uid, ulong group, int count, bool look);
		void LookAtBottom(uint player, uint uid, ulong group, int count, bool look);
		void StartLimited(uint player, Guid[] packs);
		void CancelLimited(uint player);
		void CardSwitchTo(uint player, ulong card, string alternate);
		void PlayerSetGlobalVariable(uint player, string name, string oldval, string val);
		void SetGlobalVariable(string name, string oldval, string val);
		void Ping();
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(uint player, string name);
		void Ready(uint player);
		void PlayerState(uint player, byte state);
		void RemoteCall(uint player, string function, string args);
		void GameStateReq(uint player);
		void GameState(uint toPlayer, string state);
		void DeleteCard(ulong card, uint player);
		void PlayerDisconnect(uint player);
		void AddPacks(uint player, Guid[] packs, bool selfOnly);
		void AnchorCard(ulong id, uint player, bool anchor);
		void SetCardProperty(ulong id, uint player, string name, string val, string valtype);
		void ResetCardProperties(ulong id, uint player);
		void Filter(ulong card, string color);
		void SetBoard(string name);
		void SetPlayerColor(uint player, string color);
		void SetPhase(byte phase, byte nextPhase);
		void StopPhase(uint player, byte phase);
	}
}
