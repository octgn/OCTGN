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

			internal BinFormatter(Broadcaster bcast, RequestHandler handler) : base(handler)
			{ this.bcast = bcast; }

			protected override void Send(byte[] data)
			{ bcast.binData = data; }
		}

		internal Broadcaster(RequestHandler handler)
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

    public void Welcome(long id, Guid gameSessionId, bool waitForGameState)
    {
      bin.Welcome(id, gameSessionId, waitForGameState);
      Send();
    }

    public void Settings(bool twoSidedTable, bool allowSpectators, bool muteSpectators)
    {
      bin.Settings(twoSidedTable, allowSpectators, muteSpectators);
      Send();
    }

    public void PlayerSettings(ulong playerId, bool invertedTable, bool spectator)
    {
      bin.PlayerSettings(playerId, invertedTable, spectator);
      Send();
    }

    public void NewPlayer(long id, string nick, ulong pkey, bool tableSide, bool spectator)
    {
      bin.NewPlayer(id, nick, pkey, tableSide, spectator);
      Send();
    }

    public void Leave(ulong player)
    {
      bin.Leave(player);
      Send();
    }

    public void Nick(ulong player, string nick)
    {
      bin.Nick(player, nick);
      Send();
    }

    public void Start()
    {
      bin.Start();
      Send();
    }

    public void Reset(ulong player)
    {
      bin.Reset(player);
      Send();
    }

    public void NextTurn(ulong nextPlayer)
    {
      bin.NextTurn(nextPlayer);
      Send();
    }

    public void StopTurn(ulong player)
    {
      bin.StopTurn(player);
      Send();
    }

    public void Chat(ulong player, string text)
    {
      bin.Chat(player, text);
      Send();
    }

    public void Print(ulong player, string text)
    {
      bin.Print(player, text);
      Send();
    }

    public void Random(int result)
    {
      bin.Random(result);
      Send();
    }

    public void Counter(ulong player, int counter, int value, bool isScriptChange)
    {
      bin.Counter(player, counter, value, isScriptChange);
      Send();
    }

    public void LoadDeck(int[] id, Guid[] type, int[] group, string[] size, string sleeve, bool limited)
    {
      bin.LoadDeck(id, type, group, size, sleeve, limited);
      Send();
    }

    public void CreateCard(int[] id, Guid[] type, string[] size, int group)
    {
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

    public void MoveCard(ulong player, int[] id, int group, int[] idx, bool[] faceUp, bool isScriptMove)
    {
      bin.MoveCard(player, id, group, idx, faceUp, isScriptMove);
      Send();
    }

    public void MoveCardAt(ulong player, int[] id, int[] x, int[] y, int[] idx, bool[] faceUp, bool isScriptMove)
    {
      bin.MoveCardAt(player, id, x, y, idx, faceUp, isScriptMove);
      Send();
    }

    public void Peek(ulong player, int card)
    {
      bin.Peek(player, card);
      Send();
    }

    public void Untarget(ulong player, int card, bool isScriptChange)
    {
      bin.Untarget(player, card, isScriptChange);
      Send();
    }

    public void Target(ulong player, int card, bool isScriptChange)
    {
      bin.Target(player, card, isScriptChange);
      Send();
    }

    public void TargetArrow(ulong player, int card, int otherCard, bool isScriptChange)
    {
      bin.TargetArrow(player, card, otherCard, isScriptChange);
      Send();
    }

    public void Highlight(int card, string color)
    {
      bin.Highlight(card, color);
      Send();
    }

    public void Turn(ulong player, int card, bool up)
    {
      bin.Turn(player, card, up);
      Send();
    }

    public void Rotate(ulong player, int card, CardOrientation rot)
    {
      bin.Rotate(player, card, rot);
      Send();
    }

    public void ShuffleDeprecated(int group, int[] card)
    {
      bin.ShuffleDeprecated(group, card);
      Send();
    }

    public void Shuffled(ulong player, int group, int[] card, short[] pos)
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

    public void AddMarker(ulong player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
    {
      bin.AddMarker(player, card, id, name, count, origCount, isScriptChange);
      Send();
    }

    public void RemoveMarker(ulong player, int card, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
    {
      bin.RemoveMarker(player, card, id, name, count, origCount, isScriptChange);
      Send();
    }

    public void TransferMarker(ulong player, int from, int to, Guid id, string name, ushort count, ushort origCount, bool isScriptChange)
    {
      bin.TransferMarker(player, from, to, id, name, count, origCount, isScriptChange);
      Send();
    }

    public void PassTo(ulong player, int id, ulong to, bool requested)
    {
      bin.PassTo(player, id, to, requested);
      Send();
    }

    public void TakeFrom(int id, ulong to)
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

    public void GroupVis(ulong player, int group, bool defined, bool visible)
    {
      bin.GroupVis(player, group, defined, visible);
      Send();
    }

    public void GroupVisAdd(ulong player, int group, ulong who)
    {
      bin.GroupVisAdd(player, group, who);
      Send();
    }

    public void GroupVisRemove(ulong player, int group, ulong who)
    {
      bin.GroupVisRemove(player, group, who);
      Send();
    }

    public void LookAt(ulong player, int uid, int group, bool look)
    {
      bin.LookAt(player, uid, group, look);
      Send();
    }

    public void LookAtTop(ulong player, int uid, int group, int count, bool look)
    {
      bin.LookAtTop(player, uid, group, count, look);
      Send();
    }

    public void LookAtBottom(ulong player, int uid, int group, int count, bool look)
    {
      bin.LookAtBottom(player, uid, group, count, look);
      Send();
    }

    public void StartLimited(ulong player, Guid[] packs)
    {
      bin.StartLimited(player, packs);
      Send();
    }

    public void CancelLimited(ulong player)
    {
      bin.CancelLimited(player);
      Send();
    }

    public void CardSwitchTo(ulong player, int card, string alternate)
    {
      bin.CardSwitchTo(player, card, alternate);
      Send();
    }

    public void PlayerSetGlobalVariable(ulong player, string name, string oldval, string val)
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

    public void PlaySound(ulong player, string name)
    {
      bin.PlaySound(player, name);
      Send();
    }

    public void Ready(ulong player)
    {
      bin.Ready(player);
      Send();
    }

    public void PlayerState(ulong player, byte state)
    {
      bin.PlayerState(player, state);
      Send();
    }

    public void RemoteCall(ulong player, string function, string args)
    {
      bin.RemoteCall(player, function, args);
      Send();
    }

    public void GameStateReq(ulong player)
    {
      bin.GameStateReq(player);
      Send();
    }

    public void GameState(ulong toPlayer, string state)
    {
      bin.GameState(toPlayer, state);
      Send();
    }

    public void DeleteCard(int card, ulong player)
    {
      bin.DeleteCard(card, player);
      Send();
    }

    public void PlayerDisconnect(ulong player)
    {
      bin.PlayerDisconnect(player);
      Send();
    }

    public void AddPacks(ulong player, Guid[] packs, bool selfOnly)
    {
      bin.AddPacks(player, packs, selfOnly);
      Send();
    }

    public void AnchorCard(int id, ulong player, bool anchor)
    {
      bin.AnchorCard(id, player, anchor);
      Send();
    }

    public void SetCardProperty(int id, ulong player, string name, string val, string valtype)
    {
      bin.SetCardProperty(id, player, name, val, valtype);
      Send();
    }

    public void ResetCardProperties(int id, ulong player)
    {
      bin.ResetCardProperties(id, player);
      Send();
    }

    public void Filter(int card, string color)
    {
      bin.Filter(card, color);
      Send();
    }

    public void SetBoard(string name)
    {
      bin.SetBoard(name);
      Send();
    }

    public void SetPlayerColor(ulong player, string color)
    {
      bin.SetPlayerColor(player, color);
      Send();
    }

    public void SetPhase(byte phase, byte nextPhase)
    {
      bin.SetPhase(phase, nextPhase);
      Send();
    }

    public void StopPhase(ulong player, byte phase)
    {
      bin.StopPhase(player, phase);
      Send();
    }
	}
}
