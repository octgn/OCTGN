namespace Octide.Messages
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Windows;
    using System.Collections.Specialized;

    using GalaSoft.MvvmLight;

    using CommonServiceLocator;

    public class MouseWheelTableZoom
    {
        public int Delta { get; set; }

        public Point Center { get; set; }

        public MouseWheelTableZoom(int delta, Point center)
        {
            Delta = delta;
            Center = center;
        }
    }

    public class UpdateViewModelMessage<TSource, TReturn> where TSource : ViewModelBase
    {
        public UpdateViewModelMessage(Expression<Func<TSource, TReturn>> property, TReturn val)
        {
            var prop = (PropertyInfo)((MemberExpression)property.Body).Member;

            var vm = ServiceLocator.Current.GetInstance<TSource>();

            prop.SetValue(vm, val, null);
        }
    }

    public class CardDetailsChangedMessage
    {

    }

    public class CardPropertiesUpdateMessage
    {
    }

    public class CardPropertiesChangedMessage
    {
        public NotifyCollectionChangedEventArgs Args { get; set; }

        public CardPropertiesChangedMessage(NotifyCollectionChangedEventArgs args)
        {
            Args = args;
        }
    }
}