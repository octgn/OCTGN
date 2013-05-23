namespace Octgn.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Versioning;

    using NuGet;

    public class EmptyPackage : IPackage
    {
        public string Id { get; private set; }

        public SemanticVersion Version { get; private set; }

        public string Title { get; private set; }

        public IEnumerable<string> Authors { get; private set; }

        public IEnumerable<string> Owners { get; private set; }

        public Uri IconUrl { get; set; }

        public Uri LicenseUrl { get; private set; }

        public Uri ProjectUrl { get; private set; }

        public bool RequireLicenseAcceptance { get; private set; }

        public string Description { get; private set; }

        public string Summary { get; private set; }

        public string ReleaseNotes { get; private set; }

        public string Language { get; private set; }

        public string Tags { get; private set; }

        public string Copyright { get; private set; }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; private set; }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences { get; private set; }

        public IEnumerable<PackageDependencySet> DependencySets { get; private set; }

        public Version MinClientVersion { get; private set; }

        public Uri ReportAbuseUrl { get; private set; }

        public int DownloadCount { get; private set; }

        public IEnumerable<IPackageFile> GetFiles()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            throw new NotImplementedException();
        }

        public Stream GetStream()
        {
            throw new NotImplementedException();
        }

        public bool IsAbsoluteLatestVersion { get; private set; }

        public bool IsLatestVersion { get; private set; }

        public bool Listed { get; private set; }

        public DateTimeOffset? Published { get; private set; }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; private set; }
    }
}