// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;

namespace Octide.ItemModel
{
    public class ActionSeparatorItemModel : IBaseAction
    {

        public ActionSeparatorItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            _action = new GroupActionSeparator();
        }

        public ActionSeparatorItemModel(GroupActionSeparator a, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _action = a;
        }

        public ActionSeparatorItemModel(ActionSeparatorItemModel a, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _action = new GroupActionSeparator()
            {
                HeaderExecute = a._action.HeaderExecute,
                IsGroup = a.IsGroup,
                Name = a.Name,
                ShowExecute = a._action.ShowExecute
            };
        }

        public override object Clone()
        {
            return new ActionSeparatorItemModel(this, Source);
        }
        public override object Create()
        {
            return new ActionSeparatorItemModel(Source);
        }
    }
}
