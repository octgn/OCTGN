using System;
using System.Linq;
using NUnit.Framework;
using Octgn.Core;
using Octgn.Core.Models;

namespace Octgn.Test.Core
{
    [TestFixture]
    public class GameManifestPackageAdapterTests
    {
        private GameManifest CreateTestManifest()
        {
            return new GameManifest
            {
                Guid = "a6c8d2e8-7e8c-11e5-8bcf-feff819cdc9f",
                Name = "Test Card Game",
                Version = "1.2.3",
                Description = "A test card game for unit tests.",
                Authors = new[] { "Author1", "Author2" },
                Tags = new[] { "card", "strategy" },
                RepoOwner = "octgn",
                RepoName = "TestGame",
                Branch = "main",
            };
        }

        [Test]
        public void Id_ReturnsManifestGuid()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( manifest.Guid, adapter.Id );
        }

        [Test]
        public void Title_ReturnsManifestName()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( manifest.Name, adapter.Title );
        }

        [Test]
        public void Description_ReturnsManifestDescription()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( manifest.Description, adapter.Description );
        }

        [Test]
        public void Version_ParsesManifestVersionString()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( new Version( 1, 2, 3, 0 ), adapter.Version.Version );
        }

        [Test]
        public void Version_NullVersion_ReturnsZero()
        {
            var manifest = CreateTestManifest();
            manifest.Version = null;
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( new Version( 0, 0, 0, 0 ), adapter.Version.Version );
        }

        [Test]
        public void Authors_ReturnsManifestAuthors()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            var authors = adapter.Authors.ToList();
            Assert.AreEqual( 2, authors.Count );
            Assert.AreEqual( "Author1", authors[0] );
            Assert.AreEqual( "Author2", authors[1] );
        }

        [Test]
        public void Authors_NullAuthors_ReturnsEmpty()
        {
            var manifest = CreateTestManifest();
            manifest.Authors = null;
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.IsNotNull( adapter.Authors );
            Assert.IsFalse( adapter.Authors.Any() );
        }

        [Test]
        public void Tags_JoinsArrayWithSpaces()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( "card strategy", adapter.Tags );
        }

        [Test]
        public void Tags_NullTags_ReturnsEmpty()
        {
            var manifest = CreateTestManifest();
            manifest.Tags = null;
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( "", adapter.Tags );
        }

        [Test]
        public void IsAbsoluteLatestVersion_ReturnsTrue()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.IsTrue( adapter.IsAbsoluteLatestVersion );
        }

        [Test]
        public void ProjectUrl_PointsToGitHubRepo()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreEqual( "https://github.com/octgn/TestGame", adapter.ProjectUrl.ToString() );
        }

        [Test]
        public void ProjectUrl_NullRepoOwner_ReturnsNull()
        {
            var manifest = CreateTestManifest();
            manifest.RepoOwner = null;
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.IsNull( adapter.ProjectUrl );
        }

        [Test]
        public void Manifest_ExposesOriginalManifest()
        {
            var manifest = CreateTestManifest();
            var adapter = new GameManifestPackageAdapter( manifest );
            Assert.AreSame( manifest, adapter.Manifest );
        }

        [Test]
        public void Constructor_NullManifest_Throws()
        {
            Assert.Throws<ArgumentNullException>( () => new GameManifestPackageAdapter( null ) );
        }
    }
}
