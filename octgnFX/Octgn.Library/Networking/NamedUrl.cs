namespace Octgn.Library.Networking
{
    public class NamedUrl
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public NamedUrl(string name, string url)
        {
            Url = url;
            Name = name;
        }
    }
}