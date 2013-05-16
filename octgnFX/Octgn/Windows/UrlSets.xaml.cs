using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Net;
using Octgn.Utils;


namespace Octgn.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UrlSets : Window
    {
        public DataNew.Entities.Game game;

        public UrlSets()
        {
            InitializeComponent();
        }

        private void CheckXmlClick(object sender, RoutedEventArgs e)
        {
            var wnd = new Windows.ChangeSetsProgressDialog("Testing xml...") {Owner = Program.MainWindowNew};
            String urlLink = urlBox.Text;
            bool is_ok = false;
            ThreadPool.QueueUserWorkItem(_ =>
                                             {
                                                 try
                                                 {
                                                     int current = 0, max = 10;
                                                     wnd.UpdateProgress(current, max, "Retrieving xml...", false);
                                                     XmlSetParser xmls = new XmlSetParser(urlLink);
                                                     current++;
                                                     XmlSimpleValidate validator = new XmlSimpleValidate(xmls);
                                                     validator.CheckVerboseXml(wnd, max, game);
                                                     is_ok = true;
                                                 }
                                                 catch (Exception ex)
                                                 {
                                                     wnd.UpdateProgress(10, 10, string.Format("Caught exception when retrieving / parsing xml: '{0}'", ex.Message), false);
                                                 }
                                                     
                                             });
            wnd.ShowDialog();
            button3.IsEnabled = is_ok;
            button2.IsEnabled = is_ok;
        }

        private void DownloadAndInstallClick(object sender, RoutedEventArgs e)
        {
            var wnd = new Windows.ChangeSetsProgressDialog("Downloading and installing...") { Owner = Program.MainWindowNew };
            String urlLink = urlBox.Text;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    int max = 6;
                    wnd.UpdateProgress(0, max, "Retrieving xml...", false);
                    XmlSetParser xmls = new XmlSetParser(urlLink);
                    XmlInstaller xmli = new XmlInstaller(xmls);
                    xmli.installSet(wnd, max, game);
                        
                }
                catch (Exception ex)
                {
                    wnd.UpdateProgress(6, 6, string.Format("Caught exception: '{0}'", ex.Message), false);
                }

            });
            wnd.ShowDialog();
        }

        private void SaveConfigurationClick(object sender, RoutedEventArgs e)
        {
            if (game.GetXmlByLink(urlBox.Text) != null)
            {
                MessageBox.Show("Duplicated xmls - not allowed.");
                return;
            }
            game.AddXml(urlBox.Text);
            this.Close();
        }

        private void urlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (button3 != null)
                button3.IsEnabled = false;
            if (button2 != null)
                button2.IsEnabled = false;
        }
    }
}
