/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;

namespace Octgn.Server
{
	interface IRemoteCalls
	{
		void Boot(ulong player, string reason);
		void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		void HelloAgain(ulong pid, string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
		void NickReq(string nick);
		void ResetReq();
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int min, int max);
		void CounterReq(int counter, int value, bool isScriptChange);
		void MoveCardReq(int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAtReq(int[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		void PeekReq(int card);
		void UntargetReq(int card, bool isScriptChange);
		void TargetReq(int card, bool isScriptChange);
		void TargetArrowReq(int card, int otherCard, bool isScriptChange);
		void TurnReq(int card, bool up);
		void RotateReq(int card, CardOrientation rot);
		void AddMarkerReq(int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarkerReq(int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarkerReq(int from, int to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassToReq(int id, ulong to, bool requested);
		void TakeFromReq(int id, ulong from);
		void DontTakeReq(int id, ulong to);
		void GroupVisReq(int group, bool defined, bool visible);
		void GroupVisAddReq(int group, ulong who);
		void GroupVisRemoveReq(int group, ulong who);
		void LookAtReq(int uid, int group, bool look);
		void LookAtTopReq(int uid, int group, int count, bool look);
		void LookAtBottomReq(int uid, int group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void SwitchWithAlternate();
		void AddPacksReq(Guid[] packs, bool selfOnly);
		void StopPhaseReq(int turnNumber, byte phase, bool stop);
	}
}
