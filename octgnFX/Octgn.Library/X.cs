using System;
using System.Reflection;
using System.Threading;
using log4net;

namespace Octgn.Library
{
    public class X
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Singleton

        internal static X SingletonContext { get; set; }

        private static readonly object XSingletonLocker = new object();

        public static X Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (XSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new X();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public bool Debug
        {
            get
            {
#if(DEBUG)
                return true;
#else
                return false;
#endif
            }
        }

        public bool TestServer
        {
            get
            {
#if(TestServer)
                return true;
#else
                return false;
#endif
            }
        }

        public void Retry(Action a , int times = 3)
        {
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    a.Invoke();
                    return;
                }
                catch
                {
                    if(i == times - 1)
                        throw;
                    Thread.Sleep(1000 * i);
                }
            }
        }

        public static void Try(Action a)
        {
            try
            {
                a.Invoke();
            }
            catch{}
        }
    }
}