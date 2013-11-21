using System;
using System.Net;

namespace Octgn.Library
{
    public interface IHostedGameData
    {
        int ProcessId { get; set; }
		Guid Id { get; set; }
        Guid GameGuid { get; set; }
        Version GameVersion { get; set; }
        int Port { get; set; }
        String Name { get; set; }
        string GameName { get; set; }
        string Username { get; set; }
        bool HasPassword { get; set; }
        EHostedGame GameStatus { get; set; }
        DateTime TimeStarted { get; set; }
        IPAddress IpAddress { get; set; }
        HostedGameSource Source { get; set; }
    }

    [Serializable]
    public enum HostedGameSource
    {
        Online,
        Lan
    };

    [Serializable]
    public enum EHostedGame
    {
        StartedHosting,
        GameInProgress,
        StoppedHosting
    };
}