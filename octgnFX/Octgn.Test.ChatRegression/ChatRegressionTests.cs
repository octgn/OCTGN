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
    private static void Check(bool condition, string message)
    {
        Assert.That(condition, Is.True, message);
    }
    private static void Throws<T>(Action action) where T : Exception
    {
        Assert.Throws<T>(() => action());
    }
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
        writer.Write(42); // Existing IsMuted packet header.
        writer.Write((byte)21); // ChatReq wire opcode.
        writer.Write(text);
        writer.Flush();
        new BinaryParser(new ServerSocket { Handler = handler, Server = new Server { Context = context } }).Parse(stream.ToArray());
        Check(context.State.IsMuted == 42, "Parser changed packet header handling");
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
        foreach (var text in new[] { "Hello 🌍\r\nsecond line", "", null })
        foreach (var packet in new[] { false, true })
        {
            // BinaryWriter's protocol strings do not encode null; direct calls still can.
            if (packet && text == null) continue;
            var test = Create(local, spectator, muted);
            Dispatch(packet, test.Handler, test.Context, text);
            Check(test.OfflineRpc.Calls.Count == 0, "Disconnected or unregistered recipient received a message");
            if (spectator && muted)
            {
                Check(test.SenderRpc.Calls.Count == 1 && test.PeerRpc.Calls.Count == 0, "Muted chat was broadcast");
                var error = test.SenderRpc.Calls[0];
                Check(error.Method == "Error" && Equals(error.Args[0], Octgn.Library.Localization.L.D.ServerMessage__SpectatorsMuted), "Muted error changed");
            }
            else
            {
                foreach (var rpc in new[] { test.SenderRpc, test.PeerRpc })
                {
                    Check(rpc.Calls.Count == 1, "Chat dispatch count changed");
                    var chat = rpc.Calls[0];
                    Check(chat.Method == "Chat" && Equals(chat.Args[0], (byte)7) && Equals(chat.Args[1], text), "Chat sender or payload changed");
                }
            }
        }
    }
    [Test]
    public void ChatPreservesErrorsAndNullHandling()
    {
        Throws<ArgumentNullException>(() => new Handler(null));
        Throws<ArgumentNullException>(() => new GameContext(new HostedGame(), null));
        Throws<NullReferenceException>(() => new GameContext(null, new Config()));
        var test = Create(false, false, false);
        Throws<ArgumentNullException>(() => test.Handler.SetPlayer(null));
        Throws<NullReferenceException>(() => new Handler(test.Context).ChatReq("no sender"));
        test.Sender.Settings = null;
        Throws<NullReferenceException>(() => test.Handler.ChatReq("no settings"));
        test = Create(false, true, false);
        test.Context.State.Settings = null;
        Throws<NullReferenceException>(() => test.Handler.ChatReq("no game settings"));
        test = Create(false, false, false);
        var failure = new InvalidOperationException("RPC failure");
        test.SenderRpc.Failure = failure;
        try { test.Handler.ChatReq("failure"); throw new Exception("Expected RPC failure"); }
        catch (InvalidOperationException actual) { Check(ReferenceEquals(actual, failure), "RPC exception was replaced"); }
        Check(test.PeerRpc.Calls.Count == 0, "Broadcast continued after failed recipient");
        test = Create(false, true, true);
        test.SenderRpc.Failure = failure;
        Throws<InvalidOperationException>(() => test.Handler.ChatReq(null));
        Check(test.PeerRpc.Calls.Count == 0, "Failed mute error broadcast chat");
    }
}
