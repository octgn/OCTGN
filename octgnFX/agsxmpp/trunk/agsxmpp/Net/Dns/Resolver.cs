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

//
// Bdev.Net.Dns by Rob Philpott, Big Developments Ltd. Please send all bugs/enhancements to
// rob@bigdevelopments.co.uk  This file and the code contained within is freeware and may be
// distributed and edited without restriction.
// 

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace agsXMPP.Net.Dns
{
	/// <summary>
	/// Summary description for Dns.
	/// </summary>
	public sealed class Resolver
	{
		const   int		_dnsPort            = 53;
		const   int		_udpRetryAttempts   = 2;
		static  int		_uniqueId;
        const   int     _udpTimeout         = 1000;

		/// <summary>
		/// Private constructor - this static class should never be instantiated
		/// </summary>
        private Resolver()
        {
            // no implementation
        }		

        /// <summary>
        /// Shorthand form to make SRV querying easier, essentially wraps up the retreival
        /// of the SRV records, and sorts them by preference
        /// </summary>
        /// <param name="domain">domain name to retreive SRV RRs for</param>
        /// <param name="dnsServer">the server we're going to ask</param>
        /// <returns>An array of SRVRecords</returns>
        public static SRVRecord[] SRVLookup(string domain, IPAddress dnsServer)
        {
            // check the inputs
            if (domain == null) throw new ArgumentNullException("domain");
            if (dnsServer == null)  throw new ArgumentNullException("dnsServer");

            // create a request for this
            Request request = new Request();

            // add one question - the SRV IN lookup for the supplied domain
            request.AddQuestion(new Question(domain, DnsType.SRV, DnsClass.IN));

            // fire it off
            Response response = Lookup(request, dnsServer);

            // if we didn't get a response, then return null
            if (response == null) return null;
            	
            // create a growable array of SRV records
            ArrayList resourceRecords = new ArrayList();

            // add each of the answers to the array
            foreach (Answer answer in response.Answers)
            {
                // if the answer is an SRV record
                if (answer.Type == DnsType.SRV)
                {
                   // add it to our array
                   resourceRecords.Add(answer.Record);
                }
            }

            // create array of MX records
            SRVRecord[] srvRecords = new SRVRecord[resourceRecords.Count];

            // copy from the array list
            resourceRecords.CopyTo(srvRecords);

            // sort into lowest preference order
            Array.Sort(srvRecords);

            // and return
            return srvRecords;
        }

        /// <summary>
		/// The principal look up function, which sends a request message to the given
		/// DNS server and collects a response. This implementation re-sends the message
		/// via UDP up to two times in the event of no response/packet loss
		/// </summary>
		/// <param name="request">The logical request to send to the server</param>
		/// <param name="dnsServer">The IP address of the DNS server we are querying</param>
		/// <returns>The logical response from the DNS server or null if no response</returns>
		public static Response Lookup(Request request, IPAddress dnsServer)
		{
			// check the inputs
			if (request == null) throw new ArgumentNullException("request");
			if (dnsServer == null) throw new ArgumentNullException("dnsServer");
			
			// We will not catch exceptions here, rather just refer them to the caller

			// create an end point to communicate with
			IPEndPoint server = new IPEndPoint(dnsServer, _dnsPort);
		
			// get the message
			byte[] requestMessage = request.GetMessage();

			// send the request and get the response
			byte[] responseMessage = UdpTransfer(server, requestMessage);

			// and populate a response object from that and return it
			return new Response(responseMessage);
		}

		private static byte[] UdpTransfer(IPEndPoint server, byte[] requestMessage)
		{
			// UDP can fail - if it does try again keeping track of how many attempts we've made
			int attempts = 0;

			// try repeatedly in case of failure
			while (attempts <= _udpRetryAttempts)
			{
				// firstly, uniquely mark this request with an id
				unchecked
				{
					// substitute in an id unique to this lookup, the request has no idea about this
					requestMessage[0] = (byte)(_uniqueId >> 8);
					requestMessage[1] = (byte)_uniqueId;
				}

				// we'll be send and receiving a UDP packet
				Socket socket;
                if (Socket.OSSupportsIPv6 && (server.AddressFamily == AddressFamily.InterNetworkV6))
                    socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp); // V6
                else
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			
				// we will wait at most 1 second for a dns reply
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _udpTimeout);

				// send it off to the server
				socket.SendTo(requestMessage, requestMessage.Length, SocketFlags.None, server);
		
				// RFC1035 states that the maximum size of a UDP datagram is 512 octets (bytes)
				byte[] responseMessage = new byte[512];              
           
				try
				{                    
					// wait for a response upto 1 second
					socket.Receive(responseMessage);

					// make sure the message returned is ours
					if (responseMessage[0] == requestMessage[0] && responseMessage[1] == requestMessage[1])
					{
						// its a valid response - return it, this is our successful exit point
						return responseMessage;
					}
				}
				catch (SocketException)
				{
					// failure - we better try again, but remember how many attempts
					attempts++;
				}
				finally
				{
					// increase the unique id
					_uniqueId++;

					// close the socket
					socket.Close();
				}
			}
		
			// the operation has failed, this is our unsuccessful exit point
			throw new NoResponseException();
		}
	}
}
