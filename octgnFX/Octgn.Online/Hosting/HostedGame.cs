using Octgn.Communication.Packets;
using System;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Octgn.Online.Hosting
{
    [Serializable]
    public class HostedGame
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string HostUserId { get; set; }

        public string GameName { get; set; }

        public Guid GameId { get; set; }

        public string GameVersion { get; set; }
        public string OctgnVersion { get; set; }

        public bool HasPassword { get; set; }

        public string Password { get; set; }
        public int ProcessId { get; set; }

        public bool Spectators { get; set; }

        public string GameIconUrl { get; set; }

        public string HostUserIconUrl { get; set; }
        [XmlIgnore]
        public int Port{
            get {
                var errorString = $"{nameof(HostAddress)} is not in the correct format 'host:port'. Can not determin the port from '{HostAddress}'";

                if (string.IsNullOrWhiteSpace(HostAddress))
                    throw new InvalidOperationException(errorString);

                var hostParts = HostAddress.Split(':');

                if(hostParts.Length != 2)
                    throw new InvalidOperationException(errorString);

                if(!int.TryParse(hostParts[1], out int iPort))
                    throw new InvalidOperationException(errorString);

                if(iPort <= 0)
                    throw new InvalidOperationException(errorString);

                return iPort;
            }
        }
        [XmlIgnore]
        public string Host{
            get {
                var errorString = $"{nameof(HostAddress)} is not in the correct format 'host:port'. Can not determin the port from '{HostAddress}'";

                if (string.IsNullOrWhiteSpace(HostAddress))
                    throw new InvalidOperationException(errorString);

                var hostParts = HostAddress.Split(':');

                if(hostParts.Length != 2)
                    throw new InvalidOperationException(errorString);

                return hostParts[0];
            }
        }
        public string HostAddress { get; set; }

        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateStarted { get; set; }
        public HostedGameStatus Status { get; set; }
        public HostedGameSource Source { get; set; }

        public HostedGame() {

        }

        public HostedGame(Guid id, Guid gameguid, Version gameversion, Version octgnVersion, string name, string huserId,
                          string gameName, string gameIconUrl, string userIconUrl, bool hasPassword,
                          string hostAddress, HostedGameStatus status, HostedGameSource source, bool spectators) {
            Id = id;
            Name = name;
            HostUserId = huserId;
            GameName = gameName;
            GameId = gameguid;
            GameVersion = gameversion.ToString();
            OctgnVersion = octgnVersion.ToString();
            HasPassword = hasPassword;
            Spectators = spectators;
            GameIconUrl = gameIconUrl ?? string.Empty;
            HostUserIconUrl = userIconUrl ?? string.Empty;
            HostAddress = hostAddress;
            Status = status;
            Source = source;
            DateCreated = DateTimeOffset.Now;
        }

        public HostedGame(HostedGame game, bool includeSensitiveData) {
            Id = game.Id;
            Name = game.Name;
            HostUserId = game.HostUserId;
            GameId = game.GameId;
            GameName = game.GameName;
            GameVersion = game.GameVersion;
            OctgnVersion = game.OctgnVersion;
            HasPassword = game.HasPassword;
            Spectators = game.Spectators;
            GameIconUrl = game.GameIconUrl;
            HostUserIconUrl = game.HostUserIconUrl;
            HostAddress = game.HostAddress;
            Status = game.Status;
            Source = game.Source;
            DateCreated = game.DateCreated;
            DateStarted = game.DateStarted;
            Password = includeSensitiveData ? game.Password : string.Empty;
            ProcessId = includeSensitiveData ? game.ProcessId : 0;
        }

        public override string ToString() {
            return $"{nameof(HostedGame)}(Id: {Id}, HostAddress: {HostAddress}, Source: {Source}, Status: {Status}, HostUserId: {HostUserId}, : '{Name}({GameId}) - {GameName}v{GameVersion}, OctgnV:{OctgnVersion}, Spec: {Spectators}, User Icon: {HostUserIconUrl}, Icon: {GameIconUrl}, Password: {HasPassword}, Created: {DateCreated}, Started: {DateStarted} ')";
        }

        public void KillGame() {
            if (ProcessId <= 0) throw new InvalidOperationException($"{nameof(KillGame)}: Process id '{ProcessId}' is invalid.");

            Process process = null;
            try {
                process = Process.GetProcessById(this.ProcessId);
            } catch (ArgumentException ex) {
                throw new InvalidOperationException($"{nameof(KillGame)}: Could not kill process '{ProcessId}', it doesn't exist", ex);
            } catch (InvalidOperationException ex) {
                throw new InvalidOperationException($"{nameof(KillGame)}: Could not kill process '{ProcessId}', it doesn't exist", ex);
            }

            if (process == null) throw new InvalidOperationException($"{nameof(KillGame)}: Could not kill process '{ProcessId}', it doesn't exist");

            process.Kill();
            process.WaitForExit();
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