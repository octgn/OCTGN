// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using Octgn.DataNew.Entities;

namespace Octgn.DataNew
{
    public static class ExtensionMethods
    {
        public static IDictionary<string, CardPropertySet> CloneProperties(this ICard card)
        {
            var ret = new Dictionary<string, CardPropertySet>();
            foreach (var p in card.Properties)
            {
                ret.Add((string)p.Key.Clone(), p.Value.Clone() as CardPropertySet);
            }
            return ret;
        }
    }
}