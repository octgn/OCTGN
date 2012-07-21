using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Octgn.Common.Net
{
	public class TCPTools
	{
		public static IPAddress GetExternalIP()
		{
			var wc = new WebClient();
			var utf8 = new UTF8Encoding();
			try
			{
				var requestHtml = utf8.GetString(wc.DownloadData("http://checkip.dyndns.org"));
				var a = requestHtml.Split(':');
				var a2 = a[1].Substring(1);
				var a3 = a2.Split('<');
				var a4 = a3[0];
				var externalIp = IPAddress.Parse(a4);
				return externalIp;
			}
			catch (Exception we)
			{
				Log.L("GetExternalIP: Error - " + we);
				return null;
			}
		}
	}
}
