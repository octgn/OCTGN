using Octgn.Communication.Packets;
using System;

namespace Octgn.Online.Hosting
{
    [Serializable]
    public class HostedGame : IHostedGame
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string HostUserId { get; set; }

        public string GameName { get; set; }

        public Guid GameId { get; set; }

        public Version GameVersion { get; set; }

        public bool HasPassword { get; set; }

        public string Password { get; set; }

        public bool Spectators { get; set; }

        public string GameIconUrl { get; set; }

        public string HostUserIconUrl { get; set; }
        public Uri HostUri { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateStarted { get; set; }
        public HostedGameStatus Status { get; set; }
        public HostedGameSource Source { get; set; }

        public HostedGame() {

        }

        public HostedGame(Guid id, Guid gameguid, Version gameversion, string name, string huserId,
                          string gameName, string gameIconUrl, string userIconUrl, bool hasPassword, 
                          Uri hostUri, HostedGameStatus status, HostedGameSource source, bool spectators) {
            Id = id;
            Name = name;
            HostUserId = huserId;
            GameName = gameName;
            GameId = gameguid;
            GameVersion = gameversion;
            HasPassword = hasPassword;
            Spectators = spectators;
            GameIconUrl = gameIconUrl ?? string.Empty;
            HostUserIconUrl = userIconUrl ?? string.Empty;
            HostUri = hostUri;
            Status = status;
            Source = source;
            DateCreated = DateTimeOffset.Now;
        }

        public override string ToString() {
            return $"{nameof(HostedGame)}(Id: {Id}, HostUri: {HostUri}, Source: {Source}, Status: {Status}, HostUserId: {HostUserId}, : '{Name}({GameId}) - {GameName}v{GameVersion}, Spec: {Spectators}, User Icon: {HostUserIconUrl}, Icon: {GameIconUrl}, Password: {HasPassword}, Created: {DateCreated}, Started: {DateStarted} ')";
        }

        public static HostedGame GetFromPacket(DictionaryPacket packet) {
            return (HostedGame)packet["hostedgame"];
        }

        public static void AddToPacket(DictionaryPacket packet, HostedGame game) {
            packet["hostedgame"] = game;
        }

        public static string GetIdFromPacket(DictionaryPacket packet) {
            return (string)packet["hostedgameid"];
        }

        public static void AddIdToPacket(DictionaryPacket packet, string id) {
            packet["hostedgameid"] = id;
        }
    }
}