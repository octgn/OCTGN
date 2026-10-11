using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FakeItEasy;
using NUnit.Framework;
using Octgn.Core.Play;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Play.Save;
using Model = Octgn.DataNew.Entities;
using Player = Octgn.Play.Player;
using Group = Octgn.Play.Group;

namespace Octgn.Test.OctgnApp.Play
{
    [TestFixture, NonParallelizable, Timeout(10000)]
    public class PeekShuffleTests
    {
        private GameEngine previousEngine;
        private IClient previousClient;
        private GameMessageDispatcher previousMessages;
        private Player previousLocalPlayer;
        private Octgn.Core.DiscordIntegration.DiscordWrapper previousDiscord;
        private Func<bool> previousMuteChecker;
        private Player local;
        private Player peer;
        private Pile pile;
        private Handler handler;
        private ReplayWriter replay;
        private readonly List<Card> ownedCards = new List<Card>();
        private readonly Queue<byte[]> pending = new Queue<byte[]>();

        [SetUp]
        public void SetUp()
        {
            previousEngine = Program.GameEngine;
            previousClient = Program.Client;
            previousMessages = Program.GameMess;
            previousLocalPlayer = Player.LocalPlayer;
            previousDiscord = Program.Discord;
            previousMuteChecker = GameMessage.MuteChecker;

            try
            {
                InitializeOwnedState();
            }
            catch
            {
                // NUnit may skip TearDown when SetUp fails.
                TearDown();
                throw;
            }
        }

        private void InitializeOwnedState()
        {
            // Bypass application lifecycle constructors, not the methods under test:
            // Player starts subscription/API work, Pile reads Prefs.HandDensity,
            // and normal GameEngine constructors initialize scripts/user settings.
            // Only owned in-memory state required by the real model/parser is set.
            var engine = (GameEngine)FormatterServices.GetUninitializedObject(typeof(GameEngine));
            engine.Definition = new Model.Game();
            engine.Definition.CardSizes.Add("", new Model.CardSize {
                Name = "owned", Width = 63, Height = 88, Front = "", Back = ""
            });
            replay = new ReplayWriter();
            SetField(typeof(GameEngine), engine, "<ReplayWriter>k__BackingField", replay);
            Program.GameEngine = engine;
            Program.GameMess = new GameMessageDispatcher();
            Program.Discord = null;
            GameMessage.MuteChecker = () => false;

            local = OwnedPlayer(201, "Owned local");
            peer = OwnedPlayer(202, "Owned peer");
            Player.LocalPlayer = local;
            pile = (Pile)FormatterServices.GetUninitializedObject(typeof(Pile));
            SetField(typeof(ControllableObject), pile, "_owner", local);
            SetField(typeof(ControllableObject), pile, "_controller", local);
            SetField(typeof(Group), pile, "cards", new ObservableCollection<Card>());
            SetField(typeof(Group), pile, "visibility", Model.GroupVisibility.Nobody);
            pile.LookedAt = new Dictionary<int, List<Card>>();
            pile.Viewers = new List<Player>();
            pile.Def = new Model.Group { Id = 1, Name = "Owned pile", Visibility = Model.GroupVisibility.Nobody };
            SetField(typeof(Player), local, "_groups", new Group[] { pile });
            SetField(typeof(Player), peer, "_groups", new Group[0]);
            Player.All.Add(local);
            Player.All.Add(peer);

            handler = new Handler();
            var rpc = A.Fake<IServerCalls>();
            // Explicit transport boundary: hold server echoes until delivered.
            // Actual Peek/Shuffle, incoming parser/handler and replay writer run.
            A.CallTo(() => rpc.PeekReq(A<Card>._)).Invokes(call =>
                pending.Enqueue(PeekPacket(local, call.GetArgument<Card>(0))));
            A.CallTo(() => rpc.Shuffled(A<Player>._, A<Group>._, A<int[]>._, A<short[]>._))
                .Invokes(call => pending.Enqueue(ShufflePacket(
                    call.GetArgument<Player>(0), call.GetArgument<Group>(1),
                    call.GetArgument<int[]>(2), call.GetArgument<short[]>(3))));
            var client = A.Fake<IClient>();
            A.CallTo(() => client.Rpc).Returns(rpc);
            A.CallTo(() => client.Handler).Returns(handler);
            Program.Client = client;
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                foreach (var card in ownedCards)
                {
                    Card.Remove(card);
                    CardIdentity.Delete(card.Id);
                }
                if (local != null) Player.All.Remove(local);
                if (peer != null) Player.All.Remove(peer);
                replay?.Dispose();
            }
            finally
            {
                ownedCards.Clear();
                pending.Clear();
                Program.GameEngine = previousEngine;
                Program.Client = previousClient;
                Program.GameMess = previousMessages;
                Player.LocalPlayer = previousLocalPlayer;
                Program.Discord = previousDiscord;
                GameMessage.MuteChecker = previousMuteChecker;
            }
        }

        [Test]
        public void LocalPeek_UpdatesImmediately_AndOwnEchoIsIdempotent()
        {
            var card = AddCards(1)[0];
            card.Peek();
            Assert.AreEqual(card.RealName, card.ToolTip);
            CollectionAssert.AreEqual(new[] { local }, card.PeekingPlayers);
            Assert.AreEqual(1, pending.Count);
            DeliverNext();
            CollectionAssert.AreEqual(new[] { local }, card.PeekingPlayers);
            Assert.AreEqual(1, Recorded().Length);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void DelayedOwnPeekEcho_AfterLocalShuffle_DoesNotRestorePeek(int count)
        {
            var cards = AddCards(count);
            cards[0].Peek();
            pile.Shuffle();
            var shuffledOrder = pile.Cards.ToArray();
            CollectionAssert.AreEquivalent(cards, shuffledOrder);
            Assert.AreEqual(2, pending.Count, "Real model sends PeekReq then Shuffled");
            CollectionAssert.AreEqual(new byte[] { 38, 51 }, pending.Select(message => message[4]));
            AssertNoPeeks();

            DeliverNext(); // Old own Peek echo after local shuffle.
            AssertNoPeeks(); // Fails on current master and the original PR.
            DeliverNext(); // Own Shuffled echo must not repeat the shuffle.
            AssertNoPeeks();
            CollectionAssert.AreEqual(shuffledOrder, pile.Cards);
            Assert.AreEqual("Card", cards[0].ToolTip);
            AssertRecordedMethods(38, 51);
        }

        [Test]
        public void RemotePeekAndShuffle_AreApplied_AndClearAllPeekAndLookState()
        {
            var cards = AddCards(3);
            handler.ReceiveMessage(PeekPacket(peer, cards[0]));
            CollectionAssert.AreEqual(new[] { peer }, cards[0].PeekingPlayers);
            foreach (var card in cards) card.PlayersLooking.Add(peer);
            pile.LookedAt.Add(1, cards.ToList());
            var identities = cards.Select(c => c.Id).ToArray();
            handler.ReceiveMessage(ShufflePacket(peer, pile, identities, new short[] { 2, 1, 0 }));
            CollectionAssert.AreEqual(cards.Reverse(), pile.Cards);
            AssertNoPeeks();
            Assert.IsTrue(cards.All(c => c.PlayersLooking.Count == 0));
            Assert.IsEmpty(pile.LookedAt[1]);
            Assert.IsEmpty(pending);
            AssertRecordedMethods(38, 51);
        }

        [Test]
        public void NewLocalPeek_AfterShuffle_RemainsDespiteOlderOwnEchoes()
        {
            var card = AddCards(1)[0];
            card.Peek();
            pile.Shuffle();
            AssertNoPeeks();
            card.Peek();
            Assert.AreEqual(3, pending.Count);
            CollectionAssert.AreEqual(new byte[] { 38, 51, 38 }, pending.Select(message => message[4]));
            while (pending.Count > 0) DeliverNext();
            CollectionAssert.AreEqual(new[] { local }, card.PeekingPlayers);
            Assert.AreEqual(card.RealName, card.ToolTip);
            AssertRecordedMethods(38, 51, 38);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ReplayPeekAndShuffle_ApplyIncludingRecordedLocalPlayer(bool fromLocal)
        {
            var cards = AddCards(3);
            SetField(typeof(GameEngine), Program.GameEngine, "<IsReplay>k__BackingField", true);
            var player = fromLocal ? local : peer;
            handler.ReceiveMessage(PeekPacket(player, cards[0]));
            CollectionAssert.AreEqual(new[] { player }, cards[0].PeekingPlayers);
            handler.ReceiveMessage(ShufflePacket(player, pile,
                cards.Select(c => c.Id).ToArray(), new short[] { 2, 1, 0 }));
            CollectionAssert.AreEqual(cards.Reverse(), pile.Cards);
            AssertNoPeeks();
            Assert.IsEmpty(Recorded(), "Playback must not record another replay");
        }

        [Test]
        public void LiveReplayRecording_PreservesOwnIgnoredAndRemotePayloadOrder()
        {
            var card = AddCards(1)[0];
            card.Peek();
            pile.Shuffle();
            var ownPeek = pending.Dequeue();
            var ownShuffle = pending.Dequeue();
            var remotePeek = PeekPacket(peer, card);
            handler.ReceiveMessage(ownPeek);
            AssertNoPeeks();
            handler.ReceiveMessage(ownShuffle);
            handler.ReceiveMessage(remotePeek);
            var events = Recorded();
            Assert.AreEqual(3, events.Length);
            CollectionAssert.AreEqual(new[] { local.Id, local.Id, peer.Id }, events.Select(e => e.PlayerId));
            CollectionAssert.AreEqual(new[] { ownPeek, ownShuffle, remotePeek }, events.Select(e => e.Action));
            CollectionAssert.AreEqual(new[] { 0L, 100000L, 200000L }, events.Select(e => e.Time.Ticks));
            Assert.IsTrue(events.All(e => e.Type == ReplayEventType.Action));
            CollectionAssert.AreEqual(new[] { peer }, card.PeekingPlayers);
        }

        [Test]
        public void FaceUpCardPeek_DoesNotCreatePeekOrRequest()
        {
            var card = AddCards(1)[0];
            card.SetFaceUp(true);
            card.Peek();
            Assert.IsTrue(card.FaceUp);
            Assert.IsEmpty(card.PeekingPlayers);
            Assert.IsEmpty(pending);
            Assert.IsEmpty(Recorded());
        }

        private Card[] AddCards(int count)
        {
            var result = new Card[count];
            for (var i = 0; i < count; ++i)
            {
                var id = (local.Id << 16) | (i + 1);
                Assert.IsNull(Card.Find(id), "Never replace another fixture's card");
                Assert.IsNull(CardIdentity.Find(id), "Never replace another fixture's identity");
                var size = Program.GameEngine.Definition.CardSizes[""];
                var model = new Model.Card(Guid.NewGuid(), Guid.NewGuid(), "Owned card " + i,
                    "", "", size, new Dictionary<string, Model.CardPropertySet>());
                var card = new Card(local, id, model, true, ""); // Actual production constructor.
                ownedCards.Add(card);
                pile.Add(card); // Actual group/card visibility and group-change methods.
                result[i] = card;
            }
            return result;
        }

        private static Player OwnedPlayer(byte id, string name)
        {
            Assert.IsNull(Player.Find(id), "Never replace another fixture's player");
            var player = (Player)FormatterServices.GetUninitializedObject(typeof(Player));
            player.Id = id;
            player.Name = name;
            return player;
        }

        private void DeliverNext()
        {
            handler.ReceiveMessage(pending.Dequeue());
        }

        private void AssertNoPeeks()
        {
            Assert.IsTrue(pile.Cards.All(c => c.PeekingPlayers.Count == 0),
                "A delayed acknowledgement must not restore pre-shuffle information");
        }

        private ReplayEvent[] Recorded()
        {
            // Observe the real writer's memory queue; no stream/file is opened.
            return ((Queue<ReplayEvent>)typeof(ReplayWriter).GetField("_prestartQueue",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(replay)).ToArray();
        }

        private void AssertRecordedMethods(params byte[] methods)
        {
            CollectionAssert.AreEqual(methods, Recorded().Select(e => e.Action[4]));
        }

        private static byte[] PeekPacket(Player player, Card card)
        {
            return Packet(writer => { writer.Write((byte)38); writer.Write(player.Id); writer.Write(card.Id); });
        }

        private static byte[] ShufflePacket(Player player, Group group, int[] identities, short[] positions)
        {
            return Packet(writer => {
                writer.Write((byte)51); writer.Write(player.Id); writer.Write(group.Id);
                writer.Write((short)identities.Length);
                foreach (var identity in identities) writer.Write(identity);
                writer.Write((short)positions.Length);
                foreach (var position in positions) writer.Write(position);
            });
        }

        private static byte[] Packet(Action<BinaryWriter> write)
        {
            // Protocol.xml/BinaryParser define this payload. Transport framing is
            // already stripped before Handler.ReceiveMessage; muted int remains.
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0);
                write(writer);
                writer.Flush();
                return stream.ToArray();
            }
        }

        private static void SetField(Type declaringType, object target, string name, object value)
        {
            var field = declaringType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "Fixture initialization follows the current production field: " + name);
            field.SetValue(target, value);
        }
    }
}
