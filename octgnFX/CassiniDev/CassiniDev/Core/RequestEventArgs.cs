#region

using System;
using CassiniDev.ServerLog;

#endregion

namespace CassiniDev
{
    public class RequestEventArgs : EventArgs
    {
        private readonly Guid _id;

        private readonly LogInfo _requestLog;

        private readonly LogInfo _responseLog;

        public RequestEventArgs(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            _requestLog = requestLog;
            _responseLog = responseLog;
            _id = id;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public LogInfo RequestLog
        {
            get { return _requestLog; }
        }

        public LogInfo ResponseLog
        {
            get { return _responseLog; }
        }
    }
}