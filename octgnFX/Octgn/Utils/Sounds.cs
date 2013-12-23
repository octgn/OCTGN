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
    using System.Collections.Generic;

    using NAudio.Wave.SampleProviders;

    using Octgn.Core;
    using Octgn.Library;

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

        private static bool warnedAboutMp3Sounds = false;

        public static MixingSampleProvider Mixer = new MixingSampleProvider(new List<ISampleProvider> { new SilenceProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)) });

        public static WaveOutEvent WaveOut = null;

        public static List<IDisposable> DisposeObjects = new List<IDisposable>();

        public static void PlayGameSound(GameSound sound)
        {
            var isSubscribed = SubscriptionModule.Get().IsSubscribed ?? false;
            if (isSubscribed == false)return;
            if (Prefs.EnableGameSound == false) return;
            Log.InfoFormat("Playing game sound {0}", sound.Name);
            if (!sound.Src.ToLowerInvariant().EndsWith(".mp3"))
            {
                Log.InfoFormat("Playing game sound {0} as wav", sound.Name);
                PlaySound(sound.Src);
                return;
            }
			Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Log.InfoFormat("Playing game sound {0} as mp3", sound.Name);
                        var mp3Reader = new Mp3FileReader(sound.Src);
                        var stream = new WaveChannel32(mp3Reader) { PadWithZeroes = false };
						DisposeObjects.Add(mp3Reader);
						DisposeObjects.Add(stream);
                        {
                            stream.Position = 0;
                            Mixer.AddMixerInput(stream);
                            lock (Mixer)
                            {
                                if (WaveOut == null)
                                {
                                    Log.Info("Starting up wave out");
                                    WaveOut = new WaveOutEvent();
                                    WaveOut.Init(new SampleToWaveProvider(Mixer));
                                    WaveOut.PlaybackStopped += (sender, args) =>
                                    {
										WaveOut.Dispose();
                                        WaveOut = null;
                                    };
                                    WaveOut.Play();
                                }
                            }
                            Log.InfoFormat("Initializing game sound {0} as mp3", sound.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn("PlayGameSound Error", e);
						Program.TraceWarning("Cannot play sound {0}, it must be in the format 44100:2",sound.Name);
                    }
                });
        }

        public static void Close()
        {
            if(Mixer != null)
                X.Instance.Try(Mixer.RemoveAllMixerInputs);
            if(WaveOut != null)
			    X.Instance.Try(WaveOut.Dispose);
            foreach (var i in DisposeObjects.ToArray())
            {
                X.Instance.Try(i.Dispose);
                DisposeObjects.Remove(i);
            }
        }

        public static void PlayGameSoundOld(GameSound sound)
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
                            var ttime = mp3Reader.TotalTime.Add(new TimeSpan(0,0,10));
                            var etime = DateTime.Now.Add(ttime.Add(new TimeSpan(0, 0, 10)));
                            while (wo.PlaybackState == PlaybackState.Playing)
                            {
                                Thread.Sleep(100);
                                if (DateTime.Now > etime)
                                {
                                    break;
                                }
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

    public class SilenceProvider : ISampleProvider
    {
        private readonly WaveFormat format;

        public SilenceProvider(WaveFormat format)
        {
            this.format = format;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = 0;
            }
            return count;
        }

        public WaveFormat WaveFormat
        {
            get { return format; }
        }
    }
}
