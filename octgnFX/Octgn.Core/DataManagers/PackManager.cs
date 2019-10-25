namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using Octgn.DataNew.Entities;

    using log4net;

    public class PackManager
    {
        #region Singleton
        private static PackManager Context { get; set; }
        private static object locker = new object();
        public static PackManager Get()
        {
            lock (locker) return Context ?? (Context = new PackManager());
        }
        internal PackManager()
        {

        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
    }
}