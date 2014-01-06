using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

using log4net;
using Octgn.Controls;
using Octgn.Library;

namespace Octgn.DeckBuilder
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;

    /// <summary>
    /// Interaction logic for DeckEditorPreviewControl.xaml
    /// </summary>
    public partial class DeckEditorPreviewControl : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private CardViewModel card;

        private bool isDragDropping;
        private bool alwaysShowProxy;

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("Game", typeof(Game), typeof(DeckEditorPreviewControl), new PropertyMetadata(default(Game)));

        public Game Game
        {
            get
            {
                return (Game)GetValue(GameProperty);
            }
            set
            {
                var g = new Game() { Name = "No Game Selected", CardBack = "pack://application:,,,/Resources/Back.jpg" };
                if (value != null)
                {
                    g = value;
                }
                SetValue(GameProperty, g);
                OnPropertyChanged("Card");
                OnPropertyChanged("IsCardSelected");
            }
        }

        public CardViewModel Card
        {
            get
            {
                return this.card;
            }
            set
            {
                if (this.card == value) return;
                this.card = value;
                card.AlwaysShowProxy = this.AlwaysShowProxy;
                Dispatcher.Invoke(new Action(() => this.AllowDrop = this.card != null));
                OnPropertyChanged("Card");
                OnPropertyChanged("NoCardSelected");
            }
        }

        public bool IsDragDropping
        {
            get
            {
                return this.isDragDropping;
            }
            set
            {
                if (value == this.isDragDropping) return;
                this.isDragDropping = value;
                OnPropertyChanged("IsDragDropping");
            }
        }

        public bool AlwaysShowProxy
        {
            get { return this.alwaysShowProxy; }
            set { 
                this.alwaysShowProxy = value;
                if (this.card != null)
                {
                    this.card.AlwaysShowProxy = value;
                }
            }
        }

        public DeckEditorPreviewControl()
        {
            Game = new Game() { Name = "No Game Selected", CardBack = "pack://application:,,,/Resources/Back.jpg" };
            Card = new CardViewModel();
            //Card = new CardViewModel(new Card() { ImageUri = "pack://application:,,,/Resources/Back.jpg" });

            InitializeComponent();
        }

        public void SetGame(Game game)
        {
            Game = game;
            Card.SetCard(null);
        }

        private void BackArrowMouseUp(object sender, MouseButtonEventArgs e)
        {
            Card.Index--;
        }

        private void ForwardArrowMouseUp(object sender, MouseButtonEventArgs e)
        {
            Card.Index++;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged

        public class CardViewModel : INotifyPropertyChanged
        {
            private Card card;

            private int index;
            private bool alwaysShowProxy;

            public Card Card
            {
                get
                {
                    return this.card;
                }
                set
                {
                    if (this.card == value) return;
                    this.card = value;
                    this.OnPropertyChanged("Card");
                    this.OnPropertyChanged("CardUri");
                    this.OnPropertyChanged("CardImage");
                    this.OnPropertyChanged("HasAlternates");
                    this.OnPropertyChanged("AlternateCount");
                    this.OnPropertyChanged("NoCardSelected");
                    this.OnPropertyChanged("IsNotProxyImage");
                }
            }

            public string CardUri
            {
                get
                {
                    if (Card == null) return "pack://application:,,,/Resources/Back.jpg";

                    if (AlwaysShowProxy)
                    {
                        var ret = Card.GetProxyPicture();
                        return ret;
                    }
                    else
                    {
                        var ret = Card.GetPicture();
                        return ret;
                    }
                }
            }

            public BitmapImage CardImage
            {
                get
                {
                    Stream imageStream = null;
                    if (CardUri.StartsWith("pack"))
                    {
                        var sri = Application.GetResourceStream(new Uri(CardUri));
                        imageStream = sri.Stream;
                    }
                    else
                    {
                        imageStream = File.OpenRead(CardUri);
                    }

                    var ret = new BitmapImage();
                    ret.BeginInit();
                    ret.CacheOption = BitmapCacheOption.OnLoad;
                    ret.StreamSource = imageStream;
                    //ret.UriSource = new Uri(CardUri, UriKind.Absolute);
                    ret.EndInit();
                    imageStream.Close();
                    return ret;
                }
            }

            public bool HasAlternates
            {
                get
                {
                    if (Card == null) return false;
                    return Card.Properties.Count != 1;
                }
            }

            public bool NoCardSelected
            {
                get
                {
                    return Card == null;
                }
            }

            public int AlternateCount
            {
                get
                {
                    if (Card == null) return 0;
                    return Card.Properties.Count;
                }
            }

            public bool IsNotProxyImage
            {
                get
                {
                    if (Card == null) return false; 
                    var set = Card.GetSet();
                    var files = Directory.GetFiles(set.ImagePackUri, card.GetImageUri() + ".*").OrderBy(x => x.Length).ToArray();
                    if (files.Length == 0) return false;
                    return true;
                }
            }

            public bool AlwaysShowProxy
            {
                get
                {
                    return this.alwaysShowProxy;
                }
                set
                {
                    this.alwaysShowProxy = value;
                    this.OnPropertyChanged("CardUri");
                    this.OnPropertyChanged("CardImage");
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    if (value == index) return;
                    if (value < 0)
                        index = AlternateCount - 1;
                    else if (value >= AlternateCount) index = 0;
                    else index = value;

                    Card.SetPropertySet(Card.Properties.ToArray()[index].Key);

                    for (var i = 0; i < Alternates.Count; i++)
                    {
                        if (i != index) Alternates[i].Selected = false;
                        else
                        {
                            Alternates[i].Selected = true;
                        }
                    }

                    this.OnPropertyChanged("Index");
                    this.OnPropertyChanged("CardUri");
                    this.OnPropertyChanged("CardImage");
                    this.OnPropertyChanged("IsNotProxyImage");
                }
            }

            public ObservableCollection<Alternate> Alternates { get; set; }

            public CardViewModel()
            {
                Alternates = new ObservableCollection<Alternate>();
            }

            public void SetCard(Card c)
            {
                Card = c;
                Alternates.Clear();
                if (Card == null) return;
                var i = 0;
                foreach (var a in c.Properties)
                {
                    Alternates.Add(new Alternate(this, a.Key, i));
                    i++;
                }
                Index = 0;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        public class Alternate : INotifyPropertyChanged
        {
            private CardViewModel VM;

            public string Name { get; set; }

            public int Index { get; set; }

            public bool Selected
            {
                get
                {
                    return VM.Index == Index;
                }
                set
                {
                    this.OnPropertyChanged("Selected");
                    if (value.Equals(VM.Index == Index))
                    {
                        return;
                    }
                    VM.Index = this.Index;
                }
            }

            public Alternate(CardViewModel vm, string altName, int altIndex)
            {
                VM = vm;
                Name = altName;
                Index = altIndex;
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

        private void OnImageDrop(object sender, DragEventArgs args)
        {
            try
            {
                if (!IsDragDropping) return;
                IsDragDropping = false;
                if (SubscriptionModule.Get().IsSubscribed == false)
                {
                    Program.DoCrazyException(new Exception("Not subscribed"), "You must be subscribed to do that.");
                    return;
                }
                var dropFiles = (string[])args.Data.GetData(DataFormats.FileDrop);

                var file = dropFiles[0];

                var set = Card.Card.GetSet();

                var garbage = Paths.Get().GraveyardPath;
                if (!Directory.Exists(garbage)) Directory.CreateDirectory(garbage);

                var files =
                    Directory.GetFiles(set.ImagePackUri, Card.Card.GetImageUri() + ".*")
                        .OrderBy(x => x.Length)
                        .ToArray();

                // Delete all the old picture files
                foreach (var f in files.Select(x => new FileInfo(x)))
                {
                    f.MoveTo(System.IO.Path.Combine(garbage, f.Name));
                }

                var newPath = System.IO.Path.Combine(set.ImagePackUri, Card.Card.GetImageUri() + Path.GetExtension(file));
                File.Copy(file, newPath);
                OnPropertyChanged("Card");

            }
            catch (Exception e)
            {
                Log.Warn("Could not replace image", e);
                Program.DoCrazyException(
                    e,
                    "Could not replace the image, something went terribly wrong...You might want to try restarting OCTGN and/or your computer.");
            }
        }

        private void OnImageDragEnter(object sender, DragEventArgs args)
        {
            try
            {
                if (Card == null) return;
                if (Card.Card == null) return;
                if (Game == null) return;
                if (!args.Data.GetDataPresent(DataFormats.FileDrop)) return;
                var dropFiles = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (dropFiles.Length != 1) return;
                var file = dropFiles.First();
                var attr = File.GetAttributes(file);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) return;

                using (var imageStream = File.OpenRead(file))
                using (var image = Image.FromStream(imageStream))
                {
                    // Check to see if it's an image
                    Log.Debug(image.Height);
                }
                if (!File.Exists(file)) return;

                IsDragDropping = true;
                args.Handled = true;
            }
            catch (Exception e)
            {
                Log.Warn("Drag error", e);
            }
        }

        private void OnImageDragLeave(object sender, DragEventArgs e)
        {
            IsDragDropping = false;
        }

        private void OnImageGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Copy)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.Hand);
            }
            else
                e.UseDefaultCursors = true;

            e.Handled = true;
        }

        private void DeleteImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (SubscriptionModule.Get().IsSubscribed == false)
                {
                    Program.DoCrazyException(new Exception("Not subscribed"), "You must be subscribed to do that." );
                    return;
                }
                var set = Card.Card.GetSet();

                var garbage = Paths.Get().GraveyardPath;
                if (!Directory.Exists(garbage))
                    Directory.CreateDirectory(garbage);

                var files =
                    Directory.GetFiles(set.ImagePackUri, Card.Card.GetImageUri() + ".*")
                        .OrderBy(x => x.Length)
                        .ToArray();

                // Delete all the old picture files
                foreach (var f in files.Select(x => new FileInfo(x)))
                {
                    f.MoveTo(System.IO.Path.Combine(garbage, f.Name));
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Could not delete card image", ex);
                Program.DoCrazyException(ex, "Could not delete the card image, something went terribly wrong...You might want to try restarting OCTGN and/or your computer.");
            }
        }
    }
}
