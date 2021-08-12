// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

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
            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => CurrentServiceLocator));

            ViewModelLocator.ViewModelKernel.Bind<GameLoader>().To<GameLoader>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<AssetsTabViewModel>().To<AssetsTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GameInformationTabViewModel>().To<GameInformationTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<DeckSectionTabViewModel>().To<DeckSectionTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GameFontTabViewModel>().To<GameFontTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SymbolTabViewModel>().To<SymbolTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<DocumentTabViewModel>().To<DocumentTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<MarkerTabViewModel>().To<MarkerTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GameModeTabViewModel>().To<GameModeTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SoundTabViewModel>().To<SoundTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PropertyTabViewModel>().To<PropertyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PythonTabViewModel>().To<PythonTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<VariableTabViewModel>().To<VariableTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<GameEventTabViewModel>().To<GameEventTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<SetTabViewModel>().To<SetTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<ProxyTabViewModel>().To<ProxyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Bind<PreviewTabViewModel>().To<PreviewTabViewModel>().InSingletonScope();
        }

        public static void RebindViewModelLocator()
        {
            Cleanup();
            ViewModelLocator.ViewModelKernel.Rebind<GameLoader>().To<GameLoader>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<AssetsTabViewModel>().To<AssetsTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<GameInformationTabViewModel>().To<GameInformationTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<DeckSectionTabViewModel>().To<DeckSectionTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<GameFontTabViewModel>().To<GameFontTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<SymbolTabViewModel>().To<SymbolTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<DocumentTabViewModel>().To<DocumentTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<MarkerTabViewModel>().To<MarkerTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<GameModeTabViewModel>().To<GameModeTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<SoundTabViewModel>().To<SoundTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<PropertyTabViewModel>().To<PropertyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<PythonTabViewModel>().To<PythonTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<VariableTabViewModel>().To<VariableTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<GameEventTabViewModel>().To<GameEventTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<SetTabViewModel>().To<SetTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<ProxyTabViewModel>().To<ProxyTabViewModel>().InSingletonScope();
            ViewModelLocator.ViewModelKernel.Rebind<PreviewTabViewModel>().To<PreviewTabViewModel>().InSingletonScope();
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


        public static GameLoader GameLoader => ServiceLocator.Current.GetInstance<GameLoader>();
        public static AssetsTabViewModel AssetsTabViewModel => ServiceLocator.Current.GetInstance<AssetsTabViewModel>();
        public static GameInformationTabViewModel GameInformationTabViewModel => ServiceLocator.Current.GetInstance<GameInformationTabViewModel>();
        public static DeckSectionTabViewModel DeckSectionTabViewModel => ServiceLocator.Current.GetInstance<DeckSectionTabViewModel>();
        public static GameFontTabViewModel GameFontTabViewModel => ServiceLocator.Current.GetInstance<GameFontTabViewModel>();
        public static SoundTabViewModel SoundTabViewModel => ServiceLocator.Current.GetInstance<SoundTabViewModel>();
        public static MarkerTabViewModel MarkerTabViewModel => ServiceLocator.Current.GetInstance<MarkerTabViewModel>();
        public static DocumentTabViewModel DocumentTabViewModel => ServiceLocator.Current.GetInstance<DocumentTabViewModel>();
        public static GameModeTabViewModel GameModeTabViewModel => ServiceLocator.Current.GetInstance<GameModeTabViewModel>();
        public static SymbolTabViewModel SymbolTabViewModel => ServiceLocator.Current.GetInstance<SymbolTabViewModel>();
        public static PropertyTabViewModel PropertyTabViewModel => ServiceLocator.Current.GetInstance<PropertyTabViewModel>();
        public static PythonTabViewModel PythonTabViewModel => ServiceLocator.Current.GetInstance<PythonTabViewModel>();
        public static GameEventTabViewModel GameEventTabViewModel => ServiceLocator.Current.GetInstance<GameEventTabViewModel>();
        public static VariableTabViewModel VariableTabViewModel => ServiceLocator.Current.GetInstance<VariableTabViewModel>();
        public static SetTabViewModel SetTabViewModel => ServiceLocator.Current.GetInstance<SetTabViewModel>();
        public static ProxyTabViewModel ProxyTabViewModel => ServiceLocator.Current.GetInstance<ProxyTabViewModel>();
        public static PreviewTabViewModel PreviewTabViewModel => ServiceLocator.Current.GetInstance<PreviewTabViewModel>();

        public static void Cleanup()
        {
            ServiceLocator.Current.GetInstance<GameLoader>().Cleanup();
            ServiceLocator.Current.GetInstance<AssetsTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<MainViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<GameInformationTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<DeckSectionTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<GameFontTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<SoundTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<DocumentTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<GameModeTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<MarkerTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<SymbolTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<PropertyTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<GameEventTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<PythonTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<VariableTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<SetTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<ProxyTabViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<PreviewTabViewModel>().Cleanup();
        }
    }
}