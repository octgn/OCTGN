using System;
using System.Xml;
using System.Xml.Serialization;

namespace Skylabs.ConsoleHelper
{
    [Serializable]
    public class ConsoleEventError : ConsoleEvent
    {
        [XmlElement("ExceptionMessage")]
        public String ExceptionMessage
        {
            get
            {
                if(exception.Message != null)
                    return exception.Message;
                else
                    return "No Exception Message Data.";
            }
            set { }
        }

        [XmlElement("StackTrace")]
        public String StackTrace
        {
            get
            {
                if(exception.StackTrace != null)
                    return exception.StackTrace;
                else
                    return "No Stack Trace Message Data.";
            }
            set { }
        }

        [XmlIgnore()]
        public Exception exception { get; set; }

        private ConsoleEventError()
        {
        }

        public ConsoleEventError(String message, Exception e)
            : base(message)
        {
            Header = "!Error: ";
            //Color = ConsoleColor.Red;
            exception = e;
        }
    }
}