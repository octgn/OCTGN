using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

using log4net;
using Newtonsoft.Json;

using Octgn.Core.Models;
using Octgn.Library;

namespace Octgn.Core
{
    public class RepoFeedManager
    {
        internal static ILog Log = LogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

        private static readonly HttpClient _httpClient;

        private static readonly HttpClient _apiClient;

        static RepoFeedManager()
        {
            // .NET 4.7 defaults to older TLS; GitHub requires TLS 1.2+
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // General client for raw content (manifests, feed indexes)
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue( "OCTGN", "1.0" ) );

            // API client with GitHub API accept header (for zipball downloads)
            _apiClient = new HttpClient();
            _apiClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue( "OCTGN", "1.0" ) );
            _apiClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue( "application/vnd.github+json" ) );
        }

        /// <summary>
        /// Normalize a repo string ("owner/repo" or full GitHub URL) into owner and repo components.
        /// </summary>
        public static (string owner, string repo) NormalizeRepoUrl( string repoStr )
        {
            if( string.IsNullOrWhiteSpace( repoStr ) )
                throw new ArgumentException( "Repo string cannot be null or empty.", nameof( repoStr ) );

            repoStr = repoStr.Trim().TrimEnd( '/' );

            // Handle full GitHub URLs
            if( repoStr.StartsWith( "http://", StringComparison.OrdinalIgnoreCase ) ||
                repoStr.StartsWith( "https://", StringComparison.OrdinalIgnoreCase ) )
            {
                var uri = new Uri( repoStr );
                var segments = uri.AbsolutePath.Trim( '/' ).Split( '/' );
                if( segments.Length >= 2 )
                    return (segments[0], segments[1].Replace( ".git", "" ));

                throw new ArgumentException( "Invalid GitHub URL: " + repoStr, nameof( repoStr ) );
            }

            // Handle "owner/repo" format
            var parts = repoStr.Split( '/' );
            if( parts.Length == 2 && !string.IsNullOrWhiteSpace( parts[0] ) && !string.IsNullOrWhiteSpace( parts[1] ) )
                return (parts[0], parts[1]);

            throw new ArgumentException( "Invalid repo string. Expected 'owner/repo' or a GitHub URL: " + repoStr, nameof( repoStr ) );
        }

        /// <summary>
        /// Build the raw.githubusercontent.com URL for a manifest file.
        /// </summary>
        public static string GetManifestUrl( string owner, string repo, string branch, string manifestPath = null )
        {
            var path = string.IsNullOrWhiteSpace( manifestPath ) ? "octgn-manifest.json" : manifestPath;
            return string.Format( "https://raw.githubusercontent.com/{0}/{1}/{2}/{3}", owner, repo, branch, path );
        }

        /// <summary>
        /// Build the GitHub API zipball URL for a branch.
        /// </summary>
        public static string GetZipballUrl( string owner, string repo, string branch )
        {
            return string.Format( "https://api.github.com/repos/{0}/{1}/zipball/{2}", owner, repo, branch );
        }

        /// <summary>
        /// Fetch and parse a game manifest from a GitHub repo.
        /// </summary>
        public async Task<GameManifest> FetchManifestAsync( string owner, string repo, string branch, string manifestPath = null )
        {
            var url = GetManifestUrl( owner, repo, branch, manifestPath );
            Log.InfoFormat( "Fetching manifest from {0}", url );

            try
            {
                var json = await _httpClient.GetStringAsync( url ).ConfigureAwait( false );
                var manifest = JsonConvert.DeserializeObject<GameManifest>( json );
                if( manifest != null )
                {
                    manifest.RepoOwner = owner;
                    manifest.RepoName = repo;
                    manifest.Branch = branch;
                }
                return manifest;
            }
            catch( Exception ex )
            {
                Log.WarnFormat( "Failed to fetch manifest from {0}: {1}", url, ex.Message );
                return null;
            }
        }

        /// <summary>
        /// Fetch and parse a feed index JSON file.
        /// </summary>
        public async Task<FeedIndex> FetchFeedIndexAsync( string indexUrl )
        {
            Log.InfoFormat( "Fetching feed index from {0}", indexUrl );

            try
            {
                var json = await _httpClient.GetStringAsync( indexUrl ).ConfigureAwait( false );
                return JsonConvert.DeserializeObject<FeedIndex>( json );
            }
            catch( Exception ex )
            {
                Log.WarnFormat( "Failed to fetch feed index from {0}: {1}", indexUrl, ex.Message );
                return null;
            }
        }

        /// <summary>
        /// Fetch all game manifests listed in a feed index.
        /// </summary>
        public async Task<List<GameManifest>> FetchGamesFromFeedAsync( string indexUrl )
        {
            var results = new List<GameManifest>();
            var index = await FetchFeedIndexAsync( indexUrl ).ConfigureAwait( false );
            if( index?.Games == null )
                return results;

            foreach( var entry in index.Games )
            {
                try
                {
                    var (owner, repo) = NormalizeRepoUrl( entry.Repo );
                    var branch = string.IsNullOrWhiteSpace( entry.Branch ) ? "main" : entry.Branch;
                    var manifest = await FetchManifestAsync( owner, repo, branch, entry.ManifestPath ).ConfigureAwait( false );
                    if( manifest != null )
                        results.Add( manifest );
                }
                catch( Exception ex )
                {
                    Log.WarnFormat( "Error fetching manifest for game {0} from repo {1}: {2}", entry.Name, entry.Repo, ex.Message );
                }
            }

            return results;
        }

        /// <summary>
        /// Download a repo ZIP and install game files to the database path.
        /// </summary>
        public async Task InstallFromRepoAsync( string owner, string repo, string branch, string gamePath, string gameId )
        {
            var zipUrl = GetZipballUrl( owner, repo, branch );
            Log.InfoFormat( "Downloading repo zip from {0}", zipUrl );

            var tempDir = Path.Combine( Path.GetTempPath(), "octgn-repo-" + Guid.NewGuid().ToString( "N" ) );
            var tempZip = tempDir + ".zip";

            try
            {
                // Download the zipball (use API client with GitHub Accept header)
                using( var response = await _apiClient.GetAsync( zipUrl, HttpCompletionOption.ResponseHeadersRead ).ConfigureAwait( false ) )
                {
                    response.EnsureSuccessStatusCode();
                    using( var fs = new FileStream( tempZip, FileMode.Create, FileAccess.Write, FileShare.None ) )
                    {
                        await response.Content.CopyToAsync( fs ).ConfigureAwait( false );
                    }
                }

                Log.InfoFormat( "Downloaded zip to {0}", tempZip );

                // Extract to temp directory
                Directory.CreateDirectory( tempDir );
                ZipFile.ExtractToDirectory( tempZip, tempDir );

                // GitHub zipball has a root folder like "owner-repo-shortsha/"
                var extractedDirs = Directory.GetDirectories( tempDir );
                if( extractedDirs.Length == 0 )
                    throw new InvalidOperationException( "ZIP archive contained no directories." );

                var rootFolder = extractedDirs[0];

                // Locate the gamePath subfolder within the extracted content
                var gameSourcePath = string.IsNullOrWhiteSpace( gamePath )
                    ? rootFolder
                    : Path.Combine( rootFolder, gamePath.Replace( '/', Path.DirectorySeparatorChar ) );

                if( !Directory.Exists( gameSourcePath ) )
                    throw new DirectoryNotFoundException( "Game path not found in repo: " + gamePath );

                // Destination is {DatabasePath}/{gameId}/
                var destPath = Path.Combine( Config.Instance.Paths.DatabasePath, gameId );
                Log.InfoFormat( "Installing game files from {0} to {1}", gameSourcePath, destPath );

                // Copy files
                CopyDirectoryContents( gameSourcePath, destPath );

                Log.InfoFormat( "Game {0} installed successfully from {1}/{2}", gameId, owner, repo );
            }
            catch( Exception ex )
            {
                Log.ErrorFormat( "Failed to install game {0} from {1}/{2}: {3}", gameId, owner, repo, ex );
                throw;
            }
            finally
            {
                // Clean up temp files
                try
                {
                    if( File.Exists( tempZip ) )
                        File.Delete( tempZip );
                    if( Directory.Exists( tempDir ) )
                        Directory.Delete( tempDir, true );
                }
                catch( Exception ex )
                {
                    Log.WarnFormat( "Failed to clean up temp files: {0}", ex.Message );
                }
            }
        }

        private static void CopyDirectoryContents( string sourceDir, string destDir )
        {
            Directory.CreateDirectory( destDir );

            foreach( var file in Directory.GetFiles( sourceDir ) )
            {
                var destFile = Path.Combine( destDir, Path.GetFileName( file ) );
                File.Copy( file, destFile, true );
            }

            foreach( var dir in Directory.GetDirectories( sourceDir ) )
            {
                var destSubDir = Path.Combine( destDir, Path.GetFileName( dir ) );
                CopyDirectoryContents( dir, destSubDir );
            }
        }
    }
}
