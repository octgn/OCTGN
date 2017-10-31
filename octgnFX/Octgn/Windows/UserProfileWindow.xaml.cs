using Octgn.Online;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Octgn.Windows
{
    public partial class UserProfileWindow
    {
        public static async Task Show(User user)
        {
            Application.Current.Dispatcher.VerifyAccess();

            var upw = new UserProfileWindow();
            await upw.ProfilePage.Load(user);
            upw.Show();
        }
        public UserProfileWindow()
        {
            InitializeComponent();
        }
    }
}
