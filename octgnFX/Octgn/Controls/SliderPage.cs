// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SliderPage.cs" company="Skylabs Corporation">
//   All Rights Reserved
// </copyright>
// <summary>
//   Defines the SliderPage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DriveSync
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// The slider page.
    /// </summary>
    public class SliderPage : UserControl
    {
        /// <summary>
        /// Gets or sets the slider.
        /// </summary>
        public ControlSlider Slider { get; set; }

        /// <summary>
        /// Navigate forward.
        /// </summary>
        public void NavigateForward()
        {
            if (this.Slider != null)
            {
                this.Slider.NavigateForward();
            }
        }

        /// <summary>
        /// Navigate back.
        /// </summary>
        public void NavigateBack()
        {
            if (this.Slider != null)
            {
                this.Slider.NavigateBack();
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
        public void BeginInvoke<TR>(Func<TR> call, Action<TR> callback)
        {
            if (this.Slider == null)
            {
                this.IsEnabled = false;
                call.BeginInvoke(
                    delegate(IAsyncResult ar)
                        {
                            var thecall = (Func<TR>)ar.AsyncState;
                            var res = thecall.EndInvoke(ar);
                            this.Dispatcher.Invoke(
                                new Action(
                                    () =>
                                        {
                                            this.IsEnabled = true;
                                            callback(res);
                                        }));
                        },
                    call);
            }
            else
            {
                this.Slider.BeginInvoke(call, callback);
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
        public void BeginInvoke(Action call, Action callback)
        {
            if (this.Slider == null)
            {
                this.IsEnabled = false;
                call.BeginInvoke(
                    ar => this.Dispatcher.Invoke(
                        new Action(
                              () =>
                                  {
                                      this.IsEnabled = true;
                                      callback();
                                  })),
                    call);
            }
            else
            {
                this.Slider.BeginInvoke(call, callback);
            }
        }
    }
}
