namespace Octgn.Online.Library.Coms
{
    using System;

    public interface ISASManagerServiceToGameService
    {
        void StartGameResults(Guid id, string host, int port, bool succeeded);
        void GameStateChanged(Guid id, string state);
    }
}