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