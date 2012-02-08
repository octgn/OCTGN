using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Skylabs.ConsoleHelper
{
    public class ConsoleEventLog
    {
        #region Delegates

        public delegate void EventEventDelegate(ConsoleEvent e);

        #endregion

        private static List<ConsoleEvent> _events = new List<ConsoleEvent>();

        public static List<ConsoleEvent> Events
        {
            get { return _events; }
            set { _events = value; }
        }

        public static event EventEventDelegate EAddEvent = null;

        public static void AddEvent(ConsoleEvent con, Boolean writeToConsole)
        {
            Events.Add(con);
            if (writeToConsole)
                con.WriteEvent(false);
            if (EAddEvent == null) return;
            if (EAddEvent.GetInvocationList().Length != 0)
                EAddEvent.Invoke(con);
        }

        public static void SerializeEvents(string filename)
        {
            try
            {
                using (Stream stream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
                {
                    var xmlTextWriter = new XmlTextWriter(stream, Encoding.ASCII)
                                            {Formatting = Formatting.Indented, Indentation = 4};
                    var cTypes = new List<Type>();
                    foreach (ConsoleEvent c in Events)
                    {
                        bool foundit = false;
                        foreach (Type t in cTypes)
                        {
                            if (c.GetType() != t) continue;
                            foundit = true;
                            break;
                        }
                        if (!foundit)
                            cTypes.Add(c.GetType());
                    }
                    var xs = new XmlSerializer(Events.GetType(), new XmlAttributeOverrides(), cTypes.ToArray(),
                                               new XmlRootAttribute("Events"), "Skylabs.olobby");
                    xs.Serialize(xmlTextWriter, Events);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Log error " + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}