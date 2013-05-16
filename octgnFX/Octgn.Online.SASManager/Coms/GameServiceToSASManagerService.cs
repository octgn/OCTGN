namespace Octgn.Online.SASManagerService.Coms
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    using Octgn.Online.Library.Coms;
    using Octgn.Online.Library.Models;

    using log4net;

    public class GameServiceToSASManagerService : IGameServiceToSASManagerService
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Task StartGame(HostedGameSASRequest request)
        {
            Log.InfoFormat("Start Game Request {0}",request.Id);
            var ret = new Task(() => SasManager.GetContext().StartGame(request));
            ret.Start();
            return ret; 
        }

        public Task<IEnumerable<HostedGame>> GameList()
        {
            var ret = new Task<IEnumerable<HostedGame>>(() => { return new HostedGame[0];});
            ret.Start();
            return ret;
        }

        public Task Hello(string mess1, string mess2)
        {
            var ret = new Task(()=> Console.WriteLine("{0} {1}",mess1,mess2));
            ret.Start();
            return ret;
        }
    }
}