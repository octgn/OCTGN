/* 
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;

namespace Octgn.Server
{
	interface IClientCalls
	{ 
		void Binary();
		void Error(string msg);
		void Kick(string reason);
		void Welcome(byte id, Guid gameSessionId, bool waitForGameState);
		void Settings(bool twoSidedTable);
		void PlayerSettings(byte playerId, bool invertedTable);
		void NewPlayer(byte id, string nick, ulong pkey, bool tableSide);
		void Leave(byte player);
		void Nick(byte player, string nick);
		void Start();
		void Reset(byte player);
		void NextTurn(byte nextPlayer);
		void StopTurn(byte player);
		void Chat(byte player, string text);
		void Print(byte player, string text);
		void Random(byte player, int id, int min, int max);
		void RandomAnswer1(byte player, int id, ulong value);
		void RandomAnswer2(byte player, int id, ulong value);
		void Counter(byte player, int counter, int value);
		void LoadDeck(int[] id, ulong[] type, int[] group);
		void CreateCard(int[] id, ulong[] type, int group);
		void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist);
		void CreateAliasDeprecated(int[] id, ulong[] type);
		void MoveCard(byte player, int card, int group, int idx, bool faceUp, bool isScriptMove);
		void MoveCardAt(byte player, int card, int x, int y, int idx, bool faceUp, bool isScriptMove);
		void Reveal(int card, ulong revealed, Guid guid);
		void RevealTo(byte[] players, int card, ulong[] encrypted);
		void Peek(byte player, int card);
		void Untarget(byte player, int card);
		void Target(byte player, int card);
		void TargetArrow(byte player, int card, int otherCard);
		void Highlight(int card, string color);
		void Turn(byte player, int card, bool up);
		void Rotate(byte player, int card, CardOrientation rot);
		void ShuffleDeprecated(int group, int[] card);
		void Shuffled(byte player, int group, int[] card, short[] pos);
		void UnaliasGrpDeprecated(int group);
		void UnaliasDeprecated(int[] card, ulong[] type);
		void AddMarker(byte player, int card, Guid id, string name, ushort count);
		void RemoveMarker(byte player, int card, Guid id, string name, ushort count);
		void TransferMarker(byte player, int from, int to, Guid id, string name, ushort count);
		void PassTo(byte player, int id, byte to, bool requested);
		void TakeFrom(int id, byte to);
		void DontTake(int id);
		void FreezeCardsVisibility(int group);
		void GroupVis(byte player, int group, bool defined, bool visible);
		void GroupVisAdd(byte player, int group, byte who);
		void GroupVisRemove(byte player, int group, byte who);
		void LookAt(byte player, int uid, int group, bool look);
		void LookAtTop(byte player, int uid, int group, int count, bool look);
		void LookAtBottom(byte player, int uid, int group, int count, bool look);
		void StartLimited(byte player, Guid[] packs);
		void CancelLimited(byte player);
		void CardSwitchTo(byte player, int card, string alternate);
		void PlayerSetGlobalVariable(byte player, string name, string val);
		void SetGlobalVariable(string name, string val);
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
	}
}
