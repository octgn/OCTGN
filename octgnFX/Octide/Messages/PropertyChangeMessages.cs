// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide.Messages
{
    using Octide.ItemModel;
    using System.Collections.Specialized;

    public enum PropertyChangedMessageAction
    {
        None,
        Add,
        Remove,
        Modify
    }

    public class PropertyChangedMessage
    {
        public PropertyChangedMessageAction Action { get; set; }
        public static PropertyChangedMessageAction GetEnum(NotifyCollectionChangedEventArgs a)
        {
            var action = new PropertyChangedMessageAction();
            switch (a.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    action = PropertyChangedMessageAction.Add;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    action = PropertyChangedMessageAction.Remove;
                    break;
            }
            return action;
        }
        public object GetObject(NotifyCollectionChangedEventArgs a)
        {
            object item = null;
            switch (a.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    item = a.NewItems[0];
                    break;
                case NotifyCollectionChangedAction.Remove:
                    item = a.OldItems[0];
                    break;
            }
            return item;
        }
    }

    public class GroupChangedMessage : PropertyChangedMessage
    {
        public GroupChangedMessage() { }
        public GroupChangedMessage(NotifyCollectionChangedEventArgs args)
        {
            Group = GetObject(args) as GroupItemViewModel;
            Action = GetEnum(args);
        }
        public GroupItemViewModel Group { get; set; }
    }
    public class CustomPropertyChangedMessage : PropertyChangedMessage
    {
        public CustomPropertyChangedMessage() { }
        public CustomPropertyChangedMessage(NotifyCollectionChangedEventArgs args)
        {
            Prop = GetObject(args) as PropertyItemViewModel;
            Action = GetEnum(args);
        }
        public PropertyItemViewModel Prop { get; set; }
    }

    public class CardSizeChangedMesssage : PropertyChangedMessage
    {
        public CardSizeChangedMesssage() { }
        public CardSizeChangedMesssage(NotifyCollectionChangedEventArgs args)
        {
            Size = GetObject(args) as SizeItemViewModel;
            Action = GetEnum(args);
        }

        public SizeItemViewModel Size { get; set; }
    }
    
}