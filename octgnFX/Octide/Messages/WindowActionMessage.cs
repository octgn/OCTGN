// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide.Messages
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight;

    public class WindowActionMessage<T> where T : ViewModelBase
    {
        public WindowActionType Action { get; internal set; }
        public T WindowViewModel { get; internal set; }

        public WindowActionMessage(WindowActionType action)
        {
            Action = action;
            var a = typeof(T);
            WindowViewModel = ServiceLocator.Current.GetInstance<T>();
        }
    }

    public enum WindowActionType
    {
        Close, Show, Create, SetMain, Hide
    }
}