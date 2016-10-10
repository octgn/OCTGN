/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Octgn.Controls;
using Octgn.Core;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Octgn.Tabs.GameManagement
{
    public partial class AddFeed : OverlayDialog, IDisposable
    {
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            this.OnClose?.Invoke(sender, result);
        }

        public static DependencyProperty ErrorProperty = DependencyProperty.Register(
            nameof(Error), typeof(String), typeof(AddFeed));

        public bool HasErrors { get; private set; }
        public string Error {
            get { return this.GetValue(ErrorProperty) as String; }
            private set { this.SetValue(ErrorProperty, value); }
        }

        public static DependencyProperty FeedNameProperty = DependencyProperty.Register(
            nameof(FeedName), typeof(String), typeof(AddFeed));

        public string FeedName {
            get { return this.GetValue(FeedNameProperty) as String; }
            private set { this.SetValue(FeedNameProperty, value); }
        }

        public static DependencyProperty FeedUrlProperty = DependencyProperty.Register(
            nameof(FeedUrl), typeof(String), typeof(AddFeed));

        public string FeedUrl {
            get { return this.GetValue(FeedUrlProperty) as String; }
            private set { this.SetValue(FeedUrlProperty, value); }
        }

        public static DependencyProperty FeedUsernameProperty = DependencyProperty.Register(
            nameof(FeedUsername), typeof(String), typeof(AddFeed));

        public string FeedUsername {
            get { return this.GetValue(FeedUsernameProperty) as String; }
            private set { this.SetValue(FeedUsernameProperty, value); }
        }

        public static DependencyProperty FeedPasswordProperty = DependencyProperty.Register(
            nameof(FeedPassword), typeof(String), typeof(AddFeed));

        public string FeedPassword {
            get { return this.GetValue(FeedPasswordProperty) as String; }
            private set { this.SetValue(FeedPasswordProperty, value); }
        }

        private Decorator Placeholder;

        public AddFeed()
        {
            InitializeComponent();
        }

        void ValidateFields(string name, string feed, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("You must enter a feed name");
            if (String.IsNullOrWhiteSpace(feed))
                throw new Exception("You must enter a feed path");
            var result = Core.GameFeedManager.Get().ValidateFeedUrl(feed, username, password);
            if (result != FeedValidationResult.Valid)
            {
                switch (result)
                {
                    case FeedValidationResult.InvalidFormat:
                        throw new Exception("This feed is in an invalid format.");
                    case FeedValidationResult.InvalidUrl:
                        throw new Exception("The feed is down or it is not a valid feed");
                    case FeedValidationResult.RequiresAuthentication:
                        throw new Exception("This feed requires authentication. Please enter a username and password.");
                    case FeedValidationResult.Unknown:
                        throw new Exception("An unknown error has occured.");
                }
            }
        }

        void SetError(string error = "")
        {
            this.HasErrors = !string.IsNullOrWhiteSpace(error);
            this.Error = error;
        }

        #region Dialog
        public void Show(Decorator placeholder)
        {
            this.Placeholder = placeholder;

            placeholder.Child = this;
        }

        public void Close()
        {
            this.Close(DialogResult.Abort);
        }

        private void Close(DialogResult result)
        {
            ProgressBar.IsIndeterminate = false;
            this.Placeholder.Child = null;
            this.FireOnClose(this, result);
        }

        void StartWait()
        {
            ((FrameworkElement)this.Content).IsEnabled = false;
            ProgressBar.IsIndeterminate = true;
        }

        void EndWait()
        {
            ((FrameworkElement)this.Content).IsEnabled = true;
            ProgressBar.IsIndeterminate = false;
        }

        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private async void ButtonAddClick(object sender, RoutedEventArgs e)
        {
            if (FeedName == null) FeedName = "";
            if (FeedUrl == null) FeedUrl = "";
            FeedName = FeedName.Trim();
            FeedUrl = FeedUrl.Trim();
            var feedName = FeedName;
            var feedUrl = FeedUrl;
            var username = FeedUsername;
            var password = FeedPassword;
            this.StartWait();
            Program.Dispatcher = this.Dispatcher;

            var error = "";
            try {
                await Task.Run(() => {
                    this.ValidateFields(feedName, feedUrl, username, password);
                    Core.GameFeedManager.Get().AddFeed(feedName, feedUrl, username, password);
                });
            } catch (Exception ex) {
                error = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(error))
                this.SetError(error);
            this.EndWait();
            if (string.IsNullOrWhiteSpace(error))
                this.Close(DialogResult.OK);
        }
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object, DialogResult>)d;
                }
            }
        }

        #endregion
    }
}
