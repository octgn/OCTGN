/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using Octgn.DataNew;
using Octgn.DataNew.Entities;
using System.Windows.Media.Imaging;

namespace Octgn.Core.DataExtensionMethods
{
    public static class SleeveExtensionMethods
    {
        public static BitmapImage GetImage(this ISleeve sleeve) {
            if (sleeve == null) return null;

            BitmapImage image = null;

            using (var memoryStream = new MemoryStream(sleeve.ImageData)) {
                memoryStream.Position = 0;

                image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;

                try {
                    image.StreamSource = memoryStream;

                    image.EndInit();
                } catch (Exception ex) {
                    throw new SleeveException($"Error loading deck sleeve for {sleeve.Name}, the image is invalid.", ex);
                }

                if (image.CanFreeze) {
                    image.Freeze();
                }
            }

            return image;
        }

    }
}
