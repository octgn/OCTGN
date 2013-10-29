using System.Windows;

namespace Octgn.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Timers;
    using System.Windows.Documents;
    using System.Windows.Media;

    using log4net;
    using log4net.Appender;
    using log4net.Core;

    using Microsoft.Win32;

    using Octgn.Annotations;
    using Octgn.Controls;
    using Octgn.Library;

    public partial class Diagnostics : INotifyPropertyChanged
    {
        #region Singleton

        private static Diagnostics SingletonContext { get; set; }

        private static readonly object MemCounterSingletonLocker = new object();

        public static Diagnostics Instance
        {
            get
            {
                if (SingletonContext != null) return SingletonContext;
                lock (MemCounterSingletonLocker)
                {
                    if (SingletonContext == null)
                    {
                        Application.Current.Dispatcher.Invoke(
                            new Action(
                                () =>
                                {
                                    SingletonContext = new Diagnostics();
                                }));
                    }
                    return SingletonContext;
                }
            }
        }

        public static bool IsNull
        {
            get
            {
                return SingletonContext == null;
            }
        }

        #endregion Singleton

        public string PrivateMemory
        {
            get
            {
                return this.privateMemory;
            }
            set
            {
                if (value == this.privateMemory)
                {
                    return;
                }
                this.privateMemory = value;
                this.OnPropertyChanged("PrivateMemory");
            }
        }

        public string PagedMemory
        {
            get
            {
                return this.pagedMemory;
            }
            set
            {
                if (value == this.pagedMemory)
                {
                    return;
                }
                this.pagedMemory = value;
                this.OnPropertyChanged("PagedMemory");
            }
        }

        public string VirtualMemory
        {
            get
            {
                return this.virtualMemory;
            }
            set
            {
                if (value == this.virtualMemory)
                {
                    return;
                }
                this.virtualMemory = value;
                this.OnPropertyChanged("VirtualMemory");
            }
        }

        public string WorkingMemory
        {
            get
            {
                return this.workingMemory;
            }
            set
            {
                if (value == this.workingMemory)
                {
                    return;
                }
                this.workingMemory = value;
                this.OnPropertyChanged("WorkingMemory");
            }
        }

        public string ProcessPercent
        {
            get
            {
                return this.processPercent;
            }
            set
            {
                if (value == this.processPercent)
                {
                    return;
                }
                this.processPercent = value;
                this.OnPropertyChanged("ProcessPercent");
            }
        }

        public string ProcessTime
        {
            get
            {
                return this.processTime;
            }
            set
            {
                if (value == this.processTime)
                {
                    return;
                }
                this.processTime = value;
                this.OnPropertyChanged("ProcessTime");
            }
        }

        public string FPS
        {
            get
            {
                return this.fps;
            }
            set
            {
                if (value == this.fps)
                {
                    return;
                }
                this.fps = value;
                this.OnPropertyChanged("FPS");
            }
        }

        public CrudeLatencyDetector GoogleLatency
        {
            get
            {
                return this.googleLatency;
            }
            set
            {
                if (Equals(value, this.googleLatency))
                {
                    return;
                }
                this.googleLatency = value;
                this.OnPropertyChanged("GoogleLatency");
            }
        }

        public CrudeLatencyDetector ChatServerLatency
        {
            get
            {
                return this.chatServerLatency;
            }
            set
            {
                if (Equals(value, this.chatServerLatency))
                {
                    return;
                }
                this.chatServerLatency = value;
                this.OnPropertyChanged("ChatServerLatency");
            }
        }

        public CrudeLatencyDetector GameServerLatency
        {
            get
            {
                return this.gameServerLatency;
            }
            set
            {
                if (Equals(value, this.gameServerLatency))
                {
                    return;
                }
                this.gameServerLatency = value;
                this.OnPropertyChanged("GameServerLatency");
            }
        }

        public CrudeLatencyDetector ApiServerLatency
        {
            get
            {
                return this.apiServerLatency;
            }
            set
            {
                if (Equals(value, this.apiServerLatency))
                {
                    return;
                }
                this.apiServerLatency = value;
                this.OnPropertyChanged("ApiServerLatency");
            }
        }

        public string OctgnVersion
        {
            get
            {
                return this.octgnVersion;
            }
            set
            {
                if (value == this.octgnVersion)
                {
                    return;
                }
                this.octgnVersion = value;
                this.OnPropertyChanged("OctgnVersion");
            }
        }

        public string EventCounts
        {
            get
            {
                return this.eventCounts;
            }
            set
            {
                if (value == this.eventCounts)
                {
                    return;
                }
                this.eventCounts = value;
                this.OnPropertyChanged("EventCounts");
            }
        }

        public Version LatestVersion
        {
            get
            {
                return this.latestVersion;
            }
            set
            {
                if (value == this.latestVersion)
                {
                    return;
                }
                this.latestVersion = value;
                this.OnPropertyChanged("LatestVersion");
            }
        }

        public List<string> LatestLogLines { get; set; }

        public bool AutoScroll
        {
            get
            {
                return this.autoScroll;
            }
            set
            {
                if (value.Equals(this.autoScroll))
                {
                    return;
                }
                this.autoScroll = value;
                this.OnPropertyChanged("AutoScroll");
            }
        }

        public string VersionType { get; set; }

        #region Privates
        private readonly Timer refreshTimer = new Timer(1000);

        private string privateMemory;

        private string pagedMemory;

        private string virtualMemory;

        private string workingMemory;

        private readonly PerformanceCounter pcounter;

        private string processPercent;

        private string processTime;

        private string fps;

        private CrudeLatencyDetector googleLatency;

        private CrudeLatencyDetector chatServerLatency;

        private CrudeLatencyDetector gameServerLatency;

        private CrudeLatencyDetector apiServerLatency;

        private string eventCounts;

        private long totalDebug;
        private long totalWarn;
        private long totalInfo;
        private long totalError;
        private long totalFatal;

        private bool autoScroll;

        private string octgnVersion;

        private Version latestVersion;

        private static int lastTick;
        private static int lastFrameRate;
        private static int frameRate;

        #endregion
        internal Diagnostics()
        {
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType).Info("######Created Diagnostics Window######");
            AutoScroll = true;
#if(Release_Test)
            VersionType = "Test";
#elif(DEBUG)
            VersionType = "Debug";
#else
            VersionType = "Release";
#endif
            OctgnVersion = Const.OctgnVersion.ToString();
            LatestVersion = UpdateManager.Instance.LatestVersion.Version;
            EventCounts = "Counts";
            LatestLogLines = new List<string>();
            refreshTimer.Elapsed += RefreshTimerOnElapsed;
            refreshTimer.Start();
            this.Closing += OnClosing;
            this.IsVisibleChanged += OnIsVisibleChanged;
            CompositionTarget.Rendering += CompositionTargetOnRendering;
            pcounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName, true);
            GoogleLatency = new CrudeLatencyDetector("Google", "www.google.com");
            ChatServerLatency = new CrudeLatencyDetector("Chat", "of.octgn.net");
            GameServerLatency = new CrudeLatencyDetector("Game", "games.octgn.net");
            ApiServerLatency = new CrudeLatencyDetector("Api", "www.octgn.net");
            GoogleLatency.Pause();
            ChatServerLatency.Pause();
            GameServerLatency.Pause();
            ApiServerLatency.Pause();
            this.InitializeComponent();
        }

        internal void AddLogEvent(LoggingEvent eve)
        {
            try
            {
                switch (eve.Level.Name)
                {
                    case "DEBUG":
                        totalDebug++;
                        break;
                    case "WARN":
                        totalWarn++;
                        break;
                    case "INFO":
                        totalInfo++;
                        break;
                    case "ERROR":
                        totalError++;
                        break;
                    case "FATAL":
                        totalFatal++;
                        break;
                }
                EventCounts = String.Format(
                    "DEBUG: {0}    INFO: {1}    WARN: {2}    ERROR:{3}    FATAL:{4}",
                    totalDebug,
                    totalInfo,
                    totalWarn,
                    totalError,
                    totalFatal);
                LatestLogLines.Add(eve.RenderedMessage);
                while (LatestLogLines.Count > 300)
                {
                    LatestLogLines.RemoveAt(0);
                }
                if (this.Visibility == Visibility.Visible)
                {
                    Dispatcher.BeginInvoke(
                        new Action(
                            () =>
                            {
                                LogBox.Document.Blocks.Add(new Paragraph(new Run(eve.RenderedMessage)));
                                while (LogBox.Document.Blocks.Count > 300)
                                {
                                    LogBox.Document.Blocks.Remove(LogBox.Document.Blocks.FirstBlock);
                                }
                                if (AutoScroll) LogBox.ScrollToEnd();
                            }));
                }
            }
            catch
            {
                // This comes from the logger, if we log here we could enter a recursive nightmare that we may never wake from. Imagine the horror of a thousand crying IO events, bleeding, tormented. The smell is enough to drive a made insane. Well we're on the subject I should mention that I never was a big fan of the first Freddy Kruger film. Wasn't really one of my favorites. Anyways, I suppose this comment is carrying on a little, so I'll wrap it up. Thanks for listening and stuff. Oh, and don't forget to break your while loops and dispose your objects, or the Garbage Man will come to get you!
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (this.Visibility == Visibility.Visible)
            {
                GoogleLatency.Resume();
                ChatServerLatency.Resume();
                GameServerLatency.Resume();
                ApiServerLatency.Resume();
                Dispatcher.BeginInvoke(
                    new Action(
                        () =>
                        {
                            LogBox.Document.Blocks.Clear();
                            foreach (var l in LatestLogLines)
                            {
                                LogBox.Document.Blocks.Add(new Paragraph(new Run(l)));
                            }
                            if (AutoScroll) LogBox.ScrollToEnd();
                        }));
            }
            else
            {
                GoogleLatency.Pause();
                ChatServerLatency.Pause();
                GameServerLatency.Pause();
                ApiServerLatency.Pause();
            }
        }

        private void CompositionTargetOnRendering(object sender, EventArgs e)
        {
            if (System.Environment.TickCount - lastTick >= 1000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = System.Environment.TickCount;
            }
            frameRate++;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            this.Hide();
            cancelEventArgs.Cancel = true;
        }

        private void RefreshTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var p = Process.GetCurrentProcess();
            this.PrivateMemory = Readable(p.PrivateMemorySize64);
            this.WorkingMemory = Readable(p.WorkingSet64);
            this.VirtualMemory = Readable(p.VirtualMemorySize64);
            this.PagedMemory = Readable(p.PagedMemorySize64);
            this.ProcessPercent = pcounter.NextValue() + "%";
            this.ProcessTime = p.TotalProcessorTime.ToString();
            this.FPS = lastFrameRate.ToString(CultureInfo.InvariantCulture);
            LatestVersion = UpdateManager.Instance.LatestVersion.Version;
            //Process.GetCurrentProcess().UserProcessorTime
        }

        private static readonly string[] Sizes = new string[4] { "B", "KB", "MB", "GB" };

        private string Readable(long bytes)
        {
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order + 1 < Sizes.Length)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, Sizes[order]);
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

        private void SaveCurrentLog(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(Paths.Get().CurrentLogPath))
                {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Paths.Get().CurrentLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var sfd = new SaveFileDialog();
                sfd.Title = "Save Log File To...";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                sfd.FileName = "currentlog.txt";
                sfd.OverwritePrompt = true;
                if ((sfd.ShowDialog() ?? false))
                {
                    File.Copy(Paths.Get().CurrentLogPath, sfd.FileName,true);
                    //var str = File.ReadAllText(Paths.Get().CurrentLogPath);
                    //File.WriteAllText(sfd.FileName, str);
                }

            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show(
                    "Error " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SavePreviousLog(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(Paths.Get().CurrentLogPath))
                {
                    TopMostMessageBox.Show(
                        "Log file doesn't exist at " + Paths.Get().PreviousLogPath,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                var sfd = new SaveFileDialog();
                sfd.Title = "Save Log File To...";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                sfd.FileName = "prevlog.txt";
                sfd.OverwritePrompt = true;
                if ((sfd.ShowDialog() ?? false))
                {
                    var str = File.ReadAllText(Paths.Get().PreviousLogPath);
                    File.WriteAllText(sfd.FileName, str);
                }

            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show(
                    "Error " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    public class CrudeLatencyDetector : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Ping ping;
        private readonly Timer pingTimer;
        private readonly string host;
        private readonly byte[] buffer = new byte[128];

        private bool isPaused;

        private string data;

        private bool isPinging;

        public string Name { get; set; }

        public string Data
        {
            get
            {
                return this.data;
            }
            set
            {
                if (value == this.data)
                {
                    return;
                }
                this.data = value;
                this.OnPropertyChanged("Data");
            }
        }

        public bool IsPinging
        {
            get
            {
                return this.isPinging;
            }
            set
            {
                if (value.Equals(this.isPinging))
                {
                    return;
                }
                this.isPinging = value;
                this.OnPropertyChanged("IsPinging");
            }
        }

        public CrudeLatencyDetector(string name, string host)
        {
            Data = Name + " Collecting Data...";
            Name = name;
            this.host = host;
            pingTimer = new Timer(10000);
            pingTimer.Elapsed += PingTimerOnElapsed;
            this.ping = new Ping();
            this.ping.PingCompleted += PingOnPingCompleted;
            pingTimer.Start();
        }

        private void PingTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                IsPinging = true;
                pingTimer.Enabled = false;
                this.ping.SendAsync(host, 10000, buffer, this);
            }
            catch (Exception e)
            {
                IsPinging = false;
                Log.Error("PingError " + host, e);
                Data = Name + ": Ping Error";
                if (!isPaused)
                    pingTimer.Enabled = true;
            }
        }

        private void PingOnPingCompleted(object sender, PingCompletedEventArgs args)
        {
            IsPinging = false;
            if (!isPaused)
                pingTimer.Enabled = true;
            var str = Name;
            str += Environment.NewLine;
            str += "    Status: ";
            if (args.Reply == null) str += "Unknown";
            else str += args.Reply.Status;
            str += Environment.NewLine;
            str += "    Latency: ";
            if (args.Reply == null) str += "Unknown";
            else str += TimeSpan.FromMilliseconds(args.Reply.RoundtripTime).TotalMilliseconds + "ms";
            Data = str;
        }

        public void Pause()
        {
            isPaused = true;
        }

        public void Resume()
        {
            isPaused = false;
            if (!IsPinging)
            {
                pingTimer.Enabled = true;
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
    public class DiagnosticsAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (Diagnostics.IsNull) return;
            Diagnostics.Instance.AddLogEvent(loggingEvent);
        }
    }
}
