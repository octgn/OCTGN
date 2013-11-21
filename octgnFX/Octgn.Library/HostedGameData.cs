namespace Octgn.Library
{
    using System;
    using System.Net;

    [Serializable]
    public class BroadcasterHostedGameData : IHostedGameData
    {
        public Guid GameGuid { get; set; }
        public Version GameVersion { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public string GameName { get; set; }
        public string Username { get; set; }
        public bool HasPassword { get; set; }
        public EHostedGame GameStatus { get; set; }
        public DateTime TimeStarted { get; set; }
        public IPAddress IpAddress { get; set; }
        public HostedGameSource Source { get; set; }
        public int ProcessId { get; set; }
        public Guid Id { get; set; }
    }
}