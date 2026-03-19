namespace Octgn.Core.Models
{
    public class GameManifest
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string VersionDate { get; set; }
        public string Description { get; set; }
        public string[] Authors { get; set; }
        public string MinimumOctgnVersion { get; set; }
        public string GamePath { get; set; }
        public string[] Tags { get; set; }
        public string Changelog { get; set; }
        public string IconUrl { get; set; }

        // Populated when fetched from a feed index
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
        public string Branch { get; set; }
    }
}
