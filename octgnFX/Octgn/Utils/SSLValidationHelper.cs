namespace Octgn.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Reflection;
    using System.Windows;

    using log4net;

    using Octgn.Core;

    public class SSLValidationHelper : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly List<string> HostList = new List<string>();

        public SSLValidationHelper()
        {
            Log.Info(string.Format("Bypass SSL certificate validation set to: {0}", Prefs.IgnoreSSLCertificates));
            System.Net.ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
        }

        private bool CertificateValidationCallBack(object sender,System.Security.Cryptography.X509Certificates.X509Certificate certificate,System.Security.Cryptography.X509Certificates.X509Chain chain,SslPolicyErrors sslPolicyErrors)
        {
            try
            {
                Log.Info("SSL Request");
                if (Prefs.IgnoreSSLCertificates)
                {
                    Log.Info("Ignoring SSL Validation");
                    return (true);
                }
                var request = (System.Net.HttpWebRequest)sender;

                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    Log.Info("No SSL Errors Detected");
                    return true;
                }
                Log.Info("SSL validation error detected");
                if (HostList.Contains(request.RequestUri.Host))
                {
                    Log.Info("Already showed dialog, failing ssl");
                    return false;
                }
                Log.Info("Host not listed, showing dialog");
                HostList.Add(request.RequestUri.Host);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Your machine isn't properly handling SSL Certificates.");
                sb.AppendLine("If you choose 'No' you will not be able to use OCTGN");
                sb.AppendLine(
                    "While this will allow you to use OCTGN, it is not a recommended long term solution. You should seek internet guidance to fix this issue.");
                sb.AppendLine();
                sb.AppendLine("Would you like to disable ssl verification(In OCTGN only)?");

                var ret = false;
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            MessageBoxResult result = MessageBox.Show(
                                Application.Current.MainWindow,
                                sb.ToString(),
                                "SSL Error",
                                MessageBoxButton.YesNo);
                            if (result == MessageBoxResult.Yes)
                            {
                                Log.Info("Chose to turn on SSL Validation Ignoring");
                                Prefs.IgnoreSSLCertificates = true;

                                ret = true;
                            }
                            else
                            {
                                Log.Info("Chose not to turn on SSL Validation Ignoring");
                                ret = false;
                            }
                            sb.Clear();
                            sb = null;
                        }));

                return ret;
            }
            catch (Exception e)
            {
                Log.Error("SSL Validation Hook Error", e);
                return false;
            }
        }

        public void Dispose()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = null;
        }
    }
}