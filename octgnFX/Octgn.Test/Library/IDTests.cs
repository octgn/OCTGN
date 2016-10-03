using NUnit.Framework;

namespace Octgn.Test.Library
{
    public class IDTests
    {
        [Test]
        public void ID_ConvertsUniformly() {
            var id = new ID(IDType.Group, 12, 24);
            ulong uid = id;
            ID id2 = uid;
            ulong uid2 = id2;

            Assert.AreEqual(id.Id, id2.Id);
            Assert.AreEqual(id.GameId, id2.GameId);
            Assert.AreEqual(id.PlayerId, id2.PlayerId);
            Assert.AreEqual(id.Type, id2.Type);
            Assert.AreEqual(uid, uid2);
        }
    }
}
