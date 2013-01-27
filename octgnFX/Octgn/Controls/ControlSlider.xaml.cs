// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ControlSlider.xaml.cs" company="Skylabs Corporation">
//   All Rights Reserved
// </copyright>
// <summary>
//   Interaction logic for ControlSlider.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DriveSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for ControlSlider.xaml
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public partial class ControlSlider
    {
        /// <summary>
        /// The show back button story.
        /// </summary>
        private readonly Storyboard showBackButtonStory;

        /// <summary>
        /// The hide back button story.
        /// </summary>
        private readonly Storyboard hideBackButtonStory;

        /// <summary>
        /// The show progress story.
        /// </summary>
        private readonly Storyboard showProgressStory;

        /// <summary>
        /// The hide progress story.
        /// </summary>
        private readonly Storyboard hideProgressStory;

        /// <summary>
        /// The current page.
        /// </summary>
        private int currentPage;

        /// <summary>
        /// The is sliding.
        /// </summary>
        private bool isSliding;

        /// <summary>
        /// The is sliding forward.
        /// </summary>
        private bool isSlidingForward;

        /// <summary>
        /// The is slide hiding.
        /// </summary>
        private bool isSlideHiding;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlSlider"/> class.
        /// </summary>
        public ControlSlider()
        {
            this.InitializeComponent();
            this.Pages = new List<SliderPage>();
            this.currentPage = 0;
            this.Loaded += this.OnLoaded;

            this.showBackButtonStory = ImageBack.Resources["ShowBackButtonStory"] as Storyboard;
            this.hideBackButtonStory = ImageBack.Resources["HideBackButtonStory"] as Storyboard;

            this.showProgressStory = ImageProgress.Resources["ShowProgressStory"] as Storyboard;
            this.hideProgressStory = ImageProgress.Resources["HideProgressStory"] as Storyboard;
        }

        /// <summary>
        /// Gets or sets the pages.
        /// </summary>
        public List<SliderPage> Pages { get; set; }

        /// <summary>
        /// Gets the slide num.
        /// </summary>
        internal int SlideNum
        {
            get
            {
                if (this.isSlidingForward)
                {
                    if (this.isSlideHiding)
                    {
                        return -10;
                    }

                    return 60;
                }

                if (this.isSlideHiding)
                {
                    return 60;
                }

                return -10;
            }
        }

        /// <summary>
        /// The navigate forward.
        /// </summary>
        internal void NavigateForward()
        {
            if (this.currentPage + 1 == this.Pages.Count || this.isSliding)
            {
                return;
            }

            this.isSlideHiding = true;
            this.isSlidingForward = true;

            var sbin = this.Resources["SlideOut"] as Storyboard;
            if (sbin != null)
            {
                this.isSliding = true;
                sbin.Children.OfType<DoubleAnimation>().FirstOrDefault().To = this.SlideNum;
                foreach (var a in sbin.Children)
                {
                    Storyboard.SetTarget(a, this.Pages[this.currentPage]);
                }
                sbin.Completed += this.SlideOutCompleted;
                this.BeginStoryboard(sbin);
            }
        }

        /// <summary>
        /// The navigate back.
        /// </summary>
        internal void NavigateBack()
        {
            if (this.currentPage - 1 < 0 || this.isSliding)
            {
                return;
            }

            this.isSlideHiding = true;
            this.isSlidingForward = false;

            this.HideBackButton();
            var sbin = this.Resources["SlideOut"] as Storyboard;
            if (sbin != null)
            {
                this.isSliding = true;
                sbin.Children.OfType<DoubleAnimation>().FirstOrDefault().To = this.SlideNum;
                foreach (var a in sbin.Children)
                {
                    Storyboard.SetTarget(a, this.Pages[this.currentPage]);
                }

                sbin.Completed += this.SlideOutCompleted;
                this.BeginStoryboard(sbin);
            }
        }

        /// <summary>
        /// Start a parallel task
        /// </summary>
        /// <param name="call">
        /// The task to run.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <typeparam name="TR">
        /// Return value from call
        /// </typeparam>
        internal void BeginInvoke<TR>(Func<TR> call, Action<TR> callback)
        {
            this.IsEnabled = false;
            this.HideBackButton();
            this.ShowProgress();
            call.BeginInvoke(
                delegate(IAsyncResult ar)
                {
                    var thecall = (Func<TR>)ar.AsyncState;
                    var res = thecall.EndInvoke(ar);
                    this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.IsEnabled = true;
                            this.ShowBackButton();
                            this.HideProgress();
                            callback(res);
                        }));
                },
                call);
        }

        /// <summary>
        /// Start a parallel task
        /// </summary>
        /// <param name="call">
        /// The task to run.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        internal void BeginInvoke(Action call, Action callback)
        {
            this.IsEnabled = false;
            this.HideBackButton();
            this.ShowProgress();
            call.BeginInvoke(
                ar => this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.IsEnabled = true; 
                        this.ShowBackButton();
                        this.HideProgress();
                        callback(); 
                    })),
                call);
        }

        /// <summary>
        /// The show back button.
        /// </summary>
        private void ShowBackButton()
        {
            this.BorderBack.Cursor = Cursors.Hand;
            this.BorderBack.PreviewMouseUp += this.BackPreviewMouseUp;
            ImageBack.Visibility = Visibility.Visible;
            ImageBack.BeginStoryboard(this.showBackButtonStory, HandoffBehavior.Compose);
        }

        /// <summary>
        /// Show Progress
        /// </summary>
        private void HideBackButton()
        {
            if (this.currentPage > 1)
            {
                return;
            }

            this.BorderBack.Cursor = Cursors.Arrow;
            this.BorderBack.PreviewMouseUp -= this.BackPreviewMouseUp;
            ImageBack.BeginStoryboard(this.hideBackButtonStory, HandoffBehavior.Compose);
        }

        /// <summary>
        /// The show back button.
        /// </summary>
        private void ShowProgress()
        {
            ImageProgress.Visibility = Visibility.Visible;
            ImageProgress.BeginStoryboard(this.showProgressStory);
        }

        /// <summary>
        /// Hide Progress
        /// </summary>
        private void HideProgress()
        {
            ImageProgress.BeginStoryboard(this.hideProgressStory);
        }

        /// <summary>
        /// The sbin on completed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        private void SlideOutCompleted(object sender, EventArgs eventArgs)
        {
            (sender as ClockGroup).Completed -= this.SlideOutCompleted;
            if (this.isSlidingForward)
            {
                this.currentPage += 1;
                this.ShowBackButton();
            }
            else
            {
                this.currentPage -= 1;
            }

            this.isSlideHiding = false;

            this.Content.Child = this.Pages[this.currentPage];
            var sbin = this.Resources["SlideIn"] as Storyboard;
            if (sbin != null)
            {
                sbin.Children.OfType<DoubleAnimation>().FirstOrDefault().From = this.SlideNum;
                foreach (var a in sbin.Children)
                {
                    Storyboard.SetTarget(a, this.Pages[this.currentPage]);
                }

                sbin.Completed += this.SlideInCompleted;
                this.BeginStoryboard(sbin);
            }
        }

        /// <summary>
        /// The slide in completed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        private void SlideInCompleted(object sender, EventArgs eventArgs)
        {
            (sender as ClockGroup).Completed -= this.SlideInCompleted;
            this.InvalidateVisual();
            this.isSliding = false;
        }

        /// <summary>
        /// The back preview mouse up.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        private void BackPreviewMouseUp(object sender, MouseButtonEventArgs args)
        {
            this.NavigateBack();
            args.Handled = true;
        }

        /// <summary>
        /// The on loaded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="routedEventArgs">
        /// The routed event arguments.
        /// </param>
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.Pages.Count <= 0)
            {
                return;
            }
             
            foreach (var p in this.Pages)
            {
                p.Slider = this;
                p.RenderTransform = new TranslateTransform();
            }

            this.Content.Child = this.Pages[0];
        }

        public void AddPage(SliderPage page)
        {
            page.Slider = this;
            page.RenderTransform = new TranslateTransform();
            this.Pages.Add(page);
        }
    }
}
