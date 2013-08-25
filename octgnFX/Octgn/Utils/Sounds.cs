using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using NAudio.Wave;

using Octgn.DataNew.Entities;

using log4net;

namespace Octgn.Utils
{

    public static class Sounds
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void PlaySound(Stream sound, bool subRequired = true)
        {
            if (subRequired && SubscriptionModule.Get().IsSubscribed == false) return;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var player = new SoundPlayer(sound))
                    {
                        player.PlaySync();
                    }

                }
                catch (Exception e)
                {
                    Log.Warn("PlaySound Error", e);
                }
            });
        }

        public static void PlaySound(string file, bool subRequired = true)
        {
            PlaySound(File.OpenRead(file));
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
                Log.InfoFormat("Playing game sound {0}", sound.Name);
                if (!sound.Src.ToLowerInvariant().EndsWith(".mp3"))
                {
                    Log.InfoFormat("Playing game sound {0} as wav", sound.Name);
                    PlaySound(sound.Src);
                    return;
                }
                Program.TraceWarning("[playSound] Cannot play mp3 sounds at this time due to bug.");
                return;
                Task.Factory.StartNew(() =>
                {

                    try
                    {
                        Log.InfoFormat("Playing game sound {0} as mp3", sound.Name);
                        using (var mp3Reader = new Mp3FileReader(sound.Src))
                        using (var stream = new WaveChannel32(mp3Reader) { PadWithZeroes = false })
                        using (var wo = new WaveOutEvent())
                        {
                            Log.InfoFormat("Initializing game sound {0} as mp3", sound.Name);
                            wo.Init(stream);
                            wo.Play();
                            Log.InfoFormat("Waiting for game sound {0} to complete", sound.Name);
                            while (wo.PlaybackState == PlaybackState.Playing)
                            {
                                Thread.Sleep(1);
                            }
                            Log.InfoFormat("Game sound {0} completed", sound.Name);
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
