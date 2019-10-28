// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide.Messages
{
    using Octide.ItemModel;

    public class CustomPropertyChangedMessage
    {
        public PropertyItemViewModel Prop { get; set; }
        public CustomPropertyChangedMessageAction Action { get; set; }
    }

    public class CardSizeChangedMesssage
    {
        public SizeItemViewModel Size { get; set; }
    }
    
    public enum CustomPropertyChangedMessageAction
    {
        None,
        Add,
        Remove,
        Modify
    }
}