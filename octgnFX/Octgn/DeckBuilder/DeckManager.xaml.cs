using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using IWshRuntimeLibrary;
using Octgn.DataNew.Entities;
using Octgn.Library;
using System.ComponentModel;
using Octgn.Annotations;

namespace Octgn.DeckBuilder
{
    using System.Windows.Controls;
    using System.Windows.Input;

    using Octgn.Controls;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Play.Gui;

    /// <summary>
    /// Interaction logic for DeckManager.xaml
    /// </summary>
    public partial class DeckManager : INotifyPropertyChanged
    {
        private DeckList _selectedList;
        private string _searchString;

        private MetaDeck selectedDeck;

        private bool isLoading;

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

        public MetaDeck SelectedDeck
        {
            get
            {
                return this.selectedDeck;
            }
            set
            {
                if (Equals(value, this.selectedDeck))
                {
                    return;
                }
                this.selectedDeck = value;
                this.OnPropertyChanged("SelectedDeck");
            }
        }

        public bool IsLoading
        {
            get
            {
                return this.isLoading;
            }
            set
            {
                if (value.Equals(this.isLoading))
                {
                    return;
                }
                this.isLoading = value;
                this.OnPropertyChanged("IsLoading");
            }
        }

        public ObservableCollection<DeckList> DeckLists { get; set; }

        public ObservableCollection<MetaDeck> Decks { get; set; }

        public DeckManager()
        {
            Decks = new ObservableCollection<MetaDeck>();
            DeckLists = new ObservableCollection<DeckList>();
            IsLoading = true;
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
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SelectedList = null;
                    SelectedList = list;
                }));
                IsLoading = false;
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

        private void OnSearchChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var d in SelectedList.Decks)
            {
                d.UpdateFilter(SearchString);
            }
        }

        private void SelectedDeckListChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            foreach (var dl in DeckLists)
            {
                foreach (var d in dl.Decks)
                {
                    d.UpdateFilter(SearchString);
                }
            }
            SelectedList = e.NewValue as DeckList;
        }

        private void ListBoxDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedDeck == null) return;
            if ( SelectedDeck.IsCorrupt) return;
            var g = Octgn.DataNew.DbContext.Get().Games.First(x => x.Id == SelectedDeck.GameId);

            var viewer = new DeckCardsViewer(SelectedDeck,g);
            var window = new OctgnChrome();
            window.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            window.VerticalContentAlignment = VerticalAlignment.Stretch;
            window.Content = viewer;
            window.ShowDialog();
        }

        private void DeckFocus(object sender, RoutedEventArgs e)
        {
            var s = sender as MetaDeck;
            var lb = sender as ListBoxItem;
            var sb = DeckListBox.Resources["ShowDeckInfo"] as Storyboard;
            sb.Begin(lb);
        }

        private void DeckLostFocus(object sender, RoutedEventArgs e)
        {
            var s = sender as MetaDeck;
            var lb = sender as ListBoxItem;
            var sb = DeckListBox.Resources["HideDeckInfo"] as Storyboard;
            sb.Begin(lb);
        }

        private void EditDeckClick(object sender, RoutedEventArgs e)
        {
            var de = new DeckBuilderWindow((sender as Button).DataContext as MetaDeck);
            de.ShowDialog();
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
