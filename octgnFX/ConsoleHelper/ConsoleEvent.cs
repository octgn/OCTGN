using System;
using System.Xml.Serialization;

namespace Skylabs.ConsoleHelper
{
    [Serializable]
    public class ConsoleEvent
    {
        //[XmlIgnore()]
        //public ConsoleColor Color { get; set; }

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

        [XmlIgnore]
        public string Header { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("date")]
        public DateTime Date { get; set; }

        public String GetConsoleString()
        {
            return Header + Message;
        }

        public void WriteEvent(Boolean addToEventLog)
        {
            //Console.ForegroundColor = Color;
            ConsoleWriter.WriteLine(Header + Message, false);
            ConsoleWriter.WriteCt();
            if (addToEventLog)
                ConsoleEventLog.AddEvent(this, false);
        }
    }
}