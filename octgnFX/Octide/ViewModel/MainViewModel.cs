using System.IO;

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
        private string baseTitle;
        private bool needsSave;

        public string Title
        {
            get
            {
                var ret = baseTitle;
                if (needsSave)
                    ret = "* " + ret;
                return ret;
            }
        }

        public MainViewModel()
        {
            baseTitle = "OCTIDE";
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(3000);
                var path = new DirectoryInfo(Path.Combine(Octgn.Library.Config.Instance.Paths.DataDirectory, "GameDatabase"));

                //var pathstr = Path.Combine(path.GetDirectories().First().FullName,"definition.xml");
                //ViewModelLocator.GameLoader.LoadGame(pathstr);
            });
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
                x =>
                {
                    if (String.IsNullOrWhiteSpace(x.NewValue.Name)) baseTitle = "OCTIDE";
                    else baseTitle = "OCTIDE - " + x.NewValue.Name;
                    RaisePropertyChanged(this.Title);
                });
            Messenger.Default.Register<PropertyChangedMessage<bool>>(this, x =>
            {
                if (x.PropertyName != "NeedsSave") return;
                needsSave = x.NewValue;
                RaisePropertyChanged(this.Title);
            });
        }
    }
}