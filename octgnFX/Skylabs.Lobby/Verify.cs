//Copyright 2012 Skylabs
//In order to use this software, in any manor, you must first contact Skylabs.
//Website: http://www.skylabsonline.com
//Email:   skylabsonline@gmail.com
using System.Text.RegularExpressions;

namespace Skylabs.Lobby
{
    public static class Verify
    {
        public static bool IsEmail(string inputEmail)
        {
            inputEmail = inputEmail ?? "";
            const string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                    @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                    @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            return new Regex(strRegex).IsMatch(inputEmail);
        }
    }
}