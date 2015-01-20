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
		void Boot(Player player, string reason);
		void Hello(string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password, bool spectator);
		void HelloAgain(byte pid, string nick, ulong pkey, string client, Version clientVer, Version octgnVer, Guid gameId, Version gameVersion, string password);
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
		void CounterReq(Counter counter, int value);
		void LoadDeck(int[] id, Guid[] type, Group[] group, string[] size, string sleeve);		void CreateCard(int[] id, Guid[] type, string[] size, Group group);		void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);		void CreateAliasDeprecated(int[] id, ulong[] type);
		void MoveCardReq(int[] id, Group group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAtReq(int[] id, int[] x, int[] y, int[] idx, bool isScriptMove, bool[] faceUp);
		void PeekReq(Card card);
		void UntargetReq(Card card);
		void TargetReq(Card card);
		void TargetArrowReq(Card card, Card otherCard);
		void Highlight(Card card, Color? color);
		void TurnReq(Card card, bool up);
		void RotateReq(Card card, CardOrientation rot);
		void ShuffleDeprecated(Group group, int[] card);
		void Shuffled(Player player, Group group, int[] card, short[] pos);
		void UnaliasGrpDeprecated(Group group);
		void UnaliasDeprecated(int[] card, ulong[] type);
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
		void LookAtReq(int uid, Group group, bool look);
		void LookAtTopReq(int uid, Group group, int count, bool look);
		void LookAtBottomReq(int uid, Group group, int count, bool look);
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
	}
}
