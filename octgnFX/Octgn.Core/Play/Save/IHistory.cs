using System;

namespace Octgn.Core.Play.Save
{
    public interface IHistory
    {
        Guid Id { get; }

        string Name { get; }

        Guid GameId { get; }

        IGameSaveState State { get; }

        DateTimeOffset DateSaved { get; }

        DateTimeOffset DateStarted { get; }
    }
}
