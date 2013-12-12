using System;
using System.Reflection;
using System.Threading;
using log4net;

namespace Octgn.Library
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

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

        private readonly XFile file;

        internal X()
        {
            file = new XFile();
        }

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

        public bool ReleaseTest
        {
            get
            {
#if(Release_Test)
 				return true;
#else
                return false;
#endif
            }
        }

        public XFile File
        {
            get
            {
                return file;
            }
        }

        public void Retry(Action a, int times = 3)
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
                    if (i == times - 1)
                        throw;
                    Thread.Sleep(1000 * i);
                }
            }
        }

        public void Try(Action a)
        {
            try
            {
                if (a == null) return;
                a.Invoke();
            }
            catch { }
        }

        public void ForEachProgress<T>(int max, IEnumerable<T> collection, Action<T> process, Action<int,int> updateProgress)
        {
            if (collection == null) return;
            var cur = 0;
            updateProgress(cur, max);
            foreach (var c in collection)
            {
                process(c);
                cur++;
                updateProgress(cur, max);
            }
            updateProgress(-1, 1);
        }
    }
    public class XFile
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        public bool OpenFile(string path, FileMode fileMode, FileShare share, TimeSpan timeout, out Stream stream)
        {
            //Log.DebugFormat("Open file {0} {1} {2} {3}", path, fileMode, share, timeout.ToString());
            var endTime = DateTime.Now + timeout;
            while (DateTime.Now < endTime)
            {
                //Log.DebugFormat("Trying to lock file {0}", path);
                try
                {
                    stream = System.IO.File.Open(path, fileMode, FileAccess.ReadWrite, share);
                    //Log.DebugFormat("Got lock on file {0}", path);
                    return true;
                }
                catch (IOException e)
                {
                    Log.Warn("Could not acquire lock on file " + path, e);
                }
            }
            Log.WarnFormat("Timed out reading file {0}", path);
            stream = null;
            return false;
        }
    }
}