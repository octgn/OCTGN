using System;
using NUnit.Framework;
using Octgn.Core;

namespace Octgn.Test.Core
{
    [TestFixture]
    public class RepoFeedManagerTests
    {
        #region NormalizeRepoUrl

        [Test]
        public void NormalizeRepoUrl_OwnerSlashRepo_ParsesCorrectly()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "octgn/MyGame" );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [Test]
        public void NormalizeRepoUrl_FullGitHubUrl_ParsesCorrectly()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "https://github.com/octgn/MyGame" );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [Test]
        public void NormalizeRepoUrl_FullUrlWithGitSuffix_StripsGit()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "https://github.com/octgn/MyGame.git" );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [Test]
        public void NormalizeRepoUrl_UrlWithTrailingSlash_StripsSlash()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "https://github.com/octgn/MyGame/" );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [Test]
        public void NormalizeRepoUrl_HttpUrl_ParsesCorrectly()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "http://github.com/octgn/MyGame" );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [Test]
        public void NormalizeRepoUrl_UrlWithExtraPathSegments_UsesFirstTwo()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "https://github.com/octgn/MyGame/tree/main" );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [Test]
        public void NormalizeRepoUrl_EmptyString_Throws()
        {
            Assert.Throws<ArgumentException>( () => RepoFeedManager.NormalizeRepoUrl( "" ) );
        }

        [Test]
        public void NormalizeRepoUrl_Null_Throws()
        {
            Assert.Throws<ArgumentException>( () => RepoFeedManager.NormalizeRepoUrl( null ) );
        }

        [Test]
        public void NormalizeRepoUrl_WhitespaceOnly_Throws()
        {
            Assert.Throws<ArgumentException>( () => RepoFeedManager.NormalizeRepoUrl( "   " ) );
        }

        [Test]
        public void NormalizeRepoUrl_SingleSegment_Throws()
        {
            Assert.Throws<ArgumentException>( () => RepoFeedManager.NormalizeRepoUrl( "justowner" ) );
        }

        [Test]
        public void NormalizeRepoUrl_UrlWithOnlyOnePathSegment_Throws()
        {
            Assert.Throws<ArgumentException>( () => RepoFeedManager.NormalizeRepoUrl( "https://github.com/onlyone" ) );
        }

        [Test]
        public void NormalizeRepoUrl_OwnerSlashRepo_TrimsWhitespace()
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( "  octgn/MyGame  " );
            Assert.AreEqual( "octgn", owner );
            Assert.AreEqual( "MyGame", repo );
        }

        [TestCase( "owner/repo", "owner", "repo" )]
        [TestCase( "my-org/my-game", "my-org", "my-game" )]
        [TestCase( "https://github.com/foo/bar", "foo", "bar" )]
        [TestCase( "https://github.com/foo/bar.git", "foo", "bar" )]
        public void NormalizeRepoUrl_VariousFormats_ParsesCorrectly( string input, string expectedOwner, string expectedRepo )
        {
            var (owner, repo) = RepoFeedManager.NormalizeRepoUrl( input );
            Assert.AreEqual( expectedOwner, owner );
            Assert.AreEqual( expectedRepo, repo );
        }

        #endregion

        #region GetManifestUrl

        [Test]
        public void GetManifestUrl_DefaultPath_UsesOctgnManifest()
        {
            var url = RepoFeedManager.GetManifestUrl( "octgn", "MyGame", "main" );
            Assert.AreEqual( "https://raw.githubusercontent.com/octgn/MyGame/main/octgn-manifest.json", url );
        }

        [Test]
        public void GetManifestUrl_CustomPath_UsesCustomPath()
        {
            var url = RepoFeedManager.GetManifestUrl( "octgn", "MyGame", "main", "subfolder/manifest.json" );
            Assert.AreEqual( "https://raw.githubusercontent.com/octgn/MyGame/main/subfolder/manifest.json", url );
        }

        [Test]
        public void GetManifestUrl_NullPath_UsesDefault()
        {
            var url = RepoFeedManager.GetManifestUrl( "octgn", "MyGame", "develop", null );
            Assert.AreEqual( "https://raw.githubusercontent.com/octgn/MyGame/develop/octgn-manifest.json", url );
        }

        [Test]
        public void GetManifestUrl_EmptyPath_UsesDefault()
        {
            var url = RepoFeedManager.GetManifestUrl( "octgn", "MyGame", "main", "" );
            Assert.AreEqual( "https://raw.githubusercontent.com/octgn/MyGame/main/octgn-manifest.json", url );
        }

        [Test]
        public void GetManifestUrl_WhitespacePath_UsesDefault()
        {
            var url = RepoFeedManager.GetManifestUrl( "octgn", "MyGame", "main", "   " );
            Assert.AreEqual( "https://raw.githubusercontent.com/octgn/MyGame/main/octgn-manifest.json", url );
        }

        [Test]
        public void GetManifestUrl_DifferentBranch_IncludesBranch()
        {
            var url = RepoFeedManager.GetManifestUrl( "user", "repo", "v2.0" );
            Assert.AreEqual( "https://raw.githubusercontent.com/user/repo/v2.0/octgn-manifest.json", url );
        }

        #endregion

        #region GetZipballUrl

        [Test]
        public void GetZipballUrl_ConstructsCorrectUrl()
        {
            var url = RepoFeedManager.GetZipballUrl( "octgn", "MyGame", "main" );
            Assert.AreEqual( "https://api.github.com/repos/octgn/MyGame/zipball/main", url );
        }

        [Test]
        public void GetZipballUrl_DifferentBranch_IncludesBranch()
        {
            var url = RepoFeedManager.GetZipballUrl( "user", "repo", "develop" );
            Assert.AreEqual( "https://api.github.com/repos/user/repo/zipball/develop", url );
        }

        [Test]
        public void GetZipballUrl_OwnerAndRepoInUrl()
        {
            var url = RepoFeedManager.GetZipballUrl( "my-org", "my-game", "release/1.0" );
            Assert.IsTrue( url.Contains( "my-org/my-game" ) );
            Assert.IsTrue( url.Contains( "release/1.0" ) );
        }

        #endregion
    }
}
