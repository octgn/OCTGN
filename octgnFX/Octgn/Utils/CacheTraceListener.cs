using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Octgn.Utils
{
    using System.Linq;

    public class TraceEvent
    {
        public object[] Args;
        public TraceEventCache Cache;
        public string Format;
        public int Id;
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
            if (Format != null)
            {
                sb.AppendFormat(Format, Args);
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
            events = new List<TraceEvent>();
        }

        public List<TraceEvent> Events
        {
            get
            {
                lock (this)
                {
                    return events;
                }
            }
        }

        public void ClearEvents()
        {
            lock (this)
            {
                events.Clear();
            }
        }

        private readonly List<TraceEvent> events;
        public event EventAdded OnEventAdd;

        public void ActionLock(Action<List<TraceEvent>> action)
        {
            lock (this)
            {
                action.Invoke(events);
            }
        }

        public override void Write(string message)
        {
            var te = new TraceEvent
                     {
                         Cache = new TraceEventCache(),
                         Format = message,
                         Type = TraceEventType.Information
                     };
            lock (this)
            {
                events.Add(te);
                try
                {
                    if (events.Count > 1000) events.RemoveAt(0);
                }
                catch (Exception)
                {
                }
            }
            if (OnEventAdd != null) OnEventAdd.Invoke(te);
        }

        public override void WriteLine(string message)
        {
            var te = new TraceEvent
                     {
                         Cache = new TraceEventCache(),
                         Format = message + Environment.NewLine,
                         Type = TraceEventType.Information
                     };
            lock (this)
            {
                events.Add(te);
                try
                {
                    if (events.Count > 1000) events.RemoveAt(0);
                }
                catch (Exception)
                {
                }
            }
            if (OnEventAdd != null) OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id,string message)
        {
            var te = new TraceEvent
                     {
                         Cache = eventCache,
                         Source = source,
                         Type = eventType,
                         Id = id,
                         Format = message
                     };
            lock (this)
            {
                events.Add(te);
                try
                {
                    if (events.Count > 1000) events.RemoveAt(0);
                }
                catch (Exception)
                {
                }
            }
            if (OnEventAdd != null) OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            var te = new TraceEvent { Cache = eventCache, Source = source, Type = eventType, Id = id };
            lock (this)
            {
                events.Add(te);
                try
                {
                    if (events.Count > 1000) events.RemoveAt(0);
                }
                catch (Exception)
                {
                }
            }
            if (OnEventAdd != null) OnEventAdd.Invoke(te);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            var te = new TraceEvent
                     {
                         Cache = eventCache,
                         Source = source,
                         Type = eventType,
                         Id = id,
                         Format = format,
                         Args = args
                     };
            lock (this)
            {
                events.Add(te);
                try
                {
                    if (events.Count > 1000) events.RemoveAt(0);
                }
                catch (Exception)
                {
                }
            }
            if (OnEventAdd != null)
                OnEventAdd.Invoke(te);
        }
    }
}