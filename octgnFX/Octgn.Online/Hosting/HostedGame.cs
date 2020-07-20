using Newtonsoft.Json;
using Octgn.Communication;
using Octgn.Communication.Packets;
using Octgn.Communication.Serializers;
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Octgn.Online.Hosting
{
    [DataContract]
    [Serializable]
    public class HostedGame
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public User HostUser { get; set; }

        [DataMember]
        public string GameName { get; set; }

        [DataMember]
        public Guid GameId { get; set; }

        [DataMember]
        public string GameVersion { get; set; }

        [DataMember]
        public string OctgnVersion { get; set; }

        [DataMember]
        public bool HasPassword { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public int ProcessId { get; set; }

        [DataMember]
        public bool Spectators { get; set; }

        [DataMember]
        public string GameIconUrl { get; set; }

        [DataMember]
        public string HostUserIconUrl { get; set; }

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

        /// <summary>
        /// The Hostname portion of <see cref="HostAddress"/>.
        /// </summary>
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

        /// <summary>
        /// Address of the server hosting this game.
        /// The format is 'host:port'
        /// </summary>
        [DataMember]
        public string HostAddress { get; set; }

        [DataMember]
        public DateTimeOffset DateCreated { get; set; }

        [DataMember]
        public DateTimeOffset? DateStarted { get; set; }

        [DataMember]
        public HostedGameStatus Status { get; set; }

        [DataMember]
        public HostedGameSource Source { get; set; }

        public HostedGame() {

        }

        public HostedGame(Guid id, Guid gameguid, Version gameversion, Version octgnVersion, string name, User huser,
                          string gameName, string gameIconUrl, string userIconUrl, bool hasPassword,
                          string hostAddress, HostedGameStatus status, HostedGameSource source, bool spectators) {
            Id = id;
            Name = name;
            HostUser = huser;
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
            HostUser = game.HostUser;
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
            return $"{nameof(HostedGame)}(Id: {Id}, HostAddress: {HostAddress}, Source: {Source}, Status: {Status}, HostUserId: {HostUser}, : '{Name}({GameId}) - {GameName}v{GameVersion}, OctgnV:{OctgnVersion}, Spec: {Spectators}, User Icon: {HostUserIconUrl}, Icon: {GameIconUrl}, Password: {HasPassword}, Created: {DateCreated}, Started: {DateStarted} ')";
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

        public static string Serialize(HostedGame hostedGame) {
            if (hostedGame == null) throw new ArgumentNullException(nameof(hostedGame));

            var hostedGameJson = JsonConvert.SerializeObject(hostedGame);

            var hostedGameBytes = Encoding.UTF8.GetBytes(hostedGameJson);

            return Convert.ToBase64String(hostedGameBytes);
        }

        public static HostedGame Deserialize(string serializedGame) {
            if (string.IsNullOrWhiteSpace(serializedGame)) throw new ArgumentNullException(nameof(serializedGame));

            var hostedGameBytes = Convert.FromBase64String(serializedGame);

            var hostedGameJson = Encoding.UTF8.GetString(hostedGameBytes);

            return JsonConvert.DeserializeObject<HostedGame>(hostedGameJson);
        }
    }
}