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