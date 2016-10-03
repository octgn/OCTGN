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
	interface IClientToServerCalls
	{
		void Error(string msg);
		void Boot(uint player, string reason);
		void Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		void HelloAgain(uint pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
		void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(uint playerId, bool invertedTable, bool spectator);
		void Leave(uint player);
		void NickReq(string nick);
		void Start();
		void ResetReq();
		void NextTurn(uint nextPlayer);
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int min, int max);
		void CounterReq(ulong counter, int value, bool isScriptChange);
		void LoadDeck(ulong[] id, Guid[] type, ulong[] group, string[] size, string sleeve, bool limited);
		void CreateCard(ulong[] id, Guid[] type, string[] size, ulong group);
		void CreateCardAt(ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void MoveCardReq(ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		void PeekReq(ulong card);
		void UntargetReq(ulong card, bool isScriptChange);
		void TargetReq(ulong card, bool isScriptChange);
		void TargetArrowReq(ulong card, ulong otherCard, bool isScriptChange);
		void Highlight(ulong card, string color);
		void TurnReq(ulong card, bool up);
		void RotateReq(ulong card, CardOrientation rot);
		void Shuffled(uint player, ulong group, ulong[] card, short[] pos);
		void AddMarkerReq(ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarkerReq(ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarkerReq(ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassToReq(ulong id, uint to, bool requested);
		void TakeFromReq(ulong id, uint from);
		void DontTakeReq(ulong id, uint to);
		void FreezeCardsVisibility(ulong group);
		void GroupVisReq(ulong group, bool defined, bool visible);
		void GroupVisAddReq(ulong group, uint who);
		void GroupVisRemoveReq(ulong group, uint who);
		void LookAtReq(uint uniqueid, ulong group, bool look);
		void LookAtTopReq(uint uniqueid, ulong group, int count, bool look);
		void LookAtBottomReq(uint uniqueid, ulong group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void CardSwitchTo(uint player, ulong card, string alternate);
		void PlayerSetGlobalVariable(uint player, string name, string oldval, string val);
		void SetGlobalVariable(string name, string oldval, string val);
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(uint player, string name);
		void Ready(uint player);
		void RemoteCall(uint player, string function, string args);
		void GameStateReq(uint player);
		void GameState(uint toPlayer, string state);
		void DeleteCard(ulong card, uint player);
		void AddPacksReq(Guid[] packs, bool selfOnly);
		void AnchorCard(ulong id, uint player, bool anchor);
		void SetCardProperty(ulong id, uint player, string name, string val, string valtype);
		void ResetCardProperties(ulong id, uint player);
		void Filter(ulong card, string color);
		void SetBoard(string name);
		void SetPlayerColor(uint player, string color);
		void SetPhase(byte phase, byte nextPhase);
		void StopPhaseReq(int turnNumber, byte phase, bool stop);
	}
}
