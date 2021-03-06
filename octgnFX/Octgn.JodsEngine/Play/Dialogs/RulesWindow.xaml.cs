﻿using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Octgn.Play.Dialogs
{
    using Octgn.DataNew.Entities;

    /// <summary>
    ///   Interaction logic for RulesWindow.xaml
    /// </summary>
    public partial class RulesWindow
    {

        public RulesWindow(Document doc)
        {
            InitializeComponent();
            this.Title = doc.Name;
            this.LoadText(doc);
        }

        public static void CopyStream(Stream input, Stream output)
        {
            input.CopyTo(output);
            input.Position = output.Position = 0;
        }

        private void LoadText(Document doc)
        {
            Dispatcher.Invoke(new Action(()=>this.rules.Navigate(doc.Source)));
            //using (var fileReader = new StreamReader(new FileStream(doc.Source, FileMode.Open, FileAccess.Read)))
            //{
            //    const int bufferLength = 1024 * 75;
            //    while (!fileReader.EndOfStream)
            //    {
            //        const int readLength = bufferLength;
            //        var buffer = new char[readLength];
            //        fileReader.Read(buffer, 0, readLength);
            //        // RichText loads \n as a new paragraph. Very slow for large text
            //        string currentLine = new string(buffer).Replace("\n", string.Empty).Trim('\0');
            //        // Load in background
            //        Dispatcher.BeginInvoke(new Action(() =>
            //                                              {
            //                                                  new TextRange(rules.Document.ContentEnd,
            //                                                                rules.Document.ContentEnd) { Text = currentLine };
            //                                              }), DispatcherPriority.Normal);
            //    }
            //}
        }

        //private void FindText(object sender, KeyEventArgs e)
        //{
        //    if (e.Key != Key.Enter) return;
        //    DoSearch(rules, search.Text, true);
        //    rules.Focus();

        //    // WPF RichTextBox doesn't include "scrolltocaret"
        //    Rect thisposition = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
        //    double totaloffset = thisposition.Top + rules.VerticalOffset;
        //    scroller.ScrollToVerticalOffset(totaloffset - scroller.ActualHeight / 2);
        //    // Handle the keypress. We don't want to muck with rules text
        //    e.Handled = true;
        //}

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
            int offset = searchRange.Text.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
            if (offset < 0)
                return null; // Not found
            // Try to select the text as a contiguous range
            for (TextPointer start = searchRange.Start.GetPositionAtOffset(offset);
                 start != searchRange.End && start != null;
                 start = start.GetPositionAtOffset(1))
            {
                var result = new TextRange(start, start.GetPositionAtOffset(searchText.Length));
                if (result.Text.ToLower() == searchText.ToLower())
                    return result;
            }
            return null;
        }

        //private void FindNext(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        DoSearch(rules, search.Text, true);
        //        // WPF RichTextBox doesn't include "scrolltocaret"
        //        Rect thisposition = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
        //        double totaloffset = thisposition.Top + rules.VerticalOffset;
        //        scroller.ScrollToVerticalOffset(totaloffset - scroller.ActualHeight / 2);
        //    }
        //    e.Handled = true;
        //}
    }
}