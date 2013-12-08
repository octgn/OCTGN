using System.Windows.Controls;

namespace Octide.Views
{
    using System;
    using System.IO;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml;

    using ICSharpCode.AvalonEdit.Highlighting;
    using ICSharpCode.AvalonEdit.Highlighting.Xshd;

    using Octide.ViewModel;

    /// <summary>
    /// Interaction logic for AssetView.xaml
    /// </summary>
    public partial class AssetsTabView : UserControl
    {
        public AssetsTabView()
        {
            InitializeComponent();
        }

        private void LoadAssetClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as AssetsTabViewModel;
            var tree = sender as TreeView;

            var si = (AssetTreeViewItemViewModel)(tree.SelectedItem);

            if (si == null) return;
            if (si.FileSystemInfo is DirectoryInfo) return;

            var ass = Asset.Load(si.FileSystemInfo as FileInfo);

            if (string.IsNullOrWhiteSpace(ass.FullPath)) return;

            switch (ass.Type)
            {
                case AssetType.Image:
                {
                    var t = new TabItem();
                    var i = new Image();
                    i.Source = new BitmapImage(new Uri(ass.FullPath));
					i.Stretch = Stretch.UniformToFill;

                    t.Header = ass.FileName + "." + ass.Extension;
                    t.Content = i;
                    tab.Items.Add(t);
                    tab.SelectedItem = t;
                    break;
                }
                case AssetType.PythonScript:
                {
                    var t = new TabItem();
                    var editor = new ICSharpCode.AvalonEdit.TextEditor() { Text = File.ReadAllText(ass.FullPath) };
                    var something =
                        HighlightingLoader.Load(
                            new XmlTextReader(File.Open(@"SyntaxHighlighting\Python.xshd", FileMode.Open)),
                            HighlightingManager.Instance);
                    editor.SyntaxHighlighting = something;
                    editor.ShowLineNumbers = true;
                    t.Header = ass.FileName + "." + ass.Extension;
                    t.Content = editor;
                    tab.Items.Add(t);
                    tab.SelectedItem = t;
                    break;
                }
                case AssetType.Xml:
                {
                    var t = new TabItem();
                    var editor = new ICSharpCode.AvalonEdit.TextEditor() { Text = File.ReadAllText(ass.FullPath) };
                    //var something =
                    //    HighlightingLoader.Load(
                    //        new XmlTextReader(File.Open(@"SyntaxHighlighting\Python.xshd", FileMode.Open)),
                    //        HighlightingManager.Instance);
                    //editor.SyntaxHighlighting = something;
                    editor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
                    t.Header = ass.FileName + "." + ass.Extension;
                    editor.ShowLineNumbers = true;
                    t.Content = editor;
                    tab.Items.Add(t);
                    tab.SelectedItem = t;
                    break;
                }
                case AssetType.Sound:
                    break;
                case AssetType.Other:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
