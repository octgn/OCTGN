namespace Octgn.Online.Library
{
    using System;

    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.Models;

    public interface IGameStateEngine
    {
        IHostedGameState Game { get; }

        void SetStatus(EnumHostedGameStatus status);
    }
}