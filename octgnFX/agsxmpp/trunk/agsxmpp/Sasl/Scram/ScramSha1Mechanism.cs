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
using System.Security.Cryptography;
using System.Text;
using agsXMPP.protocol.sasl;
using agsXMPP.Util;
using agsXMPP.Xml.Dom;

namespace agsXMPP.Sasl.Scram
{
    /// <summary>
    /// 
    /// </summary>
    public class ScramSha1Mechanism : Mechanism
    {
        private const int LENGHT_CLIENT_NONE = 24;
        private string firstClientMessage;
        private byte[] clientNonce;
        private string clientNonceB64;
        private string password;
        private string username;

        public ScramSha1Mechanism()
        {
            GenerateClientNonce();
        }

        public override void Init(XmppClientConnection con)
        {
            XmppClientConnection = con;

            // Todo SASLPrep
            username = Username;
            password = Password;

            firstClientMessage = GenerateFirstClientMessage();
            string msg = ToB64String(firstClientMessage);
            con.Send(new Auth(MechanismType.SCRAM_SHA_1, msg));
        }

        public override void Parse(Node e)
        {
            if (e is Challenge)
            {
                Challenge ch = e as Challenge;
                string msg = ch.TextBase64;
                string content = GenerateFinalClientMessage(msg);
                XmppClientConnection.Send(new Response(content));
            }
        }

        /// <summary>
        /// Generate a random client nonce
        /// </summary>
        private void GenerateClientNonce()
        {
            var random = new Byte[LENGHT_CLIENT_NONE];

            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(random);

            clientNonce = random;
            clientNonceB64 = Convert.ToBase64String(random);
        }

        private static Dictionary<string, string> ParseMessage(string msg)
        {
            var str = msg.Split(',');

            var dict = new Dictionary<string, string>();
            foreach (string s in str)
            {
                int equalPos = s.IndexOf('=');

                var key = s.Substring(0, equalPos - 0);
                var val = s.Substring(equalPos + 1);

                if (!dict.ContainsKey(key))
                    dict.Add(key, val);
            }
            return dict;
        }

        internal string GenerateFinalClientMessage(string sMessage)
        {
            var pairs = ParseMessage(sMessage);

            //string clientServerNonce = pairs["r"];
            string serverNonce = pairs["r"].Substring(clientNonceB64.Length);


            var salt = pairs["s"];   // the user's salt - (base64 encoded)
            var iteration = pairs["i"];  // iteation count

            // the bare of our first message
            var clientFirstMessageBare = firstClientMessage.Substring(3);

            var sb = new StringBuilder();
            sb.Append("c=biws,");
            // Client/Server nonce
            sb.Append("r=");
            sb.Append(clientNonceB64);
            sb.Append(serverNonce);

            string clientFinalMessageWithoutProof = sb.ToString();

            string authMessage = clientFirstMessageBare + "," + sMessage + "," + clientFinalMessageWithoutProof;

            var saltedPassword = Hi(password, Convert.FromBase64String(salt), Convert.ToInt32(iteration));

            var clientKey = Hash.HMAC(saltedPassword, "Client Key");
            var storedKey = Hash.Sha1HashBytes(clientKey);


            var clientSignature = Hash.HMAC(storedKey, authMessage);

            var clientProof = new byte[clientKey.Length];
            for (var i = 0; i < clientKey.Length; ++i)
                clientProof[i] = (byte)(clientKey[i] ^ clientSignature[i]);


            //var serverKey = Hash.HMAC(saltedPassword, "Server Key");
            //var serverSignature = Hash.HMAC(serverKey, authMessage);

            string clientFinalMessage = clientFinalMessageWithoutProof;
            clientFinalMessage += ",p=";
            clientFinalMessage += Convert.ToBase64String(clientProof);

            return clientFinalMessage;
        }

        private string GenerateFirstClientMessage()
        {
            var sb = new StringBuilder();

            // no channel bindings supported
            sb.Append("n,,");

            // username
            sb.Append("n=");
            sb.Append(EscapeUsername(username));
            sb.Append(",");

            // client nonce
            sb.Append("r=");
            sb.Append(clientNonceB64);

            return sb.ToString();
        }

        private static string EscapeUsername(string user)
        {
            /*
            The characters ',' or '=' in usernames are sent as '=2C' and
            '=3D' respectively.  If the server receives a username that
            contains '=' not followed by either '2C' or '3D', then the
            server MUST fail the authentication.
            */

            var ret = user.Replace(",", "=2C");
            ret = ret.Replace("=", "=3D");

            return ret;
        }

        private static string ToB64String(string sin)
        {
            byte[] msg = Encoding.UTF8.GetBytes(sin);
            return Convert.ToBase64String(msg, 0, msg.Length);
        }

        private string FromB64String(string sin)
        {
            var b = Convert.FromBase64String(sin);
            return Encoding.UTF8.GetString(b);
        }

        private byte[] Hi(string pass, byte[] salt, int iterations)
        {
            var pdb = new Rfc2898DeriveBytes(pass, salt, iterations);
            return pdb.GetBytes(20);
        }
    }
}