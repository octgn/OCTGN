namespace Octgn.Test
{
    using System;

    using NUnit.Framework;

    using Skylabs.Lobby;

    using agsXMPP;

    public class PlayGround
    {
        [Test]
        public void Spaces()
        {
            var j = new Jid("face brains@taco.net");
            Console.WriteLine(j.Bare);
            var a = new User(new Jid("asdf asdf@taco.net"));
            Console.WriteLine(a.FullUserName);
            Assert.Fail();
        }
    }
}