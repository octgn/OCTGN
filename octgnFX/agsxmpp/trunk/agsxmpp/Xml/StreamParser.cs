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
using System.IO;
using System.Threading;
using System.Collections;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;

using agsXMPP.Factory;

using agsXMPP.Xml.Xpnet;

using agsXMPP.protocol.client;

namespace agsXMPP.Xml
{	
	public delegate void StreamError		(object sender, Exception ex);
	public delegate void StreamHandler		(object sender, Node e);
    		
	/// <summary>
	/// Stream Parser is a lighweight Streaming XML Parser.
	/// </summary>
	public class StreamParser 
	{		
		// Stream Event Handlers
		public event StreamHandler		OnStreamStart;
		public event StreamHandler		OnStreamEnd;
		public event StreamHandler		OnStreamElement;
		
        /// <summary>
        /// Event for XML-Stream errors
        /// </summary>
        public event StreamError		OnStreamError;
        
        /// <summary>
        /// Event for general errors
        /// </summary>
        public event ErrorHandler       OnError;
			
		private int						m_Depth;
		private Node					m_root;
	    private Element                 current;
		
		private static System.Text.Encoding utf = System.Text.Encoding.UTF8;
		private Encoding        m_enc               = new UTF8Encoding();
		private BufferAggregate m_buf               = new BufferAggregate();
		private NamespaceStack  m_NamespaceStack    = new NamespaceStack();        
		private bool            m_cdata;

		public StreamParser()
		{
		}
	
		/// <summary>
		/// Reset the XML Stream
		/// </summary>
		public void Reset()
		{
			m_Depth		= 0;
			m_root		= null;			
			current		= null;
			m_cdata		= false;
		
			m_buf   = null;
			m_buf	= new BufferAggregate();
			
			//m_buf.Clear(0);
			m_NamespaceStack.Clear();		
		}

		/// <summary>
		/// Reset the XML Stream
		/// </summary>
		/// <param name="sr">new Stream that is used for parsing</param>
		public long Depth
		{
			get { return m_Depth; }
		}

        private Object thisLock = new Object();

		/// <summary>
		/// Put bytes into the parser.
		/// </summary>
		/// <param name="buf">The bytes to put into the parse stream</param>
		/// <param name="offset">Offset into buf to start at</param>
		/// <param name="length">Number of bytes to write</param>
		public void Push(byte[] buf, int offset, int length)
		{
            
            // or assert, really, but this is a little nicer.
            if (length == 0)
                return;

            // No locking is required.  Read() won't get called again
            // until this method returns.

            // TODO: only do this copy if we have a partial token at the
            // end of parsing.
            byte[] copy = new byte[length];
            System.Buffer.BlockCopy(buf, offset, copy, 0, length);
            m_buf.Write(copy);

            byte[] b = m_buf.GetBuffer();
            int off = 0;
            TOK tok = TOK.END_TAG;
            ContentToken ct = new ContentToken();
            try
            {
                while (off < b.Length)
                {
                    if (m_cdata)
                        tok = m_enc.tokenizeCdataSection(b, off, b.Length, ct);
                    else
                        tok = m_enc.tokenizeContent(b, off, b.Length, ct);

                    if (tok == TOK.PARTIAL) break;
                    switch (tok)
                    {
                        case TOK.EMPTY_ELEMENT_NO_ATTS:
                        case TOK.EMPTY_ELEMENT_WITH_ATTS:
                            StartTag(b, off, ct, tok);
                            EndTag(b, off, ct, tok);
                            break;
                        case TOK.START_TAG_NO_ATTS:
                        case TOK.START_TAG_WITH_ATTS:
                            StartTag(b, off, ct, tok);
                            break;
                        case TOK.END_TAG:
                            EndTag(b, off, ct, tok);
                            break;
                        case TOK.DATA_CHARS:
                        case TOK.DATA_NEWLINE:
                            AddText(utf.GetString(b, off, ct.TokenEnd - off));
                            break;
                        case TOK.CHAR_REF:
                        case TOK.MAGIC_ENTITY_REF:
                            AddText(new string(new char[] { ct.RefChar1 }));
                            break;
                        case TOK.CHAR_PAIR_REF:
                            AddText(new string(new char[] {ct.RefChar1,
															ct.RefChar2}));
                            break;
                        case TOK.COMMENT:
                            if (current != null)
                            {
                                // <!-- 4
                                //  --> 3
                                int start = off + 4 * m_enc.MinBytesPerChar;
                                int end = ct.TokenEnd - off -
                                    7 * m_enc.MinBytesPerChar;
                                string text = utf.GetString(b, start, end);
                                current.AddChild(new Comment(text));
                            }
                            break;
                        case TOK.CDATA_SECT_OPEN:
                            m_cdata = true;
                            break;
                        case TOK.CDATA_SECT_CLOSE:
                            m_cdata = false;
                            break;
                        case TOK.XML_DECL:
                            // thou shalt use UTF8, and XML version 1.
                            // i shall ignore evidence to the contrary...

                            // TODO: Throw an exception if these assuptions are
                            // wrong
                            break;
                        case TOK.ENTITY_REF:
                        case TOK.PI:
#if CF
					    throw new util.NotImplementedException("Token type not implemented: " + tok);
#else
                        throw new System.NotImplementedException("Token type not implemented: " + tok);
#endif                            
                    }                    
                    off = ct.TokenEnd;
                }
            }
            catch (PartialTokenException)
            {
                // ignored;
            }
            catch (ExtensibleTokenException)
            {
                // ignored;
            }
            catch (Exception ex)
            {
                if (OnStreamError != null)
                    OnStreamError(this, ex);
            }
            finally
            {
                m_buf.Clear(off);
            }            
		}		
		
		private void StartTag(byte[] buf, int offset,
			ContentToken ct, TOK tok)
		{
			m_Depth++;
			int colon;
			string name;
			string prefix;
			Hashtable ht = new Hashtable();
            
			m_NamespaceStack.Push();
            
			// if i have attributes
			if ((tok == TOK.START_TAG_WITH_ATTS) ||
				(tok == TOK.EMPTY_ELEMENT_WITH_ATTS))
			{
				int start;
				int end;
				string val;
				for (int i=0; i<ct.getAttributeSpecifiedCount(); i++)
				{                    
					start =  ct.getAttributeNameStart(i);
					end = ct.getAttributeNameEnd(i);
					name = utf.GetString(buf, start, end - start);
                    
					start = ct.getAttributeValueStart(i);
					end =  ct.getAttributeValueEnd(i);
					//val = utf.GetString(buf, start, end - start);
                    val = NormalizeAttributeValue(buf, start, end - start);
                    // <foo b='&amp;'/>
					// <foo b='&amp;amp;'
					// TODO: if val includes &amp;, it gets double-escaped
					if (name.StartsWith("xmlns:"))
					{
						colon = name.IndexOf(':');
						prefix = name.Substring(colon+1);
						m_NamespaceStack.AddNamespace(prefix, val);
					}
					else if (name == "xmlns")
					{
                        m_NamespaceStack.AddNamespace(string.Empty, val);						
					}
					else
					{
						ht.Add(name, val);
					}
				}
			}

			name = utf.GetString(buf,
				offset + m_enc.MinBytesPerChar,
				ct.NameEnd - offset - m_enc.MinBytesPerChar);
			
            colon = name.IndexOf(':');
			string ns = "";
			prefix = null;
			if (colon > 0)
			{
				prefix = name.Substring(0, colon);
				name = name.Substring(colon + 1);
				ns = m_NamespaceStack.LookupNamespace(prefix);
			}
			else
			{
				ns = m_NamespaceStack.DefaultNamespace;
			}
            			
			Element newel = ElementFactory.GetElement(prefix, name, ns);
			
			foreach (string attrname in ht.Keys)
			{
				newel.SetAttribute(attrname, (string)ht[attrname]);                
			}
            
			if (m_root == null)
			{
				m_root = newel;
				//FireOnDocumentStart(m_root);
				if (OnStreamStart!=null)
					OnStreamStart(this, m_root);
			}
			else
			{
				if (current != null)
					current.AddChild(newel);
				current = newel;
			}
		}

		private void EndTag(byte[] buf, int offset,	ContentToken ct, TOK tok)
		{
			m_Depth--;
			m_NamespaceStack.Pop();

			if (current == null)
			{// end of doc
				if (OnStreamEnd!=null)
					OnStreamEnd(this, m_root);
//				FireOnDocumentEnd();
				return;
			}

			string name = null;

			if ((tok == TOK.EMPTY_ELEMENT_WITH_ATTS) ||
				(tok == TOK.EMPTY_ELEMENT_NO_ATTS))
				name = utf.GetString(buf,
					offset + m_enc.MinBytesPerChar,
					ct.NameEnd - offset -
					m_enc.MinBytesPerChar);
			else
				name = utf.GetString(buf,
					offset + m_enc.MinBytesPerChar*2,
					ct.NameEnd - offset -
					m_enc.MinBytesPerChar*2);
                

//			if (current.Name != name)
//				throw new Exception("Invalid end tag: " + name +
//					" != " + current.Name);

			Element parent = (Element) current.Parent;
			if (parent == null)
            {
                DoRaiseOnStreamElement(current);
                //if (OnStreamElement!=null)
                //    OnStreamElement(this, current);
				//FireOnElement(current);
			}
			current = parent;
		}

        /// <summary>
        /// If users didnt use the library correctly and had no local error handles
        /// it always crashed here and disconencted the socket.
        /// Catch this errors here now and foreward them.
        /// </summary>
        /// <param name="el"></param>
        internal void DoRaiseOnStreamElement(Element el)
        {
            try
            {
                if (OnStreamElement != null)
                    OnStreamElement(this, el);
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(this, ex);                
            }
        }

        private string NormalizeAttributeValue(byte[] buf, int offset, int length)
        {            
            if (length == 0)
                return null;

            string val = null;
            BufferAggregate buffer = new BufferAggregate();
            byte[] copy = new byte[length];
            System.Buffer.BlockCopy(buf, offset, copy, 0, length);
            buffer.Write(copy);
            byte[] b = buffer.GetBuffer();
            int off = 0;
            TOK tok = TOK.END_TAG;
            ContentToken ct = new ContentToken();
            try
            {
                while (off < b.Length)
                {                  
                    //tok = m_enc.tokenizeContent(b, off, b.Length, ct);
                    tok = m_enc.tokenizeAttributeValue(b, off, b.Length, ct);

                    switch (tok)
                    {
                        case TOK.ATTRIBUTE_VALUE_S:
                        case TOK.DATA_CHARS:
                        case TOK.DATA_NEWLINE:                            
                            val += (utf.GetString(b, off, ct.TokenEnd - off));
                            break;
                        case TOK.CHAR_REF:
                        case TOK.MAGIC_ENTITY_REF:                        
                            val += new string(new char[] { ct.RefChar1 });
                            break;
                        case TOK.CHAR_PAIR_REF:                            
                            val += new string(new char[] {ct.RefChar1, ct.RefChar2});
                            break;                        
                        case TOK.ENTITY_REF:      
#if CF
						    throw new util.NotImplementedException("Token type not implemented: " + tok);
#else
                            throw new System.NotImplementedException("Token type not implemented: " + tok);
#endif
                    }
                    off = ct.TokenEnd;
                }
            }
            catch (PartialTokenException)
            {
                // ignored;
            }
            catch (ExtensibleTokenException)
            {
                // ignored;
            }
            catch (Exception ex)
            {
                if (OnStreamError != null)
                    OnStreamError(this, ex);
            }
            finally
            {
                buffer.Clear(off);               
            }
            return val;
        }

		private void AddText(string text)
		{
			if (text == "")
				return;

            //Console.WriteLine("AddText:" + text);
            //Console.WriteLine(lastTOK);

			if (current != null)
			{		
                if (m_cdata)
                {
                    Node last = current.LastNode;
                    if (last != null && last.NodeType == NodeType.Cdata)
                        last.Value = last.Value + text;
                    else
                        current.AddChild(new CData(text));
                }
                else
                {
				    Node last = current.LastNode;
				    if (last != null && last.NodeType == NodeType.Text)
					    last.Value = last.Value + text;
				    else
					    current.AddChild(new Text(text));
                }
			}
            else
			{
			    // text in root element
                Node last = ((Element)m_root).LastNode;
                if (m_cdata)
                {
                    if (last != null && last.NodeType == NodeType.Cdata)
                        last.Value = last.Value + text;
                    else
                        m_root.AddChild(new CData(text));
                } else
                {
                    if (last != null && last.NodeType == NodeType.Text)
                        last.Value = last.Value + text;
                    else
                        m_root.AddChild(new Text(text));
                }
			}
		}
	}
}