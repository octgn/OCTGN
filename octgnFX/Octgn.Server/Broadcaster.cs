/*
 * This file was automatically generated.
 * Do not modify, changes will get lots when the file is regenerated!
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
 
namespace Octgn.Server
{
	internal sealed class Broadcaster : Octgn.Server.IClientCalls
	{
		private byte[] binData = new byte[1024];
		private BinFormatter bin;

		private sealed class BinFormatter : BaseBinaryStub
		{
			private Broadcaster bcast;
			
			internal BinFormatter(Broadcaster bcast, Handler handler) : base(handler)
			{ this.bcast = bcast; }
			
			protected override void Send(byte[] data)
			{ bcast.binData = data; }
		}

		internal Broadcaster(Handler handler)
		{ 
			bin = new BinFormatter(this, handler);
		}
		
		internal void RefreshTypes()
		{

		}
		
		internal void Send()
		{
			foreach (var player in State.Instance.Players)
				try
				{
					if (player.Connected == false) continue;
					player.Socket.Send(binData);
				}
				catch
				{
// TODO notify disconnection
//					Program.server.Disconnected(player.Socket.Client);
				}
		}

    public void Binary()
    {
      bin.Binary();
      Send();
    }

    public void Error(string msg)
    {
      bin.Error(msg);
      Send();
    }

    public void Kick(string reason)
    {
      bin.Kick(reason);
      Send();
    }

    public void Welcome(byte id, Guid gameSessionId, bool waitForGameState)
    {
      bin.Welcome(id, gameSessionId, waitForGameState);
      Send();
    }

    public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
    {
      bin.Settings(twoSidedTable, allowSpectators, muteSpectators);
      Send();
    }

    public void PlayerSettings(byte playerId, bool invertedTable, bool spectator)
    {
      bin.PlayerSettings(playerId, invertedTable, spectator);
      Send();
    }

    public void NewPlayer(byte id, string nick, ulong pkey, bool tableSide, bool spectator)
    {
      bin.NewPlayer(id, nick, pkey, tableSide, spectator);
      Send();
    }

    public void Leave(byte player)
    {
      bin.Leave(player);
      Send();
    }

    public void Nick(byte player, string nick)
    {
      bin.Nick(player, nick);
      Send();
    }

    public void Start()
    {
      bin.Start();
      Send();
    }

    public void Reset(byte player)
    {
      bin.Reset(player);
      Send();
    }

    public void NextTurn(byte nextPlayer)
    {
      bin.NextTurn(nextPlayer);
      Send();
    }

    public void StopTurn(byte player)
    {
      bin.StopTurn(player);
      Send();
    }

    public void Chat(byte player, string text)
    {
      bin.Chat(player, text);
      Send();
    }

    public void Print(byte player, string text)
    {
      bin.Print(player, text);
      Send();
    }

    public void Random(int result)
    {
      bin.Random(result);
      Send();
    }

    public void Counter(byte player, int counter, int value)
    {
      bin.Counter(player, counter, value);
      Send();
    }

    public void LoadDeck(int[] id, Guid[] type, int[] group, string[] size, string sleeve)    {
      bin.LoadDeck(id, type, group, size, sleeve);
      Send();
    }

    public void CreateCard(int[] id, Guid[] type, string[] size, int group)    {
      bin.CreateCard(id, type, size, group);
      Send();
    }

    public void CreateCardAt(int[] id, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
    {
      bin.CreateCardAt(id, modelId, x, y, faceUp, persist);
      Send();
    }

    public void CreateAliasDeprecated(int[] id, ulong[] type)
    {
      bin.CreateAliasDeprecated(id, type);
      Send();
    }

    public void MoveCard(byte player, int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove)
    {
      bin.MoveCard(player, id, group, idx, faceUp, isScriptMove);
      Send();
    }

    public void MoveCardAt(byte player, int[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
    {
      bin.MoveCardAt(player, id, x, y, idx, faceUp, isScriptMove);
      Send();
    }

    public void Peek(byte player, int card)
    {
      bin.Peek(player, card);
      Send();
    }

    public void Untarget(byte player, int card)
    {
      bin.Untarget(player, card);
      Send();
    }

    public void Target(byte player, int card)
    {
      bin.Target(player, card);
      Send();
    }

    public void TargetArrow(byte player, int card, int otherCard)
    {
      bin.TargetArrow(player, card, otherCard);
      Send();
    }

    public void Highlight(int card, string color)
    {
      bin.Highlight(card, color);
      Send();
    }

    public void Turn(byte player, int card, bool up)
    {
      bin.Turn(player, card, up);
      Send();
    }

    public void Rotate(byte player, int card, CardOrientation rot)
    {
      bin.Rotate(player, card, rot);
      Send();
    }

    public void ShuffleDeprecated(int group, int[] card)
    {
      bin.ShuffleDeprecated(group, card);
      Send();
    }

    public void Shuffled(byte player, int group, int[] card, short[] pos)
    {
      bin.Shuffled(player, group, card, pos);
      Send();
    }

    public void UnaliasGrpDeprecated(int group)
    {
      bin.UnaliasGrpDeprecated(group);
      Send();
    }

    public void UnaliasDeprecated(int[] card, ulong[] type)
    {
      bin.UnaliasDeprecated(card, type);
      Send();
    }

    public void AddMarker(byte player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
    {
      bin.AddMarker(player, card, id, name, count, origCount, isScriptChange);
      Send();
    }

    public void RemoveMarker(byte player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
    {
      bin.RemoveMarker(player, card, id, name, count, origCount, isScriptChange);
      Send();
    }

    public void TransferMarker(byte player, int from, int to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
    {
      bin.TransferMarker(player, from, to, id, name, count, origCount, isScriptChange);
      Send();
    }

    public void PassTo(byte player, int id, byte to, bool requested)
    {
      bin.PassTo(player, id, to, requested);
      Send();
    }

    public void TakeFrom(int id, byte to)
    {
      bin.TakeFrom(id, to);
      Send();
    }

    public void DontTake(int id)
    {
      bin.DontTake(id);
      Send();
    }

    public void FreezeCardsVisibility(int group)
    {
      bin.FreezeCardsVisibility(group);
      Send();
    }

    public void GroupVis(byte player, int group, bool defined, bool visible)
    {
      bin.GroupVis(player, group, defined, visible);
      Send();
    }

    public void GroupVisAdd(byte player, int group, byte who)
    {
      bin.GroupVisAdd(player, group, who);
      Send();
    }

    public void GroupVisRemove(byte player, int group, byte who)
    {
      bin.GroupVisRemove(player, group, who);
      Send();
    }

    public void LookAt(byte player, int uid, int group, bool look)
    {
      bin.LookAt(player, uid, group, look);
      Send();
    }

    public void LookAtTop(byte player, int uid, int group, int count, bool look)
    {
      bin.LookAtTop(player, uid, group, count, look);
      Send();
    }

    public void LookAtBottom(byte player, int uid, int group, int count, bool look)
    {
      bin.LookAtBottom(player, uid, group, count, look);
      Send();
    }

    public void StartLimited(byte player, Guid[] packs)
    {
      bin.StartLimited(player, packs);
      Send();
    }

    public void CancelLimited(byte player)
    {
      bin.CancelLimited(player);
      Send();
    }

    public void CardSwitchTo(byte player, int card, string alternate)
    {
      bin.CardSwitchTo(player, card, alternate);
      Send();
    }

    public void PlayerSetGlobalVariable(byte player, string name, string oldval, string val)
    {
      bin.PlayerSetGlobalVariable(player, name, oldval, val);
      Send();
    }

    public void SetGlobalVariable(string name, string oldval, string val)
    {
      bin.SetGlobalVariable(name, oldval, val);
      Send();
    }

    public void Ping()
    {
      bin.Ping();
      Send();
    }

    public void IsTableBackgroundFlipped(bool isFlipped)
    {
      bin.IsTableBackgroundFlipped(isFlipped);
      Send();
    }

    public void PlaySound(byte player, string name)
    {
      bin.PlaySound(player, name);
      Send();
    }

    public void Ready(byte player)
    {
      bin.Ready(player);
      Send();
    }

    public void PlayerState(byte player, byte state)
    {
      bin.PlayerState(player, state);
      Send();
    }

    public void RemoteCall(byte player, string function, string args)
    {
      bin.RemoteCall(player, function, args);
      Send();
    }

    public void GameStateReq(byte player)
    {
      bin.GameStateReq(player);
      Send();
    }

    public void GameState(byte toPlayer, string state)
    {
      bin.GameState(toPlayer, state);
      Send();
    }

    public void DeleteCard(int card, byte player)
    {
      bin.DeleteCard(card, player);
      Send();
    }

    public void PlayerDisconnect(byte player)
    {
      bin.PlayerDisconnect(player);
      Send();
    }

    public void AddPacks(byte player, Guid[] packs, bool selfOnly)
    {
      bin.AddPacks(player, packs, selfOnly);
      Send();
    }

    public void AnchorCard(int id, byte player, bool anchor)
    {
      bin.AnchorCard(id, player, anchor);
      Send();
    }
	}
}
