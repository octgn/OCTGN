/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Octgn.Play;

namespace Octgn.Networking
{
	interface IServerCalls
	{
		Task Error(string msg);
		Task Boot(Player player, string reason);
		Task Hello(string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		Task HelloAgain(uint pid, string nick, long pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
		Task Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		Task PlayerSettings(Player playerId, bool invertedTable, bool spectator);
		Task Leave(Player player);
		Task NickReq(string nick);
		Task Start();
		Task ResetReq();
		Task NextTurn(Player nextPlayer);
		Task StopTurnReq(int turnNumber, bool stop);
		Task ChatReq(string text);
		Task PrintReq(string text);
		Task RandomReq(int min, int max);
		Task CounterReq(Counter counter, int value, bool isScriptChange);
		Task LoadDeck(ulong[] id, Guid[] type, Group[] group, string[] size, string sleeve, bool limited);
		Task CreateCard(ulong[] id, Guid[] type, string[] size, Group group);
		Task CreateCardAt(ulong[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		Task MoveCardReq(ulong[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove);
		Task MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		Task PeekReq(Card card);
		Task UntargetReq(Card card, bool isScriptChange);
		Task TargetReq(Card card, bool isScriptChange);
		Task TargetArrowReq(Card card, Card otherCard, bool isScriptChange);
		Task Highlight(Card card, Color? color);
		Task TurnReq(Card card, bool up);
		Task RotateReq(Card card, CardOrientation rot);
		Task Shuffled(Player player, Group group, ulong[] card, short[] pos);
		Task AddMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		Task RemoveMarkerReq(Card card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		Task TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		Task PassToReq(ControllableObject id, Player to, bool requested);
		Task TakeFromReq(ControllableObject id, Player from);
		Task DontTakeReq(ControllableObject id, Player to);
		Task FreezeCardsVisibility(Group group);
		Task GroupVisReq(Group group, bool defined, bool visible);
		Task GroupVisAddReq(Group group, Player who);
		Task GroupVisRemoveReq(Group group, Player who);
		Task LookAtReq(uint uid, Group group, bool look);
		Task LookAtTopReq(uint uid, Group group, int count, bool look);
		Task LookAtBottomReq(uint uid, Group group, int count, bool look);
		Task StartLimitedReq(Guid[] packs);
		Task CancelLimitedReq();
		Task CardSwitchTo(Player player, Card card, string alternate);
		Task PlayerSetGlobalVariable(Player player, string name, string oldval, string val);
		Task SetGlobalVariable(string name, string oldval, string val);
		Task IsTableBackgroundFlipped(bool isFlipped);
		Task PlaySound(Player player, string name);
		Task Ready(Player player);
		Task RemoteCall(Player player, string function, string args);
		Task GameStateReq(Player player);
		Task GameState(Player toPlayer, string state);
		Task DeleteCard(Card card, Player player);
		Task AddPacksReq(Guid[] packs, bool selfOnly);
		Task AnchorCard(Card id, Player player, bool anchor);
		Task SetCardProperty(Card id, Player player, string name, string val, string valtype);
		Task ResetCardProperties(Card id, Player player);
		Task Filter(Card card, Color? color);
		Task SetBoard(string name);
		Task SetPlayerColor(Player player, string color);
		Task SetPhase(byte phase, byte nextPhase);
		Task StopPhaseReq(int turnNumber, byte phase, bool stop);
	}
}
