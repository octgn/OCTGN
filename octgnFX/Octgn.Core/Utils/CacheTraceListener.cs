using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Octgn.Utils
{
    public class TraceEvent
    {
        public object[] Args;
        public TraceEventCache Cache;
        public string Format;
        public int Id;
        public String Message;
        public string Source;
        public TraceEventType Type;

        public override string ToString()
        {
            var sb = new StringBuilder();
            //sb.AppendFormat("<{0}>", Type);
            if (Cache != null)
                sb.AppendFormat("[{0} {1}]", Cache.DateTime.ToShortTimeString(), Cache.DateTime.ToShortDateString());
            if (Source != null)
                sb.AppendFormat("'{0}'", Source);
            //sb.Append(Id);
            sb.Append(" - ");
            if (Message == null && Args != null && Format != null)
            {
                sb.AppendFormat(Format, Args);
            }
            else
            {
                sb.Append(Message);
            }
            return sb.ToString();
        }
    }

    public class CacheTraceListener : TraceListener
    {
        #region Delegates

        public delegate void EventAdded(TraceEvent te);

        #endregion

        public CacheTraceListener()
        {
            Events = new List<TraceEvent>();
        }

        public List<TraceEvent> Events { get; set; }
        public event EventAdded OnEventAdd;

        public override void Write(string message)
        {
            var te = new TraceEvent
                         {Cache = new TraceEventCache(), Message = message, Type = TraceEventType.Information};
            Events.Add(te);
            try
            {
                if (Events.Count > 1000)
                    Events.RemoveAt(0);
            }
            catch (Exception) { }
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void WriteLine(string message)
        {
            var te = new TraceEvent
                         {
                             Cache = new TraceEventCache(),
                             Message = message + Environment.NewLine,
                             Type = TraceEventType.Information
                         };
            Events.Add(te);
            try
            {
                if (Events.Count > 1000)
                    Events.RemoveAt(0);
            }
            catch (Exception) { }
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
                                        string message)
        {
            var te = new TraceEvent {Cache = eventCache, Source = source, Type = eventType, Id = id, Message = message};
            Events.Add(te);
            try
            {
                if (Events.Count > 1000)
                    Events.RemoveAt(0);
            }
            catch (Exception) { }
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            var te = new TraceEvent {Cache = eventCache, Source = source, Type = eventType, Id = id};
            Events.Add(te);
            try
            {
                if (Events.Count > 1000)
                    Events.RemoveAt(0);
            }
            catch (Exception) { }
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
                                        string format, params object[] args)
        {
            var te = new TraceEvent
                         {Cache = eventCache, Source = source, Type = eventType, Id = id, Format = format, Args = args};
            Events.Add(te);
            try
            {
                if (Events.Count > 1000)
                    Events.RemoveAt(0);
            }
            catch(Exception){}
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }
    }
}