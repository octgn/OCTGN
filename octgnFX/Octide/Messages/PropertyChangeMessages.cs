namespace Octide.Messages
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Windows;
    using System.Collections.Specialized;

    using GalaSoft.MvvmLight;

    using Octide.ViewModel;
    

    public class CustomPropertyChangedMessage
    {
        public CustomPropertyItemModel Prop { get; set; }
    }

    public class CardSizeChangedMesssage
    {
        public SizeItemModel Size { get; set; }
    }

}