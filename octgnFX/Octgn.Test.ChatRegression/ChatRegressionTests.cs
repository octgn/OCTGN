using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using NUnit.Framework;
using Octgn.Online.Hosting;
using Octgn.Server;

public class RecordingRpc : RealProxy
{
    public RecordingRpc() : base(typeof(IClientCalls)) { }
    public List<(string Method, object[] Args)> Calls { get; } = new List<(string, object[])>();
    public Exception Failure { get; set; }
    public override IMessage Invoke(IMessage message)
    {
        var call = (IMethodCallMessage)message;
        Calls.Add((call.MethodName, call.Args));
        return Failure != null ? new ReturnMessage(Failure, call) : new ReturnMessage(null, null, 0, call.LogicalCallContext, call);
    }
}

[TestFixture]
public class ChatRegressionTests
{
    private static (GameContext Context, Handler Handler, Player Sender, RecordingRpc SenderRpc, RecordingRpc PeerRpc, RecordingRpc OfflineRpc) Create(bool local, bool spectator, bool muted)
    {
        var context = new GameContext(new HostedGame { Id = Guid.NewGuid() }, new Config { IsLocal = local });
        context.State.Settings.MuteSpectators = muted;
        var senderRpc = new RecordingRpc();
        var peerRpc = new RecordingRpc();
        var offlineRpc = new RecordingRpc();
        var sender = new Player { Id = 7, Nick = null, Rpc = (IClientCalls)senderRpc.GetTransparentProxy(), Settings = new PlayerSettings(false, spectator) };
        context.State.Players.Clients.AddRange(new[] {
            sender,
            new Player { Id = 8, Rpc = (IClientCalls)peerRpc.GetTransparentProxy() },
            new Player { Id = 9, Rpc = (IClientCalls)offlineRpc.GetTransparentProxy(), Connected = false },
            new Player { Id = 10, Rpc = (IClientCalls)offlineRpc.GetTransparentProxy(), SaidHello = false }
        });
        var handler = new Handler(context);
        handler.SetPlayer(sender);
        return (context, handler, sender, senderRpc, peerRpc, offlineRpc);
    }
    private static void Dispatch(bool packet, Handler handler, GameContext context, string text)
    {
        if (!packet) { handler.ChatReq(text); return; }
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write(42); // Test IsMuted header value in the production BinaryParser.
        writer.Write((byte)21); // Existing generated ChatReq opcode.
        writer.Write(text);
        writer.Flush();
        new BinaryParser(new ServerSocket { Handler = handler, Server = new Server { Context = context } }).Parse(stream.ToArray());
        Assert.That(context.State.IsMuted, Is.EqualTo(42), "Parser packet header");
    }
    [TestCase(false, false, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, false)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public void ChatPreservesPayloadAndMuteBehavior(bool local, bool spectator, bool muted)
    {
        foreach (var text in new[] { "Hello \u03a9\r\nsecond line", "", null })
        foreach (var packet in new[] { false, true })
        {
            if (packet && text == null) continue; // Protocol strings cannot encode null.
            var test = Create(local, spectator, muted);
            Dispatch(packet, test.Handler, test.Context, text);
            var scenario = $"local={local}, spectator={spectator}, muted={muted}, packet={packet}, text={text ?? "<null>"}";
            Assert.That(test.OfflineRpc.Calls, Is.Empty, "Disconnected/doubled unregistered recipients: " + scenario);
            if (spectator && muted)
            {
                Assert.That(test.SenderRpc.Calls, Has.Count.EqualTo(1), scenario);
                Assert.That(test.PeerRpc.Calls, Is.Empty, scenario);
                var error = test.SenderRpc.Calls[0];
                Assert.That(error.Method, Is.EqualTo("Error"), scenario);
                Assert.That(error.Args[0], Is.EqualTo(Octgn.Library.Localization.L.D.ServerMessage__SpectatorsMuted), scenario);
            }
            else
            {
                foreach (var rpc in new[] { test.SenderRpc, test.PeerRpc })
                {
                    Assert.That(rpc.Calls, Has.Count.EqualTo(1), scenario);
                    var chat = rpc.Calls[0];
                    Assert.That(chat.Method, Is.EqualTo("Chat"), scenario);
                    Assert.That(chat.Args, Is.EqualTo(new object[] { (byte)7, text }), scenario);
                }
            }
        }
    }
    [Test]
    public void ChatPropagatesRpcFailureUnchanged()
    {
        var test = Create(false, false, false);
        var failure = new InvalidOperationException("RPC failure");
        test.SenderRpc.Failure = failure;
        Assert.That(Assert.Throws<InvalidOperationException>(() => test.Handler.ChatReq("failure")), Is.SameAs(failure));
    }
}
