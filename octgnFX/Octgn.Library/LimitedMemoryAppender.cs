using System;
using System.Threading;
using log4net.Appender;
using log4net.Core;

namespace Octgn.Library
{
    public class LimitedMemoryAppender : MemoryAppender
    {
        public int MaxEvents { get; set; }

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public LimitedMemoryAppender()
        {
            MaxEvents = 50;
        }

        override protected void Append(LoggingEvent loggingEvent)
        {
            try
            {
				_locker.EnterWriteLock();
                base.Append(loggingEvent);
                while (m_eventsList.Count > MaxEvents)
                {
                    m_eventsList.RemoveAt(0);
                }

            }
            finally
            {
				_locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the events that have been logged.
        /// </summary>
        /// <returns>
        /// The events that have been logged
        /// </returns>
        /// <remarks>
        /// <para>
        /// Gets the events that have been logged.
        /// </para>
        /// </remarks>
        public override LoggingEvent[] GetEvents()
        {
            try
            {
                _locker.EnterReadLock();
                return base.GetEvents();
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }
    }
}