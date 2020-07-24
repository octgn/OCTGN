using Newtonsoft.Json;
using System;

namespace Octgn.Core.Play.Save
{
    [Serializable]
    public class GameSaveState : IGameSaveState
    {
        [JsonIgnore]
        IPlayerSaveState[] IGameSaveState.Players => Players;

        public PlayerSaveState[] Players { get; set; }

        public Guid SessionId { get; set; }
    }
}
