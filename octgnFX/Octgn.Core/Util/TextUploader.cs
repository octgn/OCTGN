namespace Octgn.Core.Util
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Xml.Serialization;

    using log4net;

    using Octgn.Library.Exceptions;

    public class TextUploader
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        #region Singleton

        internal static TextUploader SingletonContext { get; set; }

        private static readonly object TextUploaderSingletonLocker = new object();

        public static TextUploader Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (TextUploaderSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new TextUploader();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public string UploadText(string text)
        {
            try
            {
                var url = "http://tny.cz/api/create.xml";
                var postData = "subdomain=octgn&paste=" + text;

                var resp = SendPost(url, postData);

                var serializer = new XmlSerializer(typeof(XmlRespose));

                using (var ms = new StringReader(resp))
                {
                    var realresponse = (XmlRespose)serializer.Deserialize(ms);

                    if (realresponse.Error == null)
                    {
                        return "http://octgn.tny.cz/" + realresponse.Response;
                    }
                    throw new UserMessageException(
                        "There was an error uploading(" + realresponse.Error + "). Please try again later.");
                }
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Warn("Error uploading text", e);
                throw new UserMessageException("Error uploading");
            }

        }

        private string SendPost(string url, string postData)
        {
            string webpageContent;
            var byteArray = Encoding.UTF8.GetBytes(postData);

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = byteArray.Length;

            using (var webpageStream = webRequest.GetRequestStream())
            {
                webpageStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    webpageContent = reader.ReadToEnd();
                }
            }

            return webpageContent;
        }

        [Serializable()]
        [XmlRoot("result")]
        public class XmlRespose
        {
            [XmlElement("response")]
            public string Response { get; set; }
            [XmlElement("error")]
            public XmlResponseErrorCode? Error { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("Response=" + Response);
                sb.AppendLine("Error=" + Error);
                return sb.ToString();
            }
        }

        [Serializable()]
        public enum XmlResponseErrorCode
        {
            paste_empty,
            subdomain_short,
            subdomain_long,
            subdomain_banned,
            is_code_error,
            is_private_error,
            expires_invalid,
            auth_invalid,
            parent_not_exist,
        }
    }
}