/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2012 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */ 

using System;
using System.Reflection;

namespace agsXMPP.Util
{
	/// <summary>
	/// Provides helper functions for Enumerations.
	/// </summary>
	/// <remarks>Extends the <see cref="T:System.Enum">System.Enum Class</see>.</remarks>
	/// <seealso cref="T:System.Enum">System.Enum Class</seealso>
	public class Enum
	{
#if CF
		#region << Enum.Parse() for CF, credits to OpenNetCF.net for this function>>
		/// <summary>
		/// Use this on CF 1.0, CF 2 includes Enum.Parse() now
		/// </summary>
		/// <param name="enumType"></param>
		/// <param name="value"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>        
        public static object Parse(System.Type enumType, string value, bool ignoreCase)
		{
			//throw an exception on null value
			if(value.TrimEnd(' ')=="")
			{
				throw new ArgumentException("value is either an empty string (\"\") or only contains white space.");
			}
			else
			{
				//type must be a derivative of enum
				if(enumType.BaseType==Type.GetType("System.Enum"))
				{
					//remove all spaces
					string[] memberNames = value.Replace(" ","").Split(',');
					
					//collect the results
					//we are cheating and using a long regardless of the underlying type of the enum
					//this is so we can use ordinary operators to add up each value
					//I suspect there is a more efficient way of doing this - I will update the code if there is
					long returnVal = 0;

					//for each of the members, add numerical value to returnVal
					foreach(string thisMember in memberNames)
					{
						//skip this string segment if blank
						if(thisMember!="")
						{
							try
							{
								if(ignoreCase)
								{
									returnVal += (long)Convert.ChangeType(enumType.GetField(thisMember, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase).GetValue(null),returnVal.GetType(), null);
								}
								else
								{
									returnVal += (long)Convert.ChangeType(enumType.GetField(thisMember, BindingFlags.Public | BindingFlags.Static).GetValue(null),returnVal.GetType(), null);
								}
							}
							catch
							{
								try
								{
									//try getting the numeric value supplied and converting it
									returnVal += (long)Convert.ChangeType(System.Enum.ToObject(enumType, Convert.ChangeType(thisMember, System.Enum.GetUnderlyingType(enumType), null)),typeof(long),null);
								}
								catch
								{
									throw new ArgumentException("value is a name, but not one of the named constants defined for the enumeration.");
								}
								//
							}
						}
					}


					//return the total converted back to the correct enum type
					return System.Enum.ToObject(enumType, returnVal);
				}
				else
				{
					//the type supplied does not derive from enum
					throw new ArgumentException("enumType parameter is not an System.Enum");
				}				
			}
		}
		#endregion
#endif

#if CF || CF_2 || SL
		public static string[] GetNames(System.Type enumType)
		{
			if(enumType.BaseType==Type.GetType("System.Enum"))
			{
				//get the public static fields (members of the enum)
				System.Reflection.FieldInfo[] fi = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
			
				//create a new enum array
				string[] names = new string[fi.Length];

				//populate with the values
				for(int iEnum = 0; iEnum < fi.Length; iEnum++)
				{
					names[iEnum] = fi[iEnum].Name;
				}

				//return the array
				return names;
			}
			else
			{
				//the type supplied does not derive from enum
				throw new ArgumentException("enumType parameter is not an System.Enum");
			}
		}
#endif
		

	}
	
}
