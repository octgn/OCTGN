namespace Skylabs.LobbyServer
{
    public class Ban
    {
        /// <summary>
        /// Database ID for the ban
        /// </summary>
        public int Bid { get; set; }

        /// <summary>
        /// UID of the person banned.
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// IP associated with the ban.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// When does the ban end(in time since epoch seconds blah blah
        /// </summary>
        public long EndTime { get; set; }
    }
}