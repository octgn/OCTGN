using Octgn.Communication.Packets;
using System;

namespace Octgn.Online.Hosting
{
    public class HostGameRequest {
        public Guid GameGuid { get; set; }
        public String GameIconUrl { get; set; }
        public Version GameVersion { get; set; }
        public string GameName { get; set; }
        public String Name { get; set; }
        public String Password { get; set; }
        public Version SasVersion { get; set; }
        public bool Spectators { get; set; }

        public HostGameRequest() {

        }

        public HostGameRequest(Guid gameguid, Version gameversion, string name
            , string gameName, string gameIconurl, string password, Version sasVersion, bool spectators) {
            GameGuid = gameguid;
            GameVersion = gameversion;
            Name = name;
            GameName = gameName;
            Password = password;
            SasVersion = sasVersion;
            Spectators = spectators;
            GameIconUrl = gameIconurl;
        }

        public override string ToString() {
            return $"HostGameRequest({Name}({GameGuid}) - {GameName}v{GameVersion}, Spec: {Spectators}, Sas: {SasVersion}, Icon: {GameIconUrl}')";
        }

        public static HostGameRequest GetFromPacket(DictionaryPacket packet) {
            return (HostGameRequest)packet["hostgamerequest"];
        }

        public static void AddToPacket(DictionaryPacket packet, HostGameRequest req) {
            packet["hostgamerequest"] = req;
        }
    }
}
