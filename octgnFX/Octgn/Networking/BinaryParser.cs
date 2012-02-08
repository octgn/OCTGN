﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using Octgn.Play;

namespace Octgn.Networking
{
    internal sealed class BinaryParser
    {
        private readonly Handler _handler;

        public BinaryParser(Handler handler)
        {
            _handler = handler;
        }

        public void Parse(byte[] data)
        {
            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);
            short length;
            Program.Client.Muted = reader.ReadInt32();
            byte method = reader.ReadByte();
            switch (method)
            {
                case 0:
                    {
                        _handler.Binary();
                        break;
                    }
                case 1:
                    {
                        string arg0 = reader.ReadString();
                        _handler.Error(arg0);
                        break;
                    }
                case 3:
                    {
                        byte arg0 = reader.ReadByte();
                        _handler.Welcome(arg0);
                        break;
                    }
                case 4:
                    {
                        bool arg0 = reader.ReadBoolean();
                        _handler.Settings(arg0);
                        break;
                    }
                case 5:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[PlayerSettings] Player not found.");
                            return;
                        }
                        bool arg1 = reader.ReadBoolean();
                        _handler.PlayerSettings(arg0, arg1);
                        break;
                    }
                case 6:
                    {
                        byte arg0 = reader.ReadByte();
                        string arg1 = reader.ReadString();
                        ulong arg2 = reader.ReadUInt64();
                        _handler.NewPlayer(arg0, arg1, arg2);
                        break;
                    }
                case 7:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Leave] Player not found.");
                            return;
                        }
                        _handler.Leave(arg0);
                        break;
                    }
                case 9:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Nick] Player not found.");
                            return;
                        }
                        string arg1 = reader.ReadString();
                        _handler.Nick(arg0, arg1);
                        break;
                    }
                case 10:
                    {
                        _handler.Start();
                        break;
                    }
                case 12:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Reset] Player not found.");
                            return;
                        }
                        _handler.Reset(arg0);
                        break;
                    }
                case 13:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[NextTurn] Player not found.");
                            return;
                        }
                        _handler.NextTurn(arg0);
                        break;
                    }
                case 15:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[StopTurn] Player not found.");
                            return;
                        }
                        _handler.StopTurn(arg0);
                        break;
                    }
                case 17:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Chat] Player not found.");
                            return;
                        }
                        string arg1 = reader.ReadString();
                        _handler.Chat(arg0, arg1);
                        break;
                    }
                case 19:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Print] Player not found.");
                            return;
                        }
                        string arg1 = reader.ReadString();
                        _handler.Print(arg0, arg1);
                        break;
                    }
                case 21:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Random] Player not found.");
                            return;
                        }
                        int arg1 = reader.ReadInt32();
                        int arg2 = reader.ReadInt32();
                        int arg3 = reader.ReadInt32();
                        _handler.Random(arg0, arg1, arg2, arg3);
                        break;
                    }
                case 23:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[RandomAnswer1] Player not found.");
                            return;
                        }
                        int arg1 = reader.ReadInt32();
                        ulong arg2 = reader.ReadUInt64();
                        _handler.RandomAnswer1(arg0, arg1, arg2);
                        break;
                    }
                case 25:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[RandomAnswer2] Player not found.");
                            return;
                        }
                        int arg1 = reader.ReadInt32();
                        ulong arg2 = reader.ReadUInt64();
                        _handler.RandomAnswer2(arg0, arg1, arg2);
                        break;
                    }
                case 27:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Counter] Player not found.");
                            return;
                        }
                        Counter arg1 = Counter.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Counter] Counter not found.");
                            return;
                        }
                        int arg2 = reader.ReadInt32();
                        _handler.Counter(arg0, arg1, arg2);
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
                        var arg2 = new Group[length];
                        for (int i = 0; i < length; ++i)
                        {
                            arg2[i] = Group.Find(reader.ReadInt32());
                            if (arg2[i] == null)
                                Debug.WriteLine("[LoadDeck] Group not found.");
                        }
                        _handler.LoadDeck(arg0, arg1, arg2);
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
                        Group arg2 = Group.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[CreateCard] Group not found.");
                            return;
                        }
                        _handler.CreateCard(arg0, arg1, arg2);
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
                        _handler.CreateCardAt(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
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
                        _handler.CreateAlias(arg0, arg1);
                        break;
                    }
                case 33:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[MoveCard] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[MoveCard] Card not found.");
                            return;
                        }
                        Group arg2 = Group.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[MoveCard] Group not found.");
                            return;
                        }
                        int arg3 = reader.ReadInt32();
                        bool arg4 = reader.ReadBoolean();
                        _handler.MoveCard(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case 35:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[MoveCardAt] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[MoveCardAt] Card not found.");
                            return;
                        }
                        int arg2 = reader.ReadInt32();
                        int arg3 = reader.ReadInt32();
                        int arg4 = reader.ReadInt32();
                        bool arg5 = reader.ReadBoolean();
                        _handler.MoveCardAt(arg0, arg1, arg2, arg3, arg4, arg5);
                        break;
                    }
                case 36:
                    {
                        Card arg0 = Card.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Reveal] Card not found.");
                            return;
                        }
                        ulong arg1 = reader.ReadUInt64();
                        var arg2 = new Guid(reader.ReadBytes(16));
                        _handler.Reveal(arg0, arg1, arg2);
                        break;
                    }
                case 38:
                    {
                        length = reader.ReadInt16();
                        var arg0 = new Player[length];
                        for (int i = 0; i < length; ++i)
                        {
                            arg0[i] = Player.Find(reader.ReadByte());
                            if (arg0[i] == null)
                                Debug.WriteLine("[RevealTo] Player not found.");
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[RevealTo] Card not found.");
                            return;
                        }
                        length = reader.ReadInt16();
                        var arg2 = new ulong[length];
                        for (int i = 0; i < length; ++i)
                            arg2[i] = reader.ReadUInt64();
                        _handler.RevealTo(arg0, arg1, arg2);
                        break;
                    }
                case 40:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Peek] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Peek] Card not found.");
                            return;
                        }
                        _handler.Peek(arg0, arg1);
                        break;
                    }
                case 42:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Untarget] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Untarget] Card not found.");
                            return;
                        }
                        _handler.Untarget(arg0, arg1);
                        break;
                    }
                case 44:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Target] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Target] Card not found.");
                            return;
                        }
                        _handler.Target(arg0, arg1);
                        break;
                    }
                case 46:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[TargetArrow] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[TargetArrow] Card not found.");
                            return;
                        }
                        Card arg2 = Card.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[TargetArrow] Card not found.");
                            return;
                        }
                        _handler.TargetArrow(arg0, arg1, arg2);
                        break;
                    }
                case 47:
                    {
                        Card arg0 = Card.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Highlight] Card not found.");
                            return;
                        }
                        string temp1 = reader.ReadString();
                        Color? arg1 = temp1 == "" ? null : (Color?) ColorConverter.ConvertFromString(temp1);
                        _handler.Highlight(arg0, arg1);
                        break;
                    }
                case 49:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Turn] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Turn] Card not found.");
                            return;
                        }
                        bool arg2 = reader.ReadBoolean();
                        _handler.Turn(arg0, arg1, arg2);
                        break;
                    }
                case 51:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Rotate] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[Rotate] Card not found.");
                            return;
                        }
                        var arg2 = (CardOrientation) reader.ReadByte();
                        _handler.Rotate(arg0, arg1, arg2);
                        break;
                    }
                case 52:
                    {
                        Group arg0 = Group.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Shuffle] Group not found.");
                            return;
                        }
                        length = reader.ReadInt16();
                        var arg1 = new int[length];
                        for (int i = 0; i < length; ++i)
                            arg1[i] = reader.ReadInt32();
                        _handler.Shuffle(arg0, arg1);
                        break;
                    }
                case 53:
                    {
                        Group arg0 = Group.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[Shuffled] Group not found.");
                            return;
                        }
                        length = reader.ReadInt16();
                        var arg1 = new int[length];
                        for (int i = 0; i < length; ++i)
                            arg1[i] = reader.ReadInt32();
                        length = reader.ReadInt16();
                        var arg2 = new short[length];
                        for (int i = 0; i < length; ++i)
                            arg2[i] = reader.ReadInt16();
                        _handler.Shuffled(arg0, arg1, arg2);
                        break;
                    }
                case 54:
                    {
                        Group arg0 = Group.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[UnaliasGrp] Group not found.");
                            return;
                        }
                        _handler.UnaliasGrp(arg0);
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
                        _handler.Unalias(arg0, arg1);
                        break;
                    }
                case 57:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[AddMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[AddMarker] Card not found.");
                            return;
                        }
                        var arg2 = new Guid(reader.ReadBytes(16));
                        string arg3 = reader.ReadString();
                        ushort arg4 = reader.ReadUInt16();
                        _handler.AddMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case 59:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[RemoveMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[RemoveMarker] Card not found.");
                            return;
                        }
                        var arg2 = new Guid(reader.ReadBytes(16));
                        string arg3 = reader.ReadString();
                        ushort arg4 = reader.ReadUInt16();
                        _handler.RemoveMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case 61:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[SetMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[SetMarker] Card not found.");
                            return;
                        }
                        var arg2 = new Guid(reader.ReadBytes(16));
                        string arg3 = reader.ReadString();
                        ushort arg4 = reader.ReadUInt16();
                        _handler.SetMarker(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case 63:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[TransferMarker] Player not found.");
                            return;
                        }
                        Card arg1 = Card.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[TransferMarker] Card not found.");
                            return;
                        }
                        Card arg2 = Card.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[TransferMarker] Card not found.");
                            return;
                        }
                        var arg3 = new Guid(reader.ReadBytes(16));
                        string arg4 = reader.ReadString();
                        ushort arg5 = reader.ReadUInt16();
                        _handler.TransferMarker(arg0, arg1, arg2, arg3, arg4, arg5);
                        break;
                    }
                case 65:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[PassTo] Player not found.");
                            return;
                        }
                        ControllableObject arg1 = ControllableObject.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[PassTo] ControllableObject not found.");
                            return;
                        }
                        Player arg2 = Player.Find(reader.ReadByte());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[PassTo] Player not found.");
                            return;
                        }
                        bool arg3 = reader.ReadBoolean();
                        _handler.PassTo(arg0, arg1, arg2, arg3);
                        break;
                    }
                case 67:
                    {
                        ControllableObject arg0 = ControllableObject.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[TakeFrom] ControllableObject not found.");
                            return;
                        }
                        Player arg1 = Player.Find(reader.ReadByte());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[TakeFrom] Player not found.");
                            return;
                        }
                        _handler.TakeFrom(arg0, arg1);
                        break;
                    }
                case 69:
                    {
                        ControllableObject arg0 = ControllableObject.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[DontTake] ControllableObject not found.");
                            return;
                        }
                        _handler.DontTake(arg0);
                        break;
                    }
                case 70:
                    {
                        Group arg0 = Group.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[FreezeCardsVisibility] Group not found.");
                            return;
                        }
                        _handler.FreezeCardsVisibility(arg0);
                        break;
                    }
                case 72:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[GroupVis] Player not found.");
                            return;
                        }
                        Group arg1 = Group.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[GroupVis] Group not found.");
                            return;
                        }
                        bool arg2 = reader.ReadBoolean();
                        bool arg3 = reader.ReadBoolean();
                        _handler.GroupVis(arg0, arg1, arg2, arg3);
                        break;
                    }
                case 74:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[GroupVisAdd] Player not found.");
                            return;
                        }
                        Group arg1 = Group.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[GroupVisAdd] Group not found.");
                            return;
                        }
                        Player arg2 = Player.Find(reader.ReadByte());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[GroupVisAdd] Player not found.");
                            return;
                        }
                        _handler.GroupVisAdd(arg0, arg1, arg2);
                        break;
                    }
                case 76:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[GroupVisRemove] Player not found.");
                            return;
                        }
                        Group arg1 = Group.Find(reader.ReadInt32());
                        if (arg1 == null)
                        {
                            Debug.WriteLine("[GroupVisRemove] Group not found.");
                            return;
                        }
                        Player arg2 = Player.Find(reader.ReadByte());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[GroupVisRemove] Player not found.");
                            return;
                        }
                        _handler.GroupVisRemove(arg0, arg1, arg2);
                        break;
                    }
                case 78:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[LookAt] Player not found.");
                            return;
                        }
                        int arg1 = reader.ReadInt32();
                        Group arg2 = Group.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[LookAt] Group not found.");
                            return;
                        }
                        bool arg3 = reader.ReadBoolean();
                        _handler.LookAt(arg0, arg1, arg2, arg3);
                        break;
                    }
                case 80:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[LookAtTop] Player not found.");
                            return;
                        }
                        int arg1 = reader.ReadInt32();
                        Group arg2 = Group.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[LookAtTop] Group not found.");
                            return;
                        }
                        int arg3 = reader.ReadInt32();
                        bool arg4 = reader.ReadBoolean();
                        _handler.LookAtTop(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case 82:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[LookAtBottom] Player not found.");
                            return;
                        }
                        int arg1 = reader.ReadInt32();
                        Group arg2 = Group.Find(reader.ReadInt32());
                        if (arg2 == null)
                        {
                            Debug.WriteLine("[LookAtBottom] Group not found.");
                            return;
                        }
                        int arg3 = reader.ReadInt32();
                        bool arg4 = reader.ReadBoolean();
                        _handler.LookAtBottom(arg0, arg1, arg2, arg3, arg4);
                        break;
                    }
                case 84:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[StartLimited] Player not found.");
                            return;
                        }
                        length = reader.ReadInt16();
                        var arg1 = new Guid[length];
                        for (int i = 0; i < length; ++i)
                            arg1[i] = new Guid(reader.ReadBytes(16));
                        _handler.StartLimited(arg0, arg1);
                        break;
                    }
                case 86:
                    {
                        Player arg0 = Player.Find(reader.ReadByte());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[CancelLimited] Player not found.");
                            return;
                        }
                        _handler.CancelLimited(arg0);
                        break;
                    }
                case 87:
                    {
                        Card arg0 = Card.Find(reader.ReadInt32());
                        if (arg0 == null)
                        {
                            Debug.WriteLine("[AlternateImage] Card not found.");
                            return;
                        }
                        bool arg1 = reader.ReadBoolean();
                        _handler.IsAlternateImage(arg0, arg1);

                        break;
                    }
                case 88:
                    {
                        Player f = Player.Find(reader.ReadByte());
                        if (f == null)
                        {
                            Debug.WriteLine("[PlayerSetGlobalVariable] From Player not found.");
                            return;
                        }
                        Player p = Player.Find(reader.ReadByte());
                        if (p == null)
                        {
                            Debug.WriteLine("[PlayerSetGlobalVariable] Player not found.");
                            return;
                        }
                        String n = reader.ReadString();
                        String v = reader.ReadString();
                        _handler.PlayerSetGlobalVariable(f, p, n, v);
                        break;
                    }
                case 89:
                    {
                        String n = reader.ReadString();
                        String v = reader.ReadString();
                        _handler.SetGlobalVariable(n, v);
                        break;
                    }
                default:
                    Debug.WriteLine("[Client Parser] Unknown message (id =" + method + ")");
                    break;
            }
            reader.Close();
        }
    }
}