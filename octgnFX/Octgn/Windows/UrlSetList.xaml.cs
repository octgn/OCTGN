using System;
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
using System.Windows.Shapes;

namespace Octgn.Windows
{
    /// <summary>
    /// Interaction logic for UrlSetList.xaml
    /// </summary>
    public partial class UrlSetList : Window
    {
        public DataNew.Entities.Game game;
        public UrlSetList()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            new Windows.UrlSets { game = game }.ShowDialog();
            RefreshWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshWindow();
        }

        private void RefreshWindow()
        {
            listBox1.Items.Clear();
            List<String> list = game.GetAllXmls();
            foreach (String s in list)
            {
                listBox1.Items.Add(s);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string xml_link = listBox1.SelectedItem.ToString();
            game.DeleteXmlByLink(xml_link);
            RefreshWindow();
        }
    }
}
