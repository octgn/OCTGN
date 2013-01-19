using System;
using System.Collections;
using System.Globalization;
using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.rpc
{
    internal class RpcHelper
    {
        public static Element WriteParams(ArrayList Params)
        {
            if (Params != null && Params.Count > 0)
            {
                Element elParams = new Element("params");

                for (int i = 0; i < Params.Count; i++)
                {
                    Element param = new Element("param");
                    WriteValue(Params[i], param);
                    elParams.AddChild(param);
                }
                return elParams;
            }
            return null;
        }

        /// <summary>
        /// Writes a single value to a call
        /// </summary>
        /// <param name="param"></param>
        /// <param name="parent"></param>
        public static void WriteValue(object param, Element parent)
        {
            Element value = new Element("value");

            if (param is String)
            {
                value.AddChild(new Element("string", param as string));
            }
            else if (param is Int32)
            {
                value.AddChild(new Element("i4", ((Int32)param).ToString()));
            }
            else if (param is Double)
            {
                NumberFormatInfo numberInfo = new NumberFormatInfo();
                numberInfo.NumberDecimalSeparator = ".";
                //numberInfo.NumberGroupSeparator = ",";
                value.AddChild(new Element("double", ((Double)param).ToString(numberInfo)));
            }
            else if (param is Boolean)
            {
                value.AddChild(new Element("boolean", ((bool)param) ? "1" : "0"));
            }
            // XML-RPC dates are formatted in iso8601 standard, same as xmpp,
            else if (param is DateTime)
            {
                value.AddChild(new Element("dateTime.iso8601", Util.Time.ISO_8601Date((DateTime)param)));
            }
            // byte arrays must be encoded in Base64 encoding
            else if (param is byte[])
            {
                byte[] b = (byte[])param;
                value.AddChild(new Element("base64", Convert.ToBase64String(b, 0, b.Length)));
            }
            // Arraylist maps to an XML-RPC array
            else if (param is ArrayList)
            {
                //<array>  
                //    <data>
                //        <value>  <string>one</string>  </value>
                //        <value>  <string>two</string>  </value>
                //        <value>  <string>three</string></value>  
                //    </data> 
                //</array>
                Element array = new Element("array");
                Element data = new Element("data");

                ArrayList list = param as ArrayList;

                for (int i = 0; i < list.Count; i++)
                {
                    WriteValue(list[i], data);
                }

                array.AddChild(data);
                value.AddChild(array);
            }
            // java.util.Hashtable maps to an XML-RPC struct
            else if (param is Hashtable)
            {
                Element elStruct = new Element("struct");

                Hashtable ht = (Hashtable)param;
                IEnumerator myEnumerator = ht.Keys.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    Element member = new Element("member");
                    object key = myEnumerator.Current;

                    if (key != null)
                    {
                        member.AddChild(new Element("name", key.ToString()));
                        WriteValue(ht[key], member);
                    }

                    elStruct.AddChild(member);
                }

                value.AddChild(elStruct);
            }
            /*
            else
            {
                // Unknown Type
            }
            */
            parent.AddChild(value);
        }
    }
}
