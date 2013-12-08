namespace Octide.Messages
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Windows;

    using GalaSoft.MvvmLight;

    using Octide.ViewModel;

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

            var vm = ViewModelLocator.ServiceLocatorProvider.GetInstance<TSource>();

            prop.SetValue(vm, val, null);
        }
    }

    public class CardDetailsChangedMessage
    {
        
    }
}