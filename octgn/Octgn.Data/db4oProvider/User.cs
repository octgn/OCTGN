/// Copyright (c) 2008-2011 Brad Williams
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
/// associated documentation files (the "Software"), to deal in the Software without restriction,
/// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
/// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
/// subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial
/// portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
/// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
/// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
/// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Configuration.Provider;
using System.Web.Security;
using Db4objects.Db4o.Config.Attributes;
using WCSoft.Common;

namespace WCSoft.db4oProviders
{
    public class User : DataContainer
    {
        [Indexed] public readonly Guid PKID;

        [Indexed] public string ApplicationName;

        public string Comment;
        public DateTime CreationDate;
        [Indexed] public string Email;
        public int FailedPasswordAnswerAttemptCount;
        public DateTime FailedPasswordAnswerAttemptWindowStart;
        public int FailedPasswordAttemptCount;
        public DateTime FailedPasswordAttemptWindowStart;
        public bool IsApproved;
        public bool IsLockedOut;
        public bool IsOnLine;

        [Indexed] public DateTime LastActivityDate;
        public DateTime LastLockedOutDate;

        public DateTime LastLoginDate;
        public DateTime LastPasswordChangedDate;
        public string Password;
        public string PasswordAnswer;
        public string PasswordQuestion;
        [Indexed] public string Username;

        public User(Guid pkid,
                    string username,
                    string password,
                    string email,
                    string passwordQuestion,
                    string passwordAnswer,
                    bool isApproved,
                    string comment,
                    DateTime creationDate,
                    DateTime lastPasswordChangedDate,
                    DateTime lastActivityDate,
                    string applicationName,
                    bool isLockedOut,
                    DateTime lastLockedOutDate,
                    int failedPasswordAttemptCount,
                    DateTime failedPasswordAttemptWindowStart,
                    int failedPasswordAnswerAttemptCount,
                    DateTime failedPasswordAnswerAttemptWindowStart)
        {
            PKID = pkid;
            Username = username;
            Password = password;
            Email = email;
            PasswordQuestion = passwordQuestion;
            PasswordAnswer = passwordAnswer;
            IsApproved = isApproved;
            Comment = comment;
            CreationDate = creationDate;
            LastPasswordChangedDate = lastPasswordChangedDate;
            LastActivityDate = lastActivityDate;
            ApplicationName = applicationName;
            IsLockedOut = isLockedOut;
            LastLockedOutDate = lastLockedOutDate;
            FailedPasswordAttemptCount = failedPasswordAttemptCount;
            FailedPasswordAttemptWindowStart = failedPasswordAttemptWindowStart;
            FailedPasswordAnswerAttemptCount = failedPasswordAnswerAttemptCount;
            FailedPasswordAnswerAttemptWindowStart = failedPasswordAnswerAttemptWindowStart;
        }

        public override string ToString()
        {
            return string.Format("User:{0}:{1}",
                                 Username,
                                 ApplicationName);
        }

        public void UpdateFailureCount(string failureType, MembershipProvider provider)
        {
            DateTime windowStart;
            int failureCount;

            if (failureType == "password")
            {
                windowStart = FailedPasswordAttemptWindowStart;
                failureCount = FailedPasswordAttemptCount;
            }
            else if (failureType == "passwordAnswer")
            {
                windowStart = FailedPasswordAnswerAttemptWindowStart;
                failureCount = FailedPasswordAnswerAttemptCount;
            }
            else
                throw new ProviderException("Invalid failure type");

            DateTime windowEnd = windowStart.AddMinutes(provider.PasswordAttemptWindow);

            if (failureCount == 0 || DateTime.Now > windowEnd)
            {
                // First password failure or outside of PasswordAttemptWindow. 
                // Start a new password failure count from 1 and a new window starting now.

                if (failureType == "password")
                {
                    FailedPasswordAttemptCount = 1;
                    FailedPasswordAttemptWindowStart = DateTime.Now;
                }
                else if (failureType == "passwordAnswer")
                {
                    FailedPasswordAnswerAttemptCount = 1;
                    FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                }
            }
            else
            {
                if (failureCount++ >= provider.MaxInvalidPasswordAttempts)
                {
                    // Password attempts have exceeded the failure threshold. Lock out
                    // the user.

                    IsLockedOut = true;
                    LastLockedOutDate = DateTime.Now;
                }
                else
                {
                    // Password attempts have not exceeded the failure threshold. Update
                    // the failure counts. Leave the window the same.

                    if (failureType == "password")
                    {
                        FailedPasswordAttemptCount = failureCount;
                    }
                    else if (failureType == "passwordAnswer")
                    {
                        FailedPasswordAnswerAttemptCount = failureCount;
                    }
                }
            }
        }
    }
}
