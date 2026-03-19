using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using NuGet;

using Octgn.Core.Models;

namespace Octgn.Core
{
    /// <summary>
    /// Wraps a <see cref="GameManifest"/> to implement NuGet's <see cref="IPackage"/> interface,
    /// allowing repo-based games to be displayed alongside NuGet-based games in the UI.
    /// </summary>
    public class GameManifestPackageAdapter : IPackage
    {
        public GameManifest Manifest { get; }

        public GameManifestPackageAdapter( GameManifest manifest )
        {
            if( manifest == null )
                throw new ArgumentNullException( nameof( manifest ) );
            Manifest = manifest;
        }

        public string Id => Manifest.Guid ?? "";

        public SemanticVersion Version
        {
            get
            {
                if( string.IsNullOrWhiteSpace( Manifest.Version ) )
                    return new SemanticVersion( 0, 0, 0, 0 );
                try
                {
                    return SemanticVersion.Parse( Manifest.Version );
                }
                catch
                {
                    return new SemanticVersion( 0, 0, 0, 0 );
                }
            }
        }

        public string Title => Manifest.Name ?? "";

        public IEnumerable<string> Authors => Manifest.Authors ?? Enumerable.Empty<string>();

        public IEnumerable<string> Owners => Enumerable.Empty<string>();

        public Uri IconUrl => null;

        public Uri LicenseUrl => null;

        public Uri ProjectUrl
        {
            get
            {
                if( string.IsNullOrWhiteSpace( Manifest.RepoOwner ) || string.IsNullOrWhiteSpace( Manifest.RepoName ) )
                    return null;
                return new Uri( string.Format( "https://github.com/{0}/{1}", Manifest.RepoOwner, Manifest.RepoName ) );
            }
        }

        public bool RequireLicenseAcceptance => false;

        public bool DevelopmentDependency => false;

        public string Description => Manifest.Description ?? "";

        public string Summary => Manifest.Description ?? "";

        public string ReleaseNotes => Manifest.Changelog ?? "";

        public string Language => "";

        public string Tags => Manifest.Tags != null ? string.Join( " ", Manifest.Tags ) : "";

        public string Copyright => "";

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies => Enumerable.Empty<FrameworkAssemblyReference>();

        public ICollection<PackageReferenceSet> PackageAssemblyReferences => new List<PackageReferenceSet>();

        public IEnumerable<PackageDependencySet> DependencySets => Enumerable.Empty<PackageDependencySet>();

        public Version MinClientVersion => null;

        public Uri ReportAbuseUrl => null;

        public int DownloadCount => 0;

        public bool IsAbsoluteLatestVersion => true;

        public bool IsLatestVersion => true;

        public bool Listed => true;

        public DateTimeOffset? Published
        {
            get
            {
                if( !string.IsNullOrWhiteSpace( Manifest.VersionDate ) &&
                    DateTimeOffset.TryParse( Manifest.VersionDate, out var dt ) )
                    return dt;
                return null;
            }
        }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences => Enumerable.Empty<IPackageAssemblyReference>();

        public IEnumerable<IPackageFile> GetFiles()
        {
            return Enumerable.Empty<IPackageFile>();
        }

        public IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            return Enumerable.Empty<FrameworkName>();
        }

        public Stream GetStream()
        {
            throw new NotSupportedException( "Repo-based games do not support GetStream. Use GameFeedManager.InstallRepoGameAsync instead." );
        }

        public void ExtractContents( IFileSystem fileSystem, string extractPath )
        {
            throw new NotSupportedException( "Repo-based games do not support ExtractContents. Use GameFeedManager.InstallRepoGameAsync instead." );
        }
    }
}
