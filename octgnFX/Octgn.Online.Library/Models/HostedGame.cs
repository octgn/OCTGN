namespace Octgn.Online.Library.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Octgn.Online.Library.Enums;

    public interface IHostedGame
    {
        Guid Id { get; }

        string Name { get; }

        string HostUserName { get; }

        string GameName { get; }

        Guid GameId { get; }

        Version GameVersion { get; }

        bool HasPassword { get; }

        bool TwoSidedTable { get; }

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

        public bool TwoSidedTable { get; set; }
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
                              Password = request.Password,
                              TwoSidedTable = request.TwoSidedTable
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
                              HostUri = host,
                              TwoSidedTable = request.TwoSidedTable
                          };
            return ret;
        }

        public static IHostedGameState ToHostedGameState(this HostedGameSASModel model, EnumHostedGameStatus status = EnumHostedGameStatus.Unknown)
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
                              Status = status,
                              TwoSidedTable = model.TwoSidedTable,
                              CurrentTurnPlayer = 0,
                              Players = new List<HostedGamePlayer>()
                          };
            return ret;
        }

        /// <summary>
        /// Gets rid of sensitive data the user doesn't need to have.
        /// </summary>
        /// <param name="state">Game state</param>
        /// <returns>Censored Game State</returns>
        public static IHostedGameState ForUser(this IHostedGameState state)
        {
            var ret = new HostedGameState
                          {
                              GameId = state.GameId,
                              GameName = state.GameName,
                              GameVersion = state.GameVersion,
                              HasPassword = state.HasPassword,
                              HostUserName = state.HostUserName,
                              Id = state.Id,
                              Name = state.Name,
                              Password = null,
                              HostUri = state.HostUri,
                              Status = state.Status,
                              TwoSidedTable = state.TwoSidedTable,
                              CurrentTurnPlayer = state.CurrentTurnPlayer,
                              Players = state.Players.Select(x=>x.ForUser()).ToList()
                          };
            return ret;
        }

        /// <summary>
        /// Gets rid of sensitive data the user doesn't need to have.
        /// </summary>
        /// <param name="player">Hosted Game Player</param>
        /// <returns>Censored Hosted Game Player</returns>
        public static HostedGamePlayer ForUser(this HostedGamePlayer player)
        {
            var ret = new HostedGamePlayer
                          {
                              ConnectionState = player.ConnectionState,
                              State = player.State,
                              Id = player.Id,
                              Name = player.Name,
                              InvertedTable = player.InvertedTable,
                              IsMod = player.IsMod,
                              Key = Guid.Empty,
                              Kicked = player.Kicked
                          };
            return ret;
        }
    }
}