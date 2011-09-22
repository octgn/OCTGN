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
        public delegate void EventEventDelegate(ConsoleEvent e);
        public static event EventEventDelegate eAddEvent = null;

        public static List<ConsoleEvent> Events { get { return _Events; } set { _Events = value; } }

        private static List<ConsoleEvent> _Events = new List<ConsoleEvent>();

        public static void addEvent(ConsoleEvent con, Boolean writeToConsole)
        {
            Events.Add(con);
            if(writeToConsole)
                con.writeEvent(false);
            if(eAddEvent != null)
            {
                if(eAddEvent.GetInvocationList().Length != 0)
                    eAddEvent.Invoke(con);
            }
        }

        public static void SerializeEvents(string filename)
        {
            try
            {
                using(Stream stream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
                {
                    XmlSerializer xs;
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.ASCII);
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.Indentation = 4;
                    List<Type> cTypes = new List<Type>();
                    foreach(ConsoleEvent c in ConsoleEventLog.Events)
                    {
                        bool foundit = false;
                        foreach(Type t in cTypes)
                        {
                            if(c.GetType() == t)
                            {
                                foundit = true;
                                break;
                            }
                        }
                        if(!foundit)
                            cTypes.Add(c.GetType());
                    }
                    xs = new XmlSerializer(ConsoleEventLog.Events.GetType(), new XmlAttributeOverrides(), cTypes.ToArray(), new XmlRootAttribute("Events"), "Skylabs.olobby");
                    xs.Serialize(xmlTextWriter, ConsoleEventLog.Events);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Log error " + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}