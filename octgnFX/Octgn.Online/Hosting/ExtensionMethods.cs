using Octgn.Communication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public static class ExtensionMethods
    {
        public static void InitializeHosting(this Client client, Version octgnVersion) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            client.Attach(new Hosting(client, octgnVersion));
        }

        public static Hosting Hosting(this Client client) {
            if (client == null) throw new ArgumentNullException(nameof(client));

            var mod = client.GetModule<Hosting>();

            return mod;
        }

        public static IEnumerable<HostedGame> Sanitized(this IEnumerable<HostedGame> games) {
            foreach (var game in games) {
                yield return new HostedGame(game, false);
            }
        }

        public static Task<HostedGame> HostGame(this Client client, HostedGame game) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (game == null) throw new ArgumentNullException(nameof(game));

            var hosting = client.Hosting();

            if (hosting == null) throw new InvalidOperationException($"{nameof(Octgn.Online.Hosting.Hosting)} module not registered for {nameof(Client)} {client}");

            if (hosting.ClientRPC == null) throw new InvalidOperationException($"{nameof(Octgn.Online.Hosting.Hosting)} module not configured correctly for {nameof(Client)} {client}");

            return hosting.ClientRPC.HostGame(game);
        }
    }
}