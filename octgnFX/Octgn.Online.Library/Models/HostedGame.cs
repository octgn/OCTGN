namespace Octgn.Online.Library.Models
{
    using System;

    public class HostedGame
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string HostUserName { get; set; }

        public string GameName { get; set; }

        public Guid GameId { get; set; }

        public Version GameVersion { get; set; }

        public bool HasPassword { get; set; }
    }

    public class HostedGameRequest : HostedGame
    {
        private new Guid Id { get; set; }

        public string Password { get; set; }

        public new string GameVersion { get; set; }
    }

    public class HostedGameSASRequest : HostedGame
    {
        public new Guid Id { get; set; }

        public string Password { get; set; }
    }

    public class HostedGameSASResponse : HostedGame
    {
        public Uri HostUri { get; set; }
    }

    public class HostedGameSASModel : HostedGame
    {
        public Uri HostUri { get; set; }

        public string Password { get; set; }
    }

    public static class HostedGameExtensionMethods
    {
        public static HostedGameSASRequest ToHostedGameSasRequest(this HostedGameRequest request)
        {
            var ret = new HostedGameSASRequest
                          {
                              GameId = request.GameId,
                              GameName = request.GameName,
                              GameVersion = Version.Parse(request.GameVersion),
                              HasPassword = request.HasPassword,
                              HostUserName = request.HostUserName,
                              Id = Guid.NewGuid(),
                              Name = request.Name,
                              Password = request.Password
                          };
            return ret;
        }
    }
}