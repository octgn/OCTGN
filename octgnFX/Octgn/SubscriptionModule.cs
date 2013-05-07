namespace Octgn
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;

    using Octgn.Library.Exceptions;

    using log4net;

    public class SubscriptionModule
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        #region Singleton

        internal static SubscriptionModule SingletonContext { get; set; }

        private static readonly object SubscriptionModuleSingletonLocker = new object();

        public static SubscriptionModule Get()
        {
            lock (SubscriptionModuleSingletonLocker) return SingletonContext ?? (SingletonContext = new SubscriptionModule());
        }

        internal SubscriptionModule()
        {
            Log.Info("Creating");
            PrevSubValue = null;
            this.SubTypes = new List<SubType>();
            SubTypes.Add(new SubType { Description = "$3.00 per month", Name = "silver" });
            SubTypes.Add(new SubType { Description = "$33.00 per year", Name = "gold" });
            CheckTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
            CheckTimer.Elapsed += CheckTimerOnElapsed;
            CheckTimer.Start();
            Log.Info("Created");
            Task.Factory.StartNew(() => CheckTimerOnElapsed(null,null)).ContinueWith(
                x =>
                    { if (x.Exception != null) Log.Info("Get Is Subbed Failed", x.Exception); });
        }

        #endregion Singleton

        public List<SubType> SubTypes { get; set; }

        /// <summary>
        /// True if subbed,
        /// False if not,
        /// Null if unknown
        /// </summary>
        public bool? IsSubscribed
        {
            get
            {
                Log.Info("Getting IsSubscribed");
                bool? ret = null;
                try
                {
                    if (Program.LobbyClient.IsConnected)
                    {

                        var wc = new WebClient();
                        var res =
                            wc.DownloadString(
                                AppConfig.WebsitePath + "api/user/issubbed.php?username="
                                + Program.LobbyClient.Me.UserName).Trim();
                        switch (res)
                        {
                            case "NoSubscriptionException":
                            case "SubscriptionExpiredException":
                                {
                                    ret = false;
                                    break;
                                }
                            case "ok":
                                {
                                    ret = true;
                                    break;
                                }
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Warn("ce",e);
                }
                Log.InfoFormat("Is Subscribed = {0}",ret == null ? "Unknown" : ret.ToString());
                if (ret != PrevSubValue && ret != null)
                {
                    this.OnIsSubbedChanged((bool)ret);
                    Program.LobbyClient.SetSub((bool)ret);
                }
                PrevSubValue = ret;
                return ret;
            }
        }

        internal bool? PrevSubValue { get; set; }

        public event Action<bool> IsSubbedChanged;

        public string GetSubscribeUrl(SubType type)
        {
            try
            {
                for (var i = 0; i < 4; i++)
                {
                    try
                    {
                        Log.InfoFormat("Getting subscribe url for {0}", type.Name);
                        var wc = new WebClient();
                        var callurl = AppConfig.WebsitePath + "api/user/subscribe.php?username="
                                      + Program.LobbyClient.Me.UserName + "&subtype=" + type.Name;
                        Log.InfoFormat("Call url {0}", callurl);
                        var res = wc.DownloadString(callurl).Trim();
                        Log.InfoFormat("Result {0}", res);
                        if (res.Substring(0, 2) == "ok")
                        {
                            var url = res.Substring(3);
                            return url;
                        }
                        return null;

                    }
                    catch (Exception)
                    {
                        if (i == 3) throw;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("ss", e);
                throw new UserMessageException("Could not subscribe. Please visit " + AppConfig.WebsitePath + " to subscribe.",e);
            }
            return null;
        }

        protected virtual void OnIsSubbedChanged(bool obj)
        {
            var handler = this.IsSubbedChanged;
            if (handler != null)
            {
                handler(obj);
            }
        }

        internal Timer CheckTimer { get; set; }

        private void CheckTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Log.Info("Check timer elapsed");
            Log.Info(IsSubscribed);
        }

    }
    public class SubType
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public override string ToString()
        {
            return Description;
        }
    }
}