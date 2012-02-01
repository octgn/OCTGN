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
using System.IO;

namespace Octgn.Play.Dialogs
{
    /// <summary>
    /// Interaction logic for RulesWindow.xaml
    /// </summary>
    public partial class RulesWindow : Window
    {
        public RulesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {           
            // Get Stream of the file
            StreamReader fileReader = new StreamReader(File.Open(@"C:\Downloads\rules.txt", FileMode.Open));
            FileInfo fileInfo = new FileInfo(@"C:\Downloads\rules.txt");
            long bytesRead = 0;
            // Change the 75 for performance.  Find a number that suits your application best
            int bufferLength = 1024 * 75;
            while (!fileReader.EndOfStream)
            {
                double completePercent = ((double)bytesRead / (double)fileInfo.Length);
                int readLength = bufferLength;
                if ((fileInfo.Length - bytesRead) < readLength)
                {
                    // There is less in the file than the lenght I am going to read so change it to the 
                    // smaller value
                    readLength = (int)(fileInfo.Length - bytesRead);
                }
                char[] buffer = new char[readLength];
                // GEt the next chunk of the file
                bytesRead += (long)(fileReader.Read(buffer, 0, readLength));
                // This will help the file load much faster
                string currentLine = new string(buffer).Replace("\n", string.Empty);
                // Load in background
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextRange range = new TextRange(rules.Document.ContentEnd, rules.Document.ContentEnd);
                    range.Text = currentLine;
                }), System.Windows.Threading.DispatcherPriority.Normal);
            }
        }

        private void FindText(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {                
                DoSearch(rules, search.Text, true);
                rules.Focus();

                // Testing code for text scrolling
                /*var start = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
                var end = rules.Selection.End.GetCharacterRect(LogicalDirection.Forward);                
                rules.ScrollToVerticalOffset((start.Top + end.Bottom - rules.ViewportHeight) / 2 + rules.VerticalOffset);*/

                // WPF RichTextBox doesn't include "scrolltocaret"
                Rect thisposition = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
                double totaloffset = thisposition.Top + rules.VerticalOffset;
                scroller.ScrollToVerticalOffset(totaloffset - scroller.ActualHeight / 2);
                // Handle the keypress. We don't want to muck with rules text
                e.Handled = true;
            }
        }

        public bool DoSearch(RichTextBox richTextBox, string searchText, bool searchNext)
        {
            TextRange searchRange;
            // Get the range to search
            if (searchNext)
                searchRange = new TextRange(
                    richTextBox.Selection.Start.GetPositionAtOffset(1),
                    richTextBox.Document.ContentEnd);
            else
                searchRange = new TextRange(
                    richTextBox.Document.ContentStart,
                    richTextBox.Document.ContentEnd);
            // Do the search
            TextRange foundRange = FindTextInRange(searchRange, searchText);
            if (foundRange == null)
                return false;
            // Select the found range
            richTextBox.Selection.Select(foundRange.Start, foundRange.End);
            return true;
        }

        public TextRange FindTextInRange(TextRange searchRange, string searchText)
        {
          // Search the text with IndexOf
          int offset = searchRange.Text.IndexOf(searchText);
          if(offset<0)
            return null;  // Not found
          // Try to select the text as a contiguous range
          for(TextPointer start = searchRange.Start.GetPositionAtOffset(offset); start != searchRange.End; start = start.GetPositionAtOffset(1))
          {
            TextRange result = new TextRange(start, start.GetPositionAtOffset(searchText.Length));
            if(result.Text == searchText)
              return result;
          }
          return null;
        }

        /* Credits go to StackOverflow and it's authors for helping me fix some of these annoying problems
         * http://stackoverflow.com/questions/1756844/making-a-simple-search-function-making-the-cursor-jump-to-or-highlight-the-wo
         * http://stackoverflow.com/questions/837086/c-sharp-loading-a-large-file-into-a-wpf-richtextbox
         * http://stackoverflow.com/questions/1228714/how-do-i-find-the-viewable-area-of-a-wpf-richtextbox
         */

    }
}
