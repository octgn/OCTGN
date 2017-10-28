using Octgn.Communication;

namespace Octgn.Online.Hosting
{
    public static class ExtensionMethods
    {
        public static void InitializeHosting(this Client client) {
            client.Attach(new ClientHostingModule());
        }

        public static ClientHostingModule Hosting(this Client client) {
            var mod = client.GetModule<ClientHostingModule>();
            return mod;
        }

        //public static HostedGameSASRequest ToHostedGameSasRequest(this HostedGameRequest request) {
        //    var ret = new HostedGameSASRequest {
        //        GameId = request.GameId,
        //        GameName = request.GameName,
        //        GameVersion = Version.Parse(request.GameVersion),
        //        HasPassword = request.HasPassword,
        //        HostUserId = request.HostUserId,
        //        Id = Guid.NewGuid(),
        //        Name = request.Name,
        //        Password = request.Password,
        //        Spectators = request.Spectators,
        //        GameIconUrl = request.GameIconUrl,
        //        HostUserIconUrl = request.HostUserIconUrl
        //    };
        //    return ret;
        //}

        //public static HostedGameSASModel ToHostedGameSasModel(
        //    this HostedGameSASRequest request, Uri host) {
        //    var ret = new HostedGameSASModel {
        //        GameId = request.GameId,
        //        GameName = request.GameName,
        //        GameVersion = request.GameVersion,
        //        HasPassword = request.HasPassword,
        //        HostUserId = request.HostUserId,
        //        Id = request.Id,
        //        Name = request.Name,
        //        Password = request.Password,
        //        HostUri = host,
        //        Spectators = request.Spectators,
        //        HostUserIconUrl = request.HostUserIconUrl,
        //        GameIconUrl = request.GameIconUrl
        //    };
        //    return ret;
        //}

        //public static IHostedGameState ToHostedGameState(this HostedGameSASModel model, EnumHostedGameStatus status = EnumHostedGameStatus.Unknown) {
        //    var ret = new HostedGameState {
        //        GameId = model.GameId,
        //        GameName = model.GameName,
        //        GameVersion = model.GameVersion,
        //        HasPassword = model.HasPassword,
        //        HostUserId = model.HostUserId,
        //        Id = model.Id,
        //        Name = model.Name,
        //        Password = model.Password,
        //        HostUri = model.HostUri,
        //        Status = status,
        //        TwoSidedTable = model.TwoSidedTable,
        //        CurrentTurnPlayer = 0,
        //        Players = new List<HostedGamePlayer>(),
        //        Spectators = model.Spectators,
        //        HostUserIconUrl = model.HostUserIconUrl,
        //        GameIconUrl = model.GameIconUrl
        //    };
        //    return ret;
        //}

        ///// <summary>
        ///// Gets rid of sensitive data the user doesn't need to have.
        ///// </summary>
        ///// <param name="state">Game state</param>
        ///// <returns>Censored Game State</returns>
        //public static IHostedGameState ForUser(this IHostedGameState state) {
        //    var ret = new HostedGameState {
        //        GameId = state.GameId,
        //        GameName = state.GameName,
        //        GameVersion = state.GameVersion,
        //        HasPassword = state.HasPassword,
        //        HostUserId = state.HostUserId,
        //        Id = state.Id,
        //        Name = state.Name,
        //        Password = null,
        //        HostUri = state.HostUri,
        //        Status = state.Status,
        //        TwoSidedTable = state.TwoSidedTable,
        //        CurrentTurnPlayer = state.CurrentTurnPlayer,
        //        Players = state.Players.Select(x => x.ForUser()).ToList(),
        //        Spectators = state.Spectators,
        //        GameIconUrl = state.GameIconUrl,
        //        HostUserIconUrl = state.HostUserIconUrl
        //    };
        //    return ret;
        //}

        ///// <summary>
        ///// Gets rid of sensitive data the user doesn't need to have.
        ///// </summary>
        ///// <param name="player">Hosted Game Player</param>
        ///// <returns>Censored Hosted Game Player</returns>
        //public static HostedGamePlayer ForUser(this HostedGamePlayer player) {
        //    var ret = new HostedGamePlayer {
        //        ConnectionState = player.ConnectionState,
        //        State = player.State,
        //        Id = player.Id,
        //        Name = player.Name,
        //        InvertedTable = player.InvertedTable,
        //        IsMod = player.IsMod,
        //        Key = Guid.Empty,
        //        Kicked = player.Kicked
        //    };
        //    return ret;
        //}
    }
}