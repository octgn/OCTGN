namespace Octgn.Core.Models
{
    public class FeedIndex
    {
        public string Name { get; set; }
        public FeedGameEntry[] Games { get; set; }
    }

    public class FeedGameEntry
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Repo { get; set; }
        public string Branch { get; set; }
        public string ManifestPath { get; set; }
    }
}
