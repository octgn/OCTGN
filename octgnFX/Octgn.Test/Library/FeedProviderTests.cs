// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Linq;
using NUnit.Framework;
using Octgn.Library;
using Octgn.Library.Networking;

namespace Octgn.Test.Library
{

    public class FeedProviderTests
    {
        [Test]
        public void ReservedFeeds_ContainsCommunityGitHubFeed() {
            var curFeedProvider = FeedProvider.Instance;
            try {
                var provider = new FeedProvider();
                FeedProvider.SingletonContext = provider;

                var reserved = provider.ReservedFeeds.ToList();
                Assert.IsTrue( reserved.Any( f => f.Name == "OCTGN Community (GitHub)" ) );
            } finally {
                FeedProvider.SingletonContext = curFeedProvider;
            }
        }

        [Test]
        public void ReservedFeeds_CommunityFeedHasRepoIndexType() {
            var curFeedProvider = FeedProvider.Instance;
            try {
                var provider = new FeedProvider();
                FeedProvider.SingletonContext = provider;

                var communityFeed = provider.ReservedFeeds.FirstOrDefault( f => f.Name == "OCTGN Community (GitHub)" );
                Assert.IsNotNull( communityFeed );
                Assert.AreEqual( FeedType.RepoIndex, communityFeed.FeedType );
            } finally {
                FeedProvider.SingletonContext = curFeedProvider;
            }
        }

        [Test]
        public void ReservedFeeds_NuGetFeedsStillPresent() {
            var curFeedProvider = FeedProvider.Instance;
            try {
                var provider = new FeedProvider();
                FeedProvider.SingletonContext = provider;

                var reserved = provider.ReservedFeeds.ToList();
                Assert.IsTrue( reserved.Any( f => f.Name == "OCTGN Official" ) );
                Assert.IsTrue( reserved.Any( f => f.Name == "Community Games" ) );
            } finally {
                FeedProvider.SingletonContext = curFeedProvider;
            }
        }

        [Test]
        public void ReservedFeeds_NuGetFeedsDefaultToNuGetType() {
            var curFeedProvider = FeedProvider.Instance;
            try {
                var provider = new FeedProvider();
                FeedProvider.SingletonContext = provider;

                var officialFeed = provider.ReservedFeeds.FirstOrDefault( f => f.Name == "OCTGN Official" );
                Assert.IsNotNull( officialFeed );
                Assert.AreEqual( FeedType.NuGet, officialFeed.FeedType );
            } finally {
                FeedProvider.SingletonContext = curFeedProvider;
            }
        }

        [Test]
        public void FeedPersistence_IncludesFeedType() {
            // NamedUrl with a non-default FeedType should preserve it through serialization format
            var feed = new NamedUrl( "Test Repo", "https://example.com/index.json", "user", "pass", FeedType.RepoIndex );
            Assert.AreEqual( FeedType.RepoIndex, feed.FeedType );
            Assert.AreEqual( "Test Repo", feed.Name );
        }

        [Test]
        public void FeedPersistence_OldFormatDefaultsToNuGet() {
            // NamedUrl created with the 4-argument constructor should default to NuGet
            var feed = new NamedUrl( "Old Feed", "https://myget.org/F/test/", null, null );
            Assert.AreEqual( FeedType.NuGet, feed.FeedType );
        }
    }
}
