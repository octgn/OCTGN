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

namespace Octide.ViewModel
{
    using CommonServiceLocator;

    using Ninject;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public static IKernel ViewModelKernel = new StandardKernel();
        public static IServiceLocator CurrentServiceLocator = new NInjectServiceLocator(ViewModelKernel);

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(()=> CurrentServiceLocator));
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
            ViewModelLocator.ViewModelKernel.Bind<SizeTabViewModel>().To<SizeTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PropertyTabViewModel>().To<PropertyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<CounterViewModel>().To<CounterViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GroupViewModel>().To<GroupViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SetTabViewModel>().To<SetTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PreviewTabViewModel>().To<PreviewTabViewModel>().InSingletonScope();
        }

        public static void Cleanup()
        {
        }

        public static GameLoader GameLoader
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GameLoader>();
            }
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

        public static AssetsTabViewModel AssetsTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AssetsTabViewModel>();
            }
        }
        public static GroupViewModel GroupViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<GroupViewModel>();
            }
        }
        public static CounterViewModel CounterViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CounterViewModel>();
            }
        }
        public static PropertyTabViewModel PropertyTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PropertyTabViewModel>();
            }
        }
        public static SizeTabViewModel SizeTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SizeTabViewModel>();
            }
        }
        public static SetTabViewModel SetTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SetTabViewModel>();
            }
        }
        public static PreviewTabViewModel PreviewTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PreviewTabViewModel>();
            }
        }
    }
}