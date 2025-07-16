/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;

namespace Octgn.Server
{
	public interface IClientCalls
	{
		void Error(string msg);
		void Kick(string reason);
		void Welcome(byte id, Guid gameSessionId, string gameName, bool waitForGameState);
		void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators, bool allowCardList);
		void PlayerSettings(byte playerId, bool invertedTable, bool spectator);
		void NewPlayer(byte id, string nick, string userId, ulong pkey, bool tableSide, bool spectator);
		void Leave(byte player);
		void Start();
		void Reset(byte player, bool isSoft);
		void NextTurn(byte player, bool setActive, bool force);
		void StopTurn(byte player);
		void SetPhase(byte phase, byte[] players, bool force);
		void SetActivePlayer(byte player);
		void ClearActivePlayer();
		void Chat(byte player, string text);
		void Print(byte player, string text);
		void Random(int result);
		void Counter(byte player, int counter, int value, bool isScriptChange);
		void LoadDeck(int[] id, Guid[] type, int[] group, string[] size, string sleeve, bool limited);
		void CreateCard(int[] id, Guid[] type, string[] size, int group);
		void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void CreateAliasDeprecated(int[] id, ulong[] type);
		void MoveCard(byte player, int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAt(byte player, int[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove);
		void Peek(byte player, int card);
		void Untarget(byte player, int card, bool isScriptChange);
		void Target(byte player, int card, bool isScriptChange);
		void TargetArrow(byte player, int card, int otherCard, bool isScriptChange);
		void Highlight(int card, string color);
		void Turn(byte player, int card, bool up);
		void Rotate(byte player, int card, CardOrientation rot);
		void Shake(byte player, int card);
		void ShuffleDeprecated(int group, int[] card);
		void Shuffled(byte player, int group, int[] card, short[] pos);
		void UnaliasGrpDeprecated(int group);
		void UnaliasDeprecated(int[] card, ulong[] type);
		void AddMarker(byte player, int card, string id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarker(byte player, int card, string id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarker(byte player, int from, int to, string id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassTo(byte player, int id, byte to, bool requested);
		void TakeFrom(int id, byte to);
		void DontTake(int id);
		void FreezeCardsVisibility(int group);
		void GroupVis(byte player, int group, bool defined, bool visible);
		void GroupVisAdd(byte player, int group, byte who);
		void GroupVisRemove(byte player, int group, byte who);
		void GroupProtection(byte player, int group, string state);
		void LookAt(byte player, int uid, int group, bool look);
		void LookAtTop(byte player, int uid, int group, int count, bool look);
		void LookAtBottom(byte player, int uid, int group, int count, bool look);
		void StartLimited(byte player, Guid[] packs);
		void CancelLimited(byte player);
		void CardSwitchTo(byte player, int card, string alternate);
		void PlayerSetGlobalVariable(byte player, string name, string oldval, string val);
		void SetGlobalVariable(string name, string oldval, string val);
		void Ping();
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(byte player, string name);
		void Ready(byte player);
		void PlayerState(byte player, byte state);
		void RemoteCall(byte player, string function, string args);
		void GameStateReq(byte player);
		void GameState(byte toPlayer, string state);
		void DeleteCard(int card, byte player);
		void PlayerDisconnect(byte player);
		void AddPacks(byte player, Guid[] packs, bool selfOnly);
		void AnchorCard(int id, byte player, bool anchor);
		void SetCardProperty(int id, byte player, string name, string val, string valtype);
		void ResetCardProperties(int id, byte player);
		void Filter(int card, string color);
		void SetBoard(byte player, string name);
		void RemoveBoard(byte player);
		void SetPlayerColor(byte player, string color);
		void RequestPileViewPermission(byte requester, int group, byte targetPlayer, string viewType, int cardCount);
		void GrantPileViewPermission(byte owner, int group, byte requester, bool granted, bool permanent, string viewType, int cardCount);
	}
}
