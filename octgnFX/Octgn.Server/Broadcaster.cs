/*
 * This file was automatically generated.
 * Do not modify, changes will get lots when the file is regenerated!
 */
using System;

namespace Octgn.Server
{
	public sealed class Broadcaster : Octgn.Server.IClientCalls
	{
		private static log4net.ILog Log = log4net.LogManager.GetLogger(nameof(Broadcaster));

		private byte[] binData = new byte[1024];

		private readonly PlayerCollection _players;

		public Broadcaster(PlayerCollection players)
		{
			_players = players ?? throw new ArgumentNullException(nameof(players));
		}

	public void Error(string msg)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Error(msg);
			}
		}
	}

	public void Kick(string reason)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Kick(reason);
			}
		}
	}

	public void Welcome(byte id, Guid gameSessionId, string gameName, bool waitForGameState)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Welcome(id, gameSessionId, gameName, waitForGameState);
			}
		}
	}

	public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Settings(twoSidedTable, allowSpectators, muteSpectators);
			}
		}
	}

	public void PlayerSettings(byte playerId, bool invertedTable, bool spectator)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.PlayerSettings(playerId, invertedTable, spectator);
			}
		}
	}

	public void NewPlayer(byte id, string nick, string userId, ulong pkey, bool tableSide, bool spectator)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.NewPlayer(id, nick, userId, pkey, tableSide, spectator);
			}
		}
	}

	public void Leave(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Leave(player);
			}
		}
	}

	public void Start()
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Start();
			}
		}
	}

	public void Reset(byte player, bool isSoft)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Reset(player, isSoft);
			}
		}
	}

	public void NextTurn(byte player, bool setActive, bool force)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.NextTurn(player, setActive, force);
			}
		}
	}

	public void StopTurn(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.StopTurn(player);
			}
		}
	}

	public void SetPhase(byte phase, byte[] players, bool force)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.SetPhase(phase, players, force);
			}
		}
	}

	public void SetActivePlayer(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.SetActivePlayer(player);
			}
		}
	}

	public void ClearActivePlayer()
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.ClearActivePlayer();
			}
		}
	}

	public void Chat(byte player, string text)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Chat(player, text);
			}
		}
	}

	public void Print(byte player, string text)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Print(player, text);
			}
		}
	}

	public void Random(int result)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Random(result);
			}
		}
	}

	public void Counter(byte player, int counter, int value, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Counter(player, counter, value, isScriptChange);
			}
		}
	}

	public void LoadDeck(int[] id, Guid[] type, int[] group, string[] size, string sleeve, bool limited)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.LoadDeck(id, type, group, size, sleeve, limited);
			}
		}
	}

	public void CreateCard(int[] id, Guid[] type, string[] size, int group)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.CreateCard(id, type, size, group);
			}
		}
	}

	public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.CreateCardAt(id, modelId, x, y, faceUp, persist);
			}
		}
	}

	public void CreateAliasDeprecated(int[] id, ulong[] type)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.CreateAliasDeprecated(id, type);
			}
		}
	}

	public void MoveCard(byte player, int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.MoveCard(player, id, group, idx, faceUp, isScriptMove);
			}
		}
	}

	public void MoveCardAt(byte player, int[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.MoveCardAt(player, id, x, y, idx, faceUp, isScriptMove);
			}
		}
	}

	public void Peek(byte player, int card)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Peek(player, card);
			}
		}
	}

	public void Untarget(byte player, int card, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Untarget(player, card, isScriptChange);
			}
		}
	}

	public void Target(byte player, int card, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Target(player, card, isScriptChange);
			}
		}
	}

	public void TargetArrow(byte player, int card, int otherCard, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.TargetArrow(player, card, otherCard, isScriptChange);
			}
		}
	}

	public void Highlight(int card, string color)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Highlight(card, color);
			}
		}
	}

	public void Turn(byte player, int card, bool up)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Turn(player, card, up);
			}
		}
	}

	public void Rotate(byte player, int card, CardOrientation rot)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Rotate(player, card, rot);
			}
		}
	}

	public void ShuffleDeprecated(int group, int[] card)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.ShuffleDeprecated(group, card);
			}
		}
	}

	public void Shuffled(byte player, int group, int[] card, short[] pos)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Shuffled(player, group, card, pos);
			}
		}
	}

	public void UnaliasGrpDeprecated(int group)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.UnaliasGrpDeprecated(group);
			}
		}
	}

	public void UnaliasDeprecated(int[] card, ulong[] type)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.UnaliasDeprecated(card, type);
			}
		}
	}

	public void AddMarker(byte player, int card, string id, string name, ushort count, ushort origCount, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.AddMarker(player, card, id, name, count, origCount, isScriptChange);
			}
		}
	}

	public void RemoveMarker(byte player, int card, string id, string name, ushort count, ushort origCount, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.RemoveMarker(player, card, id, name, count, origCount, isScriptChange);
			}
		}
	}

	public void TransferMarker(byte player, int from, int to, string id, string name, ushort count, ushort origCount, bool isScriptChange)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.TransferMarker(player, from, to, id, name, count, origCount, isScriptChange);
			}
		}
	}

	public void PassTo(byte player, int id, byte to, bool requested)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.PassTo(player, id, to, requested);
			}
		}
	}

	public void TakeFrom(int id, byte to)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.TakeFrom(id, to);
			}
		}
	}

	public void DontTake(int id)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.DontTake(id);
			}
		}
	}

	public void FreezeCardsVisibility(int group)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.FreezeCardsVisibility(group);
			}
		}
	}

	public void GroupVis(byte player, int group, bool defined, bool visible)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.GroupVis(player, group, defined, visible);
			}
		}
	}

	public void GroupVisAdd(byte player, int group, byte who)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.GroupVisAdd(player, group, who);
			}
		}
	}

	public void GroupVisRemove(byte player, int group, byte who)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.GroupVisRemove(player, group, who);
			}
		}
	}

	public void LookAt(byte player, int uid, int group, bool look)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.LookAt(player, uid, group, look);
			}
		}
	}

	public void LookAtTop(byte player, int uid, int group, int count, bool look)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.LookAtTop(player, uid, group, count, look);
			}
		}
	}

	public void LookAtBottom(byte player, int uid, int group, int count, bool look)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.LookAtBottom(player, uid, group, count, look);
			}
		}
	}

	public void StartLimited(byte player, Guid[] packs)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.StartLimited(player, packs);
			}
		}
	}

	public void CancelLimited(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.CancelLimited(player);
			}
		}
	}

	public void CardSwitchTo(byte player, int card, string alternate)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.CardSwitchTo(player, card, alternate);
			}
		}
	}

	public void PlayerSetGlobalVariable(byte player, string name, string oldval, string val)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.PlayerSetGlobalVariable(player, name, oldval, val);
			}
		}
	}

	public void SetGlobalVariable(string name, string oldval, string val)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.SetGlobalVariable(name, oldval, val);
			}
		}
	}

	public void Ping()
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Ping();
			}
		}
	}

	public void IsTableBackgroundFlipped(bool isFlipped)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.IsTableBackgroundFlipped(isFlipped);
			}
		}
	}

	public void PlaySound(byte player, string name)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.PlaySound(player, name);
			}
		}
	}

	public void Ready(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Ready(player);
			}
		}
	}

	public void PlayerState(byte player, byte state)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.PlayerState(player, state);
			}
		}
	}

	public void RemoteCall(byte player, string function, string args)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.RemoteCall(player, function, args);
			}
		}
	}

	public void GameStateReq(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.GameStateReq(player);
			}
		}
	}

	public void GameState(byte toPlayer, string state)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.GameState(toPlayer, state);
			}
		}
	}

	public void DeleteCard(int card, byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.DeleteCard(card, player);
			}
		}
	}

	public void PlayerDisconnect(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.PlayerDisconnect(player);
			}
		}
	}

	public void AddPacks(byte player, Guid[] packs, bool selfOnly)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.AddPacks(player, packs, selfOnly);
			}
		}
	}

	public void AnchorCard(int id, byte player, bool anchor)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.AnchorCard(id, player, anchor);
			}
		}
	}

	public void SetCardProperty(int id, byte player, string name, string val, string valtype)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.SetCardProperty(id, player, name, val, valtype);
			}
		}
	}

	public void ResetCardProperties(int id, byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.ResetCardProperties(id, player);
			}
		}
	}

	public void Filter(int card, string color)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.Filter(card, color);
			}
		}
	}

	public void SetBoard(byte player, string name)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.SetBoard(player, name);
			}
		}
	}

	public void RemoveBoard(byte player)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.RemoveBoard(player);
			}
		}
	}

	public void SetPlayerColor(byte player, string color)
	{
		foreach(var ply in _players.Players){
			if(ply.Connected){
				ply.Rpc.SetPlayerColor(player, color);
			}
		}
	}
	}
}
