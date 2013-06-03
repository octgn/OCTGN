namespace Octgn.Test.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;

    using FakeItEasy;

    using NUnit.Framework;

    using Octgn.Core;
    using Octgn.Library;
    using Octgn.Library.Exceptions;
    using Octgn.Library.Networking;

    public class GameFeedManagerTests
    {
        [Test]
        public void GetPackages_NoDuplicates()
        {
            //var ret = (GameFeedManager.Get() as GameFeedManager).GetPackages();
            //var packList = new List<string>();
            //foreach (var pack in ret)
            //{
            //    if (packList.Any(x => x == pack.Id)) Assert.Fail("Found duplicate of a package");
            //    else packList.Add(pack.Id);
            //}
            
            //Console.WriteLine(ret.First().Version.Version);
        }

        [Test]
        public void ValidateFeedUrl_FailsOnBadUri()
        {
            var ret = GameFeedManager.Get();
            var badUri = "sdflakwejfasw";
            Assert.False(ret.ValidateFeedUrl(badUri));
        }

        [Test]
        public void ValidateFeedUrl_FailsOnNotARepo()
        {
            var ret = GameFeedManager.Get();
            var goodUriButNotAFeed = "http://en.wikipedia.org/wiki/Human_feces/";
            Assert.False(ret.ValidateFeedUrl(goodUriButNotAFeed));
        }

        #region AddFeed
        [Test]
        public void AddFeed_CallsValidate()
        {
            var fakeSimpleConfig = A.Fake<ISimpleConfig>();
            A.CallTo(fakeSimpleConfig).DoesNothing();
            A.CallTo(()=>fakeSimpleConfig.GetFeeds(false)).Returns(new List<NamedUrl>());
            var curSimpleConfig = SimpleConfig.Get();
            SimpleConfig.SingletonContext = fakeSimpleConfig;

            var cur = GameFeedManager.Get();
            GameFeedManager.SingletonContext = A.Fake<IGameFeedManager>(x=>x.Wrapping(cur));
            A.CallTo(() => GameFeedManager.SingletonContext.ValidateFeedUrl(A<string>._)).Returns(true);
            GameFeedManager.Get().AddFeed("asdfASDFasdfASDF","asdf");
            A.CallTo(()=>GameFeedManager.SingletonContext.ValidateFeedUrl(A<string>._)).MustHaveHappened(Repeated.Exactly.Once);
            GameFeedManager.SingletonContext = cur;

            SimpleConfig.SingletonContext = curSimpleConfig;
        }

        [Test]
        public void AddFeed_ThrowsIfValidateFails()
        {
            var cur = GameFeedManager.Get();
            var fake = A.Fake<IGameFeedManager>(x=>x.Wrapping(cur));
            GameFeedManager.SingletonContext = fake;
            A.CallTo(() => fake.ValidateFeedUrl(A<string>._)).Returns(false);

            bool pass = false;
            try
            {
                fake.AddFeed("asdf", "asdf");
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException is UserMessageException) pass = true;
            }

            GameFeedManager.SingletonContext = cur;

            Assert.True(pass);
        }

        [Test]
        public void AddFeed_ThrowsIfGameAlreadyExists()
        { 
            bool pass = false;
            var curSimpleConfig = SimpleConfig.Get();
            var curGameFeedManager = GameFeedManager.Get();
            var gameListWithFeed = new List<NamedUrl>();
            gameListWithFeed.Add(new NamedUrl("asdf","asdf"));
            try
            {
                // Fake out the simple config so it returns what we want.
                // This also sets the singleton context to this fake object.
                var fakeSimpleConfig = A.Fake<ISimpleConfig>();
                A.CallTo(fakeSimpleConfig).DoesNothing();
                A.CallTo(() => fakeSimpleConfig.GetFeeds(false)).Returns(gameListWithFeed);
                SimpleConfig.SingletonContext = fakeSimpleConfig;

                // Fake out the GameFeedManager so that we can make sure ValidateFeed does what we want.
                var fakeGf = A.Fake<IGameFeedManager>(x=>x.Wrapping(curGameFeedManager));
                GameFeedManager.SingletonContext = fakeGf;
                A.CallTo(() => fakeGf.ValidateFeedUrl(A<string>._)).Returns(true);

                // Now we pass in a game feed by the name asdf, and it should throw an error since it
                // already exists in the list above.
               
                try
                {
                    fakeGf.AddFeed("asdf","asdf");
                }
                catch (TargetInvocationException e)
                {
                    if (e.InnerException is UserMessageException) pass = true;
                }
            }
            finally
            {
                SimpleConfig.SingletonContext = curSimpleConfig;
                GameFeedManager.SingletonContext = curGameFeedManager;
            }
            Assert.True(pass);
        }

        [Test]
        public void AddFeed_CallsSimpleConfigAddFeedIfItPasses()
        {
            bool pass = false;
            var curSimpleConfig = SimpleConfig.Get();
            var curGameFeedManager = GameFeedManager.Get();
            var gameListWithFeed = new List<NamedUrl>();
            try
            {
                // Fake out the simple config so it returns what we want.
                // This also sets the singleton context to this fake object.
                var fakeSimpleConfig = A.Fake<ISimpleConfig>();
                A.CallTo(fakeSimpleConfig).DoesNothing();
                A.CallTo(() => fakeSimpleConfig.GetFeeds(false)).Returns(gameListWithFeed);
                SimpleConfig.SingletonContext = fakeSimpleConfig;

                // Fake out the GameFeedManager so that we can make sure ValidateFeed does what we want.
                var fakeGf = A.Fake<IGameFeedManager>(x => x.Wrapping(curGameFeedManager));
                GameFeedManager.SingletonContext = fakeGf;
                A.CallTo(() => fakeGf.ValidateFeedUrl(A<string>._)).Returns(true);

                // Now we pass in a game feed by the name asdf, and it should throw an error since it
                // already exists in the list above.

                fakeGf.AddFeed("asdf","asdf");

                // Make sure that SimpleConfig.AddFeed was called once
                A.CallTo(()=>fakeSimpleConfig.AddFeed(A<NamedUrl>._)).MustHaveHappened(Repeated.Exactly.Once);
                Assert.Pass();
            }
            finally
            {
                SimpleConfig.SingletonContext = curSimpleConfig;
                GameFeedManager.SingletonContext = curGameFeedManager;
            }
            Assert.Fail();
        }
        #endregion AddFeed

        [Test]
        public void GetFeeds_JustCallsSimpleConfigGetFeeds()
        {
            var curSimple = SimpleConfig.Get();
            try
            {
                var fake = A.Fake<ISimpleConfig>();
                A.CallTo(fake).DoesNothing();
                SimpleConfig.SingletonContext = fake;

                var res = GameFeedManager.Get().GetFeeds();
                Assert.IsNull(res);
                A.CallTo(() => fake.GetFeeds(false)).MustHaveHappened(Repeated.Exactly.Once);
            }
            finally
            {
                SimpleConfig.SingletonContext = curSimple;
            }
        }

        [Test]
        public void RemoveFeed_JustCallsSimpleConfigRemoveFeed()
        {
            var curSimple = SimpleConfig.Get();
            try
            {
                var fake = A.Fake<ISimpleConfig>();
                A.CallTo(fake).DoesNothing();
                SimpleConfig.SingletonContext = fake;

                GameFeedManager.Get().RemoveFeed("asdf");
                
                A.CallTo(() => fake.RemoveFeed(A<NamedUrl>._)).MustHaveHappened(Repeated.Exactly.Once);
            }
            finally
            {
                SimpleConfig.SingletonContext = curSimple;
            }
        }
    }
}