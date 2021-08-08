// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide.Messages
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight;
    using System;
    using System.Collections.Specialized;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Windows;

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

    public class CardPropertiesChangedMessage
    {
        public NotifyCollectionChangedEventArgs Args { get; set; }

        public CardPropertiesChangedMessage(NotifyCollectionChangedEventArgs args)
        {
            Args = args;
        }
    }
}