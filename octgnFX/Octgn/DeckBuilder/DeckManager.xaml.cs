using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Octgn.DataNew.Entities;
using Octgn.Library;
using System.ComponentModel;
using Octgn.Annotations;

namespace Octgn.DeckBuilder
{
    /// <summary>
    /// Interaction logic for DeckManager.xaml
    /// </summary>
    public partial class DeckManager : INotifyPropertyChanged
    {
        private DeckList _selectedList;
        private string _searchString;

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if (value == _searchString) return;
                _searchString = value;
                OnPropertyChanged("SearchString");
            }
        }

        public DeckList SelectedList
        {
            get { return _selectedList; }
            set
            {
                if (value == _selectedList) return;
                _selectedList = value;
                OnPropertyChanged("SelectedList");
            }
        }

        public ObservableCollection<DeckList> DeckLists { get; set; }

        public ObservableCollection<MetaDeck> Decks { get; set; }

        public DeckManager()
        {
            Decks = new ObservableCollection<MetaDeck>();
            DeckLists = new ObservableCollection<DeckList>();
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
            Task.Factory.StartNew(() =>
            {
                var list = new DeckList(Paths.Get().DeckPath, this.Dispatcher,true)
                {
                    Name = "All"
                };
                Dispatcher.Invoke(new Action(() => DeckLists.Add(list)));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class DeckList
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ObservableCollection<DeckList> DeckLists { get; set; }
        public ObservableCollection<MetaDeck> Decks { get; set; }

        public DeckList(string path, Dispatcher disp, bool gameFolders = false)
        {
            Path = path;
            Name = new DirectoryInfo(path).Name;

            DeckLists = new ObservableCollection<DeckList>();
            Decks = new ObservableCollection<MetaDeck>();

            foreach (var f in Directory.GetDirectories(path).Select(x => new DirectoryInfo(x)))
            {
                if (gameFolders)
                {
                    if (DataNew
                        .DbContext.Get()
                        .Games
                        .Any(x => x.Name.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var dl = new DeckList(f.FullName,disp);
                        disp.Invoke(new Action(() => DeckLists.Add(dl)));
                    }
                }
                else
                {
                    var dl = new DeckList(f.FullName,disp);
                    disp.Invoke(new Action(() => DeckLists.Add(dl)));
                }
            }

            foreach (var f in Directory.GetFiles(Path, "*.o8d"))
            {
                var deck = new MetaDeck(f);
                disp.Invoke(new Action(() => Decks.Add(deck)));
            }
            foreach (var d in DeckLists.SelectMany(x => x.Decks))
            {
                MetaDeck d1 = d;
                disp.Invoke(new Action(()=>Decks.Add(d1)));
            }
        }
    }


}
