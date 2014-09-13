using System.Windows;
using Exceptionless;

namespace Octide
{
    using GalaSoft.MvvmLight.Threading;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            ExceptionlessClient.Current.Register();
            DispatcherHelper.Initialize();
        }
    }
}
