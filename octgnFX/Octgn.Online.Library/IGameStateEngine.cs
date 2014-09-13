using Octgn.Online.Library.Enums;
using Octgn.Online.Library.Models;
namespace Octgn.Online.Library
{
    public interface IGameStateEngine
    {
        bool IsLocal { get; }
        IHostedGameState Game { get; }
        string ApiKey { get; }

        void SetStatus(EnumHostedGameStatus status);
    }
}