using Octgn.Communication;
using System.Collections.Generic;

namespace Octgn.Online.Hosting
{
    public static class ExtensionMethods
    {
        public static void InitializeHosting(this Client client) {
            client.Attach(new ClientHostingModule());
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
    }
}