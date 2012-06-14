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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Hosting;
using System.Web.Profile;
using Db4objects.Db4o;
using db4oProviders;
using WCSoft.Common;

namespace WCSoft.db4oProviders
{
    public class Profile : DataContainer, IComparable<Profile>
    {
        public readonly string ApplicationName;
        public readonly string Username;
        public bool IsAnonymous;
        public DateTime LastActivityDate;
        public DateTime LastUpdatedDate;
        public List<SettingsPropertyValue> SettingsPropertyValueList;

        public Profile(string username, string applicationName, bool isAnonymous, DateTime lastActivityDate,
                       DateTime lastUpdatedDate)
        {
            Username = username;
            ApplicationName = applicationName;
            IsAnonymous = isAnonymous;
            LastActivityDate = lastActivityDate;
            LastUpdatedDate = lastUpdatedDate;
            SettingsPropertyValueList = new List<SettingsPropertyValue>();
        }

        #region IComparable<Profile> Members

        public int CompareTo(Profile other)
        {
            return Username.CompareTo(other.Username);
        }

        #endregion

        public override string ToString()
        {
            return string.Format("Profile:{0}:{1}",
                                 Username,
                                 ApplicationName);
        }
    }
}
