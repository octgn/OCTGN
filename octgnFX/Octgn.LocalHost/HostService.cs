/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Octgn.Core;
using System;
using System.ServiceProcess;

namespace Octgn.LocalHost
{
    public partial class HostService : ServiceBase
    {
        internal static log4net.ILog Log = log4net.LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        public HostService() {
            InitializeComponent();
        }

        public void Start( params string[] args ) => OnStart( args );

        protected override void OnStart( string[] args ) {
            try {
                Log.Info( "Service Starting" );
                GameServer.Instance.Start( Prefs.HostPath );
            } catch( Exception e ) {
                Log.Fatal( nameof( OnStart ), e );
                throw;
            }
        }

        protected override void OnStop() {
            try {
                Log.Info( "Service Stopping" );
                GameServer.Instance.Dispose();
            } catch( Exception e ) {
                Log.Fatal( nameof( OnStop ), e );
                throw;
            }
        }
    }
}
