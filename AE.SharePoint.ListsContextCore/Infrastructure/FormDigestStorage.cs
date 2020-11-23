using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AE.SharePoint.ListsContextCore.Infrastructure
{
    internal class FormDigestStorage
    {
        private static DateTime created;
        private static string value;
        private static int time;
        
        private readonly HttpClient httpClient;

        public FormDigestStorage(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> GetFormDigestAsync()
        {
            if(string.IsNullOrEmpty(value))
            {
                await InitDigestAsync();
            }

            if((DateTime.Now - created).TotalSeconds >= time)
            {
                await InitDigestAsync();
            }

            return value;
        }

        private async Task InitDigestAsync()
        {
            var digestPath = $"_api/contextinfo";

            var response = await httpClient.PostAsync(digestPath, null);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocument = System.Text.Json.JsonDocument.Parse(json);
            var contextWebInformation = jsonDocument.RootElement.GetProperty("d").GetProperty("GetContextWebInformation");
            
            value = contextWebInformation.GetProperty("FormDigestValue").GetString();
            time = contextWebInformation.GetProperty("FormDigestTime").GetInt32();
            created = DateTime.Now;            
        }
    }
}
