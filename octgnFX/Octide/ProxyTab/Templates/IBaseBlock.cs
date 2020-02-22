// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.ProxyGenerator.Definitions;
using System.Collections.ObjectModel;

namespace Octide.ProxyTab.TemplateItemModel
{
    public abstract class IBaseBlock : IdeListBoxItemBase
    {
        public LinkDefinition.LinkWrapper _wrapper;
        public new ObservableCollection<IBaseBlock> ItemSource { get; set; }

        public override void Remove()
        {
            if (!CanRemove) return;
            ItemSource.Remove(this);
        }
    }

}
