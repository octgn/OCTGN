// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Controls;

namespace Octide.Controls
{
    /// <summary>
    /// Interaction logic for OctideItemPropertyViewer.xaml
    /// </summary>
    public partial class OctideItemPropertyViewer : UserControl
    {


        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisiblityProperty); }
            set { SetValue(VerticalScrollBarVisiblityProperty, value); }
        }
        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public object Description
        {
            get { return (object)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public object ListBoxHeader
        {
            get { return (object)GetValue(ListBoxHeaderProperty); }
            set { SetValue(ListBoxHeaderProperty, value); }
        }
        public IdeCollection<IdeBaseItem> ItemsSource
        {
            get { return (IdeCollection<IdeBaseItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty VerticalScrollBarVisiblityProperty
            = DependencyProperty.Register(nameof(VerticalScrollBarVisibility),
                                          typeof(ScrollBarVisibility),
                                          typeof(OctideItemPropertyViewer),
                                          new UIPropertyMetadata(ScrollBarVisibility.Auto));

        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(nameof(Title),
                                          typeof(object),
                                          typeof(OctideItemPropertyViewer),
                                          new UIPropertyMetadata(null));

        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register(nameof(Description),
                                          typeof(object),
                                          typeof(OctideItemPropertyViewer),
                                          new UIPropertyMetadata(null));

        public static readonly DependencyProperty ListBoxHeaderProperty
            = DependencyProperty.Register(nameof(ListBoxHeader),
                                          typeof(object),
                                          typeof(OctideItemPropertyViewer),
                                          new UIPropertyMetadata(null));

        public static readonly DependencyProperty ItemsSourceProperty
            = DependencyProperty.Register(nameof(ItemsSource),
                                          typeof(IdeCollection<IdeBaseItem>),
                                          typeof(OctideItemPropertyViewer),
                                          new PropertyMetadata(default(IdeCollection<IdeBaseItem>)));
        public OctideItemPropertyViewer()
        {
            InitializeComponent();
        }
    }
}
