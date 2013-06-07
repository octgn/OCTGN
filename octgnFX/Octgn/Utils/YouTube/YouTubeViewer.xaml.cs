namespace Octgn.Utils.YouTube
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Contains a new .NET 3.5 SP1 WebBrowser control
    /// that simply navigates to the YouTube SWF file
    /// Uri
    /// </summary>
    public partial class YouTubeViewer : UserControl
    {
        #region Data
        private string videoUrl = string.Empty;
        private static bool IsExpanded = false;
        public event EventHandler ClosedEvent;
        #endregion

        #region Ctor
        public YouTubeViewer()
        {
            this.InitializeComponent();
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the close button is clicked. This event
        /// is used by YouViewerMainWindow to set Opacity on its
        /// contained DragCanvas back to fully viewable Opacity
        /// </summary>
        protected virtual void OnClosedEvent(EventArgs e)
        {
            if (this.ClosedEvent != null)
            {
                //Invokes the delegates.
                this.ClosedEvent(this, e);
            }
        }
        #endregion

        #region Properties

        public string VideoUrl
        {
            set
            {
                if (this.videoUrl != value)
                {
                    if (!IsExpanded)
                    {
                        this.videoUrl = value;
                        this.browser.Source = new Uri(this.videoUrl, UriKind.Absolute);
                        IsExpanded = true;
                        Storyboard sbEnter = this.TryFindResource("OnMouseEnter") as Storyboard;
                        if (sbEnter != null)
                        {
                            sbEnter.Completed += new EventHandler(this.sbEnter_Completed);
                            sbEnter.Begin(this);
                        }
                    }
                }
            }

        }
        #endregion

        #region Private Methods

        private void sbEnter_Completed(object sender, EventArgs e)
        {
            this.browser.Visibility = Visibility.Visible;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (IsExpanded)
            {
                this.browser.Visibility = Visibility.Collapsed;
                IsExpanded = false;
                this.browser.Source = null;
                Storyboard sbLeave = this.TryFindResource("OnMouseLeave") as Storyboard;
                if (sbLeave != null)
                {
                    sbLeave.Completed += this.sbLeave_Completed;
                    sbLeave.Begin(this);
                }
            }
        }

        private void sbLeave_Completed(object sender, EventArgs e)
        {
            this.OnClosedEvent(new EventArgs());
        }
        #endregion
    }
}