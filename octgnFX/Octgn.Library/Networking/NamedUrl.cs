namespace Octgn.Library.Networking
{
    public class NamedUrl
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public NamedUrl(string name, string url, string username, string password)
        {
            Url = url;
            Name = name;
            Username = username;
            Password = password;
        }
    }
}