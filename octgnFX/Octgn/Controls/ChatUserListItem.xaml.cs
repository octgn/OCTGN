using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Skylabs.Lobby;
using agsXMPP;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for ChatUserListItem.xaml
    /// </summary>
    public partial class ChatUserListItem : UserControl
    {
        public NewUser User
        {
            get { return _user; }
            set { 
                _user = value;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          UserNameTextBox.Text = _user.UserName;
                                                      }));
            }
        }

        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                Dispatcher.BeginInvoke(new Action(() =>
                                                      {
                                                          ImageAdmin.Visibility = _isAdmin
                                                                                      ? Visibility.Visible
                                                                                      : Visibility.Collapsed;
                                                      }));
            }
        }

        public bool IsMod
        {
            get { return _isMod; }
            set
            {
                _isMod = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImageMod.Visibility = _isMod
                                                                                      ? Visibility.Visible
                                                                                      : Visibility.Collapsed;

                }));
            }
        }

        public bool IsOwner
        {
            get { return _isOwner; }
            set
            {
                _isOwner = value;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    ImageOwner.Visibility = _isOwner
                                                                                      ? Visibility.Visible
                                                                                      : Visibility.Collapsed;

                }));
            }
        }

        private NewUser _user;
        private bool _isAdmin;
        private bool _isMod;
        private bool _isOwner;
        public ChatUserListItem()
        {
            InitializeComponent();
            User = new NewUser(new Jid("noone@server.octgn.info"));
            IsAdmin = false;
            IsMod = false;
            IsOwner = false;
        }
    }
}
