using System;

using System.Xml;
using System.Xml.Serialization;

namespace Skylabs.ConsoleHelper
{
    [Serializable]
    public class ConsoleEvent
    {
        //[XmlIgnore()]
        //public ConsoleColor Color { get; set; }
        [XmlIgnore()]
        public string Header { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("date")]
        public DateTime Date { get; set; }

        public ConsoleEvent()
        {
        }

        public ConsoleEvent(String message)
        {
            Message = message;
            Date = DateTime.Now;
            //Color = ConsoleWriter.OutputColor;
        }

        public ConsoleEvent(String header, String message)
        {
            Header = header;
            Message = message;
            //Color = color;
            Date = DateTime.Now;
        }

        public String getConsoleString()
        {
            return Header + Message;
        }

        public void writeEvent(Boolean addToEventLog)
        {
            //Console.ForegroundColor = Color;
            ConsoleWriter.writeLine(Header + Message, false);
            ConsoleWriter.writeCT();
            if(addToEventLog)
                ConsoleEventLog.addEvent(this, false);
        }
    }
}