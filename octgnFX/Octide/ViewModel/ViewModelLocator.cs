/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Octide"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;

using Microsoft.Practices.ServiceLocation;

namespace Octide.ViewModel
{
    using CommonServiceLocator.NinjectAdapter.Unofficial;

    using Ninject;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public static IKernel ViewModelKernel = new StandardKernel();

        public static NinjectServiceLocator ServiceLocatorProvider = new NinjectServiceLocator(ViewModelKernel);

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => ServiceLocatorProvider);
        }

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                //SimpleIoc.Default.Register<IDataService, DesignDataService>();
            }
            else
            {
                // Create run time view services and models
                //SimpleIoc.Default.Register<IDataService, DataService>();
            }

            ViewModelLocator.ViewModelKernel.Bind<GameTabViewModel>().To<GameTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<TableTabViewModel>().To<TableTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GameLoader>().To<GameLoader>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<AssetsTabViewModel>().To<AssetsTabViewModel>().InSingletonScope();
        }
        
        public static void Cleanup()
        {
        }

        public static GameTabViewModel GameTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GameTabViewModel>();
            }
        }

        public static TableTabViewModel TableTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TableTabViewModel>();
            }
        }

        public static GameLoader GameLoader
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GameLoader>();
            }
        }

        public static AssetsTabViewModel AssetsTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AssetsTabViewModel>();
            }
        }
    }
}