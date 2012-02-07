using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Octgn
{
    public class TraceEvent
    {
        public TraceEventCache Cache;
        public string Source;
        public TraceEventType Type;
        public int ID;
        public String Message;
        public string Format;
        public object[] Args;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<{0}>", Type);
            if (Cache != null)
                if (Cache.DateTime != null)
                    sb.AppendFormat("[{0} {1}]", Cache.DateTime.ToShortTimeString(), Cache.DateTime.ToShortDateString());
            if (Source != null)
                sb.AppendFormat("'{0}'", Source);
            sb.Append(ID);
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
        public delegate void EventAdded(TraceEvent te);
        public event EventAdded OnEventAdd;

        public CacheTraceListener()
        {
            Events = new List<TraceEvent>();
        }

        public List<TraceEvent> Events { get; set; }

        public override void Write(string message)
        {
            TraceEvent te = new TraceEvent();
            te.Cache = new TraceEventCache();
            te.Message = message;
            te.Type = TraceEventType.Information;
            Events.Add(te);
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void WriteLine(string message)
        {
            TraceEvent te = new TraceEvent();
            te.Cache = new TraceEventCache();
            te.Message = message + System.Environment.NewLine;
            te.Type = TraceEventType.Information;
            Events.Add(te);
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent te = new TraceEvent();
            te.Cache = eventCache;
            te.Source = source;
            te.Type = eventType;
            te.ID = id;
            te.Message = message;
            Events.Add(te);
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent te = new TraceEvent();
            te.Cache = eventCache;
            te.Source = source;
            te.Type = eventType;
            te.ID = id;
            Events.Add(te);
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            TraceEvent te = new TraceEvent();
            te.Cache = eventCache;
            te.Source = source;
            te.Type = eventType;
            te.ID = id;
            te.Format = format;
            te.Args = args;
            Events.Add(te);
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }
    }
}