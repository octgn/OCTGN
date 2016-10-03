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
		void Boot(Guid player, string reason);
		void Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		void HelloAgain(Guid pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
		void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(Guid playerId, bool invertedTable, bool spectator);
		void Leave(Guid player);
		void NickReq(string nick);
		void Start();
		void ResetReq();
		void NextTurn(Guid nextPlayer);
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int min, int max);
		void CounterReq(Guid counter, int value, bool isScriptChange);
		void LoadDeck(Guid[] id, Guid[] type, Guid[] group, string[] size, string sleeve, bool limited);
		void CreateCard(Guid[] id, Guid[] type, string[] size, Guid group);
		void CreateCardAt(Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void MoveCardReq(Guid[] id, Guid group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAtReq(Guid[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		void PeekReq(Guid card);
		void UntargetReq(Guid card, bool isScriptChange);
		void TargetReq(Guid card, bool isScriptChange);
		void TargetArrowReq(Guid card, Guid otherCard, bool isScriptChange);
		void Highlight(Guid card, string color);
		void TurnReq(Guid card, bool up);
		void RotateReq(Guid card, CardOrientation rot);
		void Shuffled(Guid player, Guid group, Guid[] card, short[] pos);
		void AddMarkerReq(Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarkerReq(Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarkerReq(Guid from, Guid to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassToReq(Guid id, Guid to, bool requested);
		void TakeFromReq(Guid id, Guid from);
		void DontTakeReq(Guid id, Guid to);
		void FreezeCardsVisibility(Guid group);
		void GroupVisReq(Guid group, bool defined, bool visible);
		void GroupVisAddReq(Guid group, Guid who);
		void GroupVisRemoveReq(Guid group, Guid who);
		void LookAtReq(Guid uniqueid, Guid group, bool look);
		void LookAtTopReq(Guid uniqueid, Guid group, int count, bool look);
		void LookAtBottomReq(Guid uniqueid, Guid group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void CardSwitchTo(Guid player, Guid card, string alternate);
		void PlayerSetGlobalVariable(Guid player, string name, string oldval, string val);
		void SetGlobalVariable(string name, string oldval, string val);
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(Guid player, string name);
		void Ready(Guid player);
		void RemoteCall(Guid player, string function, string args);
		void GameStateReq(Guid player);
		void GameState(Guid toPlayer, string state);
		void DeleteCard(Guid card, Guid player);
		void AddPacksReq(Guid[] packs, bool selfOnly);
		void AnchorCard(Guid id, Guid player, bool anchor);
		void SetCardProperty(Guid id, Guid player, string name, string val, string valtype);
		void ResetCardProperties(Guid id, Guid player);
		void Filter(Guid card, string color);
		void SetBoard(string name);
		void SetPlayerColor(Guid player, string color);
		void SetPhase(byte phase, byte nextPhase);
		void StopPhaseReq(int turnNumber, byte phase, bool stop);
	}
}
