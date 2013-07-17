// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegexPatterns.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Defines the RegexPatterns type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn.Library.Utils
{
    /// <summary>
    /// The Regular Expression patterns.
    /// </summary>
    public static class RegexPatterns
    {
        /// <summary>
        /// A Pattern to detect URL's
        /// </summary>
        public const string Urlrx = @"(((file|gopher|news|nntp|telnet|http|ftp|https|ftps|sftp)://)|(www\.))+(([a-zA-Z0-9\._-]+\.[a-zA-Z]{2,6})|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(/[a-zA-Z0-9\&amp;%_\./-~-]*)?";
    }
}
