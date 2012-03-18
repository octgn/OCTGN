﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using Octgn.Data.Properties;

namespace Octgn.Server
{
    internal sealed class XmlParser
    {
        private readonly Handler _handler;

        public XmlParser(Handler handler)
        {
            _handler = handler;
        }

        public void Parse(string xml)
        {
            StringReader sr = null;
            XmlReader reader = null;
            try
            {
                sr = new StringReader(xml);
                reader = XmlReader.Create(sr);
                reader.Read();
                string method = reader.Name;
                string muted = reader.GetAttribute("muted");
                _handler.Muted = !string.IsNullOrEmpty(muted) ? int.Parse(muted) : 0;
                reader.ReadStartElement(); // <method>
                switch (method)
                {
                    case "Binary":
                        {
                            _handler.Binary();
                            break;
                        }
                    case "Ping":
                        {
                            _handler.PingReceived();
                            break;
                        }
                    case "Error":
                        {
                            string arg0 = reader.ReadElementString("msg");
                            _handler.Error(arg0);
                            break;
                        }
                    case "SwitchWithAlternate":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("cardid"));
                            _handler.SwitchWithAlternate(arg0);
                            break;
                        }
                    case "IsAlternateImage":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("cardid"));
                            bool arg1 = bool.Parse(reader.ReadElementString("isalternateimage"));

                            _handler.IsAlternateImage(arg0, arg1);
                            break;
                        }
                    case "Hello":
                        {
                            string arg0 = reader.ReadElementString("nick");
                            ulong arg1 = ulong.Parse(reader.ReadElementString("pkey"), CultureInfo.InvariantCulture);
                            string arg2 = reader.ReadElementString("client");
                            var arg3 = new Version(reader.ReadElementString("clientVer"));
                            var arg4 = new Version(reader.ReadElementString("octgnVer"));
                            var arg5 = new Guid(reader.ReadElementString("gameId"));
                            var arg6 = new Version(reader.ReadElementString("gameVersion"));
                            _handler.Hello(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            break;
                        }
                    case "Settings":
                        {
                            bool arg0 = bool.Parse(reader.ReadElementString("twoSidedTable"));
                            _handler.Settings(arg0);
                            break;
                        }
                    case "PlayerSettings":
                        {
                            byte arg0 = byte.Parse(reader.ReadElementString("playerId"), CultureInfo.InvariantCulture);
                            bool arg1 = bool.Parse(reader.ReadElementString("invertedTable"));
                            _handler.PlayerSettings(arg0, arg1);
                            break;
                        }
                    case "NickReq":
                        {
                            string arg0 = reader.ReadElementString("nick");
                            _handler.NickReq(arg0);
                            break;
                        }
                    case "Start":
                        {
                            _handler.Start();
                            break;
                        }
                    case "ResetReq":
                        {
                            _handler.ResetReq();
                            break;
                        }
                    case "NextTurn":
                        {
                            byte arg0 = byte.Parse(reader.ReadElementString("nextPlayer"), CultureInfo.InvariantCulture);
                            _handler.NextTurn(arg0);
                            break;
                        }
                    case "StopTurnReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("turnNumber"), CultureInfo.InvariantCulture);
                            bool arg1 = bool.Parse(reader.ReadElementString("stop"));
                            _handler.StopTurnReq(arg0, arg1);
                            break;
                        }
                    case "ChatReq":
                        {
                            string arg0 = reader.ReadElementString("text");
                            _handler.ChatReq(arg0);
                            break;
                        }
                    case "PrintReq":
                        {
                            string arg0 = reader.ReadElementString("text");
                            _handler.PrintReq(arg0);
                            break;
                        }
                    case "RandomReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("min"), CultureInfo.InvariantCulture);
                            int arg2 = int.Parse(reader.ReadElementString("max"), CultureInfo.InvariantCulture);
                            _handler.RandomReq(arg0, arg1, arg2);
                            break;
                        }
                    case "RandomAnswer1Req":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                            ulong arg1 = ulong.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                            _handler.RandomAnswer1Req(arg0, arg1);
                            break;
                        }
                    case "RandomAnswer2Req":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                            ulong arg1 = ulong.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                            _handler.RandomAnswer2Req(arg0, arg1);
                            break;
                        }
                    case "CounterReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("counter"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                            _handler.CounterReq(arg0, arg1);
                            break;
                        }
                    case "LoadDeck":
                        {
                            var list0 = new List<int>(30);
                            while (reader.IsStartElement("id"))
                                list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg0 = list0.ToArray();
                            var list1 = new List<ulong>(30);
                            while (reader.IsStartElement("type"))
                                list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            ulong[] arg1 = list1.ToArray();
                            var list2 = new List<int>(30);
                            while (reader.IsStartElement("group"))
                                list2.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg2 = list2.ToArray();
                            _handler.LoadDeck(arg0, arg1, arg2);
                            break;
                        }
                    case "CreateCard":
                        {
                            var list0 = new List<int>(30);
                            while (reader.IsStartElement("id"))
                                list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg0 = list0.ToArray();
                            var list1 = new List<ulong>(30);
                            while (reader.IsStartElement("type"))
                                list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            ulong[] arg1 = list1.ToArray();
                            int arg2 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            _handler.CreateCard(arg0, arg1, arg2);
                            break;
                        }
                    case "CreateCardAt":
                        {
                            var list0 = new List<int>(30);
                            while (reader.IsStartElement("id"))
                                list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg0 = list0.ToArray();
                            var list1 = new List<ulong>(30);
                            while (reader.IsStartElement("key"))
                                list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            ulong[] arg1 = list1.ToArray();
                            var list2 = new List<Guid>(30);
                            while (reader.IsStartElement("modelId"))
                                list2.Add(new Guid(reader.ReadElementString()));
                            Guid[] arg2 = list2.ToArray();
                            var list3 = new List<int>(30);
                            while (reader.IsStartElement("x"))
                                list3.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg3 = list3.ToArray();
                            var list4 = new List<int>(30);
                            while (reader.IsStartElement("y"))
                                list4.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg4 = list4.ToArray();
                            bool arg5 = bool.Parse(reader.ReadElementString("faceUp"));
                            bool arg6 = bool.Parse(reader.ReadElementString("persist"));
                            _handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                            break;
                        }
                    case "CreateAlias":
                        {
                            var list0 = new List<int>(30);
                            while (reader.IsStartElement("id"))
                                list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg0 = list0.ToArray();
                            var list1 = new List<ulong>(30);
                            while (reader.IsStartElement("type"))
                                list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            ulong[] arg1 = list1.ToArray();
                            _handler.CreateAlias(arg0, arg1);
                            break;
                        }
                    case "MoveCardReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            int arg2 = int.Parse(reader.ReadElementString("idx"), CultureInfo.InvariantCulture);
                            bool arg3 = bool.Parse(reader.ReadElementString("faceUp"));
                            _handler.MoveCardReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "MoveCardAtReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("x"), CultureInfo.InvariantCulture);
                            int arg2 = int.Parse(reader.ReadElementString("y"), CultureInfo.InvariantCulture);
                            int arg3 = int.Parse(reader.ReadElementString("idx"), CultureInfo.InvariantCulture);
                            bool arg4 = bool.Parse(reader.ReadElementString("faceUp"));
                            _handler.MoveCardAtReq(arg0, arg1, arg2, arg3, arg4);
                            break;
                        }
                    case "Reveal":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            ulong arg1 = ulong.Parse(reader.ReadElementString("revealed"), CultureInfo.InvariantCulture);
                            var arg2 = new Guid(reader.ReadElementString("guid"));
                            _handler.Reveal(arg0, arg1, arg2);
                            break;
                        }
                    case "RevealToReq":
                        {
                            byte arg0 = byte.Parse(reader.ReadElementString("sendTo"), CultureInfo.InvariantCulture);
                            var list1 = new List<byte>(8);
                            while (reader.IsStartElement("revealTo"))
                                list1.Add(byte.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            byte[] arg1 = list1.ToArray();
                            int arg2 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            var list3 = new List<ulong>(30);
                            while (reader.IsStartElement("encrypted"))
                                list3.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            ulong[] arg3 = list3.ToArray();
                            _handler.RevealToReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "PeekReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            _handler.PeekReq(arg0);
                            break;
                        }
                    case "UntargetReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            _handler.UntargetReq(arg0);
                            break;
                        }
                    case "TargetReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            _handler.TargetReq(arg0);
                            break;
                        }
                    case "TargetArrowReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("otherCard"), CultureInfo.InvariantCulture);
                            _handler.TargetArrowReq(arg0, arg1);
                            break;
                        }
                    case "Highlight":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            string arg1 = reader.ReadElementString("color");
                            _handler.Highlight(arg0, arg1);
                            break;
                        }
                    case "TurnReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            bool arg1 = bool.Parse(reader.ReadElementString("up"));
                            _handler.TurnReq(arg0, arg1);
                            break;
                        }
                    case "RotateReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            var arg1 =
                                (CardOrientation) Enum.Parse(typeof (CardOrientation), reader.ReadElementString("rot"));
                            _handler.RotateReq(arg0, arg1);
                            break;
                        }
                    case "Shuffle":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            var list1 = new List<int>(30);
                            while (reader.IsStartElement("card"))
                                list1.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg1 = list1.ToArray();
                            _handler.Shuffle(arg0, arg1);
                            break;
                        }
                    case "Shuffled":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            var list1 = new List<int>(30);
                            while (reader.IsStartElement("card"))
                                list1.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg1 = list1.ToArray();
                            var list2 = new List<short>(30);
                            while (reader.IsStartElement("pos"))
                                list2.Add(short.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            short[] arg2 = list2.ToArray();
                            _handler.Shuffled(arg0, arg1, arg2);
                            break;
                        }
                    case "UnaliasGrp":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            _handler.UnaliasGrp(arg0);
                            break;
                        }
                    case "Unalias":
                        {
                            var list0 = new List<int>(30);
                            while (reader.IsStartElement("card"))
                                list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            int[] arg0 = list0.ToArray();
                            var list1 = new List<ulong>(30);
                            while (reader.IsStartElement("type"))
                                list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                            ulong[] arg1 = list1.ToArray();
                            _handler.Unalias(arg0, arg1);
                            break;
                        }
                    case "AddMarkerReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            var arg1 = new Guid(reader.ReadElementString("id"));
                            string arg2 = reader.ReadElementString("name");
                            ushort arg3 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                            _handler.AddMarkerReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "RemoveMarkerReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            var arg1 = new Guid(reader.ReadElementString("id"));
                            string arg2 = reader.ReadElementString("name");
                            ushort arg3 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                            _handler.RemoveMarkerReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "SetMarkerReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture);
                            var arg1 = new Guid(reader.ReadElementString("id"));
                            string arg2 = reader.ReadElementString("name");
                            ushort arg3 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                            _handler.SetMarkerReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "TransferMarkerReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("from"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("to"), CultureInfo.InvariantCulture);
                            var arg2 = new Guid(reader.ReadElementString("id"));
                            string arg3 = reader.ReadElementString("name");
                            ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                            _handler.TransferMarkerReq(arg0, arg1, arg2, arg3, arg4);
                            break;
                        }
                    case "PassToReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                            byte arg1 = byte.Parse(reader.ReadElementString("to"), CultureInfo.InvariantCulture);
                            bool arg2 = bool.Parse(reader.ReadElementString("requested"));
                            _handler.PassToReq(arg0, arg1, arg2);
                            break;
                        }
                    case "TakeFromReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                            byte arg1 = byte.Parse(reader.ReadElementString("from"), CultureInfo.InvariantCulture);
                            _handler.TakeFromReq(arg0, arg1);
                            break;
                        }
                    case "DontTakeReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                            byte arg1 = byte.Parse(reader.ReadElementString("to"), CultureInfo.InvariantCulture);
                            _handler.DontTakeReq(arg0, arg1);
                            break;
                        }
                    case "FreezeCardsVisibility":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            _handler.FreezeCardsVisibility(arg0);
                            break;
                        }
                    case "GroupVisReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            bool arg1 = bool.Parse(reader.ReadElementString("defined"));
                            bool arg2 = bool.Parse(reader.ReadElementString("visible"));
                            _handler.GroupVisReq(arg0, arg1, arg2);
                            break;
                        }
                    case "GroupVisAddReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            byte arg1 = byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture);
                            _handler.GroupVisAddReq(arg0, arg1);
                            break;
                        }
                    case "GroupVisRemoveReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            byte arg1 = byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture);
                            _handler.GroupVisRemoveReq(arg0, arg1);
                            break;
                        }
                    case "LookAtReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            bool arg2 = bool.Parse(reader.ReadElementString("look"));
                            _handler.LookAtReq(arg0, arg1, arg2);
                            break;
                        }
                    case "LookAtTopReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            int arg2 = int.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                            bool arg3 = bool.Parse(reader.ReadElementString("look"));
                            _handler.LookAtTopReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "LookAtBottomReq":
                        {
                            int arg0 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                            int arg1 = int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture);
                            int arg2 = int.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                            bool arg3 = bool.Parse(reader.ReadElementString("look"));
                            _handler.LookAtBottomReq(arg0, arg1, arg2, arg3);
                            break;
                        }
                    case "StartLimitedReq":
                        {
                            var list0 = new List<Guid>(30);
                            while (reader.IsStartElement("packs"))
                                list0.Add(new Guid(reader.ReadElementString()));
                            Guid[] arg0 = list0.ToArray();
                            _handler.StartLimitedReq(arg0);
                            break;
                        }
                    case "CancelLimitedReq":
                        {
                            _handler.CancelLimitedReq();
                            break;
                        }
                    case "PlayerSetGlobalVariable":
                        {
                            byte p = byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture);
                            string n = reader.ReadElementString("name");
                            string v = reader.ReadElementString("value");
                            _handler.PlayerSetGlobalVariable(p, n, v);
                            break;
                        }
                    case "SetGlobalVariable":
                        {
                            string n = reader.ReadElementString("name");
                            string v = reader.ReadElementString("value");
                            _handler.SetGlobalVariable(n, v);
                            break;
                        }
                    default:
                        Debug.WriteLine("[Client Parser] Unknown message: " + method);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(Resource1.XmlParser_Parse_Parse_error_in_xmlparser__ + ex.StackTrace);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }
    }
}