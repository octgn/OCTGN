








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
		void Binary();
		void Error(string msg);
		void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion);
		void Settings(bool twoSidedTable);
		void PlayerSettings(IPlayPlayer playerId, bool invertedTable);
		void NickReq(string nick);
		void Start();
		void ResetReq();
		void NextTurn(IPlayPlayer nextPlayer);
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int id, int min, int max);
		void RandomAnswer1Req(int id, ulong value);
		void RandomAnswer2Req(int id, ulong value);
		void CounterReq(IPlayCounter counter, int value);
		void LoadDeck(int[] id, ulong[] type, IPlayGroup[] group);
		void CreateCard(int[] id, ulong[] type, IPlayGroup group);
		void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void CreateAlias(int[] id, ulong[] type);
		void MoveCardReq(IPlayCard card, IPlayGroup group, int idx, bool faceUp);
		void MoveCardAtReq(IPlayCard card, int x, int y, int idx, bool faceUp);
		void Reveal(IPlayCard card, ulong revealed, Guid guid);
		void RevealToReq(IPlayPlayer sendTo, IPlayPlayer[] revealTo, IPlayCard card, ulong[] encrypted);
		void PeekReq(IPlayCard card);
		void UntargetReq(IPlayCard card);
		void TargetReq(IPlayCard card);
		void TargetArrowReq(IPlayCard card, IPlayCard otherCard);
		void Highlight(IPlayCard card, Color? color);
		void TurnReq(IPlayCard card, bool up);
		void RotateReq(IPlayCard card, CardOrientation rot);
		void Shuffle(IPlayGroup group, int[] card);
		void Shuffled(IPlayGroup group, int[] card, short[] pos);
		void UnaliasGrp(IPlayGroup group);
		void Unalias(int[] card, ulong[] type);
		void AddMarkerReq(IPlayCard card, Guid id, string name, ushort count);
		void RemoveMarkerReq(IPlayCard card, Guid id, string name, ushort count);
		void SetMarkerReq(IPlayCard card, Guid id, string name, ushort count);
		void TransferMarkerReq(IPlayCard from, IPlayCard to, Guid id, string name, ushort count);
		void PassToReq(IPlayControllableObject id, IPlayPlayer to, bool requested);
		void TakeFromReq(IPlayControllableObject id, IPlayPlayer from);
		void DontTakeReq(IPlayControllableObject id, IPlayPlayer to);
		void FreezeCardsVisibility(IPlayGroup group);
		void GroupVisReq(IPlayGroup group, bool defined, bool visible);
		void GroupVisAddReq(IPlayGroup group, IPlayPlayer who);
		void GroupVisRemoveReq(IPlayGroup group, IPlayPlayer who);
		void LookAtReq(int uid, IPlayGroup group, bool look);
		void LookAtTopReq(int uid, IPlayGroup group, int count, bool look);
		void LookAtBottomReq(int uid, IPlayGroup group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void CardSwitchTo(IPlayPlayer player, IPlayCard card, string alternate);
		void PlayerSetGlobalVariable(IPlayPlayer player, string name, string val);
		void SetGlobalVariable(string name, string val);
		void Ping();
		void IsTableBackgroundFlipped(bool isFlipped);

	}
}
