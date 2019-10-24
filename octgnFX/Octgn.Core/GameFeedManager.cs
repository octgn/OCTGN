// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using Octgn.Library;
using Octgn.Library.Exceptions;
using Octgn.Library.ExtensionMethods;
using Octgn.Library.Networking;

using log4net;
using Octgn.Library.Localization;

namespace Octgn.Core
{
    public interface IGameFeedManager : IDisposable
    {
        event Action<String> OnUpdateMessage;
        void CheckForUpdates( bool localOnly = false, Action<int, int> onProgressUpdate = null );
        IEnumerable<NamedUrl> GetFeeds( bool localOnly = false );
        void AddFeed( string name, string feed, string username, string password );
        void RemoveFeed( string name );
        FeedValidationResult ValidateFeedUrl( string url, string username, string password );
        IEnumerable<IPackage> GetPackages( NamedUrl url );
        void ExtractPackage( string directory, IPackage package, Action<int, int> onProgressUpdate = null );
        void AddToLocalFeed( string file );
        event EventHandler OnUpdateFeedList;
    }

    public class GameFeedManager : IGameFeedManager
    {
        #region Singleton

        internal static IGameFeedManager SingletonContext { get; set; }

        private static readonly object GameFeedManagerSingletonLocker = new object();

        public static IGameFeedManager Get() {
            lock( GameFeedManagerSingletonLocker ) {
                if( SingletonContext != null ) return SingletonContext;
            }
            return new GameFeedManager();
        }
        public GameFeedManager() {
            lock( GameFeedManagerSingletonLocker ) {
                if( SingletonContext != null )
                    throw new InvalidOperationException( "Game feed manager already exists!" );
                SingletonContext = this;
                NuGet.HttpClient.DefaultCredentialProvider = new OctgnFeedCredentialProvider();
            }
        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

        public event EventHandler OnUpdateFeedList;

        public event Action<string> OnUpdateMessage;

        protected virtual void FireOnUpdateMessage( string obj, params object[] args ) {
            this.OnUpdateMessage?.Invoke( string.Format( obj, args ) );
        }

        public void CheckForUpdates( bool localOnly = false, Action<int, int> onProgressUpdate = null ) {
            if( onProgressUpdate == null ) onProgressUpdate = ( i, i1 ) => { };
            Log.Info( "Checking for updates" );
            try {
                foreach( var g in DataManagers.GameManager.Get().Games ) {
                    FireOnUpdateMessage( L.D.UpdateMessage__CheckingGame_Format, g.Name );
                    Log.DebugFormat( "Checking for updates for game {0} {1}", g.Id, g.Name );
                    foreach( var f in this.GetFeeds( localOnly ) ) {
                        Log.DebugFormat( "Getting feed {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url );
                        if( string.IsNullOrWhiteSpace( f.Url ) ) continue;
                        var repo = PackageRepositoryFactory.Default.CreateRepository( f.Url );
                        Log.DebugFormat( "Repo Created {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url );
                        IPackage newestPackage = default( IPackage );
                        try {
                            X.Instance.Retry(
                                () => {
                                    newestPackage =
                                        repo.GetPackages()
                                            .Where( x => x.Id.ToLower() == g.Id.ToString().ToLower() )
                                            .ToList()
                                            .OrderByDescending( x => x.Version.Version )
                                            .FirstOrDefault( x => x.IsAbsoluteLatestVersion );
                                } );
                        } catch( WebException e ) {
                            Log.WarnFormat( "Could not get feed {0} {1}", f.Name, f.Url );
                            Log.Warn( "", e );
                            continue;
                        }
                        Log.DebugFormat( "Grabbed newest package for {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url );
                        if( newestPackage == null ) {
                            Log.DebugFormat( "No package found for {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url );
                            continue;
                        }
                        Log.DebugFormat( "Got feed {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url );
                        Log.DebugFormat( "Installed Version: {0} Feed Version: {1} for {2} {3} {4} {5}", g.Version, newestPackage.Version.Version, g.Id, g.Name, f.Name, f.Url );
                        var gameVersion = new SemanticVersion( g.Version );
                        if( newestPackage.Version.Version.CompareTo( gameVersion.Version ) > 0 ) {
                            FireOnUpdateMessage( L.D.UpdateMessage__UpdatingGame_Format, g.Name, g.Version, newestPackage.Version.Version );
                            Log.DebugFormat(
                                "Update found. Updating from {0} to {1} for {2} {3} {4} {5}", g.Version, newestPackage.Version.Version, g.Id, g.Name, f.Name, f.Url );
                            DataManagers.GameManager.Get().InstallGame( newestPackage, onProgressUpdate );
                            Log.DebugFormat( "Updated game finished for {0} {1} {2} {3}", g.Id, g.Name, f.Name, f.Url );
                            break;
                        }
                    }
                }

            } catch( Exception e ) {
                Log.Warn( "Error checking for updates", e );
            } finally {
                Log.Info( "Check for updates finished" );
            }
        }

        /// <summary>
        /// Gets all saved game feeds
        /// </summary>
        /// <param name="localOnly">Only retrieves items from the local feed</param>
        /// <returns>Saved game feeds</returns>
        public IEnumerable<NamedUrl> GetFeeds( bool localOnly = false ) {
            Log.Info( "Getting Feeds" );
            if( localOnly ) return FeedProvider.Instance.LocalFeeds;
            else return FeedProvider.Instance.AllFeeds;
        }

        /// <summary>
        /// Add a feed url to the system.
        /// </summary>
        /// <exception cref="UserMessageException">If the feed name already exists or the feed is invalid.</exception>
        /// <param name="name">Feed name</param>
        /// <param name="feed">Feed url</param>
        /// <param name="username">Feed Username(Null if none)</param>
        /// <param name="password">Feed Password(Null if none)</param>
        public void AddFeed( string name, string feed, string username, string password ) {
            try {
                Log.InfoFormat( "Validating feed for {0} {1}", name, feed );
                var result = SingletonContext.ValidateFeedUrl( feed, username, password );
                if( result != FeedValidationResult.Valid ) {
                    Log.InfoFormat( "Feed not valid for {0} {1}: {2}", name, feed, result );
                    throw new UserMessageException( "{0} is not a valid feed. {1}", feed, result );
                }
                Log.InfoFormat( "Checking if feed name already exists for {0} {1}", name, feed );
                if( FeedProvider.Instance.AllFeeds.Any( x => x.Name.ToLower() == name.ToLower() ) ) {
                    Log.InfoFormat( "Feed name already exists for {0} {1}", name, feed );
                    throw new UserMessageException( "Feed name {0} already exists.", name );
                }
                Log.InfoFormat( "Adding feed {0} {1}", name, feed );
                FeedProvider.Instance.AddFeed( new NamedUrl( name, feed, username, password ) );
                Log.InfoFormat( "Firing update feed list {0} {1}", name, feed );
                this.FireOnUpdateFeedList();
                Log.InfoFormat( "Feed {0} {1} added.", name, feed );

            } finally {
                Log.InfoFormat( "Finished {0} {1}", name, feed );
            }
        }

        /// <summary>
        /// Remove a feed url from the system.
        /// </summary>
        /// <param name="name">Feed name</param>
        public void RemoveFeed( string name ) {
            Log.InfoFormat( "Removing feed {0}", name );
            var f = FeedProvider.Instance.UserFeeds.FirstOrDefault( x => x.Name.Equals( name, StringComparison.InvariantCultureIgnoreCase ) );
            if( f == null ) {
                Log.DebugFormat( "[RemoveFeed] Feed {0} not found." );
                return;
            }
            FeedProvider.Instance.RemoveFeed( f );
            Log.InfoFormat( "Firing update feed list {0}", name );
            this.FireOnUpdateFeedList();
            Log.InfoFormat( "Removed feed {0}", name );
        }

        public void AddToLocalFeed( string file ) {
            try {
                Log.InfoFormat( "Verifying {0}", file );
                this.VerifyPackage( file );
                Log.InfoFormat( "Creating Install Path {0}", file );
                var fi = new FileInfo( file );
                var newFileName = fi.Name.Replace( fi.Extension, ".nupkg" );
                var newpath = Path.Combine( Config.Instance.Paths.LocalFeedPath, newFileName );
                Log.InfoFormat( "Adding to local feed {0} to {1}", file, newpath );
                if( !File.Exists( file ) ) {
                    Log.InfoFormat( "o8g magically disappeared {0}", file );
                    return;
                }
                fi.MegaCopyTo( newpath );
                Log.InfoFormat( "Firing update feed list {0}", file );
                this.FireOnUpdateFeedList();
                Log.InfoFormat( "Feed {0} Added at {1}", file, newpath );

            } finally {
                Log.InfoFormat( "Finished {0}", file );
            }
        }

        internal void VerifyPackage( string filename ) {
            try {
                Log.InfoFormat( "Creating verify path {0}", filename );
                var fi = new FileInfo( filename );
                var tempPath =
                    new FileInfo(
                        Path.Combine(
                            Path.GetTempPath(),
                            "octgn",
                            Guid.NewGuid().ToString(),
                            fi.Name.Replace( fi.Extension, ".nupkg" ) ) );
                if( !File.Exists( filename ) ) {
                    Log.InfoFormat( "Package magically disappeared {0}", filename );
                    return;
                }
                Log.InfoFormat( "Creating directory {0}", filename );
                if( !Directory.Exists( tempPath.Directory.FullName ) ) Directory.CreateDirectory( tempPath.Directory.FullName );
                Log.InfoFormat( "Copying file {0}", filename );
                fi.MegaCopyTo( tempPath );
                Log.InfoFormat( "Creating repo to make sure it loads {0}", filename );
                var repo = new LocalPackageRepository( tempPath.Directory.FullName );
                Log.InfoFormat( "Loading repo into array to make sure it works {0}", filename );
                var arr = repo.GetPackages().ToArray();
                Log.InfoFormat( "Fully verified {0}", filename );
            } catch( Exception ) {
                Log.WarnFormat( "Package not valid {0}", filename );
                throw new UserMessageException( L.D.Exception__FileIsInvalid_Format, filename );
            } finally {
                Log.InfoFormat( "Finished {0}", filename );
            }
        }

        public IEnumerable<IPackage> GetPackages( NamedUrl url ) {
            try {
                if( url == null ) {
                    Log.Info( "Getting packages for null NamedUrl" );
                    return new List<IPackage>();
                }
                Log.InfoFormat( "Getting packages for feed {0}:{1}", url.Name, url.Url );
                var ret = new List<IPackage>();
                ret = PackageRepositoryFactory.Default.CreateRepository( url.Url ).GetPackages().ToList();
                Log.InfoFormat( "Finished getting packages for feed {0}:{1}", url.Name, url.Url );
                return ret;

            } finally {
                Log.InfoFormat( "Finished" );
            }
        }

        public void ExtractPackage( string directory, IPackage package, Action<int, int> onProgressUpdate = null ) {
            try {
                if( onProgressUpdate == null ) onProgressUpdate = ( i, i1 ) => { };
                Log.InfoFormat( "Extracting package {0} {1}", package.Id, directory );
                onProgressUpdate( -1, 1 );
                var files = package.GetFiles().ToArray();
                var curFileNum = 0;
                onProgressUpdate( curFileNum, files.Length );
                foreach( var file in files ) {
                    try {
                        Log.DebugFormat( "Got file {0} {1} {2}", file.Path, package.Id, directory );
                        var p = Path.Combine( directory, file.Path );
                        var fi = new FileInfo( p );
                        var dir = fi.Directory.FullName;
                        Log.DebugFormat( "Creating directory {0} {1} {2}", dir, package.Id, directory );
                        Directory.CreateDirectory( dir );
                        var byteList = new List<byte>();
                        Log.DebugFormat( "Reading file {0} {1}", package.Id, directory );
                        using( var sr = new BinaryReader( file.GetStream() ) ) {
                            var buffer = new byte[1024];
                            var len = sr.Read( buffer, 0, 1024 );
                            while( len > 0 ) {
                                byteList.AddRange( buffer.Take( len ) );
                                Array.Clear( buffer, 0, buffer.Length );
                                len = sr.Read( buffer, 0, 1024 );
                            }
                            Log.DebugFormat( "Writing file {0} {1}", package.Id, directory );
                            File.WriteAllBytes( p, byteList.ToArray() );
                            Log.DebugFormat( "Wrote file {0} {1}", package.Id, directory );
                        }
                        curFileNum++;
                        onProgressUpdate( curFileNum, files.Length );

                    } catch( Exception e ) {
                        Log.ErrorFormat( "ExtractPackage Error {0} {1} {2}\n{3}", file.Path, package.Id, directory, e.ToString() );
                        throw;
                    }
                }
                Log.DebugFormat( "No Errors {0} {1}", package.Id, directory );
            } finally {
                onProgressUpdate( -1, 1 );
                Log.InfoFormat( "Finished {0} {1}", package.Id, directory );
            }
        }

        /// <summary>
        /// Make sure a feed url is valid.
        /// This doesn't check to make sure it has octgn games on it, it only
        /// checks to make sure it's a valid nuget feed, and sometimes it's even
        /// wrong when it check that, so don't 100% rely on this for validation.
        /// </summary>
        /// <param name="feed">Feed url</param>
        /// <param name="username">Feed Username</param>
        /// <param name="password">Feed Password</param>
        /// <returns><see cref="FeedValidationResult"/></returns>
        public FeedValidationResult ValidateFeedUrl( string feed, string username, string password ) {
            Log.InfoFormat( "Validating feed url {0}", feed );
            if( PathValidator.IsValidUrl( feed ) && PathValidator.IsValidSource( feed ) ) {
                Log.InfoFormat( "Path Validator says feed {0} is valid", feed );
                OctgnFeedCredentialProvider.AddTemp( feed, username, password );
                try {
                    Log.InfoFormat( "Trying to query feed {0}", feed );
                    var repo = PackageRepositoryFactory.Default.CreateRepository( feed );
                    Log.InfoFormat( "Loading feed to list {0}", feed );
                    var list = repo.GetPackages().ToList();
                    // This happens so that enumerating the list isn't optimized away.
                    foreach( var l in list ) {
                        System.Diagnostics.Trace.WriteLine( l.Id );
                    }
                    Log.InfoFormat( "Queried feed {0}, feed is valid", feed );
                    return FeedValidationResult.Valid;
                } catch( WebException e ) {
                    if( (e.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized )
                        return FeedValidationResult.RequiresAuthentication;
                    Log.WarnFormat( "{0} is an invalid feed. StatusCode={1}", feed, (e.Response as HttpWebResponse).StatusCode );
                } catch( Exception ) {
                    Log.WarnFormat( "{0} is an invalid feed.", feed );
                }
                OctgnFeedCredentialProvider.RemoveTemp( feed );
                return FeedValidationResult.InvalidUrl;
            } else {
                return FeedValidationResult.InvalidFormat;
            }
            //Log.InfoFormat("Path validator failed for feed {0}", feed);
            //return FeedValidationResult.Unknown;
        }

        internal void FireOnUpdateFeedList() {
            Log.Info( "Enter" );
            if( OnUpdateFeedList != null ) {
                OnUpdateFeedList( this, null );
            }
            Log.Info( "Exit" );
        }

        public void Dispose() {
            Log.Info( "Dispose called" );
            OnUpdateFeedList = null;
            Log.Info( "Dispose finished" );
        }
    }

    public enum FeedValidationResult
    {
        Valid, InvalidFormat, InvalidUrl, RequiresAuthentication, Unknown
    }

    public class OctgnFeedCredentialProvider : ICredentialProvider
    {
        protected static Dictionary<string, NetworkCredential> TempCredentials = new Dictionary<string, NetworkCredential>( StringComparer.InvariantCultureIgnoreCase );

        public static void AddTemp( string feed, string username, string password ) {
            var f = new Uri( feed ).ToString();
            TempCredentials[f] = new NetworkCredential( username, password );
        }

        public static void RemoveTemp( string feed ) {
            NetworkCredential ret = null;
            if( TempCredentials.TryGetValue( feed, out ret ) ) {
                TempCredentials.Remove( feed );
            }
        }

        public ICredentials GetCredentials( Uri uri, IWebProxy proxy, CredentialType credentialType, bool retrying ) {
            if( retrying )
                return null;

            if( credentialType == CredentialType.ProxyCredentials )
                return null;

            NetworkCredential ret = null;
            if( TempCredentials.TryGetValue( uri.ToString(), out ret ) ) {
                TempCredentials.Remove( uri.ToString() );
                return ret;
            }

            var feed = FeedProvider.Instance.AllFeeds.FirstOrDefault( x => string.Equals(x.Url, uri.ToString(), StringComparison.InvariantCultureIgnoreCase ) );
            if( feed == null )
                return null;

            if( String.IsNullOrWhiteSpace( feed.Username ) || String.IsNullOrWhiteSpace( feed.Password ) )
                return null;

            return new NetworkCredential( feed.Username, feed.Password );
        }
    }
}
