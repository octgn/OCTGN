﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;

namespace Octgn.Tabs.Profile
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Win32;

    using Octgn.Annotations;
    using Octgn.Controls;
    using Octgn.Library.Exceptions;
    using Octgn.Site.Api;
    using Octgn.Site.Api.Models;

    using Skylabs.Lobby;

    using log4net;

    /// <summary>
    /// Interaction logic for UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private UserProfileViewModel model;

        public UserProfileViewModel Model
        {
            get
            {
                return this.model;
            }
            set
            {
                //if (Equals(value, this.model))
                //{
                //    return;
                //}
                this.model = value;
                this.OnPropertyChanged("Model");
            }
        }

        private ApiUser User { get; set; }

        public UserProfilePage()
        {
            Model = new UserProfileViewModel(new ApiUser());
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        public void Load(User user)
        {
            var client = new Octgn.Site.Api.ApiClient();
            if (user == null || user.UserName == null) return;
            var au = client.UsersFromUsername(new[] { user.UserName }).FirstOrDefault();
            if (au == null) return;
            Load(au);
        }

        public void Load(ApiUser user)
        {
            if (user == null) return;
            User = user;
            UserProfileViewModel mod = null;
            Task.Factory.StartNew(() =>
                {
                    mod = this.GetModel(user);
                })
                .ContinueWith(x =>
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                            {
                                if(mod != null)
                                    this.Model = mod;
                            }));
                    });
        }

        private UserProfileViewModel GetModel(ApiUser user)
        {
            UserProfileViewModel ret = null;
            try
            {
                var client = new ApiClient();
                ApiUser me;
                me = client.UsersFromUsername(new[] { user.UserName }).FirstOrDefault();
                ret = me == null ? new UserProfileViewModel(new ApiUser()) : new UserProfileViewModel(me);

            }
            catch (Exception ex)
            {
                Log.Warn("GetModel", ex);
            } 
            return ret;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (User != null)
            {
                Load(User);
            }
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Program.LaunchUrl(e.Uri.ToString());
        }

        #region OnPropertyChanged
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
        #endregion OnPropertyChanged

        private void ChangeIconClick(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter =
                "All Images|*.BMP;*.JPG;*.JPEG;*.PNG|BMP Files: (*.BMP)|*.BMP|JPEG Files: (*.JPG;*.JPEG)|*.JPG;*.JPEG|PNG Files: (*.PNG)|*.PNG";
            fd.CheckFileExists = true;
            if (!(bool)fd.ShowDialog()) return;
            if (!File.Exists(fd.FileName)) return;
            var finfo = new FileInfo(fd.FileName);
            try
            {
                var img = System.Drawing.Image.FromFile(fd.FileName);
                if (img.Width != 16 || img.Height != 16)
                {
                    img = ResizeImage(img, new System.Drawing.Size(16, 16));
                    //throw new UserMessageException("Image must be exactly 16x16 in size.");
                }
                using (var imgStream = new MemoryStream())
                {
                    img.Save(imgStream,ImageFormat.Png);
                    imgStream.Seek(0, SeekOrigin.Begin);
                    var client = new Octgn.Site.Api.ApiClient();
                    var res = client.SetUserIcon(
                        Program.LobbyClient.Username,
                        Program.LobbyClient.Password,
                        "png",
                        imgStream);

                    switch (res)
                    {
                        case UserIconSetResult.Ok:
                            Task.Factory.StartNew(() =>
                                {
                                    Thread.Sleep(5000);
                                    this.OnLoaded(null, null);
                                });
                            
                            TopMostMessageBox.Show(
                                "Your icon has been changed. It can take a few minutes for the change to take place.",
                                "Change Icon",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            break;
                        case UserIconSetResult.ImageSizeBad:
                            throw new UserMessageException("Image must be exactly 16x16 in size.");
                        case UserIconSetResult.NotSubscribed:
                            throw new UserMessageException("You must be subscribed to do that.");
                        case UserIconSetResult.CredentialsError:
                            throw new UserMessageException(
                                "Incorrect username/password. try exiting OCTGN and reloading it.");
                        default:
                            throw new UserMessageException(
                                "There was an error uploading your icon. Please try again later.");
                    }
                }
            }
            catch (UserMessageException ex)
            {
                TopMostMessageBox.Show(ex.Message,"Change Icon Error",MessageBoxButton.OK,MessageBoxImage.Warning);
                Log.Warn("ChangeIconClick(UserMessageException)",ex);
            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show(
                    "There was an unknown error. Please try a different image.",
                    "Change Icon Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Log.Warn("ChangeIconClick",ex);
            }
            
        }


        private static System.Drawing.Image ResizeImage(System.Drawing.Image imgToResize, System.Drawing.Size size)
        {
            return new Bitmap(imgToResize, size);
        }
    }

    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private string userName;

        private string userImage;

        private string userIcon;

        private string userSubscription;

        private bool isSubscribed;

        private bool isMe;

        private bool canChangeIcon;

        public string UserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                if (value == this.userName)
                {
                    return;
                }
                this.userName = value;
                this.OnPropertyChanged("UserName");
            }
        }

        public string UserImage
        {
            get
            {
                return this.userImage;
            }
            set
            {
                if (value == this.userImage)
                {
                    return;
                }
                this.userImage = value;
                this.OnPropertyChanged("UserImage");
            }
        }

        public string UserIcon
        {
            get
            {
                return this.userIcon;
            }
            set
            {
                if (value == this.userIcon)
                {
                    return;
                }
                this.userIcon = value;
                this.OnPropertyChanged("UserIcon");
            }
        }

        public string UserSubscription
        {
            get
            {
                return this.userSubscription;
            }
            set
            {
                if (value == this.userSubscription)
                {
                    return;
                }
                this.userSubscription = value;
                this.OnPropertyChanged("UserSubscription");
            }
        }

        public bool IsSubscribed
        {
            get
            {
                return this.isSubscribed;
            }
            set
            {
                if (value.Equals(this.isSubscribed))
                {
                    return;
                }
                this.isSubscribed = value;
                this.OnPropertyChanged("IsSubscribed");
            }
        }

        public bool IsMe
        {
            get
            {
                return this.isMe;
            }
            set
            {
                if (value.Equals(this.isMe))
                {
                    return;
                }
                this.isMe = value;
                this.OnPropertyChanged("IsMe");
            }
        }

        public bool CanChangeIcon
        {
            get
            {
                return this.canChangeIcon;
            }
            set
            {
                if (value.Equals(this.canChangeIcon))
                {
                    return;
                }
                this.canChangeIcon = value;
                this.OnPropertyChanged("CanChangeIcon");
            }
        }

        public UserProfileViewModel(ApiUser user)
        {
            UserName = user.UserName;
            UserImage = user.ImageUrl;
            UserIcon = user.IconUrl;
            UserSubscription = user.Tier;
            IsSubscribed = user.IsSubscribed;
            if(Program.LobbyClient != null && Program.LobbyClient.IsConnected)
                IsMe = Program.LobbyClient.Me.UserName.Equals(user.UserName, StringComparison.InvariantCultureIgnoreCase);
            CanChangeIcon = IsSubscribed && IsMe;
        }

        #region PropertyChanged
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
        #endregion PropertyChanged
    }
}
