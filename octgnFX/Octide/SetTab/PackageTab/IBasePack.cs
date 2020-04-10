// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Collections.ObjectModel;

namespace Octide.SetTab.ItemModel
{
    public abstract class IBasePack : IdeBaseItem
    {
        public IBasePack(IdeCollection<IdeBaseItem> source) : base(source)
        {

        }

        public IPackItem _packItem;
    }
}
