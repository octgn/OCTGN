namespace Octgn.Online.Library.Models
{
    using System;

    using Octgn.Online.Library.Enums;

    public interface IHostedGame
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string HostUserName { get; set; }

        string GameName { get; set; }

        Guid GameId { get; set; }

        Version GameVersion { get; set; }

        bool HasPassword { get; set; }

    }

    public class HostedGame : IHostedGame
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
        public string Password { get; set; }
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

        public static HostedGameSASModel ToHostedGameSasModel(
            this HostedGameSASRequest request, Uri host)
        {
            var ret = new HostedGameSASModel
                          {
                              GameId = request.GameId,
                              GameName = request.GameName,
                              GameVersion = request.GameVersion,
                              HasPassword = request.HasPassword,
                              HostUserName = request.HostUserName,
                              Id = request.Id,
                              Name = request.Name,
                              Password = request.Password,
                              HostUri = host
                          };
            return ret;
        }

        public static HostedGameState ToHostedGameState(this HostedGameSASModel model, EnumHostedGameStatus status = EnumHostedGameStatus.Unknown)
        {
            var ret = new HostedGameState
                          {
                              GameId = model.GameId,
                              GameName = model.GameName,
                              GameVersion = model.GameVersion,
                              HasPassword = model.HasPassword,
                              HostUserName = model.HostUserName,
                              Id = model.Id,
                              Name = model.Name,
                              Password = model.Password,
                              HostUri = model.HostUri,
                              Status = status
                          };
            return ret;
        }
    }
}