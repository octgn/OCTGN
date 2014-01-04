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

    public void Settings(bool twoSidedTable)
    {
      bin.Settings(twoSidedTable);
      Send();
    }

    public void PlayerSettings(byte playerId, bool invertedTable)
    {
      bin.PlayerSettings(playerId, invertedTable);
      Send();
    }

    public void NewPlayer(byte id, string nick, ulong pkey, bool tableSide)
    {
      bin.NewPlayer(id, nick, pkey, tableSide);
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

    public void Random(byte player, int id, int min, int max)
    {
      bin.Random(player, id, min, max);
      Send();
    }

    public void RandomAnswer1(byte player, int id, ulong value)
    {
      bin.RandomAnswer1(player, id, value);
      Send();
    }

    public void RandomAnswer2(byte player, int id, ulong value)
    {
      bin.RandomAnswer2(player, id, value);
      Send();
    }

    public void Counter(byte player, int counter, int value)
    {
      bin.Counter(player, counter, value);
      Send();
    }

    public void LoadDeck(int[] id, ulong[] type, int[] group)
    {
      bin.LoadDeck(id, type, group);
      Send();
    }

    public void CreateCard(int[] id, ulong[] type, int group)
    {
      bin.CreateCard(id, type, group);
      Send();
    }

    public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
    {
      bin.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
      Send();
    }

    public void CreateAliasDeprecated(int[] id, ulong[] type)
    {
      bin.CreateAliasDeprecated(id, type);
      Send();
    }

    public void MoveCard(byte player, int card, int group, int idx, bool faceUp, bool isScriptMove)
    {
      bin.MoveCard(player, card, group, idx, faceUp, isScriptMove);
      Send();
    }

    public void MoveCardAt(byte player, int card, int x, int y, int idx, bool faceUp, bool isScriptMove)
    {
      bin.MoveCardAt(player, card, x, y, idx, faceUp, isScriptMove);
      Send();
    }

    public void Reveal(int card, ulong revealed, Guid guid)
    {
      bin.Reveal(card, revealed, guid);
      Send();
    }

    public void RevealTo(byte[] players, int card, ulong[] encrypted)
    {
      bin.RevealTo(players, card, encrypted);
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

    public void AddMarker(byte player, int card, Guid id, string name, ushort count)
    {
      bin.AddMarker(player, card, id, name, count);
      Send();
    }

    public void RemoveMarker(byte player, int card, Guid id, string name, ushort count)
    {
      bin.RemoveMarker(player, card, id, name, count);
      Send();
    }

    public void TransferMarker(byte player, int from, int to, Guid id, string name, ushort count)
    {
      bin.TransferMarker(player, from, to, id, name, count);
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

    public void PlayerSetGlobalVariable(byte player, string name, string val)
    {
      bin.PlayerSetGlobalVariable(player, name, val);
      Send();
    }

    public void SetGlobalVariable(string name, string val)
    {
      bin.SetGlobalVariable(name, val);
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
	}
}
