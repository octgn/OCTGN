using System.Windows;
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
    }
}
