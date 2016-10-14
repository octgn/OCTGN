/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using log4net;
using System;
using System.ServiceProcess;

namespace Octgn.LocalHost
{
    static class Program
    {
        static void Main( params string[] args ) {
            Console.Write( " " );
            GlobalContext.Properties["version"] = typeof(Program).Assembly.GetName().Version;
            using( HostService service = new HostService() ) {
                if( Environment.UserInteractive ) {
                    service.Start( args );
                    Console.WriteLine( "Any Key To Stop" );
                    Console.ReadKey();
                    service.Stop();
                } else {
                    HostService[] services = new HostService[] { service };
                    ServiceBase.Run( services );
                }
            }
        }
    }
}
