﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;
using Octgn.UiMessages;
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
using Octgn.Core;
using Octgn.Library.Exceptions;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

using log4net;
using Octgn.Extentions;
using Octgn.Online;
using Octgn.Communication;

namespace Octgn.Tabs.Profile
{
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
                if (Equals(value, this.model))
                {
                    return;
                }
                this.model = value;
                this.OnPropertyChanged("Model");
            }
        }

        private ApiUser User { get; set; }

        public UserProfilePage()
        {
            Model = new UserProfileViewModel(new ApiUser());
            // Expected: System.NotSupportedException
            // Additional information: ImageSourceConverter cannot convert from (null).
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        public async Task Load(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var client = new ApiClient();

            var apiUser = await client.UserFromUserId(user.Id);
            if (apiUser == null) return;

            await Load(apiUser);
        }

        private async Task Load(ApiUser user)
        {
            this.Dispatcher.VerifyAccess();

            User = user ?? throw new ArgumentNullException(nameof(user));

            UserProfileViewModel mod = await this.GetModel(user);

            this.Dispatcher.VerifyAccess();

            if (mod != null)
                this.Model = mod;
        }

        private async Task<UserProfileViewModel> GetModel(ApiUser user)
        {
            UserProfileViewModel ret = null;
            try
            {
                var client = new ApiClient();
                var me = await client.UserFromUserId(user.Id.ToString());
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
            //if (User != null)
            //{
            //    Load(User);
            //}
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
            if ((SubscriptionModule.Get().IsSubscribed ?? false) == false)
            {
                TopMostMessageBox.Show(
                    "You must be subscribed to set your icon", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
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
                    img.Save(imgStream, ImageFormat.Png);
                    imgStream.Seek(0, SeekOrigin.Begin);
                    var client = new Octgn.Site.Api.ApiClient();
                    var res = client.SetUserIcon(
                        Prefs.Username,
                        Prefs.Password.Decrypt(),
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
                TopMostMessageBox.Show(ex.Message, "Change Icon Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Log.Warn("ChangeIconClick(UserMessageException)", ex);
            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show(
                    "There was an unknown error. Please try a different image.",
                    "Change Icon Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                Log.Warn("ChangeIconClick", ex);
            }

        }


        private static System.Drawing.Image ResizeImage(System.Drawing.Image imgToResize, System.Drawing.Size size)
        {
            return new Bitmap(imgToResize, size);
        }

        private void SharedDeckUrlClick(object sender, RequestNavigateEventArgs e)
        {
            Program.LaunchUrl(e.Uri.ToString());
        }

        private void SharedDeckDeleteClick(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                var str = e.Uri.ToString();
                var res = TopMostMessageBox.Show(
                    "Are you sure you want to delete '" + str + "'? You can not undo this.",
                    "Are You Sure?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk);

                if (res != MessageBoxResult.Yes) return;
                var result = new ApiClient().DeleteSharedDeck(Prefs.Username, Prefs.Password.Decrypt(), str);
                if (result.Error)
                {
                    throw new UserMessageException(result.Message);
                }
                //TODO: Shared decks
                //GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new RefreshSharedDecksMessage());
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Warn("SharedDeckDeleteClick", ex);
                throw new UserMessageException("An unknown error occurred.");
            }
        }

        private void SharedDeckCopyClick(object sender, RequestNavigateEventArgs e)
        {
            Clipboard.SetText(e.Uri.ToString());
        }
    }

    public class UserProfileViewModel : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string userName;
        private string userImage;
        private string userIcon;
        private string userSubscription;
        private bool isSubscribed;
        private bool isMe;
        private bool canChangeIcon;
        private int disconnectPercent;
        private String _totalTimePlayed;
        private String _averageGameTime;
        private int _level;
        private int _totalGamesPlayed;

        private ObservableCollection<SharedDeckGroup> decks;

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

        public int DisconnectPercent
        {
            get
            {
                return this.disconnectPercent;
            }
            set
            {
                if (value.Equals(this.disconnectPercent))
                {
                    return;
                }
                this.disconnectPercent = value;
                this.OnPropertyChanged("DisconnectPercent");
            }
        }

        public int Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
				this.OnPropertyChanged("Level");
            }
        }

        public String AverageGameTime
        {
            get { return _averageGameTime; }
            set
            {
                if (_averageGameTime == value) return;
                _averageGameTime = value;
				this.OnPropertyChanged("AverageGameTime");
            }
        }

        public String TotalTimePlayed
        {
            get { return _totalTimePlayed; }
            set
            {
                if (_totalTimePlayed == value) return;
                _totalTimePlayed = value;
				this.OnPropertyChanged("TotalTimePlayed");
            }
        }

        public int TotalGamesPlayed
        {
            get { return _totalGamesPlayed; }
            set
            {
                if (_totalGamesPlayed == value) return;
                _totalGamesPlayed = value;
				this.OnPropertyChanged("TotalGamesPlayed");
            }
        }

        public ObservableCollection<SharedDeckGroup> Decks
        {
            get
            {
                return this.decks;
            }
            set
            {
                if (value.Equals(this.decks))
                {
                    return;
                }
                this.decks = value;
                this.OnPropertyChanged("Decks");
            }
        }

        public ObservableCollection<UserExperienceViewModel> Experiences { get; set; }

        public UserProfileViewModel(ApiUser user)
        {
            Experiences = new ObservableCollection<UserExperienceViewModel>();
            Decks = new ObservableCollection<SharedDeckGroup>();
            UserName = user.UserName;
            UserImage = user.ImageUrl;
            UserIcon = user.IconUrl;
            UserSubscription = user.Tier;
            IsSubscribed = user.IsSubscribed;
            DisconnectPercent = user.DisconnectPercent;
			if(user.Experience == null)
				user.Experience = new List<ApiUserExperience>();
			Experiences.Add(new UserExperienceViewModel(user));
            foreach (var e in user.Experience.OrderByDescending(x=>x.TotalSecondsPlayed))
            {
                Experiences.Add(new UserExperienceViewModel(e));
            }
            if (Program.LobbyClient != null && Program.LobbyClient.IsConnected)
                IsMe = Program.LobbyClient.User.Id.Equals(user.Id.ToString(), StringComparison.InvariantCultureIgnoreCase);
            CanChangeIcon = IsSubscribed && IsMe;
            //TODO: shared decks
            //GalaSoft.MvvmLight.Messaging.Messenger.Default.Register<RefreshSharedDecksMessage>(this,
            //    x => Task.Factory.StartNew(this.RefreshSharedDecks));
            Task.Factory.StartNew(RefreshSharedDecks);
        }

        internal void RefreshSharedDecks()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserName)) return;
                var list = GetShareDeckList();
                WindowManager.Main.Dispatcher.Invoke(new Action(() => UpdateObservableDeckList(list)));
            }
            catch (Exception e)
            {
                Log.Warn("RefreshSharedDecks Error", e);
            }
        }

        internal List<SharedDeckInfo> GetShareDeckList()
        {
            if (string.IsNullOrWhiteSpace(UserName)) return new List<SharedDeckInfo>();

            return new ApiClient().GetUsersSharedDecks(UserName);
        }

        internal void UpdateObservableDeckList(List<SharedDeckInfo> deckList)
        {
            lock (this)
            {
                Decks.Clear();

                foreach (var g in deckList.GroupBy(x => x.GameId))
                {
                    var dg = new SharedDeckGroup(g.Key, IsMe);
                    Decks.Add(dg);
                    foreach (var d in g)
                    {
                        dg.Decks.Add(new SharedDeckInfoWithOwner(d, IsMe));
                    }
                }
            }
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

    public class SharedDeckGroup : INotifyPropertyChanged
    {
        private Game _game;

        public Game Game
        {
            get { return _game; }
            set
            {
                if (value == _game) return;
                _game = value;
                OnPropertyChanged("Game");
            }
        }

        private Guid _gameId;

        public Guid GameId
        {
            get { return _gameId; }
            set
            {
                if (value == _gameId) return;
                _gameId = value;
                OnPropertyChanged("GameId");
            }
        }

        private string _image;

        public string Image
        {
            get { return _image; }
            set
            {
                if (value == _image) return;
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public string GameName
        {
            get
            {
                return this.gameName;
            }
            set
            {
                if (value.Equals(this.gameName)) return;
                this.gameName = value;
                this.OnPropertyChanged("GameName");
            }
        }

        private bool _me;

        public bool Me
        {
            get
            {
                return _me;
            }
            set
            {
                if (value == _me) return;
                _me = value;
                OnPropertyChanged("Me");
            }
        }

        private string gameName;

        private ObservableCollection<SharedDeckInfoWithOwner> decks;

        public ObservableCollection<SharedDeckInfoWithOwner> Decks
        {
            get
            {
                return this.decks;
            }
            set
            {
                if (value.Equals(this.decks))
                {
                    return;
                }
                this.decks = value;
                this.OnPropertyChanged("Decks");
            }
        }

        public SharedDeckGroup(Guid game, bool me)
        {
            Me = me;
            Decks = new ObservableCollection<SharedDeckInfoWithOwner>();
            this.Game = GameManager.Get().GetById(game);
            if (Game == null)
            {
                Image = "pack://application:,,,/OCTGN;component/Resources/usernoimage.png";
                GameName = "Unknown Game";
            }
            else
            {
                Image = this.Game.IconUrl;
                GameName = Game.Name;
            }
            this.GameId = game;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SharedDeckInfoWithOwner : SharedDeckInfo
    {
        public bool IsMe { get; set; }

        public SharedDeckInfoWithOwner(SharedDeckInfo deck, bool me)
        {
            IsMe = me;
            this.GameId = deck.GameId;
            this.Name = deck.Name;
            this.OctgnUrl = deck.OctgnUrl;
            this.Username = deck.Username;
        }
    }

    public class UserExperienceViewModel : GalaSoft.MvvmLight.ViewModelBase
    {
        private string _image;

        private string _name;
        private string _totalTimePlayed;
        private string _averageGameTime;
        private int _totalGamesPlayed;
        private int _level;

        public int Level
        {
            get { return _level; }
            set
            {
                if (_level == value) return;
                _level = value;
                RaisePropertyChanged(() => Level);
            }
        }

        public int TotalGamesPlayed
        {
            get { return _totalGamesPlayed; }
            set
            {
                if (_totalGamesPlayed == value) return;
                _totalGamesPlayed = value;
                RaisePropertyChanged(() => TotalGamesPlayed);
            }
        }

        public string AverageGameTime
        {
            get { return _averageGameTime; }
            set
            {
                if (_averageGameTime == value) return;
                _averageGameTime = value;
                RaisePropertyChanged(() => AverageGameTime);
            }
        }

        public string TotalTimePlayed
        {
            get { return _totalTimePlayed; }
            set
            {
                if (_totalTimePlayed == value) return;
                _totalTimePlayed = value;
                RaisePropertyChanged(() => TotalTimePlayed);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public string Image
        {
            get { return _image; }
            set
            {
                if (_image == value) return;
                _image = value;
                RaisePropertyChanged(() => Image);
            }
        }

        public UserExperienceViewModel(ApiUser usr)
        {
            this.Level = usr.Level;
            this.Image = "pack://application:,,,/Resources/logolarge.png";
            this.Name = "OCTGN";
            this.TotalGamesPlayed = usr.TotalGamesPlayed;
            this.TotalTimePlayed = FormatTime(usr.TotalSecondsPlayed);
            this.AverageGameTime = FormatTime(usr.AverageGameLength);
        }

        public UserExperienceViewModel(ApiUserExperience exp)
        {
            this.Level = exp.Level;
            this.Image = exp.Game.IconUrl;
            this.Name = exp.Game.Name;
            this.TotalGamesPlayed = exp.TotalGamesPlayed;
            this.TotalTimePlayed = FormatTime(exp.TotalSecondsPlayed);
            this.AverageGameTime = FormatTime(exp.AverageGameLength);
        }

        // Modified version of http://stackoverflow.com/questions/842057/how-do-i-convert-a-timespan-to-a-formatted-string#answer-8763843
        private string FormatTime(int secs)
        {
            var span = TimeSpan.FromSeconds(secs);

            string formatted = string.Format("{0}{1}",
                span.Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                string.Format("{0:00}:{1:00} hour{2}, ", span.Hours, span.Minutes, span.Hours == 1 ? String.Empty : "s"));

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 minutes";

            return formatted;
        }
    }
}
