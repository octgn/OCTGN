using Newtonsoft.Json;
using NUnit.Framework;
using Octgn.Core.Models;

namespace Octgn.Test.Core.Models
{
    [TestFixture]
    public class GameManifestTests
    {
        [Test]
        public void Deserialize_CompleteManifest_AllFieldsPopulated()
        {
            var json = @"{
                ""Guid"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""Name"": ""Test Game"",
                ""Version"": ""1.2.3"",
                ""VersionDate"": ""2026-01-15"",
                ""Description"": ""A test game definition"",
                ""Authors"": [""Author1"", ""Author2""],
                ""MinimumOctgnVersion"": ""3.4.400.0"",
                ""GamePath"": ""game-files"",
                ""Tags"": [""card-game"", ""strategy""],
                ""Changelog"": ""Initial release""
            }";

            var manifest = JsonConvert.DeserializeObject<GameManifest>( json );

            Assert.IsNotNull( manifest );
            Assert.AreEqual( "a1b2c3d4-e5f6-7890-abcd-ef1234567890", manifest.Guid );
            Assert.AreEqual( "Test Game", manifest.Name );
            Assert.AreEqual( "1.2.3", manifest.Version );
            Assert.AreEqual( "2026-01-15", manifest.VersionDate );
            Assert.AreEqual( "A test game definition", manifest.Description );
            Assert.AreEqual( 2, manifest.Authors.Length );
            Assert.AreEqual( "Author1", manifest.Authors[0] );
            Assert.AreEqual( "Author2", manifest.Authors[1] );
            Assert.AreEqual( "3.4.400.0", manifest.MinimumOctgnVersion );
            Assert.AreEqual( "game-files", manifest.GamePath );
            Assert.AreEqual( 2, manifest.Tags.Length );
            Assert.AreEqual( "card-game", manifest.Tags[0] );
            Assert.AreEqual( "strategy", manifest.Tags[1] );
            Assert.AreEqual( "Initial release", manifest.Changelog );
        }

        [Test]
        public void Deserialize_MinimalManifest_OptionalFieldsNull()
        {
            var json = @"{
                ""Guid"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
                ""Name"": ""Minimal Game""
            }";

            var manifest = JsonConvert.DeserializeObject<GameManifest>( json );

            Assert.IsNotNull( manifest );
            Assert.AreEqual( "a1b2c3d4-e5f6-7890-abcd-ef1234567890", manifest.Guid );
            Assert.AreEqual( "Minimal Game", manifest.Name );
            Assert.IsNull( manifest.Version );
            Assert.IsNull( manifest.VersionDate );
            Assert.IsNull( manifest.Description );
            Assert.IsNull( manifest.Authors );
            Assert.IsNull( manifest.MinimumOctgnVersion );
            Assert.IsNull( manifest.GamePath );
            Assert.IsNull( manifest.Tags );
            Assert.IsNull( manifest.Changelog );
        }

        [Test]
        public void Deserialize_EmptyJson_ReturnsEmptyObject()
        {
            var json = "{}";

            var manifest = JsonConvert.DeserializeObject<GameManifest>( json );

            Assert.IsNotNull( manifest );
            Assert.IsNull( manifest.Guid );
            Assert.IsNull( manifest.Name );
        }

        [Test]
        public void RepoFields_NotInJson_DefaultToNull()
        {
            var json = @"{ ""Name"": ""Test"" }";

            var manifest = JsonConvert.DeserializeObject<GameManifest>( json );

            Assert.IsNull( manifest.RepoOwner );
            Assert.IsNull( manifest.RepoName );
            Assert.IsNull( manifest.Branch );
        }

        [Test]
        public void RepoFields_CanBeSetProgrammatically()
        {
            var manifest = new GameManifest();
            manifest.RepoOwner = "octgn";
            manifest.RepoName = "MyGame";
            manifest.Branch = "main";

            Assert.AreEqual( "octgn", manifest.RepoOwner );
            Assert.AreEqual( "MyGame", manifest.RepoName );
            Assert.AreEqual( "main", manifest.Branch );
        }

        [Test]
        public void Deserialize_EmptyAuthorsArray_ReturnsEmptyArray()
        {
            var json = @"{ ""Authors"": [] }";

            var manifest = JsonConvert.DeserializeObject<GameManifest>( json );

            Assert.IsNotNull( manifest.Authors );
            Assert.AreEqual( 0, manifest.Authors.Length );
        }
    }
}
