namespace Octide.ViewModel
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;

    using Octgn.DataNew.Entities;

    public class MainViewModel : ViewModelBase
    {
        private string title;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (value == this.title) return;
                this.title = value;
                RaisePropertyChanged(this.Title);
            }
        }

        public MainViewModel()
        {
            Title = "OCTIDE";
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                ViewModelLocator.GameLoader.LoadGame(@"C:\Users\Kelly\Documents\Octgn\GameDatabase\a6c8d2e8-7cd8-11dd-8f94-e62b56d89593\definition.xml");
            });
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
				x =>
				{
				    if (String.IsNullOrWhiteSpace(x.NewValue.Name)) Title = "OCTIDE";
				    else Title = "OCTIDE - " + x.NewValue.Name;
				});
        }
    }
}