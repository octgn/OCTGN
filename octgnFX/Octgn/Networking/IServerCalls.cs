/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Windows.Media;
using Octgn.Play;

namespace Octgn.Networking
{
	interface IServerCalls
	{
		void Error(string msg);
		void Boot(Player player, string reason);
		void Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		void HelloAgain(uint pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
		void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(Player playerId, bool invertedTable, bool spectator);
		void Leave(Player player);
		void NickReq(string nick);
		void Start();
		void ResetReq();
		void NextTurn(Player nextPlayer);
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int min, int max);
		void CounterReq(Counter counter, int value, bool isScriptChange);
		void LoadDeck(ulong[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited);
		void CreateCard(ulong[] id, Guid[] type, string[] size, Group group);
		void CreateCardAt(ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void MoveCardReq(ulong[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		void PeekReq(Card card);
		void UntargetReq(Card card, bool isScriptChange);
		void TargetReq(Card card, bool isScriptChange);
		void TargetArrowReq(Card card, Card otherCard, bool isScriptChange);
		void Highlight(Card card, Color? color);
		void TurnReq(Card card, bool up);
		void RotateReq(Card card, CardOrientation rot);
		void Shuffled(Player player, Group group, ulong[] card, short[] pos);
		void AddMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassToReq(ControllableObject id, Player to, bool requested);
		void TakeFromReq(ControllableObject id, Player from);
		void DontTakeReq(ControllableObject id, Player to);
		void FreezeCardsVisibility(Group group);
		void GroupVisReq(Group group, bool defined, bool visible);
		void GroupVisAddReq(Group group, Player who);
		void GroupVisRemoveReq(Group group, Player who);
		void LookAtReq(uint uid, Group group, bool look);
		void LookAtTopReq(uint uid, Group group, int count, bool look);
		void LookAtBottomReq(uint uid, Group group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void CardSwitchTo(Player player, Card card, string alternate);
		void PlayerSetGlobalVariable(Player player, string name, string oldval, string val);
		void SetGlobalVariable(string name, string oldval, string val);
		void Ping();
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(Player player, string name);
		void Ready(Player player);
		void RemoteCall(Player player, string function, string args);
		void GameStateReq(Player player);
		void GameState(Player toPlayer, string state);
		void DeleteCard(Card card, Player player);
		void AddPacksReq(Guid[] packs, bool selfOnly);
		void AnchorCard(Card id, Player player, bool anchor);
		void SetCardProperty(Card id, Player player, string name, string val, string valtype);
		void ResetCardProperties(Card id, Player player);
		void Filter(Card card, Color? color);
		void SetBoard(string name);
		void SetPlayerColor(Player player, string color);
		void SetPhase(byte phase, byte nextPhase);
		void StopPhaseReq(int turnNumber, byte phase, bool stop);
	}
}
