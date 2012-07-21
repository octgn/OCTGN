using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data
{
	public static class Prefs
	{
		public static int C2SPort
		{
			get { return SimpleConfig.ReadValue("c2sport" , 8123); }
			set { SimpleConfig.WriteValue("c2sport" , value); } 
		}

		public static int S2SPort
		{
			get { return SimpleConfig.ReadValue("s2sport" , 8124); }
			set { SimpleConfig.WriteValue("s2sport" , value); } 
		}
	}
}
