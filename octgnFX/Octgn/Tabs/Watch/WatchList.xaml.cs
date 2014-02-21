using System.Globalization;
using System.Windows.Controls;

namespace Octgn.Tabs.Watch
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Timers;
    using System.Windows.Input;

    using log4net;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Octgn.Annotations;

    using Skylabs.Lobby.Threading;
    using Octgn.Core;

    /// <summary>
    /// Interaction logic for WatchList.xaml
    /// </summary>
    public partial class WatchList : UserControl, INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        internal Timer RefreshTimer;

        private StreamModel selected;

        public ObservableCollection<StreamModel> Streams { get; set; }

        public StreamModel Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                if (Equals(value, this.selected))
                {
                    return;
                }
                this.selected = value;
                this.OnPropertyChanged("Selected");
            }
        }

        public bool HasSeenSpectateMessage
        {
            get { return Prefs.HasSeenSpectateMessage; }
            set { Prefs.HasSeenSpectateMessage = value; }
        }

        public WatchList()
        {
            Streams = new ObservableCollection<StreamModel>();
            InitializeComponent();
            RefreshTimer = new Timer(60000);
            RefreshTimer.Elapsed += RefreshTimerOnElapsed;

            LazyAsync.Invoke(() => RefreshTimerOnElapsed(null, null));
            RefreshTimer.Start();
        }

        private void RefreshTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                RefreshTimer.Enabled = false;
                var wc = new WebClient();
                var jsonString = wc.DownloadString("https://api.twitch.tv/kraken/search/streams?q=octgn");
                var obj = (JObject)JsonConvert.DeserializeObject(jsonString);

                var streams = new List<StreamModel>();

                foreach (var s in obj["streams"])
                {
                    var model = new StreamModel();
                    model.Title = s["channel"]["status"].ToString();
                    model.ChannelOwner = s["channel"]["display_name"].ToString();
                    model.ChannelUrl = s["channel"]["url"].ToString();
                    model.ThumbnailPreviewUrl = s["preview"]["small"].ToString();
                    model.ViewerCount = s["viewers"].ToObject<int>();
                    model.Id = s["_id"].ToObject<long>();
                    streams.Add(model);
                }
                if (DateTime.Now < DateTime.Parse("08/20/2013", new CultureInfo("en-US")))
                {
                    jsonString = wc.DownloadString("https://api.twitch.tv/kraken/channels/boardgamegeektv");
                    obj = (JObject)JsonConvert.DeserializeObject(jsonString);

                    var model = new StreamModel();
                    model.Title = obj["status"].ToString();
                    model.ChannelOwner = obj["name"].ToString();
                    model.ChannelUrl = obj["url"].ToString();
                    model.ThumbnailPreviewUrl = obj["logo"].ToString();
                    model.ViewerCount = new Random().Next(100, 1000);
                    model.Id = obj["_id"].ToObject<long>();

                    streams.Add(model);
                }

                Dispatcher.Invoke(new Action(() =>
                {
                    // Add new feeds
                    foreach (var s in streams)
                    {
                        var stream = this.Streams.FirstOrDefault(x => x.Id == s.Id);
                        if (stream == null)
                        {
                            this.Streams.Add(s);
                        }
                        else
                        {
                            stream.ChannelOwner = s.ChannelOwner;
                            stream.ChannelUrl = s.ChannelUrl;
                            stream.Id = s.Id;
                            stream.ThumbnailPreviewUrl = s.ThumbnailPreviewUrl;
                            stream.Title = s.Title;
                            stream.ViewerCount = s.ViewerCount;
                        }
                    }

                    // Remove gone feeds
                    foreach (var s in Streams.ToArray())
                    {
                        if (streams.All(x => x.Id != s.Id))
                        {
                            this.Streams.Remove(s);
                        }
                    }
                    if (streams.Count == 0)
                        NoStreamsMessage.Visibility = System.Windows.Visibility.Visible;
                    else
                        NoStreamsMessage.Visibility = System.Windows.Visibility.Collapsed;
                }));
            }
            catch (Exception e)
            {
                Log.Warn("Problem getting Twitch.tv List", e);
            }
            finally
            {
                RefreshTimer.Enabled = true;
            }
        }

        private void ListBoxStreams_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Log.Info("Trying to launch stream url");
            var sel = Selected;
            if (sel == null) return;
            Log.InfoFormat("Launching {0}", sel.ThumbnailPreviewUrl);
            Program.LaunchUrl(sel.ChannelUrl);
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

    public class StreamModel : INotifyPropertyChanged
    {
        private string title;

        private string channelOwner;

        private string channelUrl;

        private string thumbnailPreviewUrl;

        private int viewerCount;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                if (value == this.title)
                {
                    return;
                }
                this.title = value;
                this.OnPropertyChanged("Title");
            }
        }

        public string ChannelOwner
        {
            get
            {
                return this.channelOwner;
            }
            set
            {
                if (value == this.channelOwner)
                {
                    return;
                }
                this.channelOwner = value;
                this.OnPropertyChanged("ChannelOwner");
            }
        }

        public string ChannelUrl
        {
            get
            {
                return this.channelUrl;
            }
            set
            {
                if (value == this.channelUrl)
                {
                    return;
                }
                this.channelUrl = value;
                this.OnPropertyChanged("ChannelUrl");
            }
        }

        public string ThumbnailPreviewUrl
        {
            get
            {
                return this.thumbnailPreviewUrl;
            }
            set
            {
                if (value == this.thumbnailPreviewUrl)
                {
                    return;
                }
                this.thumbnailPreviewUrl = value;
                this.OnPropertyChanged("ThumbnailPreviewUrl");
            }
        }

        public int ViewerCount
        {
            get
            {
                return this.viewerCount;
            }
            set
            {
                if (value == this.viewerCount)
                {
                    return;
                }
                this.viewerCount = value;
                this.OnPropertyChanged("ViewerCount");
            }
        }

        public long Id { get; set; }
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
