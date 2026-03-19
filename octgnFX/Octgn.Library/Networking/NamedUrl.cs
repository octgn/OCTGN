namespace Octgn.Library.Networking
{
    public enum FeedType
    {
        NuGet = 0,
        RepoIndex = 1,
        DirectRepo = 2
    }

    public class NamedUrl
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public FeedType FeedType { get; set; }

        public NamedUrl(string name, string url, string username, string password)
            : this(name, url, username, password, FeedType.NuGet)
        {
        }

        public NamedUrl(string name, string url, string username, string password, FeedType feedType)
        {
            Url = url;
            Name = name;
            Username = username;
            Password = password;
            FeedType = feedType;
        }
    }
}
