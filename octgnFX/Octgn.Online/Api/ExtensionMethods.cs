using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Site.Api
{
    public static class ExtensionMethods
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(this HttpClient client, string requestUrl, TModel model, CancellationToken cancellationToken = default(CancellationToken)) {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUrl, stringContent, cancellationToken);
        }

        public static async Task<HttpResponseMessage> PutAsJsonAsync<TModel>(this HttpClient client, string requestUrl, TModel model, CancellationToken cancellationToken = default(CancellationToken)) {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PutAsync(requestUrl, stringContent, cancellationToken);
        }

        public static async Task<T> ReadAsAsync<T>(this HttpContent content) {
            var str = await content.ReadAsStringAsync();
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
            return obj;
        }
    }

}