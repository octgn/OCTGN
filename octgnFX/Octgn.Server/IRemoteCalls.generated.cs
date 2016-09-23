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
	interface IRemoteCalls
	{
		void Boot(uint player, string reason);
		void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		void HelloAgain(uint pid, string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
		void NickReq(string nick);
		void ResetReq();
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int min, int max);
		void CounterReq(ulong counter, int value, bool isScriptChange);
		void MoveCardReq(ulong[] id, ulong group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAtReq(ulong[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		void PeekReq(ulong card);
		void UntargetReq(ulong card, bool isScriptChange);
		void TargetReq(ulong card, bool isScriptChange);
		void TargetArrowReq(ulong card, ulong otherCard, bool isScriptChange);
		void TurnReq(ulong card, bool up);
		void RotateReq(ulong card, CardOrientation rot);
		void AddMarkerReq(ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarkerReq(ulong card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarkerReq(ulong from, ulong to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassToReq(ulong id, uint to, bool requested);
		void TakeFromReq(ulong id, uint from);
		void DontTakeReq(ulong id, uint to);
		void GroupVisReq(ulong group, bool defined, bool visible);
		void GroupVisAddReq(ulong group, uint who);
		void GroupVisRemoveReq(ulong group, uint who);
		void LookAtReq(uint uid, ulong group, bool look);
		void LookAtTopReq(uint uid, ulong group, int count, bool look);
		void LookAtBottomReq(uint uid, ulong group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void SwitchWithAlternate();
		void AddPacksReq(Guid[] packs, bool selfOnly);
		void StopPhaseReq(int turnNumber, byte phase, bool stop);
	}
}
