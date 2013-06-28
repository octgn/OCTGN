using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Extentions
{
	public static class Conversion
	{
		public static string ToStringValue(this object item)
		{
			return item.ToStringValue(String.Empty);
		}

		public static string ToStringValue(this object item, string defaultValue)
		{
			string value;
			try
			{
				value = item.ToString();
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

		public static object ToStringValue(this object item, object defaultValue)
		{
			string value;
			object objectValue;

			try
			{
				value = item.ToString();
				if (value.Length == 0)
				{
					objectValue = defaultValue;
				}
				else
				{
					objectValue = value;
				}
			}
			catch
			{
				objectValue = defaultValue;
			}
			return objectValue;
		}

		public static int ToInt32(this object item)
		{
			return item.ToInt32(0);
		}

		public static int ToInt32(this object item, int defaultValue)
		{
			int value;
			try
			{
				if (item.IsANumber())
				{
					value = Convert.ToInt32(item);
				}
				else
				{
					value = defaultValue;
				}
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

		public static short ToInt16(this object item)
		{
			return item.ToInt16(0);
		}

		public static short ToInt16(this object item, short defaultValue)
		{
			short value;
			try
			{
				value = Convert.ToInt16(item);
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

		public static decimal ToDecimal(this object item)
		{
			return item.ToDecimal(0);
		}

		public static decimal ToDecimal(this object item, decimal defaultValue)
		{
			decimal value;
			try
			{
				value = Convert.ToDecimal(item);
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

		public static double ToDouble(this object item)
		{
			return item.ToDouble(0);
		}

		public static double ToDouble(this object item, double defaultValue)
		{
			double value;
			try
			{
				value = Convert.ToDouble(item);
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}


		public static bool ToBool(this object item)
		{
			return item.ToBool(false);
		}

		public static bool ToBool(this object item, bool defaultValue)
		{
			bool value;
			try
			{
				value = Convert.ToBoolean(item);
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

		public static DateTime ToDateTime(this object item)
		{
			return item.ToDateTime(DateTime.Now);
		}

		public static DateTime ToDateTime(this object item, DateTime defaultValue)
		{
			DateTime value;
			try
			{
				value = Convert.ToDateTime(item);
			}
			catch
			{
				value = defaultValue;
			}
			return value;
		}

		public static bool IsANumber(this object itemValue)
		{
			double convertedNumber = 0;
			bool isSuccessful = false;
			if (itemValue == null)
			{
				return isSuccessful;
			}

			isSuccessful = Double.TryParse(itemValue.ToString()
				, System.Globalization.NumberStyles.Any
				, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat
				, out convertedNumber);
			return isSuccessful;
		}

	}
}
