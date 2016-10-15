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
	public interface IServerToClientCalls
	{
		void Error(Guid sender, string msg);
		void Kick(Guid sender, string reason);
		void Welcome(Guid sender, Guid id, Guid gameId, bool waitForGameState);
		void Settings(Guid sender, bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(Guid sender, Guid playerId, bool invertedTable, bool spectator);
		void NewPlayer(Guid sender, Guid id, string nick, long pkey, bool tableSide, bool spectator);
		void Leave(Guid sender, Guid player);
		void Nick(Guid sender, Guid player, string nick);
		void Start(Guid sender);
		void Reset(Guid sender, Guid player);
		void NextTurn(Guid sender, Guid nextPlayer);
		void StopTurn(Guid sender, Guid player);
		void Chat(Guid sender, Guid player, string text);
		void Print(Guid sender, Guid player, string text);
		void Random(Guid sender, int result);
		void Counter(Guid sender, Guid player, Guid counter, int value, bool isScriptChange);
		void LoadDeck(Guid sender, Guid player, Guid[] id, Guid[] type, Guid[] group, string[] size, string sleeve, bool limited);
		void CreateCard(Guid sender, Guid player, Guid[] id, Guid[] type, string[] size, Guid group);
		void CreateCardAt(Guid sender, Guid player, Guid[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void MoveCard(Guid sender, Guid player, Guid[] id, Guid group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAt(Guid sender, Guid player, Guid[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove);
		void Peek(Guid sender, Guid player, Guid card);
		void Untarget(Guid sender, Guid player, Guid card, bool isScriptChange);
		void Target(Guid sender, Guid player, Guid card, bool isScriptChange);
		void TargetArrow(Guid sender, Guid player, Guid card, Guid otherCard, bool isScriptChange);
		void Highlight(Guid sender, Guid card, string color);
		void Turn(Guid sender, Guid player, Guid card, bool up);
		void Rotate(Guid sender, Guid player, Guid card, CardOrientation rot);
		void Shuffled(Guid sender, Guid player, Guid group, Guid[] card, short[] pos);
		void AddMarker(Guid sender, Guid player, Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarker(Guid sender, Guid player, Guid card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarker(Guid sender, Guid player, Guid from, Guid to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassTo(Guid sender, Guid player, Guid id, Guid to, bool requested);
		void TakeFrom(Guid sender, Guid id, Guid to);
		void DontTake(Guid sender, Guid id);
		void FreezeCardsVisibility(Guid sender, Guid group);
		void GroupVis(Guid sender, Guid player, Guid group, bool defined, bool visible);
		void GroupVisAdd(Guid sender, Guid player, Guid group, Guid who);
		void GroupVisRemove(Guid sender, Guid player, Guid group, Guid who);
		void LookAt(Guid sender, Guid player, Guid uniqueid, Guid group, bool look);
		void LookAtTop(Guid sender, Guid player, Guid uniqueid, Guid group, int count, bool look);
		void LookAtBottom(Guid sender, Guid player, Guid uniqueid, Guid group, int count, bool look);
		void StartLimited(Guid sender, Guid player, Guid[] packs);
		void CancelLimited(Guid sender, Guid player);
		void CardSwitchTo(Guid sender, Guid player, Guid card, string alternate);
		void PlayerSetGlobalVariable(Guid sender, Guid player, string name, string oldval, string val);
		void SetGlobalVariable(Guid sender, string name, string oldval, string val);
		void IsTableBackgroundFlipped(Guid sender, bool isFlipped);
		void PlaySound(Guid sender, Guid player, string name);
		void Ready(Guid sender, Guid player);
		void PlayerState(Guid sender, Guid player, byte state);
		void RemoteCall(Guid sender, Guid player, string function, string args);
		void GameStateReq(Guid sender, Guid player);
		void GameState(Guid sender, Guid toPlayer, string state);
		void DeleteCard(Guid sender, Guid card, Guid player);
		void PlayerDisconnect(Guid sender, Guid player);
		void AddPacks(Guid sender, Guid player, Guid[] packs, bool selfOnly);
		void AnchorCard(Guid sender, Guid id, Guid player, bool anchor);
		void SetCardProperty(Guid sender, Guid id, Guid player, string name, string val, string valtype);
		void ResetCardProperties(Guid sender, Guid id, Guid player);
		void Filter(Guid sender, Guid card, string color);
		void SetBoard(Guid sender, string name);
		void SetPlayerColor(Guid sender, Guid player, string color);
		void SetPhase(Guid sender, byte phase, byte nextPhase);
		void StopPhase(Guid sender, Guid player, byte phase);
	}
}
