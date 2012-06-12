using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn
{
	public static class Log
	{
		/// <summary>
		/// Writes a log string, shows up in release as well.
		/// </summary>
		/// <param name="format">String Format</param>
		/// <param name="args">Arguments to add to the string</param>
		public static void L(string format, params object[] args)
		{
			Console.WriteLine("[L {0} {1}] {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), String.Format(format, args));
		}
		/// <summary>
		/// Write a Debug log, that only shows up if the DEBUG preprocessor is set.
		/// </summary>
		/// <param name="format">String Format</param>
		/// <param name="args">Arguments to add to the string</param>
		public static void D(string format, params object[] args)
		{
#if(DEBUG)
			Console.WriteLine("[D {0} {1}] {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), String.Format(format, args));
#endif
		}
	}
}
