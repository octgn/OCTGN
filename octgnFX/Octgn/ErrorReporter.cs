// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorReporter.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Defines the ErrorReporter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Octgn
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// The error reporter.
    /// </summary>
    internal static class ErrorReporter
    {
        /// <summary>
        /// The post data template.
        /// </summary>
        internal const string PostDataTemplate = @"name=%%name%%&assembly=%%assembly%%&version=%%version%%&error=%%error%%";

        /// <summary>
        /// The post uri.
        /// </summary>
        internal static readonly Uri PostUri = new Uri("http://www.octgn.info/errorreport.php");

        /// <summary>
        /// Submit an exception online.
        /// </summary>
        /// <param name="e">
        /// The Exception
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        internal static bool SumbitException(Exception e)
        {
            var err = e.ToString();
            var ass = Assembly.GetCallingAssembly().GetName().Name;
            var ver = Assembly.GetCallingAssembly().GetName().Version.ToString();
            var st = new StackTrace().GetFrame(1);
            var name = st.GetMethod().ReflectedType.FullName + st.GetMethod().Name;
            return SendPost(err, ass, ver, name);
        }

        /// <summary>
        /// Send Exception Data.
        /// </summary>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="assembly">
        /// The assembly.
        /// </param>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool SendPost(string error, string assembly, string version, string name)
        {
            var postData = PostDataTemplate.Replace("%%error%%", error)
                .Replace("%%assembly%%", assembly)
                .Replace("%%version%%", version)
                .Replace("%%name%%", name);

            var ret = string.Empty;
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                var webRequest = (HttpWebRequest)WebRequest.Create(PostUri);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = byteArray.Length;

                using (var webpageStream = webRequest.GetRequestStream())
                {
                    webpageStream.Write(byteArray, 0, byteArray.Length);
                }

                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    ret += webResponse.StatusDescription + Environment.NewLine;
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        var webpageContent = reader.ReadToEnd();
                        ret += webpageContent + Environment.NewLine;
                        if (webpageContent == "1")
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ErrorReport.SendPost Failed. " + ex);
            }

            Trace.WriteLine("Error Report Response: " + ret);
            return false;
        }
    }
}
