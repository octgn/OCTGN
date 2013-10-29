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

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.x.data
{

	/*
	 * <x xmlns='jabber:x:data'
		type='{form-type}'>
		<title/>
		<instructions/>
		<field var='field-name'
				type='{field-type}'
				label='description'>
			<desc/>
			<required/>
			<value>field-value</value>
			<option label='option-label'><value>option-value</value></option>
			<option label='option-label'><value>option-value</value></option>
		</field>
		</x>
	*/

	/// <summary>
	/// 
	/// </summary>
	public class Field : Element
	{
		public Field()
		{
			this.TagName	= "field";
			this.Namespace	= Uri.X_DATA;		
		}
		
		public Field(FieldType type) : this()
		{
			this.Type = type;			
		}

		public Field(string var, string label, FieldType type) : this()
		{
			this.Type	= type;
			this.Var	= var;
			this.Label	= label;
		}

		#region << Properties >>
		public string Var
		{
			get { return GetAttribute("var"); }
			set { SetAttribute("var", value); }			
		}

		public string Label
		{
			get { return GetAttribute("label"); }
			set { SetAttribute("label", value); }
		}

		public FieldType Type
		{
			get
			{
				switch (GetAttribute("type"))
				{
					case "boolean":
						return FieldType.Boolean;
					case "fixed":
						return FieldType.Fixed;
					case "hidden":
						return FieldType.Hidden;
					case "jid-multi":
						return FieldType.Jid_Multi;
					case "jid-single":
						return FieldType.Jid_Single;
					case "list-multi":
						return FieldType.List_Multi;
					case "list-single":
						return FieldType.List_Single;
					case "text-multi":
						return FieldType.Text_Multi;
					case "text-private":
						return FieldType.Text_Private;
					case "text-single":
						return FieldType.Text_Single;
					default:
						return FieldType.Unknown;
				}
			}
			
			set
			{
				switch (value)
				{					
					case FieldType.Boolean:
						SetAttribute("type", "boolean");
						break;
					case FieldType.Fixed:
						SetAttribute("type", "fixed");
						break;
					case FieldType.Hidden:
						SetAttribute("type", "hidden");
						break;
					case FieldType.Jid_Multi:
						SetAttribute("type", "jid-multi");
						break;
					case FieldType.Jid_Single:
						SetAttribute("type", "jid-single");
						break;
					case FieldType.List_Multi:
						SetAttribute("type", "list-multi");
						break;
					case FieldType.List_Single:
						SetAttribute("type", "list-single");
						break;
					case FieldType.Text_Multi:
						SetAttribute("type", "text-multi");
						break;
					case FieldType.Text_Private:
						SetAttribute("type", "text-private");
						break;
					case FieldType.Text_Single:
						SetAttribute("type", "text-single");
						break;
					default:
						RemoveAttribute("type");
						break;
				}

			}
		}

		
		public string Description
		{
			get { return GetTag("desc"); }
			set { SetTag("desc", value); }
		}

		/// <summary>
		/// Is this field a required field?
		/// </summary>
		public bool IsRequired
		{
			get	{ return HasTag("required"); }
			set
			{
				if (value == true)
					SetTag("required");
				else
					RemoveTag("required");
			}
		}		
		#endregion

		#region << Methods and Functions >>
		/// <summary>
		/// The value of the field.
		/// </summary>
		public string GetValue()
		{
			return GetTag(typeof(Value));
			//return GetTag("value");			
		}

		public bool HasValue(string val)
		{
			foreach(string s in GetValues())
			{
				if(s == val)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public void SetValue(string val)
		{
			SetTag(typeof(Value), val);
		}

		/// <summary>
		/// Set the value of boolean fields
		/// </summary>
		/// <param name="val"></param>
		public void SetValueBool(bool val)
		{			
			SetValue( val ? "1" : "0" );			
		}
		
		/// <summary>
		/// Get the value of boolean fields
		/// </summary>
		/// <returns></returns>
		public bool GetValueBool()
		{
			// only "0" and "1" are valid. We dont care about other buggy implementations
			string val = GetValue();
			if (val == null || val == "0")
				return false;
			else
				return true;				
		}

		/// <summary>
		/// Returns the value as Jif for the Jid fields. 
		/// Or null when the value is not a valid Jid.
		/// </summary>
		/// <returns></returns>
		public Jid GetValueJid()
		{
			try
			{
				return new Jid(GetValue());
			}
			catch
			{
				return null;
			}			
		}

		/// <summary>
		/// Adds a value
		/// </summary>
		/// <remarks>
		/// you can call this function multiple times to add values to "multi" fields
		/// </remarks> 
		/// <param name="val"></param>
		public void AddValue(string val)
		{
			AddChild(new Value(val));
			//AddTag("value", val);
		}		
		
		/// <summary>
		/// Adds multiple values to the already existing values from a string array
		/// </summary>
		/// <param name="vals"></param>
		public void AddValues(string[] vals)
		{
            if (vals.Length > 0)
            {
                foreach (string s in vals)
                    AddValue(s);
            }
		}

		/// <summary>
		/// Adds multiple values. All already existing values will be removed
		/// </summary>
		/// <param name="vals"></param>
		public void SetValues(string[] vals)
		{
            ElementList nl = SelectElements(typeof(Value));
			
			foreach(Element e in nl)
				e.Remove();

			AddValues(vals);
		}

		/// <summary>
		/// Gets all values for multi fields as Array
		/// </summary>
		/// <returns>string Array that contains all the values</returns>
		public string[] GetValues()
		{
            ElementList nl = SelectElements(typeof(Value));
			string[] values = new string[nl.Count];
			int i = 0;
			foreach (Element val in nl)
			{
				values[i] = val.Value;
				i++;
			}
			return values;
		}

		public Option AddOption(string label, string val)
		{
			Option opt = new Option(label, val);
			AddChild(opt);
			return opt;
		}

		public Option AddOption()
		{
			Option opt = new Option();
			AddChild(opt);
			return opt;
		}

		public void AddOption(Option opt)
		{			
			AddChild(opt);
		}

		public Option[] GetOptions()
		{
            ElementList nl = SelectElements(typeof(Option));
			int i = 0;
			Option[] result = new Option[nl.Count];
			foreach (Option o in nl)
			{		
				result[i] = o;				
				i++;
			}
			return result;
		}
		#endregion

	}
	
}
