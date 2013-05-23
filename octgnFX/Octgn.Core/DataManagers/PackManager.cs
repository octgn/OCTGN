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
        
        public Pack FromXml(Set set, string xml)
        {
            var ret = new Pack();
            using (var stringReader = new StringReader(xml))
            using (XmlReader reader = XmlReader.Create(stringReader))
            {
                reader.Read();
                string guid = reader.GetAttribute("id");
                if (guid != null) ret.Id = new Guid(guid);
                ret.Name = reader.GetAttribute("name");
                reader.ReadStartElement("pack");
                ret.Definition = new PackDefinition(reader);
                reader.ReadEndElement(); // </pack>
            }
            return ret;
        }
    }
}