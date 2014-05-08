/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Concurrent;
using System.Linq;
using agsXMPP.Xml.Dom;
using Message = agsXMPP.protocol.client.Message;

namespace Skylabs.Lobby.Messages
{
    public abstract class GenericMessage : Message
    {
        private static readonly BlockingCollection<Type> MessageTypes = new BlockingCollection<Type>();

        public static void Register<T>() where T : GenericMessage
        {
            MessageTypes.Add(typeof(T));
        }

        public static GenericMessage ReadMessage(Message m)
        {
            if (!m.HasAttribute("MessageName"))
                return null;
            var typeString = m.GetAttribute("MessageName");
            var type = MessageTypes.GetConsumingEnumerable()
                .FirstOrDefault(item => item.Name == typeString);

            if (type == null)
                return null;

            var ret = Activator.CreateInstance(type) as GenericMessage;

            ret.Type = m.Type;
            ret.To = m.To;
            ret.From = m.From;
            ret.Subject = m.Subject;
            ret.Body = m.Body;
			
            ret.ChildNodes.Clear();
            foreach (var i in m.ChildNodes)
            {
                ret.ChildNodes.Add(i as Node);
            }
            return ret;
        }
    }
}