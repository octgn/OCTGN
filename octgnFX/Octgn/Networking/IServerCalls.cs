








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
		void PlayerSettings(Player playerId, bool invertedTable);
		void NickReq(string nick);
		void Start();
		void ResetReq();
		void NextTurn(Player nextPlayer);
		void StopTurnReq(int turnNumber, bool stop);
		void ChatReq(string text);
		void PrintReq(string text);
		void RandomReq(int id, int min, int max);
		void RandomAnswer1Req(int id, ulong value);
		void RandomAnswer2Req(int id, ulong value);
		void CounterReq(Counter counter, int value);
		void LoadDeck(int[] id, ulong[] type, Group[] group);
		void CreateCard(int[] id, ulong[] type, Group group);
		void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void CreateAlias(int[] id, ulong[] type);
		void MoveCardReq(Card card, Group group, int idx, bool faceUp);
		void MoveCardAtReq(Card card, int x, int y, int idx, bool faceUp);
		void Reveal(Card card, ulong revealed, Guid guid);
		void RevealToReq(Player sendTo, Player[] revealTo, Card card, ulong[] encrypted);
		void PeekReq(Card card);
		void UntargetReq(Card card);
		void TargetReq(Card card);
		void TargetArrowReq(Card card, Card otherCard);
		void Highlight(Card card, Color? color);
		void TurnReq(Card card, bool up);
		void RotateReq(Card card, CardOrientation rot);
		void Shuffle(Group group, int[] card);
		void Shuffled(Group group, int[] card, short[] pos);
		void UnaliasGrp(Group group);
		void Unalias(int[] card, ulong[] type);
		void AddMarkerReq(Card card, Guid id, string name, ushort count);
		void RemoveMarkerReq(Card card, Guid id, string name, ushort count);
		void SetMarkerReq(Card card, Guid id, string name, ushort count);
		void TransferMarkerReq(Card from, Card to, Guid id, string name, ushort count);
		void PassToReq(ControllableObject id, Player to, bool requested);
		void TakeFromReq(ControllableObject id, Player from);
		void DontTakeReq(ControllableObject id, Player to);
		void FreezeCardsVisibility(Group group);
		void GroupVisReq(Group group, bool defined, bool visible);
		void GroupVisAddReq(Group group, Player who);
		void GroupVisRemoveReq(Group group, Player who);
		void LookAtReq(int uid, Group group, bool look);
		void LookAtTopReq(int uid, Group group, int count, bool look);
		void LookAtBottomReq(int uid, Group group, int count, bool look);
		void StartLimitedReq(Guid[] packs);
		void CancelLimitedReq();
		void CardSwitchTo(Player player, Card card, string alternate);
		void PlayerSetGlobalVariable(Player player, string name, string val);
		void SetGlobalVariable(string name, string val);
		void Ping();
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(Player player, string name);

	}
}
