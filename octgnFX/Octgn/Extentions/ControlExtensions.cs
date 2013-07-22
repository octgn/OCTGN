/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octgn.Extentions
{
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// The System.Windows.Controls.Control extensions.
    /// </summary>
    public static class ControlExtensions
    {
        /// <summary>
        /// The is in design mode.
        /// </summary>
        private static bool? isInDesignMode;

        /// <summary>
        /// The is in design mode.
        /// </summary>
        /// <param name="control">
        /// The control.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsInDesignMode(this System.Windows.Controls.Control control)
        {
            if (!isInDesignMode.HasValue)
            {
#if SILVERLIGHT
                    isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                var prop = DesignerProperties.IsInDesignModeProperty;
                isInDesignMode
                    = (bool)DependencyPropertyDescriptor
                    .FromProperty(prop, typeof(FrameworkElement))
                    .Metadata.DefaultValue;
#endif
            }

            return isInDesignMode.Value;
        }

        /// <summary>
        /// The is in design mode.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsInDesignMode()
        {
            if (!isInDesignMode.HasValue)
            {
#if SILVERLIGHT
                    isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                var prop = DesignerProperties.IsInDesignModeProperty;
                isInDesignMode
                    = (bool)DependencyPropertyDescriptor
                    .FromProperty(prop, typeof(FrameworkElement))
                    .Metadata.DefaultValue;
#endif
            }

            return isInDesignMode.Value;
        }
    }
}
