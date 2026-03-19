using NUnit.Framework;
using Octgn.Library.Networking;

namespace Octgn.Test.Library
{
    [TestFixture]
    public class NamedUrlTests
    {
        [Test]
        public void Constructor_FourArgs_DefaultsToNuGet()
        {
            var url = new NamedUrl( "Test Feed", "http://example.com/feed", "user", "pass" );

            Assert.AreEqual( FeedType.NuGet, url.FeedType );
        }

        [Test]
        public void Constructor_FourArgs_SetsAllProperties()
        {
            var url = new NamedUrl( "Test Feed", "http://example.com/feed", "user", "pass" );

            Assert.AreEqual( "Test Feed", url.Name );
            Assert.AreEqual( "http://example.com/feed", url.Url );
            Assert.AreEqual( "user", url.Username );
            Assert.AreEqual( "pass", url.Password );
        }

        [Test]
        public void Constructor_WithFeedType_RepoIndex_SetsCorrectly()
        {
            var url = new NamedUrl( "GitHub Feed", "http://example.com/index.json", null, null, FeedType.RepoIndex );

            Assert.AreEqual( FeedType.RepoIndex, url.FeedType );
            Assert.AreEqual( "GitHub Feed", url.Name );
            Assert.AreEqual( "http://example.com/index.json", url.Url );
        }

        [Test]
        public void Constructor_WithFeedType_DirectRepo_SetsCorrectly()
        {
            var url = new NamedUrl( "Direct Repo", "owner/repo", null, null, FeedType.DirectRepo );

            Assert.AreEqual( FeedType.DirectRepo, url.FeedType );
        }

        [Test]
        public void FeedType_NuGet_HasValueZero()
        {
            Assert.AreEqual( 0, (int) FeedType.NuGet );
        }

        [Test]
        public void FeedType_RepoIndex_HasValueOne()
        {
            Assert.AreEqual( 1, (int) FeedType.RepoIndex );
        }

        [Test]
        public void FeedType_DirectRepo_HasValueTwo()
        {
            Assert.AreEqual( 2, (int) FeedType.DirectRepo );
        }

        [Test]
        public void Constructor_NullCredentials_SetsNull()
        {
            var url = new NamedUrl( "Feed", "http://example.com", null, null );

            Assert.IsNull( url.Username );
            Assert.IsNull( url.Password );
        }

        [Test]
        public void Constructor_FiveArgs_SetsAllProperties()
        {
            var url = new NamedUrl( "My Feed", "http://example.com", "user1", "pass1", FeedType.RepoIndex );

            Assert.AreEqual( "My Feed", url.Name );
            Assert.AreEqual( "http://example.com", url.Url );
            Assert.AreEqual( "user1", url.Username );
            Assert.AreEqual( "pass1", url.Password );
            Assert.AreEqual( FeedType.RepoIndex, url.FeedType );
        }
    }
}
