// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;

namespace Octide
{
    using GalaSoft.MvvmLight.Threading;
    using Octgn.Library;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void StartupHandler(object sender, System.Windows.StartupEventArgs e)
        {
			Config.Instance = new Config();
        }

        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
