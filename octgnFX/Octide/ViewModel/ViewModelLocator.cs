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
            ViewModelLocator.ViewModelKernel.Bind<ActionViewModel>().To<ActionViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<ActionMenuViewModel>().To<ActionMenuViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<TableViewModel>().To<TableViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GameLoader>().To<GameLoader>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<AssetsTabViewModel>().To<AssetsTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SizeViewModel>().To<SizeViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<BoardViewModel>().To<BoardViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PropertyTabViewModel>().To<PropertyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<CounterViewModel>().To<CounterViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GroupViewModel>().To<GroupViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SetTabViewModel>().To<SetTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SetCardsViewModel>().To<SetCardsViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SetPackagesViewModel>().To<SetPackagesViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SetSummaryViewModel>().To<SetSummaryViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<ProxyTabViewModel>().To<ProxyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<ProxyCardViewModel>().To<ProxyCardViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PreviewTabViewModel>().To<PreviewTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PhaseViewModel>().To<PhaseViewModel>().InSingletonScope();
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

        public static ActionViewModel ActionViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ActionViewModel>();
            }
        }

        public static ActionMenuViewModel ActionMenuViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ActionMenuViewModel>();
            }
        }

        public static TableViewModel TableViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TableViewModel>();
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
        public static SizeViewModel SizeViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SizeViewModel>();
            }
        }
        public static BoardViewModel BoardViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BoardViewModel>();
            }
        }
        public static SetTabViewModel SetTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SetTabViewModel>();
            }
        }
        public static SetPackagesViewModel SetPackagesViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SetPackagesViewModel>();
            }
        }
        public static SetCardsViewModel SetCardsViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SetCardsViewModel>();
            }
        }
        public static SetSummaryViewModel SetSummaryViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SetSummaryViewModel>();
            }
        }
        public static ProxyTabViewModel ProxyTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProxyTabViewModel>();
            }
        }
        public static ProxyCardViewModel ProxyCardViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProxyCardViewModel>();
            }
        }
        public static PreviewTabViewModel PreviewTabViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PreviewTabViewModel>();
            }
        }
        public static PhaseViewModel PhaseViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PhaseViewModel>();
            }
        }
    }
}