/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;

namespace Octgn.Server
{
	interface IClientCalls
	{
		void Error(string msg);
		void Kick(string reason);
		void Welcome(long id, Guid gameSessionId, bool waitForGameState);
		void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators);
		void PlayerSettings(ulong playerId, bool invertedTable, bool spectator);
		void NewPlayer(long id, string nick, ulong pkey, bool tableSide, bool spectator);
		void Leave(ulong player);
		void Nick(ulong player, string nick);
		void Start();
		void Reset(ulong player);
		void NextTurn(ulong nextPlayer);
		void StopTurn(ulong player);
		void Chat(ulong player, string text);
		void Print(ulong player, string text);
		void Random(int result);
		void Counter(ulong player, int counter, int value, bool isScriptChange);
		void LoadDeck(int[] id, Guid[] type, int[] group, string[] size, string sleeve, bool limited);
		void CreateCard(int[] id, Guid[] type, string[] size, int group);
		void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void CreateAliasDeprecated(int[] id, ulong[] type);
		void MoveCard(ulong player, int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove);
		void MoveCardAt(ulong player, int[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove);
		void Peek(ulong player, int card);
		void Untarget(ulong player, int card, bool isScriptChange);
		void Target(ulong player, int card, bool isScriptChange);
		void TargetArrow(ulong player, int card, int otherCard, bool isScriptChange);
		void Highlight(int card, string color);
		void Turn(ulong player, int card, bool up);
		void Rotate(ulong player, int card, CardOrientation rot);
		void ShuffleDeprecated(int group, int[] card);
		void Shuffled(ulong player, int group, int[] card, short[] pos);
		void UnaliasGrpDeprecated(int group);
		void UnaliasDeprecated(int[] card, ulong[] type);
		void AddMarker(ulong player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void RemoveMarker(ulong player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void TransferMarker(ulong player, int from, int to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange);
		void PassTo(ulong player, int id, ulong to, bool requested);
		void TakeFrom(int id, ulong to);
		void DontTake(int id);
		void FreezeCardsVisibility(int group);
		void GroupVis(ulong player, int group, bool defined, bool visible);
		void GroupVisAdd(ulong player, int group, ulong who);
		void GroupVisRemove(ulong player, int group, ulong who);
		void LookAt(ulong player, int uid, int group, bool look);
		void LookAtTop(ulong player, int uid, int group, int count, bool look);
		void LookAtBottom(ulong player, int uid, int group, int count, bool look);
		void StartLimited(ulong player, Guid[] packs);
		void CancelLimited(ulong player);
		void CardSwitchTo(ulong player, int card, string alternate);
		void PlayerSetGlobalVariable(ulong player, string name, string oldval, string val);
		void SetGlobalVariable(string name, string oldval, string val);
		void Ping();
		void IsTableBackgroundFlipped(bool isFlipped);
		void PlaySound(ulong player, string name);
		void Ready(ulong player);
		void PlayerState(ulong player, byte state);
		void RemoteCall(ulong player, string function, string args);
		void GameStateReq(ulong player);
		void GameState(ulong toPlayer, string state);
		void DeleteCard(int card, ulong player);
		void PlayerDisconnect(ulong player);
		void AddPacks(ulong player, Guid[] packs, bool selfOnly);
		void AnchorCard(int id, ulong player, bool anchor);
		void SetCardProperty(int id, ulong player, string name, string val, string valtype);
		void ResetCardProperties(int id, ulong player);
		void Filter(int card, string color);
		void SetBoard(string name);
		void SetPlayerColor(ulong player, string color);
		void SetPhase(byte phase, byte nextPhase);
		void StopPhase(ulong player, byte phase);
	}
}
