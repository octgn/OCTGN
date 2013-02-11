namespace Octgn.Online.GameService.Coms
{
    using System;
    using System.Reflection;

    using Microsoft.AspNet.SignalR.Hubs;

    using Octgn.Online.Library.SignalR.Coms;

    public class GameServiceToSASManagerService : IGameServiceToSASManagerService
    {
        public dynamic Return;
        internal dynamic To;
        public GameServiceToSASManagerService(dynamic to)
        {
            To = to;
        }

        public void StartGame(Guid id, string name, string oGameName, Guid oGameId)
        {
            Return = MethodBase.GetCurrentMethod().Invoke(To, new object[] { id, name, oGameName, oGameId });
        }

        public void Hello(string mess1, string mess2)
        {
            StatefulSignalProxy r = To;
            Return = r.Invoke(MethodBase.GetCurrentMethod().Name,new object[] { mess1,mess2 });
        }
    }
}