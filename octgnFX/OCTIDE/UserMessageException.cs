namespace OCTIDE
{
    using System;

    public class UserMessageException:Exception
    {
         
        public UserMessageException(string message)
            : base(message)
        {
            
        }
    }
}