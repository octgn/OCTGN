/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Skylabs.Lobby
{
    using System;

    using agsXMPP.Xml.Dom;

    public class InviteToGameRequest : Element
    {
        public InviteToGameRequest()
            : base("invitetogamerequest", "invitetogamerequest", "octgn:invitetogamerequest")
        {
            
        }

        public InviteToGameRequest(Guid sessionId, string password)
            : base("invitetogamerequest", "invitetogamerequest", "octgn:invitetogamerequest")
        {
            this.SessionId = sessionId;
            this.Password = password;
        }

        public Guid SessionId
        {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(this.GetTag("SessionId"), out ret);
                return ret;
            }
            set { this.SetTag("SessionId", value.ToString()); }
        }

        public string Password
        {
            get
            {
                return this.GetTag("Password");
            }
            set
            {
                this.SetTag("Password", value);
            }
        }
    }
}