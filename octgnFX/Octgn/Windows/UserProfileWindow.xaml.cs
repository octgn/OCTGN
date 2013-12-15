using System;
using System.Windows;

namespace Octgn.Windows
{
    using System.Linq;

    using Octgn.Site.Api.Models;
    using Octgn.Tabs.Profile;

    using Skylabs.Lobby;

    /// <summary>
    /// Interaction logic for UserProfileWindow.xaml
    /// </summary>
    public partial class UserProfileWindow
    {
        public static void Show(User user)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(new Action(() => Show(user)));
                return;
            }

            var upw = new UserProfileWindow();
            upw.ProfilePage.Load(user);
            upw.Show();
        }
        public UserProfileWindow()
        {
            InitializeComponent();
            //ProfilePage.Model = new UserProfileViewModel(new ApiUser());
        }
    }
}
