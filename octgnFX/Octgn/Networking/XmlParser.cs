/*
 * This file was automatically generated.
 * Do not modify, changes will get lost when the file is regenerated!
 */

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
        Handler handler;

        public XmlParser(Handler handler)
        { this.handler = handler; }

        public void Parse(string xml)
        {
            StringReader sr = new StringReader(xml);
            XmlReader reader = XmlReader.Create(sr);
            reader.Read();
            string method = reader.Name;
            string muted = reader.GetAttribute("muted");
            if (!string.IsNullOrEmpty(muted))
                Program.Client.Muted = int.Parse(muted);
            else
                Program.Client.Muted = 0;
            reader.ReadStartElement();	// <method>
            switch (method)
            {
                case "Binary":
                    {
                        handler.Binary();
                        break;
                    }
                case "IsAlternate":
                    {
                        int arg0 = int.Parse(reader.ReadElementString("cardid"));
                        bool arg1 = bool.Parse(reader.ReadElementString("isalternate"));
                        Card c = Card.Find(arg0);
                        handler.IsAlternate(c, arg1);
                        break;
                    }
                case "IsAlternateImage":
                    {
                        int arg0 = int.Parse(reader.ReadElementString("cardid"));
                        bool arg1 = bool.Parse(reader.ReadElementString("isalternateimage"));
                        Card c = Card.Find(arg0);
                        handler.IsAlternateImage(c, arg1);
                        break;
                    }

                case "Error":
                    {
                        string arg0 = reader.ReadElementString("msg");
                        handler.Error(arg0);
                        break;
                    }
                case "Welcome":
                    {
                        byte arg0 = byte.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        handler.Welcome(arg0);
                        break;
                    }
                case "Settings":
                    {
                        bool arg0 = bool.Parse(reader.ReadElementString("twoSidedTable"));
                        handler.Settings(arg0);
                        break;
                    }
                case "PlayerSettings":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("playerId"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[PlayerSettings] Player not found."); return; }
                        bool arg1 = bool.Parse(reader.ReadElementString("invertedTable"));
                        handler.PlayerSettings(arg0, arg1);
                        break;
                    }
                case "NewPlayer":
                    {
                        byte arg0 = byte.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        string arg1 = reader.ReadElementString("nick");
                        ulong arg2 = ulong.Parse(reader.ReadElementString("pkey"), CultureInfo.InvariantCulture);
                        handler.NewPlayer(arg0, arg1, arg2);
                        break;
                    }
                case "Leave":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Leave] Player not found."); return; }
                        handler.Leave(arg0);
                        break;
                    }
                case "Nick":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Nick] Player not found."); return; }
                        string arg1 = reader.ReadElementString("nick");
                        handler.Nick(arg0, arg1);
                        break;
                    }
                case "Start":
                    {
                        handler.Start();
                        break;
                    }
                case "Reset":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Reset] Player not found."); return; }
                        handler.Reset(arg0);
                        break;
                    }
                case "NextTurn":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("nextPlayer"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[NextTurn] Player not found."); return; }
                        handler.NextTurn(arg0);
                        break;
                    }
                case "StopTurn":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[StopTurn] Player not found."); return; }
                        handler.StopTurn(arg0);
                        break;
                    }
                case "Chat":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Chat] Player not found."); return; }
                        string arg1 = reader.ReadElementString("text");
                        handler.Chat(arg0, arg1);
                        break;
                    }
                case "Print":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Print] Player not found."); return; }
                        string arg1 = reader.ReadElementString("text");
                        handler.Print(arg0, arg1);
                        break;
                    }
                case "Random":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Random] Player not found."); return; }
                        int arg1 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        int arg2 = int.Parse(reader.ReadElementString("min"), CultureInfo.InvariantCulture);
                        int arg3 = int.Parse(reader.ReadElementString("max"), CultureInfo.InvariantCulture);
                        handler.Random(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "RandomAnswer1":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[RandomAnswer1] Player not found."); return; }
                        int arg1 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        ulong arg2 = ulong.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                        handler.RandomAnswer1(arg0, arg1, arg2);
                        break;
                    }
                case "RandomAnswer2":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[RandomAnswer2] Player not found."); return; }
                        int arg1 = int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture);
                        ulong arg2 = ulong.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                        handler.RandomAnswer2(arg0, arg1, arg2);
                        break;
                    }
                case "Counter":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Counter] Player not found."); return; }
                        Counter arg1 = Counter.Find(int.Parse(reader.ReadElementString("counter"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine("[Counter] Counter not found."); return; }
                        int arg2 = int.Parse(reader.ReadElementString("value"), CultureInfo.InvariantCulture);
                        handler.Counter(arg0, arg1, arg2);
                        break;
                    }
                case "LoadDeck":
                    {
                        List<int> list0 = new List<int>(30);
                        while (reader.IsStartElement("id"))
                            list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg0 = list0.ToArray();
                        List<ulong> list1 = new List<ulong>(30);
                        while (reader.IsStartElement("type"))
                            list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg1 = list1.ToArray();
                        List<Group> list2 = new List<Group>(30);
                        while (reader.IsStartElement("group"))
                        {
                            Group g = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                            if (g == null)
                            { Debug.WriteLine("[LoadDeck] Group not found."); continue; }
                            list2.Add(g);
                        }
                        Group[] arg2 = list2.ToArray();
                        handler.LoadDeck(arg0, arg1, arg2);
                        break;
                    }
                case "CreateCard":
                    {
                        List<int> list0 = new List<int>(30);
                        while (reader.IsStartElement("id"))
                            list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg0 = list0.ToArray();
                        List<ulong> list1 = new List<ulong>(30);
                        while (reader.IsStartElement("type"))
                            list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg1 = list1.ToArray();
                        Group arg2 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[CreateCard] Group not found."); return; }
                        handler.CreateCard(arg0, arg1, arg2);
                        break;
                    }
                case "CreateCardAt":
                    {
                        List<int> list0 = new List<int>(30);
                        while (reader.IsStartElement("id"))
                            list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg0 = list0.ToArray();
                        List<ulong> list1 = new List<ulong>(30);
                        while (reader.IsStartElement("key"))
                            list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg1 = list1.ToArray();
                        List<Guid> list2 = new List<Guid>(30);
                        while (reader.IsStartElement("modelId"))
                            list2.Add(new Guid(reader.ReadElementString()));
                        Guid[] arg2 = list2.ToArray();
                        List<int> list3 = new List<int>(30);
                        while (reader.IsStartElement("x"))
                            list3.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg3 = list3.ToArray();
                        List<int> list4 = new List<int>(30);
                        while (reader.IsStartElement("y"))
                            list4.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg4 = list4.ToArray();
                        bool arg5 = bool.Parse(reader.ReadElementString("faceUp"));
                        bool arg6 = bool.Parse(reader.ReadElementString("persist"));
                        handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                        break;
                    }
                case "CreateAlias":
                    {
                        List<int> list0 = new List<int>(30);
                        while (reader.IsStartElement("id"))
                            list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg0 = list0.ToArray();
                        List<ulong> list1 = new List<ulong>(30);
                        while (reader.IsStartElement("type"))
                            list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg1 = list1.ToArray();
                        handler.CreateAlias(arg0, arg1);
                        break;
                    }
                case "MoveCard":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[MoveCard] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[MoveCard] Card not found.", method)); return; }
                        Group arg2 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[MoveCard] Group not found."); return; }
                        int arg3 = int.Parse(reader.ReadElementString("idx"), CultureInfo.InvariantCulture);
                        bool arg4 = bool.Parse(reader.ReadElementString("faceUp"));
                        handler.MoveCard(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "MoveCardAt":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[MoveCardAt] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[MoveCardAt] Card not found.", method)); return; }
                        int arg2 = int.Parse(reader.ReadElementString("x"), CultureInfo.InvariantCulture);
                        int arg3 = int.Parse(reader.ReadElementString("y"), CultureInfo.InvariantCulture);
                        int arg4 = int.Parse(reader.ReadElementString("idx"), CultureInfo.InvariantCulture);
                        bool arg5 = bool.Parse(reader.ReadElementString("faceUp"));
                        handler.MoveCardAt(arg0, arg1, arg2, arg3, arg4, arg5);
                        break;
                    }
                case "Reveal":
                    {
                        Card arg0 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine(string.Format("[Reveal] Card not found.", method)); return; }
                        ulong arg1 = ulong.Parse(reader.ReadElementString("revealed"), CultureInfo.InvariantCulture);
                        Guid arg2 = new Guid(reader.ReadElementString("guid"));
                        handler.Reveal(arg0, arg1, arg2);
                        break;
                    }
                case "RevealTo":
                    {
                        var list0 = new List<Player>(8);
                        while (reader.IsStartElement("players"))
                        {
                            var p = Player.Find(byte.Parse(reader.ReadElementString("players"), CultureInfo.InvariantCulture));
                            if (p == null)
                            { Debug.WriteLine("[RevealTo] Player not found."); continue; }
                            list0.Add(p);
                        }
                        Player[] arg0 = list0.ToArray();
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[RevealTo] Card not found.", method)); return; }
                        List<ulong> list2 = new List<ulong>(30);
                        while (reader.IsStartElement("encrypted"))
                            list2.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg2 = list2.ToArray();
                        handler.RevealTo(arg0, arg1, arg2);
                        break;
                    }
                case "Peek":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Peek] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[Peek] Card not found.", method)); return; }
                        handler.Peek(arg0, arg1);
                        break;
                    }
                case "Untarget":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Untarget] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[Untarget] Card not found.", method)); return; }
                        handler.Untarget(arg0, arg1);
                        break;
                    }
                case "Target":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Target] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[Target] Card not found.", method)); return; }
                        handler.Target(arg0, arg1);
                        break;
                    }
                case "TargetArrow":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[TargetArrow] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[TargetArrow] Card not found.", method)); return; }
                        Card arg2 = Card.Find(int.Parse(reader.ReadElementString("otherCard"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine(string.Format("[TargetArrow] Card not found.", method)); return; }
                        handler.TargetArrow(arg0, arg1, arg2);
                        break;
                    }
                case "Highlight":
                    {
                        Card arg0 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine(string.Format("[Highlight] Card not found.", method)); return; }
                        string temp1 = reader.ReadElementString("color");
                        Color? arg1 = temp1 == "" ? (Color?)null : (Color?)ColorConverter.ConvertFromString(temp1);
                        handler.Highlight(arg0, arg1);
                        break;
                    }
                case "Turn":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Turn] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[Turn] Card not found.", method)); return; }
                        bool arg2 = bool.Parse(reader.ReadElementString("up"));
                        handler.Turn(arg0, arg1, arg2);
                        break;
                    }
                case "Rotate":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Rotate] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[Rotate] Card not found.", method)); return; }
                        CardOrientation arg2 = (CardOrientation)Enum.Parse(typeof(CardOrientation), reader.ReadElementString("rot"));
                        handler.Rotate(arg0, arg1, arg2);
                        break;
                    }
                case "Shuffle":
                    {
                        Group arg0 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Shuffle] Group not found."); return; }
                        List<int> list1 = new List<int>(30);
                        while (reader.IsStartElement("card"))
                            list1.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg1 = list1.ToArray();
                        handler.Shuffle(arg0, arg1);
                        break;
                    }
                case "Shuffled":
                    {
                        Group arg0 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[Shuffled] Group not found."); return; }
                        List<int> list1 = new List<int>(30);
                        while (reader.IsStartElement("card"))
                            list1.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg1 = list1.ToArray();
                        List<short> list2 = new List<short>(30);
                        while (reader.IsStartElement("pos"))
                            list2.Add(short.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        short[] arg2 = list2.ToArray();
                        handler.Shuffled(arg0, arg1, arg2);
                        break;
                    }
                case "UnaliasGrp":
                    {
                        Group arg0 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[UnaliasGrp] Group not found."); return; }
                        handler.UnaliasGrp(arg0);
                        break;
                    }
                case "Unalias":
                    {
                        List<int> list0 = new List<int>(30);
                        while (reader.IsStartElement("card"))
                            list0.Add(int.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        int[] arg0 = list0.ToArray();
                        List<ulong> list1 = new List<ulong>(30);
                        while (reader.IsStartElement("type"))
                            list1.Add(ulong.Parse(reader.ReadElementString(), CultureInfo.InvariantCulture));
                        ulong[] arg1 = list1.ToArray();
                        handler.Unalias(arg0, arg1);
                        break;
                    }
                case "AddMarker":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[AddMarker] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[AddMarker] Card not found.", method)); return; }
                        Guid arg2 = new Guid(reader.ReadElementString("id"));
                        string arg3 = reader.ReadElementString("name");
                        ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        handler.AddMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "RemoveMarker":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[RemoveMarker] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[RemoveMarker] Card not found.", method)); return; }
                        Guid arg2 = new Guid(reader.ReadElementString("id"));
                        string arg3 = reader.ReadElementString("name");
                        ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        handler.RemoveMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "SetMarker":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[SetMarker] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("card"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[SetMarker] Card not found.", method)); return; }
                        Guid arg2 = new Guid(reader.ReadElementString("id"));
                        string arg3 = reader.ReadElementString("name");
                        ushort arg4 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        handler.SetMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "TransferMarker":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[TransferMarker] Player not found."); return; }
                        Card arg1 = Card.Find(int.Parse(reader.ReadElementString("from"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine(string.Format("[TransferMarker] Card not found.", method)); return; }
                        Card arg2 = Card.Find(int.Parse(reader.ReadElementString("to"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine(string.Format("[TransferMarker] Card not found.", method)); return; }
                        Guid arg3 = new Guid(reader.ReadElementString("id"));
                        string arg4 = reader.ReadElementString("name");
                        ushort arg5 = ushort.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        handler.TransferMarker(arg0, arg1, arg2, arg3, arg4, arg5);
                        break;
                    }
                case "PassTo":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[PassTo] Player not found."); return; }
                        ControllableObject arg1 = ControllableObject.Find(int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine("[PassTo] ControllableObject not found."); return; }
                        Player arg2 = Player.Find(byte.Parse(reader.ReadElementString("to"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[PassTo] Player not found."); return; }
                        bool arg3 = bool.Parse(reader.ReadElementString("requested"));
                        handler.PassTo(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "TakeFrom":
                    {
                        ControllableObject arg0 = ControllableObject.Find(int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[TakeFrom] ControllableObject not found."); return; }
                        Player arg1 = Player.Find(byte.Parse(reader.ReadElementString("to"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine("[TakeFrom] Player not found."); return; }
                        handler.TakeFrom(arg0, arg1);
                        break;
                    }
                case "DontTake":
                    {
                        ControllableObject arg0 = ControllableObject.Find(int.Parse(reader.ReadElementString("id"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[DontTake] ControllableObject not found."); return; }
                        handler.DontTake(arg0);
                        break;
                    }
                case "FreezeCardsVisibility":
                    {
                        Group arg0 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[FreezeCardsVisibility] Group not found."); return; }
                        handler.FreezeCardsVisibility(arg0);
                        break;
                    }
                case "GroupVis":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[GroupVis] Player not found."); return; }
                        Group arg1 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine("[GroupVis] Group not found."); return; }
                        bool arg2 = bool.Parse(reader.ReadElementString("defined"));
                        bool arg3 = bool.Parse(reader.ReadElementString("visible"));
                        handler.GroupVis(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "GroupVisAdd":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[GroupVisAdd] Player not found."); return; }
                        Group arg1 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine("[GroupVisAdd] Group not found."); return; }
                        Player arg2 = Player.Find(byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[GroupVisAdd] Player not found."); return; }
                        handler.GroupVisAdd(arg0, arg1, arg2);
                        break;
                    }
                case "GroupVisRemove":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[GroupVisRemove] Player not found."); return; }
                        Group arg1 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg1 == null)
                        { Debug.WriteLine("[GroupVisRemove] Group not found."); return; }
                        Player arg2 = Player.Find(byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[GroupVisRemove] Player not found."); return; }
                        handler.GroupVisRemove(arg0, arg1, arg2);
                        break;
                    }
                case "LookAt":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[LookAt] Player not found."); return; }
                        int arg1 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                        Group arg2 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[LookAt] Group not found."); return; }
                        bool arg3 = bool.Parse(reader.ReadElementString("look"));
                        handler.LookAt(arg0, arg1, arg2, arg3);
                        break;
                    }
                case "LookAtTop":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[LookAtTop] Player not found."); return; }
                        int arg1 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                        Group arg2 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[LookAtTop] Group not found."); return; }
                        int arg3 = int.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        bool arg4 = bool.Parse(reader.ReadElementString("look"));
                        handler.LookAtTop(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "LookAtBottom":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[LookAtBottom] Player not found."); return; }
                        int arg1 = int.Parse(reader.ReadElementString("uid"), CultureInfo.InvariantCulture);
                        Group arg2 = Group.Find(int.Parse(reader.ReadElementString("group"), CultureInfo.InvariantCulture));
                        if (arg2 == null)
                        { Debug.WriteLine("[LookAtBottom] Group not found."); return; }
                        int arg3 = int.Parse(reader.ReadElementString("count"), CultureInfo.InvariantCulture);
                        bool arg4 = bool.Parse(reader.ReadElementString("look"));
                        handler.LookAtBottom(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case "StartLimited":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[StartLimited] Player not found."); return; }
                        List<Guid> list1 = new List<Guid>(30);
                        while (reader.IsStartElement("packs"))
                            list1.Add(new Guid(reader.ReadElementString()));
                        Guid[] arg1 = list1.ToArray();
                        handler.StartLimited(arg0, arg1);
                        break;
                    }
                case "CancelLimited":
                    {
                        Player arg0 = Player.Find(byte.Parse(reader.ReadElementString("player"), CultureInfo.InvariantCulture));
                        if (arg0 == null)
                        { Debug.WriteLine("[CancelLimited] Player not found."); return; }
                        handler.CancelLimited(arg0);
                        break;
                    }
                case "PlayerSetGlobalVariable":
                    {
                        Player f = Player.Find(byte.Parse(reader.ReadElementString("from"), CultureInfo.InvariantCulture));
                        if (f == null)
                        { Debug.WriteLine("[PlayerSetGlobalVariable] From Player not found."); return; }
                        Player p = Player.Find(byte.Parse(reader.ReadElementString("who"), CultureInfo.InvariantCulture));
                        if (p == null)
                        { Debug.WriteLine("[PlayerSetGlobalVariable] Player not found."); return; }
                        string n = reader.ReadElementString("name");
                        string v = reader.ReadElementString("value");
                        handler.PlayerSetGlobalVariable(f, p, n, v);
                        break;
                    }
                case "SetGlobalVariable":
                    {
                        string n = reader.ReadElementString("name");
                        string v = reader.ReadElementString("value");
                        handler.SetGlobalVariable(n, v);
                        break;
                    }
                default:
                    Debug.WriteLine("[Client Parser] Unknown message: " + method);
                    break;
            }
        }
    }
}