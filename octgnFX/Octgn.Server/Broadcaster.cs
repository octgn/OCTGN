/*
 * This file was automatically generated.
 * Do not modify, changes will get lots when the file is regenerated!
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Octgn.Server
{
    internal sealed class Broadcaster : Octgn.Server.IClientCalls
    {
        private byte[] xmlData = new byte[1024], binData;
        private int xmlLength;
        private Dictionary<TcpClient, Handler.PlayerInfo> to;
        private XmlFormatter xml;
        private BinFormatter bin;
        private Handler handler;

        private sealed class XmlFormatter : BaseXmlStub
        {
            private Broadcaster bcast;

            internal XmlFormatter(Broadcaster bcast, Handler handler)
                : base(handler)
            { this.bcast = bcast; }

            protected override void Send(string xml)
            {
                bcast.xmlLength = Encoding.UTF8.GetByteCount(xml) + 1;
                if(bcast.xmlLength > bcast.xmlData.Length)
                    bcast.xmlData = new byte[bcast.xmlLength];
                Encoding.UTF8.GetBytes(xml, 0, xml.Length, bcast.xmlData, 0);
                bcast.xmlData[bcast.xmlLength - 1] = 0;
            }
        }

        private sealed class BinFormatter : BaseBinaryStub
        {
            private Broadcaster bcast;

            internal BinFormatter(Broadcaster bcast, Handler handler)
                : base(handler)
            { this.bcast = bcast; }

            protected override void Send(byte[] data)
            { bcast.binData = data; }
        }

        internal Broadcaster(Dictionary<TcpClient, Handler.PlayerInfo> to, Handler handler)
        {
            this.to = to; this.handler = handler;
            xml = new XmlFormatter(this, handler);
        }

        internal void RefreshTypes()
        {
            bool bBin = false, bXml = false;
            foreach(Handler.PlayerInfo pi in to.Values)
            {
                bBin |= pi.binary == true;
                bXml |= pi.binary == false;
            }
            if(bBin && bin == null) bin = new BinFormatter(this, handler);
            else if(!bBin && bin != null) bin = null;
            if(bXml && xml == null) xml = new XmlFormatter(this, handler);
            else if(!bXml && xml != null) xml = null;
        }

        internal void Send()
        {
            foreach(KeyValuePair<TcpClient, Handler.PlayerInfo> kvp in to)
                try
                {
                    Stream stream = kvp.Key.GetStream();
                    if(kvp.Value.binary)
                        stream.Write(binData, 0, binData.Length);
                    else
                        stream.Write(xmlData, 0, xmlLength);
                    stream.Flush();
                }
                catch
                {
                    // TODO notify disconnection
                    //					Program.server.Disconnected(kvp.Key);
                }
        }

        public void Ping()
        {
            if (xml != null)
                xml.Ping();
            if (bin != null)
                bin.Ping();
            Send();
        }

        public void Binary()
        {
            if(xml != null)
                xml.Binary();
            if(bin != null)
                bin.Binary();
            Send();
        }

        public void Error(string msg)
        {
            if(xml != null)
                xml.Error(msg);
            if(bin != null)
                bin.Error(msg);
            Send();
        }

        public void Welcome(byte id)
        {
            if(xml != null)
                xml.Welcome(id);
            if(bin != null)
                bin.Welcome(id);
            Send();
        }

        public void IsAlternateImage(int c, bool isaltertnate)
        {
            if(xml != null)
                xml.IsAlternateImage(c, isaltertnate);
            if(bin != null)
                bin.IsAlternateImage(c, isaltertnate);
            Send();
        }

        public void Settings(bool twoSidedTable)
        {
            if(xml != null)
                xml.Settings(twoSidedTable);
            if(bin != null)
                bin.Settings(twoSidedTable);
            Send();
        }

        public void PlayerSettings(byte playerId, bool invertedTable)
        {
            if(xml != null)
                xml.PlayerSettings(playerId, invertedTable);
            if(bin != null)
                bin.PlayerSettings(playerId, invertedTable);
            Send();
        }

        public void NewPlayer(byte id, string nick, ulong pkey)
        {
            if(xml != null)
                xml.NewPlayer(id, nick, pkey);
            if(bin != null)
                bin.NewPlayer(id, nick, pkey);
            Send();
        }

        public void Leave(byte player)
        {
            if(xml != null)
                xml.Leave(player);
            if(bin != null)
                bin.Leave(player);
            Send();
        }

        public void Nick(byte player, string nick)
        {
            if(xml != null)
                xml.Nick(player, nick);
            if(bin != null)
                bin.Nick(player, nick);
            Send();
        }

        public void Start()
        {
            if(xml != null)
                xml.Start();
            if(bin != null)
                bin.Start();
            Send();
        }

        public void Reset(byte player)
        {
            if(xml != null)
                xml.Reset(player);
            if(bin != null)
                bin.Reset(player);
            Send();
        }

        public void NextTurn(byte nextPlayer)
        {
            if(xml != null)
                xml.NextTurn(nextPlayer);
            if(bin != null)
                bin.NextTurn(nextPlayer);
            Send();
        }

        public void StopTurn(byte player)
        {
            if(xml != null)
                xml.StopTurn(player);
            if(bin != null)
                bin.StopTurn(player);
            Send();
        }

        public void Chat(byte player, string text)
        {
            if(xml != null)
                xml.Chat(player, text);
            if(bin != null)
                bin.Chat(player, text);
            Send();
        }

        public void Print(byte player, string text)
        {
            if(xml != null)
                xml.Print(player, text);
            if(bin != null)
                bin.Print(player, text);
            Send();
        }

        public void Random(byte player, int id, int min, int max)
        {
            if(xml != null)
                xml.Random(player, id, min, max);
            if(bin != null)
                bin.Random(player, id, min, max);
            Send();
        }

        public void RandomAnswer1(byte player, int id, ulong value)
        {
            if(xml != null)
                xml.RandomAnswer1(player, id, value);
            if(bin != null)
                bin.RandomAnswer1(player, id, value);
            Send();
        }

        public void RandomAnswer2(byte player, int id, ulong value)
        {
            if(xml != null)
                xml.RandomAnswer2(player, id, value);
            if(bin != null)
                bin.RandomAnswer2(player, id, value);
            Send();
        }

        public void Counter(byte player, int counter, int value)
        {
            if(xml != null)
                xml.Counter(player, counter, value);
            if(bin != null)
                bin.Counter(player, counter, value);
            Send();
        }

        public void LoadDeck(int[] id, ulong[] type, int[] group)
        {
            if(xml != null)
                xml.LoadDeck(id, type, group);
            if(bin != null)
                bin.LoadDeck(id, type, group);
            Send();
        }

        public void CreateCard(int[] id, ulong[] type, int group)
        {
            if(xml != null)
                xml.CreateCard(id, type, group);
            if(bin != null)
                bin.CreateCard(id, type, group);
            Send();
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            if(xml != null)
                xml.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
            if(bin != null)
                bin.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
            Send();
        }

        public void CreateAlias(int[] id, ulong[] type)
        {
            if(xml != null)
                xml.CreateAlias(id, type);
            if(bin != null)
                bin.CreateAlias(id, type);
            Send();
        }

        public void MoveCard(byte player, int card, int group, int idx, bool faceUp)
        {
            if(xml != null)
                xml.MoveCard(player, card, group, idx, faceUp);
            if(bin != null)
                bin.MoveCard(player, card, group, idx, faceUp);
            Send();
        }

        public void MoveCardAt(byte player, int card, int x, int y, int idx, bool faceUp)
        {
            if(xml != null)
                xml.MoveCardAt(player, card, x, y, idx, faceUp);
            if(bin != null)
                bin.MoveCardAt(player, card, x, y, idx, faceUp);
            Send();
        }

        public void Reveal(int card, ulong revealed, Guid guid)
        {
            if(xml != null)
                xml.Reveal(card, revealed, guid);
            if(bin != null)
                bin.Reveal(card, revealed, guid);
            Send();
        }

        public void RevealTo(byte[] players, int card, ulong[] encrypted)
        {
            if(xml != null)
                xml.RevealTo(players, card, encrypted);
            if(bin != null)
                bin.RevealTo(players, card, encrypted);
            Send();
        }

        public void Peek(byte player, int card)
        {
            if(xml != null)
                xml.Peek(player, card);
            if(bin != null)
                bin.Peek(player, card);
            Send();
        }

        public void Untarget(byte player, int card)
        {
            if(xml != null)
                xml.Untarget(player, card);
            if(bin != null)
                bin.Untarget(player, card);
            Send();
        }

        public void Target(byte player, int card)
        {
            if(xml != null)
                xml.Target(player, card);
            if(bin != null)
                bin.Target(player, card);
            Send();
        }

        public void TargetArrow(byte player, int card, int otherCard)
        {
            if(xml != null)
                xml.TargetArrow(player, card, otherCard);
            if(bin != null)
                bin.TargetArrow(player, card, otherCard);
            Send();
        }

        public void Highlight(int card, string color)
        {
            if(xml != null)
                xml.Highlight(card, color);
            if(bin != null)
                bin.Highlight(card, color);
            Send();
        }

        public void Turn(byte player, int card, bool up)
        {
            if(xml != null)
                xml.Turn(player, card, up);
            if(bin != null)
                bin.Turn(player, card, up);
            Send();
        }

        public void Rotate(byte player, int card, CardOrientation rot)
        {
            if(xml != null)
                xml.Rotate(player, card, rot);
            if(bin != null)
                bin.Rotate(player, card, rot);
            Send();
        }

        public void Shuffle(int group, int[] card)
        {
            if(xml != null)
                xml.Shuffle(group, card);
            if(bin != null)
                bin.Shuffle(group, card);
            Send();
        }

        public void Shuffled(int group, int[] card, short[] pos)
        {
            if(xml != null)
                xml.Shuffled(group, card, pos);
            if(bin != null)
                bin.Shuffled(group, card, pos);
            Send();
        }

        public void UnaliasGrp(int group)
        {
            if(xml != null)
                xml.UnaliasGrp(group);
            if(bin != null)
                bin.UnaliasGrp(group);
            Send();
        }

        public void Unalias(int[] card, ulong[] type)
        {
            if(xml != null)
                xml.Unalias(card, type);
            if(bin != null)
                bin.Unalias(card, type);
            Send();
        }

        public void AddMarker(byte player, int card, Guid id, string name, ushort count)
        {
            if(xml != null)
                xml.AddMarker(player, card, id, name, count);
            if(bin != null)
                bin.AddMarker(player, card, id, name, count);
            Send();
        }

        public void RemoveMarker(byte player, int card, Guid id, string name, ushort count)
        {
            if(xml != null)
                xml.RemoveMarker(player, card, id, name, count);
            if(bin != null)
                bin.RemoveMarker(player, card, id, name, count);
            Send();
        }

        public void SetMarker(byte player, int card, Guid id, string name, ushort count)
        {
            if(xml != null)
                xml.SetMarker(player, card, id, name, count);
            if(bin != null)
                bin.SetMarker(player, card, id, name, count);
            Send();
        }

        public void TransferMarker(byte player, int from, int to, Guid id, string name, ushort count)
        {
            if(xml != null)
                xml.TransferMarker(player, from, to, id, name, count);
            if(bin != null)
                bin.TransferMarker(player, from, to, id, name, count);
            Send();
        }

        public void PassTo(byte player, int id, byte to, bool requested)
        {
            if(xml != null)
                xml.PassTo(player, id, to, requested);
            if(bin != null)
                bin.PassTo(player, id, to, requested);
            Send();
        }

        public void TakeFrom(int id, byte to)
        {
            if(xml != null)
                xml.TakeFrom(id, to);
            if(bin != null)
                bin.TakeFrom(id, to);
            Send();
        }

        public void DontTake(int id)
        {
            if(xml != null)
                xml.DontTake(id);
            if(bin != null)
                bin.DontTake(id);
            Send();
        }

        public void FreezeCardsVisibility(int group)
        {
            if(xml != null)
                xml.FreezeCardsVisibility(group);
            if(bin != null)
                bin.FreezeCardsVisibility(group);
            Send();
        }

        public void GroupVis(byte player, int group, bool defined, bool visible)
        {
            if(xml != null)
                xml.GroupVis(player, group, defined, visible);
            if(bin != null)
                bin.GroupVis(player, group, defined, visible);
            Send();
        }

        public void GroupVisAdd(byte player, int group, byte who)
        {
            if(xml != null)
                xml.GroupVisAdd(player, group, who);
            if(bin != null)
                bin.GroupVisAdd(player, group, who);
            Send();
        }

        public void GroupVisRemove(byte player, int group, byte who)
        {
            if(xml != null)
                xml.GroupVisRemove(player, group, who);
            if(bin != null)
                bin.GroupVisRemove(player, group, who);
            Send();
        }

        public void LookAt(byte player, int uid, int group, bool look)
        {
            if(xml != null)
                xml.LookAt(player, uid, group, look);
            if(bin != null)
                bin.LookAt(player, uid, group, look);
            Send();
        }

        public void LookAtTop(byte player, int uid, int group, int count, bool look)
        {
            if(xml != null)
                xml.LookAtTop(player, uid, group, count, look);
            if(bin != null)
                bin.LookAtTop(player, uid, group, count, look);
            Send();
        }

        public void LookAtBottom(byte player, int uid, int group, int count, bool look)
        {
            if(xml != null)
                xml.LookAtBottom(player, uid, group, count, look);
            if(bin != null)
                bin.LookAtBottom(player, uid, group, count, look);
            Send();
        }

        public void StartLimited(byte player, Guid[] packs)
        {
            if(xml != null)
                xml.StartLimited(player, packs);
            if(bin != null)
                bin.StartLimited(player, packs);
            Send();
        }

        public void CancelLimited(byte player)
        {
            if(xml != null)
                xml.CancelLimited(player);
            if(bin != null)
                bin.CancelLimited(player);
            Send();
        }
        public void PlayerSetGlobalVariable(byte from, byte p, string n, string v)
        {
            if (xml != null)
                xml.PlayerSetGlobalVariable(from,p,n,v);
            if (bin != null)
                bin.PlayerSetGlobalVariable(from, p, n, v);
            Send();
        }
        public void SetGlobalVariable( string n, string v)
        {
            if (xml != null)
                xml.SetGlobalVariable( n, v);
            if (bin != null)
                bin.SetGlobalVariable( n, v);
            Send();
        }
    }
}