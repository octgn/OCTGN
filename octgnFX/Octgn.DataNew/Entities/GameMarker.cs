// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Text;
namespace Octgn.DataNew.Entities
{
    using System;

    public class GameMarker : ICloneable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }

        public string ModelString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("('{0}','{1}')", Name, Id);
            return sb.ToString();
        }

        public bool Equals(GameMarker other)
        {
            if (other == null) return false;
            if (other.Id != Id) return false;
            if (other.Name == null) return false;
            if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) == false) return false;
            return true;
        }

        public virtual object Clone()
        {
            var ret = new GameMarker();
            ret.Id = this.Id;
            ret.Name = this.Name.Clone() as string;
            ret.Source = Source;
            return ret;
        }
    }
}