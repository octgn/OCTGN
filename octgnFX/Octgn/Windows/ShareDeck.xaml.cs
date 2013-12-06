using GalaSoft.MvvmLight.Messaging;
using Octgn.UiMessages;

namespace Octgn.Windows
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using log4net;

    using Octgn.Annotations;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;
    using Octgn.Site.Api;

    /// <summary>
    /// Interaction logic for ShareDeck.xaml
    /// </summary>
    public partial class ShareDeck : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        private string errorText;

        private string shareUrl;

        private bool enableInput;

        private bool showProgressBar;

        private string deckName;

        public bool ShowError
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ErrorText);
            }
        }

        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
            set
            {
                if (value == this.errorText)
                {
                    return;
                }
                this.errorText = value;
                this.OnPropertyChanged("ErrorText");
                this.OnPropertyChanged("ShowError");
            }
        }

        public bool ShowShareUrl
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ShareUrl);
            }
        }

        public string ShareUrl
        {
            get
            {
                return this.shareUrl;
            }
            set
            {
                if (value == this.shareUrl)
                {
                    return;
                }
                this.shareUrl = value;
                this.OnPropertyChanged("ShareUrl");
                this.OnPropertyChanged("ShowShareUrl");
            }
        }

        public bool EnableInput
        {
            get
            {
                return this.enableInput;
            }
            set
            {
                if (value.Equals(this.enableInput))
                {
                    return;
                }
                this.enableInput = value;
                this.OnPropertyChanged("EnableInput");
            }
        }

        public bool ShowProgressBar
        {
            get
            {
                return this.showProgressBar;
            }
            set
            {
                if (value.Equals(this.showProgressBar))
                {
                    return;
                }
                this.showProgressBar = value;
                this.OnPropertyChanged("ShowProgressBar");
            }
        }

        public string DeckName
        {
            get
            {
                return this.deckName;
            }
            set
            {
                if (value == this.deckName)
                {
                    return;
                }
                this.deckName = value;
                this.OnPropertyChanged("DeckName");
            }
        }

        public IDeck Deck { get; set; }

        public ShareDeck():this(null)
        {
        }

        public ShareDeck(IDeck deck)
        {
            EnableInput = true;
            Deck = deck;
            InitializeComponent();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShareClicked(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(shareDeck);
        }

        private void shareDeck()
        {
            try
            {
                Log.Info("Start");
                ErrorText = "";
                ShareUrl = "";
                EnableInput = false;
                ShowProgressBar = true;

				// Do work here.
                var tempFile = Path.GetTempFileName();
                var game = GameManager.Get().GetById(Deck.GameId);
                Deck.Save(game, tempFile);

                var client = new ApiClient();
                if (!Program.LobbyClient.IsConnected) throw new UserMessageException("You must be logged into OCTGN to share a deck.");
                if (string.IsNullOrWhiteSpace(DeckName)) throw new UserMessageException("The deck name can't be blank.");
                if (DeckName.Length > 32) throw new UserMessageException("The deck name is too long.");
                var result = client.ShareDeck(Program.LobbyClient.Username, Program.LobbyClient.Password, DeckName, tempFile);
                if (result.Error)
                {
                    throw new UserMessageException(result.Message);
                }
				Messenger.Default.Send(new RefreshSharedDecksMessage());
                var path = result.DeckPath;
                ShareUrl = path;
                ErrorText = "";
            }
            catch (UserMessageException e)
            {
                ErrorText = e.Message;
                ShareUrl = "";
                Log.Warn("Error sharing deck", e);
            }
            catch (Exception e)
            {
                ErrorText = "Something unexpected happened. Please try again in a little bit.";
                ShareUrl = "";
                Log.Warn("Error sharing deck", e);
            }
            finally
            {
                EnableInput = true;
                ShowProgressBar = false;
                Log.Info("Finished");
            }
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
}
