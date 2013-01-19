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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.IO;
using System.Xml;

namespace agsXMPP.Xml.Dom
{	

	public class Element : Node
	{
		// Member Variables
		private		string			m_TagName;
		private		string			m_Prefix;
        private     ListDictionary  m_Attributes;
		private		Text			m_Value				= new Text();

		public Element() 
		{
			NodeType = NodeType.Element;
			AddChild(m_Value);

            m_Attributes = new ListDictionary();

            m_TagName	= "";
			Value		= "";			
		}

		public Element(string tagName) :this() 
		{
            m_TagName = tagName;
		}

		public Element(string tagName, string tagText) : this(tagName)
		{
            Value		= tagText;			
		}

        public Element(string tagName, bool tagText) : this(tagName, tagText ? "true" : "false")
		{            
		}		

		public Element(string tagName, string tagText, string ns) : this(tagName, tagText)
		{
           Namespace		= ns;			
		}
				
		/// <summary>
		/// Is this Element a Rootnode?
		/// </summary>
		public bool IsRootElement
		{
			get
			{
				return (Parent != null ? false : true);
			}
		}

		public override string Value
		{
			get	{ return m_Value.Value;	}
			set	{ m_Value.Value = value; }
		}
		
		public string Prefix
		{
			get { return m_Prefix; }
			set { m_Prefix = value; }
		}

		/// <summary>
		/// The Full Qualified Name
		/// </summary>
		public string TagName 
		{
			get	{ return m_TagName;	}
			set { m_TagName = value; }
		}

		public string TextBase64
		{
			get 
			{
				byte[] b = Convert.FromBase64String(Value);
				return Encoding.ASCII.GetString(b, 0, b.Length);
			}
			set 
			{
                byte[] b = Encoding.UTF8.GetBytes(value);
				//byte[] b = Encoding.Default.GetBytes(value);
				Value = Convert.ToBase64String(b, 0, b.Length);
			}
		}
      
        public ListDictionary Attributes
        {
            get { return m_Attributes; }
        }

		public object GetAttributeEnum(string name, Type enumType)
		{
			string att = GetAttribute(name);
			if ((att == null))
				return -1;
			try
			{
#if CF
				return util.Enum.Parse(enumType, att, true);
#else
				return Enum.Parse(enumType, att, true);
#endif
			}
			catch (Exception)
			{
				return -1;
			}
		}

		public string GetAttribute(string name)
		{
		    if (HasAttribute(name))
				return (string) m_Attributes[name];
		    return null;
		}

	    public int GetAttributeInt(string name)
	    {
	        if (HasAttribute(name))
			{				
				return int.Parse((string) m_Attributes[name]);
			}
	        return 0;
	    }

	    public long GetAttributeLong(string name)
	    {
	        if (HasAttribute(name))
			{				
				return long.Parse((string) m_Attributes[name]);
			}
	        return 0;
	    }

	    /// <summary>
        /// Reads a boolean Attribute, if the attrib is absent it returns also false.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetAttributeBool(string name)
	    {
	        if (HasAttribute(name))
            {
                string tmp = (string) m_Attributes[name];
                if (tmp.ToLower() == "true")
                    return true;
                return false;
            }
	        return false;
	    }

	    public Jid GetAttributeJid(string name)
	    {
	        if (HasAttribute(name))
                return new Jid(this.GetAttribute(name));
	        return null;
	    }

	    /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ifp"></param>
        /// <returns></returns>
        public double GetAttributeDouble(string name, IFormatProvider ifp)
	    {
	        if (HasAttribute(name))
            {
                try
                {
                    return double.Parse((string)m_Attributes[name], ifp);
                }
                catch
                {
                    return double.NaN;
                }
            }
	        return double.NaN;
	    }

	    /// <summary>
        /// Get a Attribute of type double (Decimal seperator = ".")
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetAttributeDouble(string name)
        {
            // Parse the double always in english format ==> "." = Decimal seperator
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            return GetAttributeDouble(name, nfi); 
        }

		public bool HasAttribute(string name)
		{
			return Attributes.Contains(name);
		}
		
		/// <summary>
		/// Return the Text of the first Tag with a specified Name.
		/// It doesnt traverse the while tree and checks only the unerlying childnodes
		/// </summary>
		/// <param name="TagName">Name of Tag to find as string</param>
		/// <returns></returns>
		public string GetTag(string TagName)
		{
			Element tag = this._SelectElement(this, TagName);
			if ( tag != null)
				return tag.Value;
		    return null;
		}

		public string GetTag(string TagName, bool traverseChildren)
		{
			Element tag = this._SelectElement(this, TagName, traverseChildren);
			if ( tag != null)
				return tag.Value;
		    return null;
		}

		public string GetTag(System.Type type)
		{
			Element tag = this._SelectElement(this, type);
			if ( tag != null)
				return tag.Value;
		    return null;
		}

		public string GetTagBase64(string TagName)
		{
			byte[] b = Convert.FromBase64String(GetTag(TagName));
			return Encoding.ASCII.GetString(b, 0, b.Length);
		}
		
		/// <summary>
		/// Adds a Tag and encodes the Data to BASE64
		/// </summary>
		/// <param name="argTagname"></param>
		/// <param name="argText"></param>
		public void SetTagBase64(string argTagname, string argText)
		{
			byte[] b = Encoding.Unicode.GetBytes(argText);
			SetTag(argTagname, Convert.ToBase64String(b, 0, b.Length));
		}

		/// <summary>
		/// Adds a Tag end decodes the byte buffer to BASE64
		/// </summary>
		/// <param name="argTagname"></param>
		/// <param name="buffer"></param>
		public void SetTagBase64(string argTagname, byte[] buffer)
		{
			SetTag(argTagname, Convert.ToBase64String(buffer, 0, buffer.Length));
		}
		
		public void SetTag(string argTagname, string argText)
		{
			if (HasTag(argTagname) == false)
				AddChild(new Element(argTagname, argText));
			else
				SelectSingleElement(argTagname).Value = argText;
		}

		public void SetTag(Type type, string argText)
		{
			if (HasTag(type) == false)
			{
				Element newel;
				newel		= (Element) Activator.CreateInstance(type);
				newel.Value = argText;
				AddChild(newel);
			}
			else
				SelectSingleElement(type).Value = argText;
		}

		public void SetTag(Type type)
		{
			if (HasTag(type))			
				RemoveTag(type);
			
			AddChild( (Element) Activator.CreateInstance(type) );
		}

		public void SetTag(string argTagname)
		{
			SetTag(argTagname, "");
		}
                
		public void SetTag(string argTagname, string argText, string argNS)
		{
			if (HasTag(argTagname) == false)				
				AddChild(new Element(argTagname, argText, argNS));			
			else
			{
				Element e = SelectSingleElement(argTagname);
				e.Value		= argText;
				e.Namespace = argNS;
			}
		}

        public void SetTag(string argTagname, double dbl, IFormatProvider ifp)
        {
            SetTag(argTagname, dbl.ToString(ifp));
        }

        public void SetTag(string argTagname, double dbl)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            SetTag(argTagname, dbl, nfi);
        }

        public void SetTag(string argTagname, bool val)
        {
            SetTag(argTagname, val ? "true" : "false");
        }

        public void SetTag(string argTagname, int val)
        {
            SetTag(argTagname, val.ToString());
        }

        public void SetTag(string argTagname, Jid jid)
        {
            SetTag(argTagname, jid.ToString());
        }

		public void AddTag(string argTagname, string argText)
		{			
			AddChild(new Element(argTagname, argText));			
		}

		public void AddTag(string argTagname)
		{			
			AddChild(new Element(argTagname));			
		}

		public object GetTagEnum(string name, System.Type enumType)
		{			
			string tag = this.GetTag(name);
			if ( (tag == null) || (tag.Length == 0) )
				return -1;
			try
			{
#if CF
				return util.Enum.Parse(enumType, tag, true);
#else
				return Enum.Parse(enumType, tag, true);
#endif
			}
			catch (Exception)
			{
				return -1;
			}
		}

		/// <summary>
		/// Return the Text of the first Tag with a specified Name in all childnodes as boolean
		/// </summary>
		/// <param name="TagName">name of Tag to findas string</param>
		/// <returns></returns>
		public bool GetTagBool(string TagName)
		{
			Element tag = this._SelectElement(this, TagName);
			if ( tag != null)
			{
			    if (tag.Value.ToLower() == "false" || tag.Value.ToLower() == "0")
				{
					return false;
				}
			    if(tag.Value.ToLower() == "true" ||	tag.Value.ToLower() == "1")
			    {
			        return true;
			    }
			    return false;
			}
		    return false;
		}

		public int GetTagInt(string TagName)
		{
			Element tag = _SelectElement(this, TagName);
			if ( tag != null)
				return int.Parse(tag.Value);
		    return 0;
		}


        public Jid GetTagJid(string TagName)
        {
            string jid = GetTag(TagName);
            
            if (jid != null)
                return new Jid(jid);
            return null;
        }         
         
        
        /// <summary>
        /// Get a Tag of type double (Decimal seperator = ".")
        /// </summary>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public double GetTagDouble(string argTagName)
        {
            // Parse the double always in english format ==> "." = Decimal seperator
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            
            return GetTagDouble(argTagName, nfi);
        }

        /// <summary>
        /// Get a Tag of type double with the given iFormatProvider
        /// </summary>
        /// <param name="TagName"></param>
        /// <param name="nfi"></param>
        /// <returns></returns>
        public double GetTagDouble(string argTagName, IFormatProvider ifp)
        {           
            string val = GetTag(argTagName);
            if (val != null)
                return Double.Parse(val, ifp);
            return Double.NaN;
        }

		public bool HasTag(string name)
		{
			Element tag = _SelectElement(this, name);
			if ( tag != null)
				return true;
		    return false;
		}

		public bool HasTag(string name, bool traverseChildren)
		{
			Element tag = _SelectElement(this, name, traverseChildren);
			if ( tag != null)
				return true;
		    return false;
		}		

		public bool HasTag(Type type)
		{
			Element tag = _SelectElement(this, type);
			if ( tag != null)
				return true;
		    return false;
		}

        public bool HasTag<T>() where T : Element
        {
            return SelectSingleElement<T>() != null;
        }

        public bool HasTagt<T>(bool traverseChildren) where T : Element
        {
            return SelectSingleElement<T>(traverseChildren) != null;
        }


		public bool HasTag(Type type, bool traverseChildren)
		{
			Element tag = this._SelectElement(this, type, traverseChildren);
			if ( tag != null)
				return true;
		    return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enumType"></param>
		/// <returns></returns>		
        public object HasTagEnum(Type enumType)
		{			
#if CF || CF_2            
			string[] members = Util.Enum.GetNames(enumType);	
#else
			string[] members = Enum.GetNames(enumType);
#endif
			foreach (string member in members)
			{
				if (HasTag(member))
#if CF
					return util.Enum.Parse(enumType, member, false);
#else
					return Enum.Parse(enumType, member, false);
#endif
			}
			return -1;			
		}

		/// <summary>
		/// Remove a Tag when it exists
		/// </summary>
		/// <param name="TagName">Tagname to remove</param>
		/// <returns>true when existing and removed, false when not existing</returns>
		public bool RemoveTag(string TagName)
		{
			Element tag = _SelectElement(this, TagName);
			if ( tag != null)
			{
				tag.Remove();
				return true;
			}
		    return false;
		}

		/// <summary>
		/// Remove a Tag when it exists
		/// </summary>
		/// <param name="type">Type of the tag that should be removed</param>
		/// <returns>true when existing and removed, false when not existing</returns>
		public bool RemoveTag(Type type)
		{
			Element tag = _SelectElement(this, type);
			if (tag != null)
			{
				tag.Remove();
				return true;
			}
			
            return false;
		}

        public bool RemoveTag<T>() where T : Element
        {
            Element tag = SelectSingleElement<T>();
            if (tag != null)
            {
                tag.Remove();
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Removes all Tags of the given type. Doesnt traverse the tree
        /// </summary>
        /// <param name="type">Type of the tags that should be removed</param>
        /// <returns>true when tags were removed, false when no tags were found and removed</returns>
        public bool RemoveTags(Type type)
        {
            bool ret = false;

            ElementList list = SelectElements(type);
            
            if (list.Count > 0)
                ret = true;

            foreach (Element e in list)
                e.Remove();

            return ret;
        }

        /// <summary>
        /// Removes all Tags of the given type. Doesnt traverse the tree
        /// </summary>
        /// <typeparam name="T">Type of the tags that should be removed</typeparam>
        /// <returns>true when tags were removed, false when no tags were found and removed</returns>
        public bool RemoveTags<T>() where T : Element
        {
            return RemoveTags(typeof (T));
        }

		/// <summary>
		/// Same as AddChild, but Replaces the childelement when it exists
		/// </summary>
		/// <param name="e"></param>
		public void ReplaceChild(Element e)
		{
			if (HasTag(e.TagName))
				RemoveTag(e.TagName);
			
			AddChild(e);
		}

		public string Attribute(string name) 
		{
			return (string) m_Attributes[name];
		}

		/// <summary>
		/// Removes a Attribute
		/// </summary>
		/// <param name="name">Attribute as string to remove</param>
		public void RemoveAttribute(string name)
		{
			if (HasAttribute(name))
			{
				Attributes.Remove(name);
			}
		}

		/// <summary>
		/// Adds a new Attribue or changes a Attriv when already exists
		/// </summary>
		/// <param name="name">name of Attribute to add/change</param>
		/// <param name="value">value of teh Attribute to add/change</param>
		public void SetAttribute(string name, string val) 
		{
			// When the attrib already exists then we overweite it
			// So we must remove it first and add it again then
			if (HasAttribute(name))
			{
				Attributes.Remove(name);
			}
			m_Attributes.Add(name, val);
			
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
		public void SetAttribute(string name, int value)
		{
			SetAttribute(name, value.ToString());
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetAttribute(string name, long value)
        {
            SetAttribute(name, value.ToString());
        }

        /// <summary>
        /// Writes a boolean attribute, the value is either 'true' or 'false'
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public void SetAttribute(string name, bool val)
        {
            // When the attrib already exists then we overweite it
            // So we must remove it first and add it again then
            if (HasAttribute(name))
            {
                Attributes.Remove(name);
            }
            m_Attributes.Add(name, val ? "true" : "false");
        }

        /// <summary>
        /// Set a attribute of type Jid
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetAttribute(string name, Jid value)
        {
             if (value != null)
                    SetAttribute(name, value.ToString());
                else
                    RemoveAttribute(name);
        }

        /// <summary>
        /// Set a attribute from a double in english number format
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetAttribute(string name, double value)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberGroupSeparator = ".";
            SetAttribute(name, value, nfi);
        }

        /// <summary>
        /// Set a attribute from a double with the given Format provider
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="ifp"></param>
        public void SetAttribute(string name, double value, IFormatProvider ifp)
        {
            SetAttribute(name, value.ToString(ifp));
        }


		public void SetNamespace(string value) 
		{
			SetAttribute("xmlns", value);	
		}

        private CData GetFirstCDataNode()
        {
            foreach (Node ch in ChildNodes)
            {
                if (ch.NodeType == NodeType.Cdata)
                    return ch as CData;
            }
            return null;
        }

        /// <summary>
        /// Has this Element some CDATA?
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return GetFirstCDataNode() != null;
        }

        /// <summary>
        /// Get the CDATA
        /// </summary>
        /// <returns></returns>
	    public string GetData()
	    {
	        var data = GetFirstCDataNode();
	        return data == null ? null : data.Value;
	    }

        /// <summary>
        /// Set the CDATA
        /// </summary>
        /// <param name="cdata"></param>
        public void SetData(string cdata)
        {
            var data = GetFirstCDataNode();
            if (data == null)
            {
                data = new CData();
                AddChild(data);
            }
            data.Value = cdata;
        }

	    public string InnerXml
		{
			get
			{
			    if (ChildNodes.Count > 0)
				{
					string xml = "";
					try
					{						
						for (int i = 0; i < ChildNodes.Count; i++)
						{
							if (ChildNodes.Item(i).NodeType == NodeType.Element)
								xml += ChildNodes.Item(i).ToString();
							else if (ChildNodes.Item(i).NodeType == NodeType.Text)
								xml += ChildNodes.Item(i).Value;
						
						}
					}
					catch (Exception)
					{
					}
					return xml;

				}
			    return null;
			}
		    set
            {
                Document doc = new Document();
                doc.LoadXml(value);
                Element root = doc.RootElement;
                if (root != null)
                {
                    ChildNodes.Clear();
                    AddChild(root);
                }
                
            }
		}

        /// <summary>
        /// returns whether the current element has child elements or not.
        /// cares only about element, not text nodes etc...
        /// </summary>
        public bool HasChildElements
        {
	        get
	        {
                foreach (Node e in ChildNodes)
                {
                    if (e.NodeType == NodeType.Element)
                        return true;
                }
		        return false;	     
	        }
        }

		/// <summary>
		/// returns the first child element (no textNodes)
		/// </summary>
		public Element FirstChild
		{
			get
			{
			    if (ChildNodes.Count > 0)
                {
                    foreach (Node e in ChildNodes)
                    {
                        if (e.NodeType == NodeType.Element)
                            return e as Element;
                    }
                    return null;
	            }
			    return null;
			}
		}

		/// <summary>
		/// Returns the first ChildNode, doesnt matter of which type it is
		/// </summary>
		public Node FirstNode
		{
			get
			{
			    if(ChildNodes.Count > 0)
					return ChildNodes.Item(0) as Node;
			    return null;
			}
		}

		/// <summary>
		/// Returns the last ChildNode, doesnt matter of which type it is
		/// </summary>
		public Node LastNode
		{
			get
			{
			    if(ChildNodes.Count > 0)
					return ChildNodes.Item(ChildNodes.Count -1) as Node;
			    return null;
			}
		}

        internal string StartTag()
        {
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tw = new XmlTextWriter(sw))
                {
                    tw.Formatting = Formatting.None;

                    if (Prefix == null)
                        tw.WriteStartElement(TagName);
                    else
                        tw.WriteStartElement(Prefix + ":" + TagName);

                    // Write Namespace
                    if (Namespace != null
                        && Namespace.Length != 0
                        )
                    {
                        if (Prefix == null)
                            tw.WriteAttributeString("xmlns", Namespace);
                        else
                            tw.WriteAttributeString("xmlns:" + Prefix, Namespace);
                    }

                    foreach (string attName in this.Attributes.Keys)
                    {
                        tw.WriteAttributeString(attName, Attribute(attName));
                    }

                    tw.Flush();
                    tw.Close();

                    return sw.ToString().Replace("/>", ">");
                }
            }
        }

	    internal string EndTag()
	    {
	        if (Prefix == null)
                return "</" + TagName + ">";
	        return "</" + Prefix + ":" + TagName + ">";
	    }

	    #region << Xml Select Functions >>
        /// <summary>
        /// Find a Element by type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
		public Element SelectSingleElement(System.Type type)
		{
			return _SelectElement(this, type);
		}

        /// <summary>
        /// find a Element by type and loop thru all children
        /// </summary>
        /// <param name="type"></param>
        /// <param name="loopChildren"></param>
        /// <returns></returns>
        public Element SelectSingleElement(System.Type type, bool loopChildren)
        {
            return _SelectElement(this, type, true);
        }

		public Element SelectSingleElement(string TagName)
		{
			return _SelectElement(this, TagName);
		}

        public Element SelectSingleElement(string TagName, bool traverseChildren)
        {
            return _SelectElement(this, TagName, true);
        }

		public Element SelectSingleElement(string TagName, string AttribName, string AttribValue)
		{
			return _SelectElement(this, TagName, AttribName, AttribValue);
		}

		public Element SelectSingleElement(string TagName, string ns)
		{
            return _SelectElement(this, TagName, ns, true);
		}

        public Element SelectSingleElement(string TagName, string ns, bool traverseChildren)
        {            
            return _SelectElement(this, TagName, ns, traverseChildren);
        }
        
        public T SelectSingleElement<T>() where T : Element
        {
            return (T)_SelectElement(this, typeof(T));
        }

        public T SelectSingleElement<T>(bool traverseChildren) where T : Element
        {
            return (T)_SelectElement(this, typeof(T), traverseChildren);
        } 

		/// <summary>
		/// Returns all childNodes with the given Tagname,
		/// this function doesn't traverse the whole tree!!!
		/// </summary>
		/// <param name="TagName"></param>
		/// <returns></returns>
		public ElementList SelectElements(string TagName)
		{
            ElementList es = new ElementList();
			//return this._SelectElements(this, TagName, es);
			return _SelectElements(this, TagName, es, false);
		}

        public ElementList SelectElements(string TagName, bool traverseChildren)
        {
            ElementList es = new ElementList();
            //return this._SelectElements(this, TagName, es);
            return _SelectElements(this, TagName, es, traverseChildren);
        }

        public ElementList SelectElements(System.Type type)
		{
            ElementList es = new ElementList();
			return _SelectElements(this, type, es);
		}

		/// <summary>
		/// returns a nodelist of all found nodes of the given Type
		/// </summary>
		/// <param name="e"></param>
		/// <param name="type"></param>
		/// <param name="es"></param>
		/// <returns></returns>
        private ElementList _SelectElements(Element e, Type type, ElementList es)
		{			
			return _SelectElements(e, type, es, false);	
		}

        private ElementList _SelectElements(Element e, Type type, ElementList es, bool traverseChildren)
		{						
			if (e.ChildNodes.Count > 0) 
			{		
				foreach(Node n in e.ChildNodes) 
				{
					if (n.NodeType == NodeType.Element)
					{
                        if (n.GetType() == type)                        
						{
							es.Add(n);
						}
						if (traverseChildren)
							_SelectElements((Element) n, type, es, true);
					}						            
				}
			}								
			return es;
		}

		/// <summary>
		/// Select a single element.
		/// This function doesnt traverse the whole tree and checks only the underlying childnodes
		/// </summary>
		/// <param name="se"></param>
		/// <param name="tagname"></param>
		/// <returns></returns>
		private Element _SelectElement(Node se, string tagname) 
		{
			return _SelectElement(se, tagname, false);
		}

		/// <summary>
		/// Select a single element
		/// </summary>
		/// <param name="se"></param>
		/// <param name="tagname"></param>
		/// <param name="traverseChildren">when set to true then the function traverses the whole tree</param>
		/// <returns></returns>
		private Element _SelectElement(Node se, string tagname, bool traverseChildren) 
		{
			Element rElement = null;			
					
			if (se.ChildNodes.Count > 0) 
			{
				foreach(Node ch in se.ChildNodes) 
				{
					if (ch.NodeType == NodeType.Element)
					{
						if ( ((Element) ch).TagName == tagname )
						{
							rElement = (Element) ch;
							return rElement;
						}
						else
						{
							if( traverseChildren)
							{
								rElement = _SelectElement(ch, tagname, true);
								if (rElement != null)
									break;
							}
						}
					}
				}				
			}    
						
			return rElement;
		}


		private Element _SelectElement(Node se, System.Type type)
		{
			return _SelectElement(se, type, false);
		}

		private Element _SelectElement(Node se, System.Type type, bool traverseChildren)
		{
			Element rElement = null;			
					
			if (se.ChildNodes.Count > 0) 
			{
				foreach(Node ch in se.ChildNodes) 
				{                    
					if (ch.NodeType == NodeType.Element)
					{                        
						if ( ch.GetType() == type )
						{
							rElement = (Element) ch;
							return rElement;
						}
						else
						{
							if( traverseChildren)
							{
								rElement = _SelectElement(ch, type, true);
								if (rElement != null)
									break;
							}
						}
					}
				}				
			}    						
			return rElement;
		}

		private Element _SelectElement(Node se, string tagname, string AttribName, string AttribValue) 
		{
			Element rElement = null;
						
			if (se.NodeType == NodeType.Element)
			{
				Element e = se as Element;
				if (e.m_TagName == tagname)
				{
					if (e.HasAttribute(AttribName))
					{
						if (e.GetAttribute(AttribName) == AttribValue)
						{
							rElement = e;
							return rElement;
						}
					}
				}
			}		
	
			if (se.ChildNodes.Count > 0) 
			{
				foreach(Node ch in se.ChildNodes) 
				{
					rElement = _SelectElement(ch, tagname, AttribName, AttribValue);            
					if (rElement != null)
						break;
				}				
			}    		
			
			return rElement;
		}

        /// <summary>
        /// Find Element by Namespace
        /// </summary>
        /// <param name="se">The se.</param>
        /// <param name="tagname">The tagname.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="traverseChildren">if set to <c>true</c> [traverse children].</param>
        /// <returns></returns>
        private Element _SelectElement(Node se, string tagname, string nameSpace, bool traverseChildren)
        {
            Element rElement = null;

            if (se.ChildNodes.Count > 0)
            {
                foreach (Node ch in se.ChildNodes)
                {
                    if (ch.NodeType == NodeType.Element)
                    {
                        Element e = ch as Element;
                        if (e.TagName == tagname && e.Namespace == nameSpace)
                        {
                            rElement = (Element)ch;
                            return rElement;
                        }
                        else
                        {
                            if (traverseChildren)
                            {
                                rElement = _SelectElement(ch, tagname, nameSpace, traverseChildren);
                                if (rElement != null)
                                    break;
                            }
                        }
                    }
                }
            }
            return rElement;
        }

        private ElementList _SelectElements(Element e, string tagname, ElementList es, bool traverseChildren) 
		{
			if (e.ChildNodes.Count > 0) 
			{		
				foreach(Node n in e.ChildNodes) 
				{
					if (n.NodeType == NodeType.Element)
					{
						if ( ((Element) n).m_TagName == tagname)
						{
							es.Add(n);
						}
						if (traverseChildren)
							_SelectElements((Element) n, tagname, es, true);
					}
									
				}
			}
			return es;
        }


        public List<T> SelectElements<T>() where T : Element
        {
            return SelectElements<T>(false);
        }

        public List<T> SelectElements<T>(bool traverseChildren) where T : Element
        {
            List<T> list = new List<T>();
            return this._SelectElements<T>(this, list, traverseChildren);
        }

        private List<T> _SelectElements<T>(Element e, List<T> list, bool traverseChildren) where T : Element
        {
            if (e.ChildNodes.Count > 0)
            {
                foreach (Node n in e.ChildNodes)
                {
                    if (n.NodeType == NodeType.Element)
                    {
                        if (n.GetType() == typeof(T))
                        {
                            list.Add(n as T);
                        }
                        if (traverseChildren)
                            _SelectElements((Element)n, list, true);
                    }
                }
            }
            return list;
        }
        #endregion
    }
}