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

        public async Task<string> GetListAsync(string listName, ApiRequestParameters parameters)
        {
            var path = BuildPathWithParams($"_api/web/lists/GetByTitle('{listName}')", parameters);
            var json = await GetAsync(path);

            return json;
        }


        public async Task<string> GetItemsAsync(string listName, ApiRequestParameters parameters)
        {
            var path = BuildPathWithParams($"_api/web/lists/GetByTitle('{listName}')/items", parameters);
            var json = await GetAsync(path);

            return json;
        }

        public async Task<string> GetItemAsync(string listName, int id, ApiRequestParameters parameters)
        {
            var path = BuildPathWithParams($"_api/web/lists/GetByTitle('{listName}')/items({id})", parameters);
            var json = await GetAsync(path);

            return json;
        }


        public async Task<string> GetItemsAsync(string listName, string digest, string camlQuery, ApiRequestParameters parameters)
        {
            var path = BuildPathWithParams($"_api/web/lists/GetByTitle('{listName}')/GetItems", parameters);
            var data = new { query = new { __metadata = new { type = "SP.CamlQuery" }, ViewXml = camlQuery } };
                        
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            var dataJson = System.Text.Json.JsonSerializer.Serialize(data, options);
            string result = await PostAsync(path, digest, dataJson);

            return result;
        }

        public async Task<string> AddItemAsync(string listName, string digest, string itemJson)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/items";
            string result = await PostAsync(path, digest, itemJson);

            return result;
        }

        public async Task DeleteItemAsync(string listName, string digest, int id)
        {
            var path = $"_api/web/lists/GetByTitle('{listName}')/items({id})";

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
            {
                requestMessage.Headers.Add("X-RequestDigest", digest);
                requestMessage.Headers.Add("If-Match", "*");
                requestMessage.Headers.Add("X-HTTP-Method", "DELETE");
                var response = await httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task<string> GetAsync(string path)
        {
            var response = await httpClient.GetAsync(path);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            return json;
        }

        private async Task<string> PostAsync(string path, string digest, string dataJson)
        {
            string result;

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

        private string BuildPathWithParams(string path, ApiRequestParameters parameters)
        {
            //Для выбора полей GET https://{site_url}/_api/web/lists('{list_guid}')/items?$select=Title,Products/Name&$expand=Products/Name

            bool isFirstParam = true;
            
            var pathBuilder = new StringBuilder(path);

            if(!string.IsNullOrEmpty(parameters.Select))
            {
                pathBuilder.Append(isFirstParam ? "?" : "&");
                isFirstParam = false;
                pathBuilder.Append($"$select={parameters.Select}");
            }

            if (parameters.Top != 100)
            {
                pathBuilder.Append(isFirstParam ? "?" : "&");
                isFirstParam = false;
                pathBuilder.Append($"$top={parameters.Top}");
            }

            var result = pathBuilder.ToString();

            return result;
        }

    }
}
