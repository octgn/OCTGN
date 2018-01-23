/*
 * This file was automatically generated!
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Octgn.Library.Localization;

namespace Octgn.Server
{
	sealed class BinaryParser
	{
        private static log4net.ILog Log = log4net.LogManager.GetLogger(nameof(BinaryParser));

		Handler handler;

		public BinaryParser(Handler handler)
		{ this.handler = handler; }

		public void Parse(byte[] data)
		{
			MemoryStream stream = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(stream);
			short length;
			handler.muted = reader.ReadInt32();
			byte method = reader.ReadByte();
			switch (method)
			{
				case 0:
				{
					Log.Debug($"SERVER IN:  Binary");
					handler.Binary();
					break;
				}
				case 1:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  Error");
					handler.Error(arg0);
					break;
				}
				case 2:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  Boot");
					handler.Boot(arg0, arg1);
					break;
				}
				case 4:
				{
					string arg0 = reader.ReadString();
					string arg1 = reader.ReadString();
					ulong arg2 = reader.ReadUInt64();
					string arg3 = reader.ReadString();
					Version arg4 = new Version(reader.ReadString());
					Version arg5 = new Version(reader.ReadString());
					Guid arg6 = new Guid(reader.ReadBytes(16));
					Version arg7 = new Version(reader.ReadString());
					string arg8 = reader.ReadString();
					bool arg9 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  Hello");
					handler.Hello(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
					break;
				}
				case 5:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					ulong arg3 = reader.ReadUInt64();
					string arg4 = reader.ReadString();
					Version arg5 = new Version(reader.ReadString());
					Version arg6 = new Version(reader.ReadString());
					Guid arg7 = new Guid(reader.ReadBytes(16));
					Version arg8 = new Version(reader.ReadString());
					string arg9 = reader.ReadString();
					Log.Debug($"SERVER IN:  HelloAgain");
					handler.HelloAgain(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
					break;
				}
				case 7:
				{
					bool arg0 = reader.ReadBoolean();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  Settings");
					handler.Settings(arg0, arg1, arg2);
					break;
				}
				case 8:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  PlayerSettings");
					handler.PlayerSettings(arg0, arg1, arg2);
					break;
				}
				case 10:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  Leave");
					handler.Leave(arg0);
					break;
				}
				case 11:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  NickReq");
					handler.NickReq(arg0);
					break;
				}
				case 13:
				{
					Log.Debug($"SERVER IN:  Start");
					handler.Start();
					break;
				}
				case 14:
				{
					Log.Debug($"SERVER IN:  ResetReq");
					handler.ResetReq();
					break;
				}
				case 16:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  NextTurn");
					handler.NextTurn(arg0, arg1, arg2);
					break;
				}
				case 17:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  StopTurnReq");
					handler.StopTurnReq(arg0, arg1);
					break;
				}
				case 19:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  SetPhaseReq");
					handler.SetPhaseReq(arg0, arg1);
					break;
				}
				case 21:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  StopPhaseReq");
					handler.StopPhaseReq(arg0, arg1);
					break;
				}
				case 22:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  SetActivePlayer");
					handler.SetActivePlayer(arg0);
					break;
				}
				case 23:
				{
					Log.Debug($"SERVER IN:  ClearActivePlayer");
					handler.ClearActivePlayer();
					break;
				}
				case 24:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  ChatReq");
					handler.ChatReq(arg0);
					break;
				}
				case 26:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  PrintReq");
					handler.PrintReq(arg0);
					break;
				}
				case 28:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  RandomReq");
					handler.RandomReq(arg0, arg1);
					break;
				}
				case 30:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  CounterReq");
					handler.CounterReq(arg0, arg1, arg2);
					break;
				}
				case 32:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					Guid[] arg1 = new Guid[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = new Guid(reader.ReadBytes(16));
					length = reader.ReadInt16();
					int[] arg2 = new int[length];
					for (int i = 0; i < length; ++i)
					arg2[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					string[] arg3 = new string[length];
					for (int i = 0; i < length; ++i)
						arg3[i] = reader.ReadString();
					string arg4 = reader.ReadString();
					bool arg5 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LoadDeck");
					handler.LoadDeck(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 33:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					Guid[] arg1 = new Guid[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = new Guid(reader.ReadBytes(16));
					length = reader.ReadInt16();
					string[] arg2 = new string[length];
					for (int i = 0; i < length; ++i)
						arg2[i] = reader.ReadString();
					int arg3 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  CreateCard");
					handler.CreateCard(arg0, arg1, arg2, arg3);
					break;
				}
				case 34:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					Guid[] arg1 = new Guid[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = new Guid(reader.ReadBytes(16));
					length = reader.ReadInt16();
					int[] arg2 = new int[length];
					for (int i = 0; i < length; ++i)
						arg2[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg3 = new int[length];
					for (int i = 0; i < length; ++i)
						arg3[i] = reader.ReadInt32();
					bool arg4 = reader.ReadBoolean();
					bool arg5 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  CreateCardAt");
					handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 35:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					ulong[] arg1 = new ulong[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = reader.ReadUInt64();
					Log.Debug($"SERVER IN:  CreateAliasDeprecated");
					handler.CreateAliasDeprecated(arg0, arg1);
					break;
				}
				case 36:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg2 = new int[length];
					for (int i = 0; i < length; ++i)
						arg2[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					bool[] arg3 = new bool[length];
					for (int i = 0; i < length; ++i)
						arg3[i] = reader.ReadBoolean();
					bool arg4 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  MoveCardReq");
					handler.MoveCardReq(arg0, arg1, arg2, arg3, arg4);
					break;
				}
				case 38:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg1 = new int[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg2 = new int[length];
					for (int i = 0; i < length; ++i)
						arg2[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg3 = new int[length];
					for (int i = 0; i < length; ++i)
						arg3[i] = reader.ReadInt32();
					bool arg4 = reader.ReadBoolean();
					length = reader.ReadInt16();
					bool[] arg5 = new bool[length];
					for (int i = 0; i < length; ++i)
						arg5[i] = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  MoveCardAtReq");
					handler.MoveCardAtReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 40:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  PeekReq");
					handler.PeekReq(arg0);
					break;
				}
				case 42:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  UntargetReq");
					handler.UntargetReq(arg0, arg1);
					break;
				}
				case 44:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TargetReq");
					handler.TargetReq(arg0, arg1);
					break;
				}
				case 46:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TargetArrowReq");
					handler.TargetArrowReq(arg0, arg1, arg2);
					break;
				}
				case 48:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  Highlight");
					handler.Highlight(arg0, arg1);
					break;
				}
				case 49:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TurnReq");
					handler.TurnReq(arg0, arg1);
					break;
				}
				case 51:
				{
					int arg0 = reader.ReadInt32();
					CardOrientation arg1 = (CardOrientation)reader.ReadByte();
					Log.Debug($"SERVER IN:  RotateReq");
					handler.RotateReq(arg0, arg1);
					break;
				}
				case 53:
				{
					int arg0 = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg1 = new int[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = reader.ReadInt32();
					Log.Debug($"SERVER IN:  ShuffleDeprecated");
					handler.ShuffleDeprecated(arg0, arg1);
					break;
				}
				case 54:
				{
					byte arg0 = reader.ReadByte();
					int arg1 = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg2 = new int[length];
					for (int i = 0; i < length; ++i)
						arg2[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					short[] arg3 = new short[length];
					for (int i = 0; i < length; ++i)
						arg3[i] = reader.ReadInt16();
					Log.Debug($"SERVER IN:  Shuffled");
					handler.Shuffled(arg0, arg1, arg2, arg3);
					break;
				}
				case 55:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  UnaliasGrpDeprecated");
					handler.UnaliasGrpDeprecated(arg0);
					break;
				}
				case 56:
				{
					length = reader.ReadInt16();
					int[] arg0 = new int[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
					ulong[] arg1 = new ulong[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = reader.ReadUInt64();
					Log.Debug($"SERVER IN:  UnaliasDeprecated");
					handler.UnaliasDeprecated(arg0, arg1);
					break;
				}
				case 57:
				{
					int arg0 = reader.ReadInt32();
					Guid arg1 = new Guid(reader.ReadBytes(16));
					string arg2 = reader.ReadString();
					ushort arg3 = reader.ReadUInt16();
					ushort arg4 = reader.ReadUInt16();
					bool arg5 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  AddMarkerReq");
					handler.AddMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 59:
				{
					int arg0 = reader.ReadInt32();
					Guid arg1 = new Guid(reader.ReadBytes(16));
					string arg2 = reader.ReadString();
					ushort arg3 = reader.ReadUInt16();
					ushort arg4 = reader.ReadUInt16();
					bool arg5 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  RemoveMarkerReq");
					handler.RemoveMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 61:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					Guid arg2 = new Guid(reader.ReadBytes(16));
					string arg3 = reader.ReadString();
					ushort arg4 = reader.ReadUInt16();
					ushort arg5 = reader.ReadUInt16();
					bool arg6 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TransferMarkerReq");
					handler.TransferMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
					break;
				}
				case 63:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  PassToReq");
					handler.PassToReq(arg0, arg1, arg2);
					break;
				}
				case 65:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  TakeFromReq");
					handler.TakeFromReq(arg0, arg1);
					break;
				}
				case 67:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  DontTakeReq");
					handler.DontTakeReq(arg0, arg1);
					break;
				}
				case 69:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  FreezeCardsVisibility");
					handler.FreezeCardsVisibility(arg0);
					break;
				}
				case 70:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  GroupVisReq");
					handler.GroupVisReq(arg0, arg1, arg2);
					break;
				}
				case 72:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  GroupVisAddReq");
					handler.GroupVisAddReq(arg0, arg1);
					break;
				}
				case 74:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  GroupVisRemoveReq");
					handler.GroupVisRemoveReq(arg0, arg1);
					break;
				}
				case 76:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LookAtReq");
					handler.LookAtReq(arg0, arg1, arg2);
					break;
				}
				case 78:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LookAtTopReq");
					handler.LookAtTopReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 80:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LookAtBottomReq");
					handler.LookAtBottomReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 82:
				{
					length = reader.ReadInt16();
					Guid[] arg0 = new Guid[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = new Guid(reader.ReadBytes(16));
					Log.Debug($"SERVER IN:  StartLimitedReq");
					handler.StartLimitedReq(arg0);
					break;
				}
				case 84:
				{
					Log.Debug($"SERVER IN:  CancelLimitedReq");
					handler.CancelLimitedReq();
					break;
				}
				case 86:
				{
					byte arg0 = reader.ReadByte();
					int arg1 = reader.ReadInt32();
					string arg2 = reader.ReadString();
					Log.Debug($"SERVER IN:  CardSwitchTo");
					handler.CardSwitchTo(arg0, arg1, arg2);
					break;
				}
				case 87:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					string arg3 = reader.ReadString();
					Log.Debug($"SERVER IN:  PlayerSetGlobalVariable");
					handler.PlayerSetGlobalVariable(arg0, arg1, arg2, arg3);
					break;
				}
				case 88:
				{
					string arg0 = reader.ReadString();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetGlobalVariable");
					handler.SetGlobalVariable(arg0, arg1, arg2);
					break;
				}
				case 90:
				{
					handler.Ping();
					break;
				}
				case 91:
				{
					bool arg0 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  IsTableBackgroundFlipped");
					handler.IsTableBackgroundFlipped(arg0);
					break;
				}
				case 92:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  PlaySound");
					handler.PlaySound(arg0, arg1);
					break;
				}
				case 93:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  Ready");
					handler.Ready(arg0);
					break;
				}
				case 95:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					Log.Debug($"SERVER IN:  RemoteCall");
					handler.RemoteCall(arg0, arg1, arg2);
					break;
				}
				case 96:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  GameStateReq");
					handler.GameStateReq(arg0);
					break;
				}
				case 97:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  GameState");
					handler.GameState(arg0, arg1);
					break;
				}
				case 98:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  DeleteCard");
					handler.DeleteCard(arg0, arg1);
					break;
				}
				case 100:
				{
					length = reader.ReadInt16();
					Guid[] arg0 = new Guid[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = new Guid(reader.ReadBytes(16));
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  AddPacksReq");
					handler.AddPacksReq(arg0, arg1);
					break;
				}
				case 102:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  AnchorCard");
					handler.AnchorCard(arg0, arg1, arg2);
					break;
				}
				case 103:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					string arg2 = reader.ReadString();
					string arg3 = reader.ReadString();
					string arg4 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetCardProperty");
					handler.SetCardProperty(arg0, arg1, arg2, arg3, arg4);
					break;
				}
				case 104:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  ResetCardProperties");
					handler.ResetCardProperties(arg0, arg1);
					break;
				}
				case 105:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  Filter");
					handler.Filter(arg0, arg1);
					break;
				}
				case 106:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetBoard");
					handler.SetBoard(arg0);
					break;
				}
				case 107:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetPlayerColor");
					handler.SetPlayerColor(arg0, arg1);
					break;
				}
				default:
					Debug.WriteLine(L.D.ServerMessage__UnknownBinaryMessage + method);
					break;
			}
			reader.Close();
		}
	}
}
