using System;
using System.Collections.Generic;
using System.Linq;

// Socket-bearing players, timed collections and hosting/service dependencies are
// doubled. Handler, parsing, broadcasting, context/state and settings are linked.
namespace log4net
{
    public interface ILog
    {
        void Info(object message);
        void Warn(object message);
        void Debug(object message);
        void Error(object message, Exception error);
    }
    public static class LogManager
    {
        private sealed class NullLog : ILog
        {
            public void Info(object message) { }
            public void Warn(object message) { }
            public void Debug(object message) { }
            public void Error(object message, Exception error) { }
        }
        public static ILog GetLogger(object name) => new NullLog();
    }
}
namespace Octgn.Library.Localization
{
    public static class L
    {
        public static class D
        {
            public const string ServerMessage__SpectatorsMuted = "Spectators are muted.";
            public const string ServerMessage__SpectatorsNotAllowed = "Spectators not allowed";
            public const string ServerMessage__SayHelloOnlyOnce = "Hello once";
            public const string ServerMessage__IncorrectPassword = "Incorrect password";
            public const string ServerMessage__IncompatibleOctgnClient_Format = "Incompatible client";
            public const string ServerMessage__InvalidGame_Format = "Invalid game";
            public const string ServerMessage__IncompatibleGameVersion_Format = "Incompatible game";
            public const string ServerMessage__GameStartedNotAcceptingNewPlayers = "Game started";
            public const string ServerMessage__CanNotReconnectFirstTimeConnecting = "No player";
            public const string ServerMessage__PublicKeyDoesNotMatch = "Key mismatch";
            public const string ServerMessage__CallDepreciated = "Deprecated";
            public const string ServerMessage__CanNotBootPlayerDoesNotExist = "No player";
            public const string ServerMessage__CanNotBootNotHost = "Not host";
            public const string ServerMessage__UnknownBinaryMessage = "Unknown message";
        }
    }
}
namespace Octgn.Site.Api.Models
{
    public class PutGameHistoryReq
    {
        public PutGameHistoryReq(string apiKey, string session) { }
        public List<string> Usernames { get; } = new List<string>();
    }
}
namespace Octgn.Site.Api
{
    public class ApiClient
    {
        public ApiClient() => throw new InvalidOperationException("Unexpected API client construction");
        public void CreateGameHistory(Models.PutGameHistoryReq request) => throw new InvalidOperationException("Unexpected API call");
        public void GameMessage(string apiKey, Models.GameMessage message) => throw new InvalidOperationException("Unexpected API call");
    }
}
namespace Octgn.Online.Hosting
{
    public enum HostedGameStatus { StartedHosting, GameInProgress }
    public class User { public string Id { get; set; } }
    public class HostedGame
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameVersion { get; set; }
        public string Name { get; set; }
        public User HostUser { get; set; }
        public bool Spectators { get; set; }
        public bool HasPassword { get; set; }
        public string Password { get; set; }
        public DateTimeOffset? DateStarted { get; set; }
        public HostedGameStatus Status { get; set; }
    }
}
namespace Octgn.Data
{
    public class GameSettings
    {
        public bool AllowSpectators { get; set; }
        public bool MuteSpectators { get; set; }
        public bool AllowCardList { get; set; }
        public bool UseTwoSidedTable { get; set; }
    }
}
namespace Octgn.Server
{
    public class Player
    {
        public const byte HOSTPLAYERID = 1;
        public byte Id { get; set; }
        public string Nick { get; set; }
        public string UserId { get; set; }
        public ulong Pkey { get; set; }
        public bool Connected { get; set; } = true;
        public bool SaidHello { get; set; } = true;
        public bool IsSubbed { get; set; }
        public PlayerSettings Settings { get; set; } = new PlayerSettings(false, false);
        public IClientCalls Rpc { get; set; }
        public void Setup(byte id, string nick, string user, ulong key, string software, bool inverted, bool spectator, PlayerCollection collection) => throw new NotSupportedException();
        public void ResetSocket(Player player) => throw new NotSupportedException();
        public void Leave() => throw new NotSupportedException();
        public void Kick(string message) => throw new NotSupportedException();
    }
    public class PlayerCollection
    {
        public PlayerCollection(GameContext context) { }
        public List<Player> Clients { get; } = new List<Player>();
        public Player[] Players => Clients.Where(p => p.SaidHello).ToArray();
        public List<ulong> KickedPlayers { get; } = new List<ulong>();
        public Player GetPlayer(byte id) => Players.FirstOrDefault(p => p.Id == id);
    }
    public class Server { public GameContext Context { get; set; } }
    public class ServerSocket
    {
        public Server Server { get; set; }
        public Handler Handler { get; set; }
    }
}
