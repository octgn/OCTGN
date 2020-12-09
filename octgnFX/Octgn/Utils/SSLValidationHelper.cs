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
        private static readonly List<string> BypassList = new List<string>();

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

                var host = request.RequestUri.Host;
                Log.Info($"SSL validation error detected for {host}, {sslPolicyErrors.ToString()}");
                if (BypassList.Contains(host))
                {
                    Log.Info($"Host {host} is in temporary bypass list, ignoring certificate error");
                    return true;
                }

                if (HostList.Contains(host))
                {
                    Log.Info("Already showed dialog, failing ssl");
                    return false;
                }

                Log.Info("Host not listed, showing dialog");
                HostList.Add(host);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Problems were encountered while contacting the server {host}:");

                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
                {
                    sb.AppendLine("   * No certificate was provided by the server.");
                }
                
                if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
                {
                    sb.AppendLine("   * The name on the certificate did not match the server's hostname.");
                }

                if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    if (DateTime.TryParse(certificate.GetExpirationDateString(), out DateTime expiry)
                        && expiry.ToUniversalTime() < DateTime.UtcNow)
                    {
                        sb.AppendLine("   * The server's certificate has expired.");
                    }
                    else
                    {
                        sb.AppendLine("   * The certificate failed validation.");
                    }
                }

                sb.AppendLine("");
                sb.AppendLine("The server's identity cannot be guaranteed in this situation.");
                sb.AppendLine("");
                sb.AppendLine("You may choose to connect anyway, at your own risk. Your choice for this server will be saved for the current OCTGN session only.");
                sb.AppendLine("");
                sb.AppendLine("If you are unsure, choose No and seek support online.");
                sb.AppendLine();
                sb.AppendLine("Would you like to allow connections to this host anyway?");
                
                var ret = false;
                Application.Current.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            MessageBoxResult result = MessageBox.Show(
                                Application.Current.MainWindow,
                                sb.ToString(),
                                "SSL Error",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.None,
                                MessageBoxResult.No);
                            if (result == MessageBoxResult.Yes)
                            {
                                Log.Info($"Chose to ignore validation issues for {host}");
                                BypassList.Add(host);
                                ret = true;
                            }
                            else
                            {
                                Log.Info($"Chose not to ignore validation issues for {host}");
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