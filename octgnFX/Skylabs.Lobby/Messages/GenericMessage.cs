/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using agsXMPP.Xml.Dom;
using Octgn.Library.ExtensionMethods;
using Message = agsXMPP.protocol.client.Message;

namespace Skylabs.Lobby.Messages
{
    public abstract class GenericMessage : Message
    {
        private static readonly List<Type> MessageTypes = new List<Type>();

        static GenericMessage()
        {
            MessageTypes = GetImplementors<GenericMessage>().ToList();
        }

        public static GenericMessage ReadMessage(Message m)
        {
            if (!m.HasAttribute("MessageName"))
                return null;
            var typeString = m.GetAttribute("MessageName");

            var type = MessageTypes.FirstOrDefault(item => item.Name == typeString);

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

        protected GenericMessage()
        {
            this.Attributes.Add("MessageName", this.GetType().Name);
        }

        private static IEnumerable<Type> GetImplementors<T>()
        {
            var app = AppDomain.CurrentDomain;
            var ass = app.GetAssemblies();
            var targetType = typeof(T);

            foreach (var a in ass)
            {
                var types = a.GetTypesSafe();
                foreach (var t in types)
                {
                    if (t.IsInterface) continue;
                    if (t.IsAbstract) continue;
					if(t.IsSubclassOf(targetType))
					{
					    yield return t;
                        //yield return (T)Activator.CreateInstance(t);
                    }
                }
            }
        }
    }
}