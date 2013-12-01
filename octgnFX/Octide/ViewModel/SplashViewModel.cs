using GalaSoft.MvvmLight;

namespace Octide.ViewModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;
    using Octide.Views;

    public class SplashViewModel : ViewModelBase
    {
        public string Title
        {
            get
            {
                return "OCTIDE";
            }
        }

        public Version Version
        {
            get
            {
                return typeof(SplashViewModel).Assembly.GetName().Version;
            }
        }
        public SplashViewModel()
        {
            Task.Factory.StartNew(Spin);
        }

        private void Spin()
        {
            var endTime = DateTime.Now.AddSeconds(1);
            while (DateTime.Now < endTime)
            {
                Thread.Sleep(10);
            }
            Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Create));
            Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.Show));
            Messenger.Default.Send(new WindowActionMessage<MainViewModel>(WindowActionType.SetMain));
            Messenger.Default.Send(new WindowActionMessage<SplashViewModel>(WindowActionType.Close));
			this.Cleanup();
        }
    }
}