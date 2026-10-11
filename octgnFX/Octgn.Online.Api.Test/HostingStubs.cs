using System.Collections.Generic;

// Only hosting dependencies are substituted; the API client, HTTP formatters,
// exception factory, and API models are linked directly from production.
namespace Octgn.Online.Hosting
{
    public class HostedGame { }

    public static class ExtensionMethods
    {
        public static IEnumerable<HostedGame> Sanitized(this IEnumerable<HostedGame> games) => games;
    }
}
