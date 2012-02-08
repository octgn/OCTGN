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
using System.IO.Packaging;

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

        private const string _packageRelationshipType = "http://schemas.openxmlformats.org/package/2007/" + "relationships/htmx/root-html";
        private const string _resourceRelationshipType = "http://schemas.openxmlformats.org/package/2007/" + "relationships/htmx/required-resource";
        private string ExtractPackageParts(string packageFile, string targetDirectory)
        {
            Uri uriDocumentTarget = null;
            // Open the Package.
            using (Package package = Package.Open(packageFile, FileMode.Open, FileAccess.Read))
            {
                PackagePart documentPart = null,  resourcePart = null;
                // Examine the package-level relationships and look for
                // the relationship with the "root-html" RelationshipType.
                foreach (PackageRelationship relationship in
                    package.GetRelationshipsByType(_packageRelationshipType))
                {
                    // Resolve the relationship target URI so
                    // the root-html document part can be retrieved.
                    uriDocumentTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), relationship.TargetUri);
                    // Open the document part and write its contents to a file.
                    documentPart = package.GetPart(uriDocumentTarget);
                    ExtractPart(documentPart, targetDirectory);
                }
                // Examine the root part’s part-level relationships and look
                // for relationships with "required-resource" RelationshipTypes.
                Uri uriResourceTarget = null;
                foreach (PackageRelationship relationship in
                    documentPart.GetRelationshipsByType(_resourceRelationshipType))
                {
                    // Resolve the Relationship Target Uri so the resource part
                    // can be retrieved.
                    uriResourceTarget = PackUriHelper.ResolvePartUri(documentPart.Uri, relationship.TargetUri);
                    // Open the resource part and write the contents to a file.
                    resourcePart = package.GetPart(uriResourceTarget);
                    ExtractPart(resourcePart, targetDirectory);
                }
            }
            // Return the path and filename to the file referenced
            // by the HTMX package’s "root-html" package-level relationship.
            return targetDirectory + uriDocumentTarget.ToString().TrimStart('/');
        }

        private static void ExtractPart(PackagePart packagePart, string targetDirectory)
        {
            // Remove leading slash from the Part Uri and make a new
            // relative Uri from the result.
            string stringPart = packagePart.Uri.ToString().TrimStart('/');
            Uri partUri = new Uri(stringPart, UriKind.Relative);
            // Create an absolute file URI by combining the target directory
            // with the relative part URI created from the part name.
            Uri uriFullFilePath = new Uri(new Uri(targetDirectory, UriKind.Absolute), partUri);
            // Create the necessary directories based on the full part path
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(uriFullFilePath.LocalPath));
            // Write the file from the part’s content stream.
            using (FileStream fileStream = File.Create(uriFullFilePath.LocalPath))
            {                
                CopyStream(packagePart.GetStream(), fileStream);
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            input.CopyTo(output);
            input.Position = output.Position = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Uri uri = new Uri(Program.Game.Definition.PackUri + "/rules.txt");
            Package mypackage = Package.Open(@"D:\Magic-v3.0.2.o8g", FileMode.Open);
            PackagePart srootPart = mypackage.GetPart(new Uri("/rules.txt"));

            // Path and name of the package file.
            string packageFile = @"D:\Magic-v3.0.2.o8g"; 
            // Name for the root package relationship type.
            string rootHtmlRelationshipType = "http://schemas.openxmlformats.org/" +  "package/2007/relationships/htmx/root-html";
            string resourceRelationshipType = "http://schemas.openxmlformats.org/" + "package/2007/relationships/htmx/required-resource";
            // Open the package for reading. - - - - - - - - - - - - - - -
            try
            {
                using (Package package = Package.Open(packageFile, FileMode.Open, FileAccess.Read))
                {
                    // A package can contain multiple root items, iterate through each.  Get the “root-html” package relationship.
                    foreach (PackageRelationship relationship in package.GetRelationshipsByType(rootHtmlRelationshipType))
                    {
                        // Get the part referenced by the relationship TargetUri.
                        PackagePart rootPart = package.GetPart(relationship.TargetUri);
                        // Open and access the part data stream.
                        using (Stream dataStream = rootPart.GetStream())
                        {
                            //  Access the root part’s data stream.
                        }
                        // A part can have associations with other parts. Locate and iterate through each associated part.
                        // Iterate through each “required-resource” part.
                        foreach (PackageRelationship resourceRelationship in rootPart.GetRelationshipsByType(resourceRelationshipType))
                        {
                            // Open the Resource Part and write the contents to a file.
                            PackagePart resourcePart = package.GetPart(resourceRelationship.TargetUri);
                            // Party with the resource part.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Extract the Web page and its local resources to the temp folder.
            string _tempFolder = @"D:\Temp";
            // Extract the Web page and its local resources to the temp folder.
            string _htmlFilepath = ExtractPackageParts(packageFile, _tempFolder);
            // Convert the path and filename to a URI for the browser control.
            Uri webpageUri;
            try
            {
                webpageUri = new Uri(_htmlFilepath, UriKind.Absolute);
            }
            catch (System.UriFormatException)
            {
                string msg = _htmlFilepath + "\n\nThe specified path and " + "filename cannot be converted to a valid URI.\n\n";
                System.Windows.MessageBox.Show(msg, "Invalid URI", MessageBoxButton.OK, MessageBoxImage.Error);
            }







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

        private void FindNext(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoSearch(rules, search.Text, true);
                // WPF RichTextBox doesn't include "scrolltocaret"
                Rect thisposition = rules.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
                double totaloffset = thisposition.Top + rules.VerticalOffset;
                scroller.ScrollToVerticalOffset(totaloffset - scroller.ActualHeight / 2);
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
