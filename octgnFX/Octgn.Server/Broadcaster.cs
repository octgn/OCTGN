using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Octgn.Server
{
    internal sealed class Broadcaster : IClientCalls
    {
        private readonly Handler _handler;
        private readonly Dictionary<TcpClient, Handler.PlayerInfo> _to;
        private BinFormatter _bin;
        private byte[] _binData;
        private XmlFormatter _xml;
        private byte[] _xmlData = new byte[1024];
        private int _xmlLength;

        internal Broadcaster(Dictionary<TcpClient, Handler.PlayerInfo> to, Handler handler)
        {
            _to = to;
            _handler = handler;
            _xml = new XmlFormatter(this, handler);
        }

        #region IClientCalls Members

        public void Ping()
        {
            if (_xml != null)
                _xml.Ping();
            if (_bin != null)
                _bin.Ping();
            Send();
        }

        public void Binary()
        {
            if (_xml != null)
                _xml.Binary();
            if (_bin != null)
                _bin.Binary();
            Send();
        }

        public void Error(string msg)
        {
            if (_xml != null)
                _xml.Error(msg);
            if (_bin != null)
                _bin.Error(msg);
            Send();
        }

        public void Welcome(byte id)
        {
            if (_xml != null)
                _xml.Welcome(id);
            if (_bin != null)
                _bin.Welcome(id);
            Send();
        }

        public void Settings(bool twoSidedTable)
        {
            if (_xml != null)
                _xml.Settings(twoSidedTable);
            if (_bin != null)
                _bin.Settings(twoSidedTable);
            Send();
        }

        public void PlayerSettings(byte playerId, bool invertedTable)
        {
            if (_xml != null)
                _xml.PlayerSettings(playerId, invertedTable);
            if (_bin != null)
                _bin.PlayerSettings(playerId, invertedTable);
            Send();
        }

        public void NewPlayer(byte id, string nick, ulong pkey)
        {
            if (_xml != null)
                _xml.NewPlayer(id, nick, pkey);
            if (_bin != null)
                _bin.NewPlayer(id, nick, pkey);
            Send();
        }

        public void Leave(byte player)
        {
            if (_xml != null)
                _xml.Leave(player);
            if (_bin != null)
                _bin.Leave(player);
            Send();
        }

        public void Nick(byte player, string nick)
        {
            if (_xml != null)
                _xml.Nick(player, nick);
            if (_bin != null)
                _bin.Nick(player, nick);
            Send();
        }

        public void Start()
        {
            if (_xml != null)
                _xml.Start();
            if (_bin != null)
                _bin.Start();
            Send();
        }

        public void Reset(byte player)
        {
            if (_xml != null)
                _xml.Reset(player);
            if (_bin != null)
                _bin.Reset(player);
            Send();
        }

        public void NextTurn(byte nextPlayer)
        {
            if (_xml != null)
                _xml.NextTurn(nextPlayer);
            if (_bin != null)
                _bin.NextTurn(nextPlayer);
            Send();
        }

        public void StopTurn(byte player)
        {
            if (_xml != null)
                _xml.StopTurn(player);
            if (_bin != null)
                _bin.StopTurn(player);
            Send();
        }

        public void Chat(byte player, string text)
        {
            if (_xml != null)
                _xml.Chat(player, text);
            if (_bin != null)
                _bin.Chat(player, text);
            Send();
        }

        public void Print(byte player, string text)
        {
            if (_xml != null)
                _xml.Print(player, text);
            if (_bin != null)
                _bin.Print(player, text);
            Send();
        }

        public void Random(byte player, int id, int min, int max)
        {
            if (_xml != null)
                _xml.Random(player, id, min, max);
            if (_bin != null)
                _bin.Random(player, id, min, max);
            Send();
        }

        public void RandomAnswer1(byte player, int id, ulong value)
        {
            if (_xml != null)
                _xml.RandomAnswer1(player, id, value);
            if (_bin != null)
                _bin.RandomAnswer1(player, id, value);
            Send();
        }

        public void RandomAnswer2(byte player, int id, ulong value)
        {
            if (_xml != null)
                _xml.RandomAnswer2(player, id, value);
            if (_bin != null)
                _bin.RandomAnswer2(player, id, value);
            Send();
        }

        public void Counter(byte player, int counter, int value)
        {
            if (_xml != null)
                _xml.Counter(player, counter, value);
            if (_bin != null)
                _bin.Counter(player, counter, value);
            Send();
        }

        public void LoadDeck(int[] id, ulong[] type, int[] group)
        {
            if (_xml != null)
                _xml.LoadDeck(id, type, group);
            if (_bin != null)
                _bin.LoadDeck(id, type, group);
            Send();
        }

        public void CreateCard(int[] id, ulong[] type, int group)
        {
            if (_xml != null)
                _xml.CreateCard(id, type, group);
            if (_bin != null)
                _bin.CreateCard(id, type, group);
            Send();
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            if (_xml != null)
                _xml.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
            if (_bin != null)
                _bin.CreateCardAt(id, key, modelId, x, y, faceUp, persist);
            Send();
        }

        public void CreateAlias(int[] id, ulong[] type)
        {
            if (_xml != null)
                _xml.CreateAlias(id, type);
            if (_bin != null)
                _bin.CreateAlias(id, type);
            Send();
        }

        public void MoveCard(byte player, int card, int group, int idx, bool faceUp)
        {
            if (_xml != null)
                _xml.MoveCard(player, card, group, idx, faceUp);
            if (_bin != null)
                _bin.MoveCard(player, card, group, idx, faceUp);
            Send();
        }

        public void MoveCardAt(byte player, int card, int x, int y, int idx, bool faceUp)
        {
            if (_xml != null)
                _xml.MoveCardAt(player, card, x, y, idx, faceUp);
            if (_bin != null)
                _bin.MoveCardAt(player, card, x, y, idx, faceUp);
            Send();
        }

        public void Reveal(int card, ulong revealed, Guid guid)
        {
            if (_xml != null)
                _xml.Reveal(card, revealed, guid);
            if (_bin != null)
                _bin.Reveal(card, revealed, guid);
            Send();
        }

        public void RevealTo(byte[] players, int card, ulong[] encrypted)
        {
            if (_xml != null)
                _xml.RevealTo(players, card, encrypted);
            if (_bin != null)
                _bin.RevealTo(players, card, encrypted);
            Send();
        }

        public void Peek(byte player, int card)
        {
            if (_xml != null)
                _xml.Peek(player, card);
            if (_bin != null)
                _bin.Peek(player, card);
            Send();
        }

        public void Untarget(byte player, int card)
        {
            if (_xml != null)
                _xml.Untarget(player, card);
            if (_bin != null)
                _bin.Untarget(player, card);
            Send();
        }

        public void Target(byte player, int card)
        {
            if (_xml != null)
                _xml.Target(player, card);
            if (_bin != null)
                _bin.Target(player, card);
            Send();
        }

        public void TargetArrow(byte player, int card, int otherCard)
        {
            if (_xml != null)
                _xml.TargetArrow(player, card, otherCard);
            if (_bin != null)
                _bin.TargetArrow(player, card, otherCard);
            Send();
        }

        public void Highlight(int card, string color)
        {
            if (_xml != null)
                _xml.Highlight(card, color);
            if (_bin != null)
                _bin.Highlight(card, color);
            Send();
        }

        public void Turn(byte player, int card, bool up)
        {
            if (_xml != null)
                _xml.Turn(player, card, up);
            if (_bin != null)
                _bin.Turn(player, card, up);
            Send();
        }

        public void Rotate(byte player, int card, CardOrientation rot)
        {
            if (_xml != null)
                _xml.Rotate(player, card, rot);
            if (_bin != null)
                _bin.Rotate(player, card, rot);
            Send();
        }

        public void Shuffle(int group, int[] card)
        {
            if (_xml != null)
                _xml.Shuffle(group, card);
            if (_bin != null)
                _bin.Shuffle(group, card);
            Send();
        }

        public void Shuffled(int group, int[] card, short[] pos)
        {
            if (_xml != null)
                _xml.Shuffled(group, card, pos);
            if (_bin != null)
                _bin.Shuffled(group, card, pos);
            Send();
        }

        public void UnaliasGrp(int group)
        {
            if (_xml != null)
                _xml.UnaliasGrp(group);
            if (_bin != null)
                _bin.UnaliasGrp(group);
            Send();
        }

        public void Unalias(int[] card, ulong[] type)
        {
            if (_xml != null)
                _xml.Unalias(card, type);
            if (_bin != null)
                _bin.Unalias(card, type);
            Send();
        }

        public void AddMarker(byte player, int card, Guid id, string name, ushort count)
        {
            if (_xml != null)
                _xml.AddMarker(player, card, id, name, count);
            if (_bin != null)
                _bin.AddMarker(player, card, id, name, count);
            Send();
        }

        public void RemoveMarker(byte player, int card, Guid id, string name, ushort count)
        {
            if (_xml != null)
                _xml.RemoveMarker(player, card, id, name, count);
            if (_bin != null)
                _bin.RemoveMarker(player, card, id, name, count);
            Send();
        }

        public void SetMarker(byte player, int card, Guid id, string name, ushort count)
        {
            if (_xml != null)
                _xml.SetMarker(player, card, id, name, count);
            if (_bin != null)
                _bin.SetMarker(player, card, id, name, count);
            Send();
        }

        public void TransferMarker(byte player, int from, int lTo, Guid id, string name, ushort count)
        {
            if (_xml != null)
                _xml.TransferMarker(player, from, lTo, id, name, count);
            if (_bin != null)
                _bin.TransferMarker(player, from, lTo, id, name, count);
            Send();
        }

        public void PassTo(byte player, int id, byte lTo, bool requested)
        {
            if (_xml != null)
                _xml.PassTo(player, id, lTo, requested);
            if (_bin != null)
                _bin.PassTo(player, id, lTo, requested);
            Send();
        }

        public void TakeFrom(int id, byte lTo)
        {
            if (_xml != null)
                _xml.TakeFrom(id, lTo);
            if (_bin != null)
                _bin.TakeFrom(id, lTo);
            Send();
        }

        public void DontTake(int id)
        {
            if (_xml != null)
                _xml.DontTake(id);
            if (_bin != null)
                _bin.DontTake(id);
            Send();
        }

        public void FreezeCardsVisibility(int group)
        {
            if (_xml != null)
                _xml.FreezeCardsVisibility(group);
            if (_bin != null)
                _bin.FreezeCardsVisibility(group);
            Send();
        }

        public void GroupVis(byte player, int group, bool defined, bool visible)
        {
            if (_xml != null)
                _xml.GroupVis(player, group, defined, visible);
            if (_bin != null)
                _bin.GroupVis(player, group, defined, visible);
            Send();
        }

        public void GroupVisAdd(byte player, int group, byte who)
        {
            if (_xml != null)
                _xml.GroupVisAdd(player, group, who);
            if (_bin != null)
                _bin.GroupVisAdd(player, group, who);
            Send();
        }

        public void GroupVisRemove(byte player, int group, byte who)
        {
            if (_xml != null)
                _xml.GroupVisRemove(player, group, who);
            if (_bin != null)
                _bin.GroupVisRemove(player, group, who);
            Send();
        }

        public void LookAt(byte player, int uid, int group, bool look)
        {
            if (_xml != null)
                _xml.LookAt(player, uid, group, look);
            if (_bin != null)
                _bin.LookAt(player, uid, group, look);
            Send();
        }

        public void LookAtTop(byte player, int uid, int group, int count, bool look)
        {
            if (_xml != null)
                _xml.LookAtTop(player, uid, group, count, look);
            if (_bin != null)
                _bin.LookAtTop(player, uid, group, count, look);
            Send();
        }

        public void LookAtBottom(byte player, int uid, int group, int count, bool look)
        {
            if (_xml != null)
                _xml.LookAtBottom(player, uid, group, count, look);
            if (_bin != null)
                _bin.LookAtBottom(player, uid, group, count, look);
            Send();
        }

        public void StartLimited(byte player, Guid[] packs)
        {
            if (_xml != null)
                _xml.StartLimited(player, packs);
            if (_bin != null)
                _bin.StartLimited(player, packs);
            Send();
        }

        public void CancelLimited(byte player)
        {
            if (_xml != null)
                _xml.CancelLimited(player);
            if (_bin != null)
                _bin.CancelLimited(player);
            Send();
        }

        #endregion

        internal void RefreshTypes()
        {
            bool bBin = false, bXml = false;
            foreach (Handler.PlayerInfo pi in _to.Values)
            {
                bBin |= pi.Binary;
                bXml |= pi.Binary == false;
            }
            if (bBin && _bin == null) _bin = new BinFormatter(this, _handler);
            else if (!bBin && _bin != null) _bin = null;
            if (bXml && _xml == null) _xml = new XmlFormatter(this, _handler);
            else if (!bXml && _xml != null) _xml = null;
        }

        internal void Send()
        {
            foreach (KeyValuePair<TcpClient, Handler.PlayerInfo> kvp in _to)
                try
                {
                    Stream stream = kvp.Key.GetStream();
                    if (kvp.Value.Binary)
                        stream.Write(_binData, 0, _binData.Length);
                    else
                        stream.Write(_xmlData, 0, _xmlLength);
                    stream.Flush();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            //					Program.server.Disconnected(kvp.Key);
        }

        public void IsAlternateImage(int c, bool isaltertnate)
        {
            if (_xml != null)
                _xml.IsAlternateImage(c, isaltertnate);
            if (_bin != null)
                _bin.IsAlternateImage(c, isaltertnate);
            Send();
        }

        public void PlayerSetGlobalVariable(byte from, byte p, string n, string v)
        {
            if (_xml != null)
                _xml.PlayerSetGlobalVariable(from, p, n, v);
            if (_bin != null)
                _bin.PlayerSetGlobalVariable(from, p, n, v);
            Send();
        }

        public void SetGlobalVariable(string n, string v)
        {
            if (_xml != null)
                _xml.SetGlobalVariable(n, v);
            if (_bin != null)
                _bin.SetGlobalVariable(n, v);
            Send();
        }

        #region Nested type: BinFormatter

        private sealed class BinFormatter : BaseBinaryStub
        {
            private readonly Broadcaster _bcast;

            internal BinFormatter(Broadcaster bcast, Handler handler)
                : base(handler)
            {
                _bcast = bcast;
            }

            protected override void Send(byte[] data)
            {
                _bcast._binData = data;
            }
        }

        #endregion

        #region Nested type: XmlFormatter

        private sealed class XmlFormatter : BaseXmlStub
        {
            private readonly Broadcaster _bcast;

            internal XmlFormatter(Broadcaster bcast, Handler handler)
                : base(handler)
            {
                _bcast = bcast;
            }

            protected override void Send(string xml)
            {
                _bcast._xmlLength = Encoding.UTF8.GetByteCount(xml) + 1;
                if (_bcast._xmlLength > _bcast._xmlData.Length)
                    _bcast._xmlData = new byte[_bcast._xmlLength];
                Encoding.UTF8.GetBytes(xml, 0, xml.Length, _bcast._xmlData, 0);
                _bcast._xmlData[_bcast._xmlLength - 1] = 0;
            }
        }

        #endregion
    }
}