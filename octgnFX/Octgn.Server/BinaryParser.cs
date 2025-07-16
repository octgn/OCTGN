/*
 * This file was automatically generated!
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Diagnostics;
using System.IO;
using Octgn.Library.Localization;

namespace Octgn.Server
{
	public sealed class BinaryParser
	{
		private static log4net.ILog Log = log4net.LogManager.GetLogger(nameof(BinaryParser));

		private readonly ServerSocket _socket;

		public BinaryParser(ServerSocket socket)
		{
			_socket = socket ?? throw new ArgumentNullException(nameof(socket));
		}

		public void Parse(byte[] data)
		{
			MemoryStream stream = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(stream);
			short length;
			_socket.Server.Context.State.IsMuted = reader.ReadInt32();
			byte method = reader.ReadByte();
			switch (method)
			{
				case 0:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  Error");
					_socket.Handler.Error(arg0);
					break;
				}
				case 1:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  Boot");
					_socket.Handler.Boot(arg0, arg1);
					break;
				}
				case 3:
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
					_socket.Handler.Hello(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
					break;
				}
				case 4:
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
					_socket.Handler.HelloAgain(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
					break;
				}
				case 6:
				{
					bool arg0 = reader.ReadBoolean();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					bool arg3 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  Settings");
					_socket.Handler.Settings(arg0, arg1, arg2, arg3);
					break;
				}
				case 7:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  PlayerSettings");
					_socket.Handler.PlayerSettings(arg0, arg1, arg2);
					break;
				}
				case 9:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  Leave");
					_socket.Handler.Leave(arg0);
					break;
				}
				case 10:
				{
					Log.Debug($"SERVER IN:  Start");
					_socket.Handler.Start();
					break;
				}
				case 11:
				{
					bool arg0 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  ResetReq");
					_socket.Handler.ResetReq(arg0);
					break;
				}
				case 13:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  NextTurn");
					_socket.Handler.NextTurn(arg0, arg1, arg2);
					break;
				}
				case 14:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  StopTurnReq");
					_socket.Handler.StopTurnReq(arg0, arg1);
					break;
				}
				case 16:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  SetPhaseReq");
					_socket.Handler.SetPhaseReq(arg0, arg1);
					break;
				}
				case 18:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  StopPhaseReq");
					_socket.Handler.StopPhaseReq(arg0, arg1);
					break;
				}
				case 19:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  SetActivePlayer");
					_socket.Handler.SetActivePlayer(arg0);
					break;
				}
				case 20:
				{
					Log.Debug($"SERVER IN:  ClearActivePlayer");
					_socket.Handler.ClearActivePlayer();
					break;
				}
				case 21:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  ChatReq");
					_socket.Handler.ChatReq(arg0);
					break;
				}
				case 23:
				{
					string arg0 = reader.ReadString();
					Log.Debug($"SERVER IN:  PrintReq");
					_socket.Handler.PrintReq(arg0);
					break;
				}
				case 25:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  RandomReq");
					_socket.Handler.RandomReq(arg0, arg1);
					break;
				}
				case 27:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  CounterReq");
					_socket.Handler.CounterReq(arg0, arg1, arg2);
					break;
				}
				case 29:
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
					_socket.Handler.LoadDeck(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 30:
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
					_socket.Handler.CreateCard(arg0, arg1, arg2, arg3);
					break;
				}
				case 31:
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
					_socket.Handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 32:
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
					_socket.Handler.CreateAliasDeprecated(arg0, arg1);
					break;
				}
				case 33:
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
					_socket.Handler.MoveCardReq(arg0, arg1, arg2, arg3, arg4);
					break;
				}
				case 35:
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
					_socket.Handler.MoveCardAtReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 37:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  PeekReq");
					_socket.Handler.PeekReq(arg0);
					break;
				}
				case 39:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  UntargetReq");
					_socket.Handler.UntargetReq(arg0, arg1);
					break;
				}
				case 41:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TargetReq");
					_socket.Handler.TargetReq(arg0, arg1);
					break;
				}
				case 43:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TargetArrowReq");
					_socket.Handler.TargetArrowReq(arg0, arg1, arg2);
					break;
				}
				case 45:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  Highlight");
					_socket.Handler.Highlight(arg0, arg1);
					break;
				}
				case 46:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TurnReq");
					_socket.Handler.TurnReq(arg0, arg1);
					break;
				}
				case 48:
				{
					int arg0 = reader.ReadInt32();
					CardOrientation arg1 = (CardOrientation)reader.ReadByte();
					Log.Debug($"SERVER IN:  RotateReq");
					_socket.Handler.RotateReq(arg0, arg1);
					break;
				}
				case 49:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  ShakeReq");
					_socket.Handler.ShakeReq(arg0);
					break;
				}
				case 51:
				{
					int arg0 = reader.ReadInt32();
					length = reader.ReadInt16();
					int[] arg1 = new int[length];
					for (int i = 0; i < length; ++i)
						arg1[i] = reader.ReadInt32();
					Log.Debug($"SERVER IN:  ShuffleDeprecated");
					_socket.Handler.ShuffleDeprecated(arg0, arg1);
					break;
				}
				case 52:
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
					_socket.Handler.Shuffled(arg0, arg1, arg2, arg3);
					break;
				}
				case 53:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  UnaliasGrpDeprecated");
					_socket.Handler.UnaliasGrpDeprecated(arg0);
					break;
				}
				case 54:
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
					_socket.Handler.UnaliasDeprecated(arg0, arg1);
					break;
				}
				case 55:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					ushort arg3 = reader.ReadUInt16();
					ushort arg4 = reader.ReadUInt16();
					bool arg5 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  AddMarkerReq");
					_socket.Handler.AddMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 57:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					ushort arg3 = reader.ReadUInt16();
					ushort arg4 = reader.ReadUInt16();
					bool arg5 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  RemoveMarkerReq");
					_socket.Handler.RemoveMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 59:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					string arg2 = reader.ReadString();
					string arg3 = reader.ReadString();
					ushort arg4 = reader.ReadUInt16();
					ushort arg5 = reader.ReadUInt16();
					bool arg6 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  TransferMarkerReq");
					_socket.Handler.TransferMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
					break;
				}
				case 61:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  PassToReq");
					_socket.Handler.PassToReq(arg0, arg1, arg2);
					break;
				}
				case 63:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  TakeFromReq");
					_socket.Handler.TakeFromReq(arg0, arg1);
					break;
				}
				case 65:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  DontTakeReq");
					_socket.Handler.DontTakeReq(arg0, arg1);
					break;
				}
				case 67:
				{
					int arg0 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  FreezeCardsVisibility");
					_socket.Handler.FreezeCardsVisibility(arg0);
					break;
				}
				case 68:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  GroupVisReq");
					_socket.Handler.GroupVisReq(arg0, arg1, arg2);
					break;
				}
				case 70:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  GroupVisAddReq");
					_socket.Handler.GroupVisAddReq(arg0, arg1);
					break;
				}
				case 72:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  GroupVisRemoveReq");
					_socket.Handler.GroupVisRemoveReq(arg0, arg1);
					break;
				}
				case 74:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  GroupProtectionReq");
					_socket.Handler.GroupProtectionReq(arg0, arg1);
					break;
				}
				case 76:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LookAtReq");
					_socket.Handler.LookAtReq(arg0, arg1, arg2);
					break;
				}
				case 78:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LookAtTopReq");
					_socket.Handler.LookAtTopReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 80:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  LookAtBottomReq");
					_socket.Handler.LookAtBottomReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 82:
				{
					length = reader.ReadInt16();
					Guid[] arg0 = new Guid[length];
					for (int i = 0; i < length; ++i)
						arg0[i] = new Guid(reader.ReadBytes(16));
					Log.Debug($"SERVER IN:  StartLimitedReq");
					_socket.Handler.StartLimitedReq(arg0);
					break;
				}
				case 84:
				{
					Log.Debug($"SERVER IN:  CancelLimitedReq");
					_socket.Handler.CancelLimitedReq();
					break;
				}
				case 86:
				{
					byte arg0 = reader.ReadByte();
					int arg1 = reader.ReadInt32();
					string arg2 = reader.ReadString();
					Log.Debug($"SERVER IN:  CardSwitchTo");
					_socket.Handler.CardSwitchTo(arg0, arg1, arg2);
					break;
				}
				case 87:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					string arg3 = reader.ReadString();
					Log.Debug($"SERVER IN:  PlayerSetGlobalVariable");
					_socket.Handler.PlayerSetGlobalVariable(arg0, arg1, arg2, arg3);
					break;
				}
				case 88:
				{
					string arg0 = reader.ReadString();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetGlobalVariable");
					_socket.Handler.SetGlobalVariable(arg0, arg1, arg2);
					break;
				}
				case 90:
				{
					_socket.Handler.Ping();
					break;
				}
				case 91:
				{
					bool arg0 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  IsTableBackgroundFlipped");
					_socket.Handler.IsTableBackgroundFlipped(arg0);
					break;
				}
				case 92:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  PlaySound");
					_socket.Handler.PlaySound(arg0, arg1);
					break;
				}
				case 93:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  Ready");
					_socket.Handler.Ready(arg0);
					break;
				}
				case 95:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					Log.Debug($"SERVER IN:  RemoteCall");
					_socket.Handler.RemoteCall(arg0, arg1, arg2);
					break;
				}
				case 96:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  GameStateReq");
					_socket.Handler.GameStateReq(arg0);
					break;
				}
				case 97:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  GameState");
					_socket.Handler.GameState(arg0, arg1);
					break;
				}
				case 98:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  DeleteCard");
					_socket.Handler.DeleteCard(arg0, arg1);
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
					_socket.Handler.AddPacksReq(arg0, arg1);
					break;
				}
				case 102:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					bool arg2 = reader.ReadBoolean();
					Log.Debug($"SERVER IN:  AnchorCard");
					_socket.Handler.AnchorCard(arg0, arg1, arg2);
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
					_socket.Handler.SetCardProperty(arg0, arg1, arg2, arg3, arg4);
					break;
				}
				case 104:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					Log.Debug($"SERVER IN:  ResetCardProperties");
					_socket.Handler.ResetCardProperties(arg0, arg1);
					break;
				}
				case 105:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  Filter");
					_socket.Handler.Filter(arg0, arg1);
					break;
				}
				case 106:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetBoard");
					_socket.Handler.SetBoard(arg0, arg1);
					break;
				}
				case 107:
				{
					byte arg0 = reader.ReadByte();
					Log.Debug($"SERVER IN:  RemoveBoard");
					_socket.Handler.RemoveBoard(arg0);
					break;
				}
				case 108:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					Log.Debug($"SERVER IN:  SetPlayerColor");
					_socket.Handler.SetPlayerColor(arg0, arg1);
					break;
				}
				case 109:
				{
					byte arg0 = reader.ReadByte();
					int arg1 = reader.ReadInt32();
					byte arg2 = reader.ReadByte();
					string arg3 = reader.ReadString();
					int arg4 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  RequestPileViewPermission");
					_socket.Handler.RequestPileViewPermission(arg0, arg1, arg2, arg3, arg4);
					break;
				}
				case 110:
				{
					byte arg0 = reader.ReadByte();
					int arg1 = reader.ReadInt32();
					byte arg2 = reader.ReadByte();
					bool arg3 = reader.ReadBoolean();
					bool arg4 = reader.ReadBoolean();
					string arg5 = reader.ReadString();
					int arg6 = reader.ReadInt32();
					Log.Debug($"SERVER IN:  GrantPileViewPermission");
					_socket.Handler.GrantPileViewPermission(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
					break;
				}
				default:
					Debug.WriteLine(L.D.ServerMessage__UnknownBinaryMessage + method);
					break;
			}
			reader.Close();
		}

		public static byte PingId = 89;

		public static byte[] AnonymousCalls = new byte[] { 0, 3, 4, 89 };
	}
}
