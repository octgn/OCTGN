using System;
using System.IO;
using System.Media;

namespace Octgn.Utils
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using NAudio.Wave;

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
                    using (sound)
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
                    Log.Warn("PlayMessageSound Error", e);
                }
            }
        }

        public static void PlayGameSound(GameSound sound)
        {
            var isSubscribed = SubscriptionModule.Get().IsSubscribed ?? false;
            if (isSubscribed && Prefs.EnableGameSound)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (sound.Src.ToLowerInvariant().EndsWith(".mp3"))
                        {
                            using (var mp3Reader = new Mp3FileReader(sound.Src))
                            using (var stream = new WaveChannel32(mp3Reader))
                            using (var wo = new WaveOut())
                            {
                                wo.Init(stream);
                                wo.Play();
                                while (wo.PlaybackState == PlaybackState.Playing)
                                {
                                    Thread.Sleep(1);
                                }
                            }
                        }
                        else
                        {
                            using (var player = new SoundPlayer(sound.Src))
                            {
                                player.PlaySync();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn("PlayGameSound Error", e);
                    }
                });
            }
        }
    }
}
