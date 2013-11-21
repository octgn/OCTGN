namespace Octgn.Online.Library
{
    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.Models;

    public interface IGameStateEngine
    {
        bool IsLocal { get; }
        IHostedGameState Game { get; }

        void SetStatus(EnumHostedGameStatus status);
    }
}