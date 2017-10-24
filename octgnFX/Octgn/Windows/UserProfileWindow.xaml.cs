using System;
using System.Windows;
using Octgn.Library;

namespace Octgn.Windows
{
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
