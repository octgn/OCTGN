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

        public InviteToGameRequest(Guid gameId, string password)
            : base("invitetogamerequest", "invitetogamerequest", "octgn:invitetogamerequest")
        {
            this.GameId = gameId;
            this.Password = password;
        }

        public Guid GameId {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(this.GetTag("GameId"), out ret);
                return ret;
            }
            set { this.SetTag("GameId", value.ToString()); }
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