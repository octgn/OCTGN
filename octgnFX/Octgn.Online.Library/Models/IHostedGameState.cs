using System;
using System.Collections.Generic;

namespace Octgn.Online.Library.Models
{
    public interface IHostedGameState : IHostedGame
    {
        Uri HostUri { get; }

        string Password { get; }

        Enums.EnumHostedGameStatus Status { get; }

        IList<IHostedGamePlayer> Players { get; }
        IList<IHostedGamePlayer> KickedPlayers { get; }
        IList<IHostedGamePlayer> DisconnectedPlayers { get; }

        int CurrentTurnPlayer { get; }
        int CurrentTurnNumber { get; set; }
        HashSet<uint> TurnStopPlayers { get; }
        HashSet<Tuple<uint, byte>> PhaseStopPlayers { get; }
    }

}