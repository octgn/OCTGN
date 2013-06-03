using System;
using System.IO;
using System.Media;

namespace Octgn.Utils
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using Octgn.DataNew.Entities;

    using log4net;

    public class Sounds
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void PlaySound(Stream sound, bool subRequired = true)
        {
            if (subRequired && SubscriptionModule.Get().IsSubscribed == false) return;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    using(sound)
                    using (var player = new SoundPlayer(sound))
                    {
                        player.PlaySync();
                    }

                }
                catch (Exception e)
                {
                    Log.Warn("Play Sound Error", e);
                }
            });
        }

        public static void PlaySound(string file, bool subRequired = true)
        {
            if (subRequired && SubscriptionModule.Get().IsSubscribed == false) return;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var player = new SoundPlayer(file))
                    {
                        player.PlaySync();
                    }

                }
                catch (Exception e)
                {
                    Log.Warn("Play Sound Error", e);
                }
            });
        }

        public static void PlayMessageSound()
        {
            if (Prefs.EnableNameSound)
            {
                try
                {
                    var si = Application.GetResourceStream(new Uri("pack://application:,,,/OCTGN;component/Resources/messagenotify.wav"));
                    PlaySound(si.Stream);
                }
                catch (Exception e)
                {
                    Debugger.Break();
                }
            }
        }

        public static void PlayGameSound(GameSound sound)
        {
            var isSubscribed = SubscriptionModule.Get().IsSubscribed ?? false;
            if (isSubscribed && Prefs.EnableGameSound)
            {
                try
                {
                    using (var player = new SoundPlayer(sound.Src))
                    {
                        player.PlaySync();
                    }
                }
                catch (Exception e)
                {
                    Log.Warn("PlayGameSound Error",e);
                }
            }
        }
    }
}
