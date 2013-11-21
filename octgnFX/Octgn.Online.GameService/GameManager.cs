using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Skylabs.Lobby;

namespace Octgn.Online.GameService
{
    public class GameManager
    {
        #region Singleton

        internal static GameManager SingletonContext { get; set; }

        private static readonly object GameManagerSingletonLocker = new object();

        public static GameManager Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new GameManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<HostedGameData> Games
        {
            get
            {
                return new HostedGameData[0];
                // Need some readwritelockslim up in this
            }
        }

        public void HostGame(HostGameRequest req, User u)
        {
            
        }
    }
}