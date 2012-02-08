using System;
using System.Diagnostics;
using System.IO;

namespace Octgn.Server
{
    internal sealed class BinaryParser
    {
        private readonly Handler handler;

        public BinaryParser(Handler handler)
        {
            this.handler = handler;
        }

        public void Parse(byte[] data)
        {
            BinaryReader reader = null;
            try
            {
                MemoryStream stream = new MemoryStream(data);
                reader = new BinaryReader(stream);
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
                            string arg0 = reader.ReadString();
                            ulong arg1 = reader.ReadUInt64();
                            string arg2 = reader.ReadString();
                            var arg3 = new Version(reader.ReadString());
                            var arg4 = new Version(reader.ReadString());
                            var arg5 = new Guid(reader.ReadBytes(16));
                            var arg6 = new Version(reader.ReadString());
                            handler.Hello(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            break;
                        }
                    case 4:
                        {
                            bool arg0 = reader.ReadBoolean();
                            handler.Settings(arg0);
                            break;
                        }
                    case 5:
                        {
                            byte arg0 = reader.ReadByte();
                            bool arg1 = reader.ReadBoolean();
                            handler.PlayerSettings(arg0, arg1);
                            break;
                        }
                    case 8:
                        {
                            string arg0 = reader.ReadString();
                            handler.NickReq(arg0);
                            break;
                        }
                    case 10:
                        {
                            handler.Start();
                            break;
                        }
                    case 11:
                        {
                            handler.ResetReq();
                            break;
                        }
                    case 13:
                        {
                            byte arg0 = reader.ReadByte();
                            handler.NextTurn(arg0);
                            break;
                        }
                    case 14:
                        {
                            int arg0 = reader.ReadInt32();
                            bool arg1 = reader.ReadBoolean();
                            handler.StopTurnReq(arg0, arg1);
                            break;
                        }
                    case 16:
                        {
                            string arg0 = reader.ReadString();
                            handler.ChatReq(arg0);
                            break;
                        }
                    case 18:
                        {
                            string arg0 = reader.ReadString();
                            handler.PrintReq(arg0);
                            break;
                        }
                    case 20:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            int arg2 = reader.ReadInt32();
                            handler.RandomReq(arg0, arg1, arg2);
                            break;
                        }
                    case 22:
                        {
                            int arg0 = reader.ReadInt32();
                            ulong arg1 = reader.ReadUInt64();
                            handler.RandomAnswer1Req(arg0, arg1);
                            break;
                        }
                    case 24:
                        {
                            int arg0 = reader.ReadInt32();
                            ulong arg1 = reader.ReadUInt64();
                            handler.RandomAnswer2Req(arg0, arg1);
                            break;
                        }
                    case 26:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            handler.CounterReq(arg0, arg1);
                            break;
                        }
                    case 28:
                        {
                            length = reader.ReadInt16();
                            var arg0 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg0[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new ulong[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadUInt64();
                            length = reader.ReadInt16();
                            var arg2 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg2[i] = reader.ReadInt32();
                            handler.LoadDeck(arg0, arg1, arg2);
                            break;
                        }
                    case 29:
                        {
                            length = reader.ReadInt16();
                            var arg0 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg0[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new ulong[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadUInt64();
                            int arg2 = reader.ReadInt32();
                            handler.CreateCard(arg0, arg1, arg2);
                            break;
                        }
                    case 30:
                        {
                            length = reader.ReadInt16();
                            var arg0 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg0[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new ulong[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadUInt64();
                            length = reader.ReadInt16();
                            var arg2 = new Guid[length];
                            for (int i = 0; i < length; ++i)
                                arg2[i] = new Guid(reader.ReadBytes(16));
                            length = reader.ReadInt16();
                            var arg3 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg3[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg4 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg4[i] = reader.ReadInt32();
                            bool arg5 = reader.ReadBoolean();
                            bool arg6 = reader.ReadBoolean();
                            handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            break;
                        }
                    case 31:
                        {
                            length = reader.ReadInt16();
                            var arg0 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg0[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new ulong[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadUInt64();
                            handler.CreateAlias(arg0, arg1);
                            break;
                        }
                    case 32:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            int arg2 = reader.ReadInt32();
                            bool arg3 = reader.ReadBoolean();
                            handler.MoveCardReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 34:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            int arg2 = reader.ReadInt32();
                            int arg3 = reader.ReadInt32();
                            bool arg4 = reader.ReadBoolean();
                            handler.MoveCardAtReq(arg0, arg1, arg2, arg3, arg4);
                            break;
                        }
                    case 36:
                        {
                            int arg0 = reader.ReadInt32();
                            ulong arg1 = reader.ReadUInt64();
                            var arg2 = new Guid(reader.ReadBytes(16));
                            handler.Reveal(arg0, arg1, arg2);
                            break;
                        }
                    case 37:
                        {
                            byte arg0 = reader.ReadByte();
                            length = reader.ReadInt16();
                            var arg1 = new byte[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadByte();
                            int arg2 = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg3 = new ulong[length];
                            for (int i = 0; i < length; ++i)
                                arg3[i] = reader.ReadUInt64();
                            handler.RevealToReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 39:
                        {
                            int arg0 = reader.ReadInt32();
                            handler.PeekReq(arg0);
                            break;
                        }
                    case 41:
                        {
                            int arg0 = reader.ReadInt32();
                            handler.UntargetReq(arg0);
                            break;
                        }
                    case 43:
                        {
                            int arg0 = reader.ReadInt32();
                            handler.TargetReq(arg0);
                            break;
                        }
                    case 45:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            handler.TargetArrowReq(arg0, arg1);
                            break;
                        }
                    case 47:
                        {
                            int arg0 = reader.ReadInt32();
                            string arg1 = reader.ReadString();
                            handler.Highlight(arg0, arg1);
                            break;
                        }
                    case 48:
                        {
                            int arg0 = reader.ReadInt32();
                            bool arg1 = reader.ReadBoolean();
                            handler.TurnReq(arg0, arg1);
                            break;
                        }
                    case 50:
                        {
                            int arg0 = reader.ReadInt32();
                            var arg1 = (CardOrientation) reader.ReadByte();
                            handler.RotateReq(arg0, arg1);
                            break;
                        }
                    case 52:
                        {
                            int arg0 = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadInt32();
                            handler.Shuffle(arg0, arg1);
                            break;
                        }
                    case 53:
                        {
                            int arg0 = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg2 = new short[length];
                            for (int i = 0; i < length; ++i)
                                arg2[i] = reader.ReadInt16();
                            handler.Shuffled(arg0, arg1, arg2);
                            break;
                        }
                    case 54:
                        {
                            int arg0 = reader.ReadInt32();
                            handler.UnaliasGrp(arg0);
                            break;
                        }
                    case 55:
                        {
                            length = reader.ReadInt16();
                            var arg0 = new int[length];
                            for (int i = 0; i < length; ++i)
                                arg0[i] = reader.ReadInt32();
                            length = reader.ReadInt16();
                            var arg1 = new ulong[length];
                            for (int i = 0; i < length; ++i)
                                arg1[i] = reader.ReadUInt64();
                            handler.Unalias(arg0, arg1);
                            break;
                        }
                    case 56:
                        {
                            int arg0 = reader.ReadInt32();
                            var arg1 = new Guid(reader.ReadBytes(16));
                            string arg2 = reader.ReadString();
                            ushort arg3 = reader.ReadUInt16();
                            handler.AddMarkerReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 58:
                        {
                            int arg0 = reader.ReadInt32();
                            var arg1 = new Guid(reader.ReadBytes(16));
                            string arg2 = reader.ReadString();
                            ushort arg3 = reader.ReadUInt16();
                            handler.RemoveMarkerReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 60:
                        {
                            int arg0 = reader.ReadInt32();
                            var arg1 = new Guid(reader.ReadBytes(16));
                            string arg2 = reader.ReadString();
                            ushort arg3 = reader.ReadUInt16();
                            handler.SetMarkerReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 62:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            var arg2 = new Guid(reader.ReadBytes(16));
                            string arg3 = reader.ReadString();
                            ushort arg4 = reader.ReadUInt16();
                            handler.TransferMarkerReq(arg0, arg1, arg2, arg3, arg4);
                            break;
                        }
                    case 64:
                        {
                            int arg0 = reader.ReadInt32();
                            byte arg1 = reader.ReadByte();
                            bool arg2 = reader.ReadBoolean();
                            handler.PassToReq(arg0, arg1, arg2);
                            break;
                        }
                    case 66:
                        {
                            int arg0 = reader.ReadInt32();
                            byte arg1 = reader.ReadByte();
                            handler.TakeFromReq(arg0, arg1);
                            break;
                        }
                    case 68:
                        {
                            int arg0 = reader.ReadInt32();
                            byte arg1 = reader.ReadByte();
                            handler.DontTakeReq(arg0, arg1);
                            break;
                        }
                    case 70:
                        {
                            int arg0 = reader.ReadInt32();
                            handler.FreezeCardsVisibility(arg0);
                            break;
                        }
                    case 71:
                        {
                            int arg0 = reader.ReadInt32();
                            bool arg1 = reader.ReadBoolean();
                            bool arg2 = reader.ReadBoolean();
                            handler.GroupVisReq(arg0, arg1, arg2);
                            break;
                        }
                    case 73:
                        {
                            int arg0 = reader.ReadInt32();
                            byte arg1 = reader.ReadByte();
                            handler.GroupVisAddReq(arg0, arg1);
                            break;
                        }
                    case 75:
                        {
                            int arg0 = reader.ReadInt32();
                            byte arg1 = reader.ReadByte();
                            handler.GroupVisRemoveReq(arg0, arg1);
                            break;
                        }
                    case 77:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            bool arg2 = reader.ReadBoolean();
                            handler.LookAtReq(arg0, arg1, arg2);
                            break;
                        }
                    case 79:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            int arg2 = reader.ReadInt32();
                            bool arg3 = reader.ReadBoolean();
                            handler.LookAtTopReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 81:
                        {
                            int arg0 = reader.ReadInt32();
                            int arg1 = reader.ReadInt32();
                            int arg2 = reader.ReadInt32();
                            bool arg3 = reader.ReadBoolean();
                            handler.LookAtBottomReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case 83:
                        {
                            length = reader.ReadInt16();
                            var arg0 = new Guid[length];
                            for (int i = 0; i < length; ++i)
                                arg0[i] = new Guid(reader.ReadBytes(16));
                            handler.StartLimitedReq(arg0);
                            break;
                        }
                    case 85:
                        {
                            handler.CancelLimitedReq();
                            break;
                        }
                    case 87:
                        {
                            int arg0 = reader.ReadInt32();
                            bool arg1 = reader.ReadBoolean();
                            handler.IsAlternateImage(arg0, arg1);

                            break;
                        }
                    case 88:
                        {
                            byte b = reader.ReadByte();
                            string n = reader.ReadString();
                            string v = reader.ReadString();
                            handler.PlayerSetGlobalVariable(b, n, v);
                            break;
                        }
                    case 89:
                        {
                            string n = reader.ReadString();
                            string v = reader.ReadString();
                            handler.SetGlobalVariable(n, v);
                            break;
                        }
                    case 255:
                        {
                            handler.PingReceived();
                            Debug.WriteLine("[Ping recieved]");
                            break;
                        }
                    default:
                        Debug.WriteLine("[Server Parser] Unknown message: " + method);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parse error in binaryparser: " + ex.StackTrace);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }
    }
}