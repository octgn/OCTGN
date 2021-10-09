/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Octgn.Extentions;

namespace Octgn.Tabs.Login
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here."),
    SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed. Suppression is OK here.")]
    public partial class LoginTab : UserControl
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LoginTabViewModel LoginVM { get; set; }

        public LoginTab()
        {
            if (!this.IsInDesignMode())
            {
                LoginVM = new LoginTabViewModel();
         //       this.Loaded += RefreshNews_EventCallback;
            }
            InitializeComponent();

            if (this.IsInDesignMode()) return;

            this.labelRegister.MouseLeftButtonUp += (sender, args) => Program.LaunchUrl(AppConfig.WebsitePath);
            this.labelForgot.MouseLeftButtonUp +=
                (sender, args) => Program.LaunchUrl(AppConfig.WebsitePath);
            this.labelSubscribe.MouseLeftButtonUp += delegate
                {
                    var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
                    if (url != null)
                    {
                        Program.LaunchUrl(url);
                    }
                };

 //           var timer = new DispatcherTimer(TimeSpan.FromMinutes(2), DispatcherPriority.Normal, RefreshNews_EventCallback, Dispatcher);
 //           timer.Start();
        }

 //       private async void RefreshNews_EventCallback(object sender, EventArgs e) {
 //           await LoginVM.News.Refresh();
 //       }

        #region UI Events
        private void Button1Click(object sender, RoutedEventArgs e) {
            LoginVM.LoginAsync();
        }
        private void PasswordBox1KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginVM.LoginAsync();
            }
        }
        #endregion

        internal struct NewsFeedItem
        {
            public DateTime Time { get; set; }
            public string Message { get; set; }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Program.LaunchUrl(e.Uri.ToString());
        }
    }
}
