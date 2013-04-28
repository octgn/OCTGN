using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;

namespace Octgn.Utils
{
    public class Sounds
    {
        public static void PlaySound(Stream sound)
        {
            SoundPlayer player = new SoundPlayer(sound);
            player.Play();
            player.Dispose();
        }

        public static void PlaySound(string file)
        {
            SoundPlayer player = new SoundPlayer(file);
            player.Play();
            player.Dispose();
        }
    }
}
