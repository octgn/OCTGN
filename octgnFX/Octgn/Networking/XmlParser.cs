using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using System.Xml;
using Octgn.Play;

namespace Octgn.Networking
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
            var sr = new StringReader(xml);
            XmlReader reader = XmlReader.Create(sr);
            reader.Read();
            string method = reader.Name;
            string muted = reader.GetAttribute("muted");
            Program.Client.Muted = !string.IsNullOrEmpty(muted) ? int.Parse(muted) : 0;
            reader.ReadStartElement(); // <method>
            switch (method)
            {
                case "Binary":
                    {
                        _handler.Binary();
                        break;
                    }
                case "SwitchWithAlternate":
                    {
                        int arg0 = int.Parse(reader.ReadElementString("cardid"));
                        Card c = Card.Find(arg0);
                        _handler.SwitchWithAlternate(c);
                        break;
                    }
                case "IsAlternateImage":
                    {
                        int arg0 = int.Parse(reader.ReadElementString("cardid"));
                        bool arg1 = bool.Parse(reader.ReadElementString("isalternateimage"));
                        Card c = Card.Find(arg0);
                        _handler.IsAlternateImage(c, arg1);
                        break;
                    }

                case "Error":
                    {
                        string arg0 = reader.ReadElementString("msg");
                        _handler.Error(arg0);
                        break;
                    }
                case "Welcome":
                    {
                        byte arg0 = byte.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        _handler.Welcome(arg0);
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
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("playerId"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[PlayerSettings] Player not found.");
                            return;
                        }
                        bool arg1 = bool.Parse(reader.ReadElementString("invertedTable"));
                        _handler.PlayerSettings(arg0, arg1);
                        break;
                    }
                case "NewPlayer":
                    {
                        byte arg0 = byte.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        string arg1 = reader.ReadElementString("nick");
                        ulong arg2 = ulong.Parse(reader.ReadElementString("pkey"), CultureInfo.InvariantCulture);
                        _handler.NewPlayer(arg0, arg1, arg2);
                        break;
                    }
                case "Leave":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Leave] Player not found.");
                            return;
                        }
                        _handler.Leave(arg0);
                        break;
                    }
                case "Nick":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Nick] Player not found.");
                            return;
                        }
                        string arg1 = reader.ReadElementString("nick");
                        _handler.Nick(arg0, arg1);
                        break;
                    }
                case "Start":
                    {
                        _handler.Start();
                        break;
                    }
                case "Reset":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Reset] Player not found.");
                            return;
                        }
                        _handler.Reset(arg0);
                        break;
                    }
                case "NextTurn":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("nextPlayer"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[NextTurn] Player not found.");
                            return;
                        }
                        _handler.NextTurn(arg0);
                        break;
                    }
                case "StopTurn":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[StopTurn] Player not found.");
                            return;
                        }
                        _handler.StopTurn(arg0);
                        break;
                    }
                case "Chat":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Chat] Player not found.");
                            return;
                        }
                        string arg1 = reader.ReadElementString("text");
                        _handler.Chat(arg0, arg1);
                        break;
                    }
                case "Print":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Print] Player not found.");
                            return;
                        }
                        string arg1 = reader.ReadElementString("text");
                        _handler.Print(arg0, arg1);
                        break;
                    }
                case "Random":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Random] Player not found.");
                            return;
                        }
                        int arg1 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        int arg2 = int.Parse(reader.ReadElementString("min"), CultureInfo.InvariantCulture);
                        int arg3 = int.Parse(reader.ReadElementString("max"), CultureInfo.InvariantCulture);
                        _handler.Random(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "RandomAnswer1":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[RandomAnswer1] Player not found.");
                            return;
                        }
                        int arg1 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        ulong arg2 = ulong.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                        _handler.RandomAnswer1(arg0, arg1, arg2);
                        break;
                    }
                case "RandomAnswer2":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[RandomAnswer2] Player not found.");
                            return;
                        }
                        int arg1 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        ulong arg2 = ulong.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                        _handler.RandomAnswer2(arg0, arg1, arg2);
                        break;
                    }
                case "Counter":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Counter] Player not found.");
                            return;
                        }
                        Counter arg1 =
                            Counter.Find(int.Parse(reader.ReadElementString("counter"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Counter] Counter not found.");
                            return;
                        }
                        int arg2 = int.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                        _handler.Counter(arg0, arg1, arg2);
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
                        var list2 = new List<Group>(30);
                        while (reader.IsStartElement("group"))
                        {
                            Group g =
                                Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                            if (g == null)
                            {
                                Debug.WriteLine("[LoadDeck] Group not found.");
                                continue;
                            }
                            list2.Add(g);
                        }
                        Group[] arg2 = list2.ToArray();
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
                        Group arg2 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[CreateCard] Group not found.");
                            return;
                        }
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
                case "MoveCard":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[MoveCard] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[MoveCard] Card not found."));
                            return;
                        }
                        Group arg2 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[MoveCard] Group not found.");
                            return;
                        }
                        int arg3 = int.Parse(reader.ReadElementString("idx"), CultureInfo.InvariantCulture);
                        bool arg4 = bool.Parse(reader.ReadElementString("faceUp"));
                        _handler.MoveCard(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "MoveCardAt":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[MoveCardAt] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[MoveCardAt] Card not found."));
                            return;
                        }
                        int arg2 = int.Parse(reader.ReadElementString("x"), CultureInfo.InvariantCulture);
                        int arg3 = int.Parse(reader.ReadElementString("y"), CultureInfo.InvariantCulture);
                        int arg4 = int.Parse(reader.ReadElementString("idx"), CultureInfo.InvariantCulture);
                        bool arg5 = bool.Parse(reader.ReadElementString("faceUp"));
                        _handler.MoveCardAt(arg0, arg1, arg2, arg3, arg4, arg5);
                        break;
                    }
                case "Reveal":
                    {
                        Card arg0 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine(string.Format("[Reveal] Card not found."));
                            return;
                        }
                        ulong arg1 = ulong.Parse(reader.ReadElementString("revealed"), CultureInfo.InvariantCulture);
                        var arg2 = new Guid(reader.ReadElementString("guid"));
                        _handler.Reveal(arg0, arg1, arg2);
                        break;
                    }
                case "RevealTo":
                    {
                        var list0 = new List<Player>(8);
                        while (reader.IsStartElement("players"))
                        {
                            Player p =
                                Player.Find(byte.Parse(reader.ReadElementString("players"), CultureInfo.InvariantCulture));
                            if (p == null)
                            {
                                Debug.WriteLine("[RevealTo] Player not found.");
                                continue;
                            }
                            list0.Add(p);
                        }
                        Player[] arg0 = list0.ToArray();
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[RevealTo] Card not found."));
                            return;
                        }
                        var list2 = new List<ulong>(30);
                        while (reader.IsStartElement("encrypted"))
                            list2.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg2 = list2.ToArray();
                        _handler.RevealTo(arg0, arg1, arg2);
                        break;
                    }
                case "Peek":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Peek] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[Peek] Card not found."));
                            return;
                        }
                        _handler.Peek(arg0, arg1);
                        break;
                    }
                case "Untarget":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Untarget] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[Untarget] Card not found."));
                            return;
                        }
                        _handler.Untarget(arg0, arg1);
                        break;
                    }
                case "Target":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Target] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[Target] Card not found."));
                            return;
                        }
                        _handler.Target(arg0, arg1);
                        break;
                    }
                case "TargetArrow":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[TargetArrow] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[TargetArrow] Card not found."));
                            return;
                        }
                        Card arg2 =
                            Card.Find(int.Parse(reader.ReadElementString("otherCard"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine(string.Format("[TargetArrow] Card not found."));
                            return;
                        }
                        _handler.TargetArrow(arg0, arg1, arg2);
                        break;
                    }
                case "Highlight":
                    {
                        Card arg0 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine(string.Format("[Highlight] Card not found."));
                            return;
                        }
                        string temp1 = reader.ReadElementString("color");
                        Color? arg1 = temp1 == "" ? null : (Color?) ColorConverter.ConvertFromString(temp1);
                        _handler.Highlight(arg0, arg1);
                        break;
                    }
                case "Turn":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Turn] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[Turn] Card not found."));
                            return;
                        }
                        bool arg2 = bool.Parse(reader.ReadElementString("up"));
                        _handler.Turn(arg0, arg1, arg2);
                        break;
                    }
                case "Rotate":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Rotate] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[Rotate] Card not found."));
                            return;
                        }
                        var arg2 =
                            (CardOrientation) Enum.Parse(typeof (CardOrientation), reader.ReadElementString("rot"));
                        _handler.Rotate(arg0, arg1, arg2);
                        break;
                    }
                case "Shuffle":
                    {
                        Group arg0 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Shuffle] Group not found.");
                            return;
                        }
                        var list1 = new List<int>(30);
                        while (reader.IsStartElement("card"))
                            list1.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg1 = list1.ToArray();
                        _handler.Shuffle(arg0, arg1);
                        break;
                    }
                case "Shuffled":
                    {
                        Group arg0 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Shuffled] Group not found.");
                            return;
                        }
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
                        Group arg0 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[UnaliasGrp] Group not found.");
                            return;
                        }
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
                case "AddMarker":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[AddMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[AddMarker] Card not found."));
                            return;
                        }
                        var arg2 = new Guid(reader.ReadElementString("id"));
                        string arg3 = reader.ReadElementString("name");
                        ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        _handler.AddMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "RemoveMarker":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[RemoveMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[RemoveMarker] Card not found."));
                            return;
                        }
                        var arg2 = new Guid(reader.ReadElementString("id"));
                        string arg3 = reader.ReadElementString("name");
                        ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        _handler.RemoveMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "SetMarker":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[SetMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[SetMarker] Card not found."));
                            return;
                        }
                        var arg2 = new Guid(reader.ReadElementString("id"));
                        string arg3 = reader.ReadElementString("name");
                        ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        _handler.SetMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "TransferMarker":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[TransferMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("from"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine(string.Format("[TransferMarker] Card not found."));
                            return;
                        }
                        Card arg2 = Card.Find(int.Parse(reader.ReadElementString("lTo"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine(string.Format("[TransferMarker] Card not found."));
                            return;
                        }
                        var arg3 = new Guid(reader.ReadElementString("id"));
                        string arg4 = reader.ReadElementString("name");
                        ushort arg5 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        _handler.TransferMarker(arg0, arg1, arg2, arg3, arg4, arg5);
                        break;
                    }
                case "PassTo":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[PassTo] Player not found.");
                            return;
                        }
                        ControllableObject arg1 =
                            ControllableObject.Find(int.Parse(reader.ReadElementString("id"),
                                                              CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[PassTo] ControllableObject not found.");
                            return;
                        }
                        Player arg2 =
                            Player.Find(byte.Parse(reader.ReadElementString("lTo"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[PassTo] Player not found.");
                            return;
                        }
                        bool arg3 = bool.Parse(reader.ReadElementString("requested"));
                        _handler.PassTo(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "TakeFrom":
                    {
                        ControllableObject arg0 =
                            ControllableObject.Find(int.Parse(reader.ReadElementString("id"),
                                                              CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[TakeFrom] ControllableObject not found.");
                            return;
                        }
                        Player arg1 =//Fixed bug:  Message=Element 'to' was not found. Line 1, position 26.
                            Player.Find(byte.Parse(reader.ReadElementString("lTo"), CultureInfo.InvariantCulture)); 
                        //Why are we running byte.Parse on a long?
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[TakeFrom] Player not found.");
                            return;
                        }
                        _handler.TakeFrom(arg0, arg1);
                        break;
                    }
                case "DontTake":
                    {
                        ControllableObject arg0 =
                            ControllableObject.Find(int.Parse(reader.ReadElementString("id"),
                                                              CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[DontTake] ControllableObject not found.");
                            return;
                        }
                        _handler.DontTake(arg0);
                        break;
                    }
                case "FreezeCardsVisibility":
                    {
                        Group arg0 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[FreezeCardsVisibility] Group not found.");
                            return;
                        }
                        _handler.FreezeCardsVisibility(arg0);
                        break;
                    }
                case "GroupVis":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[GroupVis] Player not found.");
                            return;
                        }
                        Group arg1 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[GroupVis] Group not found.");
                            return;
                        }
                        bool arg2 = bool.Parse(reader.ReadElementString("defined"));
                        bool arg3 = bool.Parse(reader.ReadElementString("visible"));
                        _handler.GroupVis(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "GroupVisAdd":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[GroupVisAdd] Player not found.");
                            return;
                        }
                        Group arg1 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[GroupVisAdd] Group not found.");
                            return;
                        }
                        Player arg2 =
                            Player.Find(byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[GroupVisAdd] Player not found.");
                            return;
                        }
                        _handler.GroupVisAdd(arg0, arg1, arg2);
                        break;
                    }
                case "GroupVisRemove":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[GroupVisRemove] Player not found.");
                            return;
                        }
                        Group arg1 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[GroupVisRemove] Group not found.");
                            return;
                        }
                        Player arg2 =
                            Player.Find(byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[GroupVisRemove] Player not found.");
                            return;
                        }
                        _handler.GroupVisRemove(arg0, arg1, arg2);
                        break;
                    }
                case "LookAt":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[LookAt] Player not found.");
                            return;
                        }
                        int arg1 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                        Group arg2 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[LookAt] Group not found.");
                            return;
                        }
                        bool arg3 = bool.Parse(reader.ReadElementString("look"));
                        _handler.LookAt(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "LookAtTop":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[LookAtTop] Player not found.");
                            return;
                        }
                        int arg1 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                        Group arg2 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[LookAtTop] Group not found.");
                            return;
                        }
                        int arg3 = int.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        bool arg4 = bool.Parse(reader.ReadElementString("look"));
                        _handler.LookAtTop(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "LookAtBottom":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[LookAtBottom] Player not found.");
                            return;
                        }
                        int arg1 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                        Group arg2 =
                            Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[LookAtBottom] Group not found.");
                            return;
                        }
                        int arg3 = int.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        bool arg4 = bool.Parse(reader.ReadElementString("look"));
                        _handler.LookAtBottom(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "StartLimited":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[StartLimited] Player not found.");
                            return;
                        }
                        var list1 = new List<Guid>(30);
                        while (reader.IsStartElement("packs"))
                            list1.Add(new Guid(reader.ReadElementString()));
                        Guid[] arg1 = list1.ToArray();
                        _handler.StartLimited(arg0, arg1);
                        break;
                    }
                case "CancelLimited":
                    {
                        Player arg0 =
                            Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[CancelLimited] Player not found.");
                            return;
                        }
                        _handler.CancelLimited(arg0);
                        break;
                    }
                case "PlayerSetGlobalVariable":
                    {
                        Player f =
                            Player.Find(byte.Parse(reader.ReadElementString("from"), CultureInfo.InvariantCulture));
                        if (f == null)
                        {
                            Debug.WriteLine("[PlayerSetGlobalVariable] From Player not found.");
                            return;
                        }
                        Player p = Player.Find(byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture));
                        if (p == null)
                        {
                            Debug.WriteLine("[PlayerSetGlobalVariable] Player not found.");
                            return;
                        }
                        string n = reader.ReadElementString("name");
                        string v = reader.ReadElementString("value");
                        _handler.PlayerSetGlobalVariable(f, p, n, v);
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
    }
}