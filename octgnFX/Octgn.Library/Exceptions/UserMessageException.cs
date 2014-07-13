namespace Octgn.Library.Exceptions
{
    using System;

    public class UserMessageException : Exception
    {
        public UserMessageExceptionMode Mode { get; set; }

        public UserMessageException(UserMessageExceptionMode mode, string message)
            : base(message)
        {
            Mode = mode;
        }

        public UserMessageException(UserMessageExceptionMode mode, string message, Exception innerException)
            : base(message, innerException)
        {
            Mode = mode;
        }
        public UserMessageException(UserMessageExceptionMode mode, string message, params object[] args)
            : base(String.Format(message, args))
        {
            Mode = mode;
        }

        public UserMessageException(string message)
            : base(message)
        {
			Mode = UserMessageExceptionMode.Blocking;
        }

        public UserMessageException(string message, Exception innerException)
            : base(message, innerException)
        {
			Mode = UserMessageExceptionMode.Blocking;
        }

        public UserMessageException(string message, params object[] args)
            : base(String.Format(message, args))
        {
			Mode = UserMessageExceptionMode.Blocking;
        }

        //public UserMessageException(UserMessageExceptionMode mode, string message, Action retry)
        //    : base(message)
        //{
        //TODO Add some logic so that we can make Retryable user message exceptions. So A user can see an error and decide if they want to retry. Could save lots of guess work.
        //}
    }

    public enum UserMessageExceptionMode
    {
        Blocking, Background
    }
}
