// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using log4net;

using Octgn.Library.Networking;

namespace Octgn.Library
{
    public interface IFeedProvider
    {
        IEnumerable<NamedUrl> AllFeeds { get; }
        IEnumerable<NamedUrl> UserFeeds { get; }
        IEnumerable<NamedUrl> LocalFeeds { get; }
        IEnumerable<NamedUrl> ReservedFeeds { get; }

        void AddFeed( NamedUrl feed );

        void RemoveFeed( NamedUrl feed );
    }

    public class FeedProvider : IFeedProvider
    {
        private static ILog Log = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

        #region Singleton

        internal static IFeedProvider SingletonContext { get; set; }

        private static readonly object FeedProviderSingletonLocker = new object();

        public static IFeedProvider Instance {
            get {
                if( SingletonContext == null ) {
                    lock( FeedProviderSingletonLocker ) {
                        if( SingletonContext == null ) {
                            SingletonContext = new FeedProvider();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public IEnumerable<NamedUrl> AllFeeds => ReservedFeeds.Concat( UserFeeds );

        public IEnumerable<NamedUrl> UserFeeds => GetUserFeeds();

        public IEnumerable<NamedUrl> LocalFeeds => _reservedFeeds.Where( x => x.Name == "Local" ).ToArray();

        public IEnumerable<NamedUrl> ReservedFeeds => _reservedFeeds;

        private DateTime cachedFeedsExpireDate;
        private NamedUrl[] cachedFeeds;

        private readonly NamedUrl[] _reservedFeeds;
        private readonly object _feedLock = new object();

        internal FeedProvider() {
            cachedFeedsExpireDate = DateTime.MinValue;
            cachedFeeds = new NamedUrl[0];
            _reservedFeeds = new NamedUrl[] {
                new NamedUrl("All", null, null, null),
                new NamedUrl("OCTGN Official", Config.Instance.Paths.MainOctgnFeed, null, null),
                new NamedUrl("Community Games", Config.Instance.Paths.CommunityFeedPath, null, null),
                new NamedUrl("The Spoils", Config.Instance.Paths.SpoilsFeedPath, null, null),
                new NamedUrl("Local (Developers)", Config.Instance.Paths.LocalFeedPath, null, null),
            };
        }

        public void AddFeed( NamedUrl feed ) {
            lock( _feedLock ) {
                if( ReservedFeeds.Any( x => x.Name.Equals( feed.Name, StringComparison.InvariantCultureIgnoreCase ) ) )
                    return;
                List<NamedUrl> newList = UserFeeds.Where( x => !x.Name.Equals( feed.Name ) )
                    .ToList();

                newList.Add( feed );
                WriteFeedListToDisk( newList );

                cachedFeedsExpireDate = DateTime.MinValue; // Expire cache
            }
        }

        public void RemoveFeed( NamedUrl feed ) {
            lock( _feedLock ) {
                if( ReservedFeeds.Any( x => x.Name.Equals( feed.Name, StringComparison.InvariantCultureIgnoreCase ) ) )
                    return;
                NamedUrl[] remList = UserFeeds.Where( x => !x.Name.Equals( feed.Name ) ).ToArray();
                WriteFeedListToDisk( remList );

                cachedFeedsExpireDate = DateTime.MinValue; // Expire cache
            }
        }

        internal static IEnumerable<NamedUrl> ReadFeedListFromDisk() {
            Stream stream = null;
            bool wasLocked = false;
            while( !X.Instance.File.OpenFile( Config.Instance.Paths.FeedListPath, FileMode.OpenOrCreate, FileShare.None, TimeSpan.FromDays( 1 ), out stream ) ) {
                wasLocked = true;
                Log.Info( "Getting feed list file still locked." );
                Thread.Sleep( 2000 );
            }
            if( wasLocked ) Log.Debug( "Getting feed list file unlocked." );
            using( StreamReader sr = new StreamReader( stream ) ) {
                List<NamedUrl> lines = sr.ReadToEnd()
                    .Replace( "\r", "" )
                    .Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries )
                    .Where( x => !String.IsNullOrWhiteSpace( x.Trim() ) )
                    .Select( x => x.Split( new[] { (char)1 }, StringSplitOptions.RemoveEmptyEntries ) )
                    .Select( x => {
                        if( x.Length != 2 && x.Length != 4 )
                            return null;
                        if( x.Length == 2 ) {
                            return new NamedUrl( x[0].Trim(), x[1].Trim(), null, null );
                        }
                        if( x.Length == 4 ) {
                            return new NamedUrl( x[0].Trim(), x[1].Trim(), x[2].Trim(), x[3].Trim() );
                        }
                        return null;
                    } )
                    .Where( x => x != null ).ToList();

                lines.ForEach( line => line.Url = CorrectMyGetFeed( line.Url ) );

                return lines;
            }
        }

        internal static void WriteFeedListToDisk( IEnumerable<NamedUrl> feeds ) {
            List<NamedUrl> lines = feeds.ToList();

            // correct myGet URLS -- correct them both here before the check to make sure we don't get an http and https version of the same.
            lines.ForEach( line => line.Url = CorrectMyGetFeed( line.Url ) );

            Stream stream = null;
            while( !X.Instance.File.OpenFile( Config.Instance.Paths.FeedListPath, FileMode.Create, FileShare.None, TimeSpan.FromDays( 1 ), out stream ) ) {
                Thread.Sleep( 10 );
            }
            using( StreamWriter sr = new StreamWriter( stream ) ) {
                lines.ForEach( line => sr.WriteLine( line.Name + (char)1 + line.Url + (char)1 + line.Username + (char)1 + line.Password ) );
            }
        }

        private IEnumerable<NamedUrl> GetUserFeeds() {
            lock( _feedLock ) {
                if( DateTime.Now > cachedFeedsExpireDate ) {
                    cachedFeeds = ReadFeedListFromDisk().ToArray();
                    cachedFeedsExpireDate = DateTime.Now.AddMinutes( 1 );
                }

                return cachedFeeds.ToArray();
            }
        }

        private static string CorrectMyGetFeed( string url ) {
            string bad = @"http://www.myget.org";
            string good = @"https://www.myget.org";

            if( url.ToLower().StartsWith( bad ) ) {
                string remainder = url.Substring( bad.Length );
                url = good + remainder;
            }

            return url;
        }
    }
}
