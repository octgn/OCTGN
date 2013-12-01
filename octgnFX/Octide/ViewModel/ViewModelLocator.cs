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
        }
        
        public static void Cleanup()
        {
        }
    }
}