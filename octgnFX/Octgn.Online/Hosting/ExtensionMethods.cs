using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public static class ExtensionMethods
    {
        public static void InitializeHosting(this Client client, Version octgnVersion) {
            client.Attach(new ClientHostingModule(client, octgnVersion));
        }

        public static ClientHostingModule Hosting(this Client client) {
            var mod = client.GetModule<ClientHostingModule>();
            return mod;
        }

        public static IEnumerable<HostedGame> Sanitized(this IEnumerable<HostedGame> games) {
            foreach(var game in games) {
                yield return new HostedGame(game, false);
            }
        }

        public static Task<HostedGame> HostGame(this Client client, HostedGame game)
        {
            return client.Hosting().RPC.HostGame(game);
        }
    }
}