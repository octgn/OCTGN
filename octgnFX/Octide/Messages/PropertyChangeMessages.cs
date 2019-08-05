namespace Octide.Messages
{
    using Octide.ItemModel;

    public class CustomPropertyChangedMessage
    {
        public PropertyItemViewModel Prop { get; set; }
    }

    public class CardSizeChangedMesssage
    {
        public SizeItemViewModel Size { get; set; }
    }
    
}