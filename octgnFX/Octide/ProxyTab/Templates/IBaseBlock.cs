// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.ProxyGenerator.Definitions;

namespace Octide.ProxyTab.ItemModel
{
    public abstract class IBaseBlock : IdeBaseItem
    {
        public LinkDefinition.LinkWrapper _wrapper;

        public IBaseBlock(IdeCollection<IdeBaseItem> source) : base(source)
        {

        }

    }

}
