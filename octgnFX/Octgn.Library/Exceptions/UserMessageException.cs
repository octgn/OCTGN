namespace Octgn.Library.Exceptions
{
    using System;

    public class UserMessageException : Exception
    {
        public UserMessageException(string message)
            :base(message)
        {
            
        }

        public UserMessageException(string message, Exception innerException)
            :base(message,innerException)
        {
            
        }
        public UserMessageException(string message, params object[] args)
            : base(String.Format(message, args))
        {

        }
    }
}
