//using System.Net.Http;

using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Octgn.Library;
using System;
using System.Reflection;
using log4net;

using Octgn.Library.Exceptions;
using Octgn.Library.Localization;

namespace Octgn.Core.Util
{
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

        //public string UploadText(string text)
        //{
        //    try
        //    {
        //        string ret = "";
        //        X.Instance.Retry(() =>
        //        {
        //            using (var client = new HttpClient())
        //            {
        //                var resultx = client.PostAsync("http://hastebin.com/documents", new StringContent("All\nYour\nBase\nAre\nBelong\nTo\nUs")).Result;
        //                var result = resultx.Content.ReadAsAsync<KeyResp>().Result;
        //                if (string.IsNullOrWhiteSpace(result.Key) || result.Key.Length > 100)
        //                {
        //                    Log.WarnFormat("UploadText Problem\n{0}", resultx.Content.ReadAsStringAsync().Result);
        //                    throw new UserMessageException(UserMessageExceptionMode.Background,
        //                        "There was an error uploading the text.");
        //                }

        //                ret = "http://hastebin.com/" + result.Key;
        //            }
        //        });

        //        return ret;
        //    }
        //    catch (UserMessageException)
        //    {
        //        throw;
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Warn("Error uploading text", e);
        //        throw new UserMessageException(UserMessageExceptionMode.Background, "Error uploading");
        //    }
        //    Log.WarnFormat("UploadText Problem...");
        //    throw new UserMessageException(UserMessageExceptionMode.Background,
        //        "There was an error uploading the text.");
        //    return null;
        //}

        public string UploadText(string text)
        {
            try
            {
                string ret = "";
                if (text.Length > 400000)
                {
                    text = text.Substring(text.Length - 400000);
                }
                X.Instance.Retry(() =>
                {
                    var response = SendPost("http://hastebin.com/documents", text);
                    if (string.IsNullOrWhiteSpace(response) || response.Length > 100)
                    {
                        Log.WarnFormat("UploadText Problem 1\n{0}", response);
                        throw new UserMessageException(UserMessageExceptionMode.Background,L.D.Exception__CanNotUploadText);
                    }

                    var result = JsonConvert.DeserializeObject<KeyResp>(response);
                    if (result == null || string.IsNullOrWhiteSpace(result.Key))
                    {
                        Log.WarnFormat("UploadText Problem 2\n{0}", response);
                        throw new UserMessageException(UserMessageExceptionMode.Background,L.D.Exception__CanNotUploadText);
                    }

                    ret = "http://hastebin.com/" + result.Key;
                });

                return ret;
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Warn("UploadText Problem 3", e);
                throw new UserMessageException(UserMessageExceptionMode.Background,L.D.Exception__CanNotUploadText);
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

        private class KeyResp
        {
            public string Key { get; set; }
        }
    }
}