using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class SharePointRestApiClient
    {
        private readonly HttpClient httpClient;

        public SharePointRestApiClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GetAsync(string path)
        {
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return json;
        }

        public async Task<string> PostAsync(string path, string digest, object data)
        {
            string result;
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            var dataJson = System.Text.Json.JsonSerializer.Serialize(data, options);
            var content = new StringContent(dataJson, Encoding.UTF8);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/json;odata=verbose;charset=utf-8");

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
            {
                requestMessage.Headers.Add("X-RequestDigest", digest);
                requestMessage.Content = content;
                var response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        public async Task DeleteAsync(string path, string digest)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
            {
                requestMessage.Headers.Add("X-RequestDigest", digest);
                requestMessage.Headers.Add("If-Match", "*");
                requestMessage.Headers.Add("X-HTTP-Method", "DELETE");
                var response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
            }
        }

    }
}
