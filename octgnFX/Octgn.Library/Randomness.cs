using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Octgn.Library
{
    public static class Randomness
    {
        public static Random Rand { get; set; }
        public const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";


        static Randomness()
        {
            Rand = new Random();
        }
        public static string RandomStringOfLetters()
        {
            var len = Rand.Next(5 , 20);
            var sb = new StringBuilder(len);
            for(var i = 0;i < len;i++)
            {
                var offset = Rand.Next(0 , Alphabet.Length);
                sb.Append(Alphabet[i]);
            }
            return sb.ToString();
        }
        public static bool RandomBool(){return Rand.Next()%2 == 0;}
        public static string GrabRandomJargonWord()
        {
            try
            {
                var l = new List<string>();
                using(var s = new StringReader(Resources.jargonlist))
                {
                    var ns = "";
                    while((ns = s.ReadLine()) != null)
                    {
                        l.Add(ns);
                    }                    
                }
                return l[Rand.Next(0 , l.Count)];

            }
            catch(Exception e)
            {
                if(Debugger.IsAttached)Debugger.Break();
                Trace.WriteLine(e.Message);
                return RandomStringOfLetters();
            }
        }
        public static string GrabRandomNounWord()
        {
            try
            {
                var l = new List<string>();
                using (var s = new StringReader(Resources.nounlist))
                {
                    var ns = "";
                    while ((ns = s.ReadLine()) != null)
                    {
                        l.Add(ns);
                    }
                }
                return l[Rand.Next(0, l.Count)];
            }
            catch(Exception e)
            {
                if(Debugger.IsAttached)Debugger.Break();
                Trace.WriteLine(e.Message);
                return RandomStringOfLetters();
            }
        }
        public static string RandomRoomName()
        {
            var Jargon = GrabRandomJargonWord();
            var Noun = GrabRandomNounWord();
            var rnum = Rand.Next(0 , 1000);
            return Jargon + Noun + rnum.ToString(CultureInfo.InvariantCulture);
        }
    }
}
