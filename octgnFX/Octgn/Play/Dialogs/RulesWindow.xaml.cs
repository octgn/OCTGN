﻿using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace Octgn.Play.Dialogs
{
    /// <summary>
    ///   Interaction logic for RulesWindow.xaml
    /// </summary>
    public partial class RulesWindow
    {
        public RulesWindow()
        {
            InitializeComponent();
        }

        public static void CopyStream(Stream input, Stream output)
        {
            input.CopyTo(output);
            input.Position = output.Position = 0;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Program.Game.Definition.PackUri.Replace(',', '/'));
            string defLoc = uri.LocalPath.Remove(0, 3).Replace('/', '\\');
            using (Package package = Package.Open(defLoc, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PackageRelationship defRelationship =
                    package.GetRelationshipsByType("http://schemas.octgn.org/game/rules").First();
                PackagePart definition = package.GetPart(defRelationship.TargetUri);
                using (var fileReader = new StreamReader(definition.GetStream(FileMode.Open, FileAccess.Read)))
                {
                    // Change the 75 for performance.  Find a number that suits your application best
                    const int bufferLength = 1024*75;
                    while (!fileReader.EndOfStream)
                    {
                        const int readLength = bufferLength;
                        var buffer = new char[readLength];
                        fileReader.Read(buffer, 0, readLength);
                        // This will help the file load much faster 
                        // RichText loads \n as a new paragraph. Very slow for large text
                        string currentLine = new string(buffer).Replace("\n", string.Empty);
                        // Load in background
                        Dispatcher.BeginInvoke(new Action(() =>
                                                              {
                                                                  new TextRange(rules.Document.ContentEnd,
                                                                                rules.Document.ContentEnd)
                                                                      {Text = currentLine};
                                                              }), DispatcherPriority.Normal);
                    }
                }
            }
        }

        private void FindText(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DoSearch(rules, search.Text, true);
            rules.Focus();

            // Testing code for text scrolling
            /*var start = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
                var end = rules.Selection.End.GetCharacterRect(LogicalDirection.Forward);                
                rules.ScrollToVerticalOffset((start.Top + end.Bottom - rules.ViewportHeight) / 2 + rules.VerticalOffset);*/

            // WPF RichTextBox doesn't include "scrolltocaret"
            Rect thisposition = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
            double totaloffset = thisposition.Top + rules.VerticalOffset;
            scroller.ScrollToVerticalOffset(totaloffset - scroller.ActualHeight/2);
            // Handle the keypress. We don't want to muck with rules text
            e.Handled = true;
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
            int offset = searchRange.Text.IndexOf(searchText, StringComparison.Ordinal);
            if (offset < 0)
                return null; // Not found
            // Try to select the text as a contiguous range
            for (TextPointer start = searchRange.Start.GetPositionAtOffset(offset);
                 start != searchRange.End;
                 start = start.GetPositionAtOffset(1))
            {
                if (start == null) continue;
                var result = new TextRange(start, start.GetPositionAtOffset(searchText.Length));
                if (result.Text == searchText)
                    return result;
            }
            return null;
        }

        private void FindNext(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoSearch(rules, search.Text, true);
                // WPF RichTextBox doesn't include "scrolltocaret"
                Rect thisposition = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
                double totaloffset = thisposition.Top + rules.VerticalOffset;
                scroller.ScrollToVerticalOffset(totaloffset - scroller.ActualHeight/2);
            }
            e.Handled = true;
        }

        /* Credits go to StackOverflow and it's authors for helping me fix some of these annoying problems
         * http://stackoverflow.com/questions/1756844/making-a-simple-search-function-making-the-cursor-jump-to-or-highlight-the-wo
         * http://stackoverflow.com/questions/837086/c-sharp-loading-a-large-file-into-a-wpf-richtextbox
         * http://stackoverflow.com/questions/1228714/how-do-i-find-the-viewable-area-of-a-wpf-richtextbox
         */
    }
}