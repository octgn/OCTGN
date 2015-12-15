using Microsoft.AspNet.SignalR.Hubs;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;

namespace Octgn.Client.Modules
{
    public class CallerCulturePipelineModule : HubPipelineModule
    {
        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            // Use the value we stored in the Culture property of the caller's state when they connected
            if (context.Hub.Context.Request.Headers["Accept-Language"] != null)
            {
                var lang = context.Hub.Context.Request.Headers["Accept-Language"].Split(new[] { ',' })
                    .Select(a => StringWithQualityHeaderValue.Parse(a))
                    .Select(a => new StringWithQualityHeaderValue(a.Value,
                        a.Quality.GetValueOrDefault(1)))
                    .OrderByDescending(a => a.Quality)
                    .Select(x => CultureInfo.GetCultureInfo(x.Value))
                    .FirstOrDefault();

                if (lang != null)
                {
                    Thread.CurrentThread.CurrentUICulture = lang;
                    Thread.CurrentThread.CurrentCulture = lang;
                }
            }

            return base.OnBeforeIncoming(context);
        }
    }
}
