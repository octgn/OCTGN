using Newtonsoft.Json;
using NUnit.Framework;
using Octgn.Core.Models;

namespace Octgn.Test.Core.Models
{
    [TestFixture]
    public class FeedIndexTests
    {
        [Test]
        public void Deserialize_CompleteIndex_ParsesAllGames()
        {
            var json = @"{
                ""Name"": ""Community Feed"",
                ""Games"": [
                    {
                        ""Guid"": ""aaaa-bbbb"",
                        ""Name"": ""Game One"",
                        ""Repo"": ""octgn/game-one"",
                        ""Branch"": ""main"",
                        ""ManifestPath"": ""game/octgn-manifest.json""
                    },
                    {
                        ""Guid"": ""cccc-dddd"",
                        ""Name"": ""Game Two"",
                        ""Repo"": ""octgn/game-two"",
                        ""Branch"": ""develop""
                    }
                ]
            }";

            var index = JsonConvert.DeserializeObject<FeedIndex>( json );

            Assert.IsNotNull( index );
            Assert.AreEqual( "Community Feed", index.Name );
            Assert.AreEqual( 2, index.Games.Length );
            Assert.AreEqual( "Game One", index.Games[0].Name );
            Assert.AreEqual( "octgn/game-one", index.Games[0].Repo );
            Assert.AreEqual( "Game Two", index.Games[1].Name );
            Assert.AreEqual( "develop", index.Games[1].Branch );
        }

        [Test]
        public void Deserialize_EmptyGames_ReturnsEmptyArray()
        {
            var json = @"{ ""Name"": ""Empty Feed"", ""Games"": [] }";

            var index = JsonConvert.DeserializeObject<FeedIndex>( json );

            Assert.IsNotNull( index );
            Assert.AreEqual( "Empty Feed", index.Name );
            Assert.IsNotNull( index.Games );
            Assert.AreEqual( 0, index.Games.Length );
        }

        [Test]
        public void Deserialize_EntryWithManifestPath_ParsesPath()
        {
            var json = @"{
                ""Games"": [{
                    ""Guid"": ""1234"",
                    ""Name"": ""PathGame"",
                    ""Repo"": ""owner/repo"",
                    ""Branch"": ""main"",
                    ""ManifestPath"": ""subfolder/manifest.json""
                }]
            }";

            var index = JsonConvert.DeserializeObject<FeedIndex>( json );

            Assert.AreEqual( "subfolder/manifest.json", index.Games[0].ManifestPath );
        }

        [Test]
        public void Deserialize_EntryWithoutManifestPath_NullPath()
        {
            var json = @"{
                ""Games"": [{
                    ""Guid"": ""1234"",
                    ""Name"": ""NoPathGame"",
                    ""Repo"": ""owner/repo"",
                    ""Branch"": ""main""
                }]
            }";

            var index = JsonConvert.DeserializeObject<FeedIndex>( json );

            Assert.IsNull( index.Games[0].ManifestPath );
        }

        [Test]
        public void Deserialize_NoGamesProperty_GamesIsNull()
        {
            var json = @"{ ""Name"": ""Feed Only"" }";

            var index = JsonConvert.DeserializeObject<FeedIndex>( json );

            Assert.IsNotNull( index );
            Assert.AreEqual( "Feed Only", index.Name );
            Assert.IsNull( index.Games );
        }

        [Test]
        public void Deserialize_AllEntryFields_PopulatedCorrectly()
        {
            var json = @"{
                ""Games"": [{
                    ""Guid"": ""a1b2c3d4"",
                    ""Name"": ""Full Entry"",
                    ""Repo"": ""myorg/mygame"",
                    ""Branch"": ""v2"",
                    ""ManifestPath"": ""custom/path.json""
                }]
            }";

            var index = JsonConvert.DeserializeObject<FeedIndex>( json );
            var entry = index.Games[0];

            Assert.AreEqual( "a1b2c3d4", entry.Guid );
            Assert.AreEqual( "Full Entry", entry.Name );
            Assert.AreEqual( "myorg/mygame", entry.Repo );
            Assert.AreEqual( "v2", entry.Branch );
            Assert.AreEqual( "custom/path.json", entry.ManifestPath );
        }
    }
}
