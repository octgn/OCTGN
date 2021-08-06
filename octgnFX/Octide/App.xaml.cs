// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;

namespace Octide
{
    using GalaSoft.MvvmLight.Threading;
    using Octgn.Library;
    using System.IO;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void StartupHandler(object sender, System.Windows.StartupEventArgs e)
        {
            Config.Instance = new Config();
            var garbageFolder = new DirectoryInfo(Config.Instance.Paths.GraveyardPath).Parent;
            foreach (var dir in garbageFolder.GetDirectories())
            {
                try
                {
                    dir.Delete(true);
                }
                catch
                {
                }
            }
        }

        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
