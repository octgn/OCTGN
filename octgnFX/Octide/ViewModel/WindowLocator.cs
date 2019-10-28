// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide.ViewModel
{
    using System;
    using System.Windows;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;

    using Octide.Messages;
    using Octide.Views;
    using CommonServiceLocator;

    public class WindowLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public WindowLocator()
        {
            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                //SimpleIoc.Default.Register<IDataService, DesignDataService>();
            }
            else
            {
                RegisterWindowEvents<MainViewModel, MainWindow>();
				RegisterWindowEvents<LoaderViewModel, LoaderWindow>();
            }


            ViewModelLocator.ViewModelKernel.Bind<MainViewModel>().To<MainViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<LoaderViewModel>().To<LoaderViewModel>().InSingletonScope();

        }

        public void RegisterWindowEvents<TVM, TWIN>()
            where TVM : ViewModelBase
            where TWIN : Window
        {
            Messenger.Default.Register<WindowActionMessage<TVM>>(this, HandleWindowMessage<TVM, TWIN>);
        }

        public void HandleWindowMessage<TVM, TWIN>(WindowActionMessage<TVM> message)
            where TVM : ViewModelBase
            where TWIN : Window
        {
            switch (message.Action)
            {
                case WindowActionType.Create:
                    CreateWindow<TWIN>();
                    break;
            }
        }

        public void CreateWindow<T>()
        {
            DispatcherHelper.UIDispatcher.Invoke(new Action(() => Activator.CreateInstance<T>()));
        }
		public static MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();
		public static LoaderViewModel LoaderViewModel => ServiceLocator.Current.GetInstance<LoaderViewModel>();

		public static void Cleanup()
        {
			ServiceLocator.Current.GetInstance<LoaderViewModel>().Cleanup();
			ServiceLocator.Current.GetInstance<MainViewModel>().Cleanup();
        }
    }
}
