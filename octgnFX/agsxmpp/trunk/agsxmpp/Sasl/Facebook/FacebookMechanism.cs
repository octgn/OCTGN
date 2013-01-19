/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2009 by AG-Software 											 *
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
using System.Text;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.sasl;

namespace agsXMPP.Sasl.Facebook
{
    public class FacebookMechanism : Mechanism
    {
        private readonly string callId = new Random().Next().ToString();
        private const string VERSION = "1.0";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="con"></param>
        public override void Init(XmppClientConnection con)
        {
            con.Send(new Auth(MechanismType.X_FACEBOOK_PLATFORM));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public override void Parse(Node e)
        {
            if (e is Challenge)
            {
                var c = e as Challenge;
                
                byte[] bytes = Convert.FromBase64String(c.Value);
                string msg = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                var pairs = ParseMessage(msg);
                string res = BuildResponse(pairs);

                XmppClientConnection.Send(new Response(ToB64String(res)));
            }
        }

        static string ToB64String(string sin)
        {
            byte[] msg = Encoding.UTF8.GetBytes(sin);
            return Convert.ToBase64String(msg, 0, msg.Length);
        }

        private static Dictionary<string, string> ParseMessage(string msg)
        {
            // example:
            // version=1&method=auth.xmpp_login&nonce=4346B9BFC5A160D46AF25732ACFC7CC3
            var str = msg.Split('&');

            var dict = new Dictionary<string, string>();
            foreach (string s in str)
            {
                int equalPos = s.IndexOf('=');

                var key = s.Substring(0, equalPos - 0);
                var val = s.Substring(equalPos + 1);
                val = System.Uri.UnescapeDataString(val);
                if (!dict.ContainsKey(key))
                    dict.Add(key, val);
            }
            return dict;
        }

        private string BuildResponse(Dictionary<string, string> pairs)
        {
            /*
                * string method: Should be the same as the method specified by the server.
                * string api_key: The application key associated with the calling application.
                * string session_key: The session key of the logged in user.
                * float call_id: The request's sequence number.
                * string sig: An MD5 hash of the current request and your secret key.
                * string v: This must be set to 1.0 to use this version of the API.
                * string format: Optional - Ignored.
                * string cnonce: Optional - Client-selected nonce. Ignored.
                * string nonce: Should be the same as the nonce specified by the server.
    
                creates the response array, new, without session_key and sig
                http://developers.facebook.com/blog/post/555/
                https://developers.facebook.com/docs/chat/
                $resp_array = array(
                    'method' => $challenge_array['method'],
                    'nonce' => $challenge_array['nonce'],
                    'access_token' => $access_token,
                    'api_key' => $options['app_id'],
                    'call_id' => 0,
                    'v' => '1.0',
                );
            */

            var extData = ExtentedData as FacebookExtendedData;

            string res = "";
            res = res + "method=" + System.Uri.EscapeDataString(pairs["method"]);
            res = res + "&api_key=" + System.Uri.EscapeDataString(extData.ApiKey);
            res = res + "&access_token=" + System.Uri.EscapeDataString(extData.AccessToken);
            res = res + "&v=" + System.Uri.EscapeDataString(VERSION);
            res = res + "&call_id=" + System.Uri.EscapeDataString(callId);
            res = res + "&nonce=" + System.Uri.EscapeDataString(pairs["nonce"]);

            return res;
        }
    }
}
