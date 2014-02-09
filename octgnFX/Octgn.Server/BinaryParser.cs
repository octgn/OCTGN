








/* 
 * This file was automatically generated!
 * Do not modify, changes will get lost when the file is regenerated!
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Octgn.Server
{
	sealed class BinaryParser
	{
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
					handler.Binary();
					break;
				}
				case 1:
				{
					string arg0 = reader.ReadString();
					handler.Error(arg0);
					break;
				}
				case 2:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					handler.Boot(arg0, arg1);
					break;
				}
				case 4:
				{
					string arg0 = reader.ReadString();
					ulong arg1 = reader.ReadUInt64();
					string arg2 = reader.ReadString();
					Version arg3 = new Version(reader.ReadString());
					Version arg4 = new Version(reader.ReadString());
					Guid arg5 = new Guid(reader.ReadBytes(16));
					Version arg6 = new Version(reader.ReadString());
					string arg7 = reader.ReadString();
					bool arg8 = reader.ReadBoolean();
					handler.Hello(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
					break;
				}
				case 5:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					ulong arg2 = reader.ReadUInt64();
					string arg3 = reader.ReadString();
					Version arg4 = new Version(reader.ReadString());
					Version arg5 = new Version(reader.ReadString());
					Guid arg6 = new Guid(reader.ReadBytes(16));
					Version arg7 = new Version(reader.ReadString());
					string arg8 = reader.ReadString();
					handler.HelloAgain(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
					break;
				}
				case 7:
				{
					bool arg0 = reader.ReadBoolean();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					handler.Settings(arg0, arg1, arg2);
					break;
				}
				case 8:
				{
					byte arg0 = reader.ReadByte();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					handler.PlayerSettings(arg0, arg1, arg2);
					break;
				}
				case 10:
				{
					byte arg0 = reader.ReadByte();
					handler.Leave(arg0);
					break;
				}
				case 11:
				{
					string arg0 = reader.ReadString();
					handler.NickReq(arg0);
					break;
				}
				case 13:
				{
					handler.Start();
					break;
				}
				case 14:
				{
					handler.ResetReq();
					break;
				}
				case 16:
				{
					byte arg0 = reader.ReadByte();
					handler.NextTurn(arg0);
					break;
				}
				case 17:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					handler.StopTurnReq(arg0, arg1);
					break;
				}
				case 19:
				{
					string arg0 = reader.ReadString();
					handler.ChatReq(arg0);
					break;
				}
				case 21:
				{
					string arg0 = reader.ReadString();
					handler.PrintReq(arg0);
					break;
				}
				case 23:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					handler.RandomReq(arg0, arg1, arg2);
					break;
				}
				case 25:
				{
					int arg0 = reader.ReadInt32();
					ulong arg1 = reader.ReadUInt64();
					handler.RandomAnswer1Req(arg0, arg1);
					break;
				}
				case 27:
				{
					int arg0 = reader.ReadInt32();
					ulong arg1 = reader.ReadUInt64();
					handler.RandomAnswer2Req(arg0, arg1);
					break;
				}
				case 29:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					handler.CounterReq(arg0, arg1);
					break;
				}
				case 31:
				{
					length = reader.ReadInt16();
int[] arg0 = new int[length];
for (int i = 0; i < length; ++i)
	arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
ulong[] arg1 = new ulong[length];
for (int i = 0; i < length; ++i)
	arg1[i] = reader.ReadUInt64();
					length = reader.ReadInt16();
int[] arg2 = new int[length];
for (int i = 0; i < length; ++i)
arg2[i] = reader.ReadInt32();
					handler.LoadDeck(arg0, arg1, arg2);
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
					int arg2 = reader.ReadInt32();
					handler.CreateCard(arg0, arg1, arg2);
					break;
				}
				case 33:
				{
					length = reader.ReadInt16();
int[] arg0 = new int[length];
for (int i = 0; i < length; ++i)
	arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
ulong[] arg1 = new ulong[length];
for (int i = 0; i < length; ++i)
	arg1[i] = reader.ReadUInt64();
					length = reader.ReadInt16();
Guid[] arg2 = new Guid[length];
for (int i = 0; i < length; ++i)
	arg2[i] = new Guid(reader.ReadBytes(16));
					length = reader.ReadInt16();
int[] arg3 = new int[length];
for (int i = 0; i < length; ++i)
	arg3[i] = reader.ReadInt32();
					length = reader.ReadInt16();
int[] arg4 = new int[length];
for (int i = 0; i < length; ++i)
	arg4[i] = reader.ReadInt32();
					bool arg5 = reader.ReadBoolean();
					bool arg6 = reader.ReadBoolean();
					handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
					break;
				}
				case 34:
				{
					length = reader.ReadInt16();
int[] arg0 = new int[length];
for (int i = 0; i < length; ++i)
	arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
ulong[] arg1 = new ulong[length];
for (int i = 0; i < length; ++i)
	arg1[i] = reader.ReadUInt64();
					handler.CreateAliasDeprecated(arg0, arg1);
					break;
				}
				case 35:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					bool arg4 = reader.ReadBoolean();
					handler.MoveCardReq(arg0, arg1, arg2, arg3, arg4);
					break;
				}
				case 37:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					int arg3 = reader.ReadInt32();
					bool arg4 = reader.ReadBoolean();
					bool arg5 = reader.ReadBoolean();
					handler.MoveCardAtReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 39:
				{
					int arg0 = reader.ReadInt32();
					ulong arg1 = reader.ReadUInt64();
					Guid arg2 = new Guid(reader.ReadBytes(16));
					handler.Reveal(arg0, arg1, arg2);
					break;
				}
				case 40:
				{
					byte arg0 = reader.ReadByte();
					length = reader.ReadInt16();
byte[] arg1 = new byte[length];
for (int i = 0; i < length; ++i)
arg1[i] = reader.ReadByte();
					int arg2 = reader.ReadInt32();
					length = reader.ReadInt16();
ulong[] arg3 = new ulong[length];
for (int i = 0; i < length; ++i)
	arg3[i] = reader.ReadUInt64();
					handler.RevealToReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 42:
				{
					int arg0 = reader.ReadInt32();
					handler.PeekReq(arg0);
					break;
				}
				case 44:
				{
					int arg0 = reader.ReadInt32();
					handler.UntargetReq(arg0);
					break;
				}
				case 46:
				{
					int arg0 = reader.ReadInt32();
					handler.TargetReq(arg0);
					break;
				}
				case 48:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					handler.TargetArrowReq(arg0, arg1);
					break;
				}
				case 50:
				{
					int arg0 = reader.ReadInt32();
					string arg1 = reader.ReadString();
					handler.Highlight(arg0, arg1);
					break;
				}
				case 51:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					handler.TurnReq(arg0, arg1);
					break;
				}
				case 53:
				{
					int arg0 = reader.ReadInt32();
					CardOrientation arg1 = (CardOrientation)reader.ReadByte();
					handler.RotateReq(arg0, arg1);
					break;
				}
				case 55:
				{
					int arg0 = reader.ReadInt32();
					length = reader.ReadInt16();
int[] arg1 = new int[length];
for (int i = 0; i < length; ++i)
	arg1[i] = reader.ReadInt32();
					handler.ShuffleDeprecated(arg0, arg1);
					break;
				}
				case 56:
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
					handler.Shuffled(arg0, arg1, arg2, arg3);
					break;
				}
				case 57:
				{
					int arg0 = reader.ReadInt32();
					handler.UnaliasGrpDeprecated(arg0);
					break;
				}
				case 58:
				{
					length = reader.ReadInt16();
int[] arg0 = new int[length];
for (int i = 0; i < length; ++i)
	arg0[i] = reader.ReadInt32();
					length = reader.ReadInt16();
ulong[] arg1 = new ulong[length];
for (int i = 0; i < length; ++i)
	arg1[i] = reader.ReadUInt64();
					handler.UnaliasDeprecated(arg0, arg1);
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
					handler.AddMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 61:
				{
					int arg0 = reader.ReadInt32();
					Guid arg1 = new Guid(reader.ReadBytes(16));
					string arg2 = reader.ReadString();
					ushort arg3 = reader.ReadUInt16();
					ushort arg4 = reader.ReadUInt16();
					bool arg5 = reader.ReadBoolean();
					handler.RemoveMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5);
					break;
				}
				case 63:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					Guid arg2 = new Guid(reader.ReadBytes(16));
					string arg3 = reader.ReadString();
					ushort arg4 = reader.ReadUInt16();
					ushort arg5 = reader.ReadUInt16();
					bool arg6 = reader.ReadBoolean();
					handler.TransferMarkerReq(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
					break;
				}
				case 65:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					bool arg2 = reader.ReadBoolean();
					handler.PassToReq(arg0, arg1, arg2);
					break;
				}
				case 67:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					handler.TakeFromReq(arg0, arg1);
					break;
				}
				case 69:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					handler.DontTakeReq(arg0, arg1);
					break;
				}
				case 71:
				{
					int arg0 = reader.ReadInt32();
					handler.FreezeCardsVisibility(arg0);
					break;
				}
				case 72:
				{
					int arg0 = reader.ReadInt32();
					bool arg1 = reader.ReadBoolean();
					bool arg2 = reader.ReadBoolean();
					handler.GroupVisReq(arg0, arg1, arg2);
					break;
				}
				case 74:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					handler.GroupVisAddReq(arg0, arg1);
					break;
				}
				case 76:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					handler.GroupVisRemoveReq(arg0, arg1);
					break;
				}
				case 78:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					bool arg2 = reader.ReadBoolean();
					handler.LookAtReq(arg0, arg1, arg2);
					break;
				}
				case 80:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					handler.LookAtTopReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 82:
				{
					int arg0 = reader.ReadInt32();
					int arg1 = reader.ReadInt32();
					int arg2 = reader.ReadInt32();
					bool arg3 = reader.ReadBoolean();
					handler.LookAtBottomReq(arg0, arg1, arg2, arg3);
					break;
				}
				case 84:
				{
					length = reader.ReadInt16();
Guid[] arg0 = new Guid[length];
for (int i = 0; i < length; ++i)
	arg0[i] = new Guid(reader.ReadBytes(16));
					handler.StartLimitedReq(arg0);
					break;
				}
				case 86:
				{
					handler.CancelLimitedReq();
					break;
				}
				case 88:
				{
					byte arg0 = reader.ReadByte();
					int arg1 = reader.ReadInt32();
					string arg2 = reader.ReadString();
					handler.CardSwitchTo(arg0, arg1, arg2);
					break;
				}
				case 89:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					handler.PlayerSetGlobalVariable(arg0, arg1, arg2);
					break;
				}
				case 90:
				{
					string arg0 = reader.ReadString();
					string arg1 = reader.ReadString();
					handler.SetGlobalVariable(arg0, arg1);
					break;
				}
				case 92:
				{
					handler.Ping();
					break;
				}
				case 93:
				{
					bool arg0 = reader.ReadBoolean();
					handler.IsTableBackgroundFlipped(arg0);
					break;
				}
				case 94:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					handler.PlaySound(arg0, arg1);
					break;
				}
				case 95:
				{
					byte arg0 = reader.ReadByte();
					handler.Ready(arg0);
					break;
				}
				case 97:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					string arg2 = reader.ReadString();
					handler.RemoteCall(arg0, arg1, arg2);
					break;
				}
				case 98:
				{
					byte arg0 = reader.ReadByte();
					handler.GameStateReq(arg0);
					break;
				}
				case 99:
				{
					byte arg0 = reader.ReadByte();
					string arg1 = reader.ReadString();
					handler.GameState(arg0, arg1);
					break;
				}
				case 100:
				{
					int arg0 = reader.ReadInt32();
					byte arg1 = reader.ReadByte();
					handler.DeleteCard(arg0, arg1);
					break;
				}
				case 102:
				{
					length = reader.ReadInt16();
Guid[] arg0 = new Guid[length];
for (int i = 0; i < length; ++i)
	arg0[i] = new Guid(reader.ReadBytes(16));
					bool arg1 = reader.ReadBoolean();
					handler.AddPacksReq(arg0, arg1);
					break;
				}

				default:
					Debug.WriteLine("[Server Parser] Unknown message: " + method);
					break;
			}
			reader.Close();
		}
	}
}
