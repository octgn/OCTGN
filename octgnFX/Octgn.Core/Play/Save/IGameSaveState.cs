using System;

namespace Octgn.Core.Play.Save
{
    public interface IGameSaveState
    {
        IPlayerSaveState[] Players { get; }

        Guid SessionId { get; }
    }
}
