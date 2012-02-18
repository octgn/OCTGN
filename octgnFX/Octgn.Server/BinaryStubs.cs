using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace Octgn.Server
{
    internal abstract class BaseBinaryStub : IClientCalls
    {
        private readonly Handler _handler;

        protected BaseBinaryStub(Handler handler)
        {
            _handler = handler;
        }

        #region IClientCalls Members

        public void Ping()
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 255);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Binary()
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 0);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Error(string msg)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 1);
            writer.Write(msg);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Welcome(byte id)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 3);
            writer.Write(id);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Settings(bool twoSidedTable)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 4);
            writer.Write(twoSidedTable);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void PlayerSettings(byte playerId, bool invertedTable)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 5);
            writer.Write(playerId);
            writer.Write(invertedTable);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void NewPlayer(byte id, string nick, ulong pkey)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 6);
            writer.Write(id);
            writer.Write(nick);
            writer.Write(pkey);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Leave(byte player)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 7);
            writer.Write(player);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Nick(byte player, string nick)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 9);
            writer.Write(player);
            writer.Write(nick);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Start()
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 10);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Reset(byte player)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 12);
            writer.Write(player);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void NextTurn(byte nextPlayer)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 13);
            writer.Write(nextPlayer);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void StopTurn(byte player)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 15);
            writer.Write(player);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Chat(byte player, string text)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 17);
            writer.Write(player);
            writer.Write(text);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Print(byte player, string text)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 19);
            writer.Write(player);
            writer.Write(text);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Random(byte player, int id, int min, int max)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 21);
            writer.Write(player);
            writer.Write(id);
            writer.Write(min);
            writer.Write(max);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void RandomAnswer1(byte player, int id, ulong value)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 23);
            writer.Write(player);
            writer.Write(id);
            writer.Write(value);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void RandomAnswer2(byte player, int id, ulong value)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 25);
            writer.Write(player);
            writer.Write(id);
            writer.Write(value);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Counter(byte player, int counter, int value)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 27);
            writer.Write(player);
            writer.Write(counter);
            writer.Write(value);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void LoadDeck(int[] id, ulong[] type, int[] group)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 28);
            writer.Write((short) id.Length);
            foreach (int p in id)
                writer.Write(p);
            writer.Write((short) type.Length);
            foreach (ulong p in type)
                writer.Write(p);
            writer.Write((short) group.Length);
            foreach (int p in group)
                writer.Write(p);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void CreateCard(int[] id, ulong[] type, int group)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 29);
            writer.Write((short) id.Length);
            foreach (int p in id)
                writer.Write(p);
            writer.Write((short) type.Length);
            foreach (ulong p in type)
                writer.Write(p);
            writer.Write(group);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void CreateCardAt(int[] id, ulong[] key, Guid[] modelId, int[] x, int[] y, bool faceUp, bool persist)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 30);
            writer.Write((short) id.Length);
            foreach (int p in id)
                writer.Write(p);
            writer.Write((short) key.Length);
            foreach (ulong p in key)
                writer.Write(p);
            writer.Write((short) modelId.Length);
            foreach (Guid g in modelId)
                writer.Write(g.ToByteArray());
            writer.Write((short) x.Length);
            foreach (int p in x)
                writer.Write(p);
            writer.Write((short) y.Length);
            foreach (int p in y)
                writer.Write(p);
            writer.Write(faceUp);
            writer.Write(persist);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void CreateAlias(int[] id, ulong[] type)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 31);
            writer.Write((short) id.Length);
            foreach (int p in id)
                writer.Write(p);
            writer.Write((short) type.Length);
            foreach (ulong p in type)
                writer.Write(p);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void MoveCard(byte player, int card, int group, int idx, bool faceUp)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 33);
            writer.Write(player);
            writer.Write(card);
            writer.Write(group);
            writer.Write(idx);
            writer.Write(faceUp);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void MoveCardAt(byte player, int card, int x, int y, int idx, bool faceUp)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 35);
            writer.Write(player);
            writer.Write(card);
            writer.Write(x);
            writer.Write(y);
            writer.Write(idx);
            writer.Write(faceUp);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Reveal(int card, ulong revealed, Guid guid)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 36);
            writer.Write(card);
            writer.Write(revealed);
            writer.Write(guid.ToByteArray());
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void RevealTo(byte[] players, int card, ulong[] encrypted)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 38);
            writer.Write((short) players.Length);
            foreach (byte p in players)
                writer.Write(p);
            writer.Write(card);
            writer.Write((short) encrypted.Length);
            foreach (ulong p in encrypted)
                writer.Write(p);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Peek(byte player, int card)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 40);
            writer.Write(player);
            writer.Write(card);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Untarget(byte player, int card)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 42);
            writer.Write(player);
            writer.Write(card);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Target(byte player, int card)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 44);
            writer.Write(player);
            writer.Write(card);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void TargetArrow(byte player, int card, int otherCard)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 46);
            writer.Write(player);
            writer.Write(card);
            writer.Write(otherCard);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Highlight(int card, string color)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 47);
            writer.Write(card);
            writer.Write(color);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Turn(byte player, int card, bool up)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 49);
            writer.Write(player);
            writer.Write(card);
            writer.Write(up);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Rotate(byte player, int card, CardOrientation rot)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 51);
            writer.Write(player);
            writer.Write(card);
            writer.Write((byte) rot);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Shuffle(int group, int[] card)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 52);
            writer.Write(group);
            writer.Write((short) card.Length);
            foreach (int p in card)
                writer.Write(p);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Shuffled(int group, int[] card, short[] pos)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 53);
            writer.Write(group);
            writer.Write((short) card.Length);
            foreach (int p in card)
                writer.Write(p);
            writer.Write((short) pos.Length);
            foreach (short p in pos)
                writer.Write(p);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void UnaliasGrp(int group)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 54);
            writer.Write(group);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void Unalias(int[] card, ulong[] type)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 55);
            writer.Write((short) card.Length);
            foreach (int p in card)
                writer.Write(p);
            writer.Write((short) type.Length);
            foreach (ulong p in type)
                writer.Write(p);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void AddMarker(byte player, int card, Guid id, string name, ushort count)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 57);
            writer.Write(player);
            writer.Write(card);
            writer.Write(id.ToByteArray());
            writer.Write(name);
            writer.Write(count);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void RemoveMarker(byte player, int card, Guid id, string name, ushort count)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 59);
            writer.Write(player);
            writer.Write(card);
            writer.Write(id.ToByteArray());
            writer.Write(name);
            writer.Write(count);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void SetMarker(byte player, int card, Guid id, string name, ushort count)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 61);
            writer.Write(player);
            writer.Write(card);
            writer.Write(id.ToByteArray());
            writer.Write(name);
            writer.Write(count);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void TransferMarker(byte player, int from, int lTo, Guid id, string name, ushort count)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 63);
            writer.Write(player);
            writer.Write(from);
            writer.Write(lTo);
            writer.Write(id.ToByteArray());
            writer.Write(name);
            writer.Write(count);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void PassTo(byte player, int id, byte lTo, bool requested)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 65);
            writer.Write(player);
            writer.Write(id);
            writer.Write(lTo);
            writer.Write(requested);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void TakeFrom(int id, byte lTo)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 67);
            writer.Write(id);
            writer.Write(lTo);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void DontTake(int id)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 69);
            writer.Write(id);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void FreezeCardsVisibility(int group)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 70);
            writer.Write(group);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void GroupVis(byte player, int group, bool defined, bool visible)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 72);
            writer.Write(player);
            writer.Write(group);
            writer.Write(defined);
            writer.Write(visible);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void GroupVisAdd(byte player, int group, byte who)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 74);
            writer.Write(player);
            writer.Write(group);
            writer.Write(who);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void GroupVisRemove(byte player, int group, byte who)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 76);
            writer.Write(player);
            writer.Write(group);
            writer.Write(who);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void LookAt(byte player, int uid, int group, bool look)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 78);
            writer.Write(player);
            writer.Write(uid);
            writer.Write(group);
            writer.Write(look);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void LookAtTop(byte player, int uid, int group, int count, bool look)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 80);
            writer.Write(player);
            writer.Write(uid);
            writer.Write(group);
            writer.Write(count);
            writer.Write(look);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void LookAtBottom(byte player, int uid, int group, int count, bool look)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 82);
            writer.Write(player);
            writer.Write(uid);
            writer.Write(group);
            writer.Write(count);
            writer.Write(look);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void StartLimited(byte player, Guid[] packs)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 84);
            writer.Write(player);
            writer.Write((short) packs.Length);
            foreach (Guid g in packs)
                writer.Write(g.ToByteArray());
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void CancelLimited(byte player)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 86);
            writer.Write(player);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        #endregion

        protected abstract void Send(byte[] data);

        public void IsAlternate(int c, bool isAlternate)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(handler.muted);
            writer.Write((byte)90);
            writer.Write(c);
            writer.Write(isAlternate);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int)stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void IsAlternateImage(int c, bool isAlternateImage)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 87);
            writer.Write(c);
            writer.Write(isAlternateImage);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void PlayerSetGlobalVariable(byte from, byte p, string name, string value)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 88);
            writer.Write(from);
            writer.Write(p);
            writer.Write(name);
            writer.Write(value);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }

        public void SetGlobalVariable(string name, string value)
        {
            var stream = new MemoryStream(512);
            stream.Seek(4, SeekOrigin.Begin);
            var writer = new BinaryWriter(stream);

            writer.Write(_handler.Muted);
            writer.Write((byte) 89);
            writer.Write(name);
            writer.Write(value);
            writer.Flush();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int) stream.Length);
            writer.Close();
            Send(stream.ToArray());
        }
    }

    internal class BinarySenderStub : BaseBinaryStub
    {
        private readonly TcpClient _to;

        public BinarySenderStub(TcpClient to, Handler handler)
            : base(handler)
        {
            _to = to;
        }

        protected override void Send(byte[] data)
        {
            try
            {
                Stream stream = _to.GetStream();
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
                // TODO: Notify disconnection
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            //				if (Program.Server != null && Program.Server.Disconnected(lTo))
            //					return;
            //				Program.Client.Disconnected();
        }
    }
}