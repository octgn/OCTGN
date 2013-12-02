namespace Octide.ViewModel
{
    using System;
    using System.Windows;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;

    using Microsoft.Practices.ServiceLocation;

    using Octide.Messages;
    using Octide.Views;

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
				RegisterWindowEvents<SplashViewModel,SplashWindow>();
            }


            ViewModelLocator.ViewModelKernel.Bind<SplashViewModel>().To<SplashViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<MainViewModel>().To<MainViewModel>().InSingletonScope();

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

        public static SplashViewModel SplashViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SplashViewModel>();
            }
        }

        public static MainViewModel MainViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            ServiceLocator.Current.GetInstance<SplashViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<MainViewModel>().Cleanup();
        }
    }
}
