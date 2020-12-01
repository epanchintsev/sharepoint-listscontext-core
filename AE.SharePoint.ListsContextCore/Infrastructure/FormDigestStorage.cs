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
        private static int timeoutSeconds;

        private readonly SharePointRestApiClient restApiClient;

        public FormDigestStorage(SharePointRestApiClient restApiClient)
        {
            this.restApiClient = restApiClient;
        }

        public async Task<string> GetFormDigestAsync()
        {
            if(string.IsNullOrEmpty(value))
            {
                await InitDigestAsync();
            }

            if((DateTime.Now - created).TotalSeconds >= timeoutSeconds)
            {
                await InitDigestAsync();
            }

            return value;
        }

        private async Task InitDigestAsync()
        {
            var json = await restApiClient.GetContextInfoAsync();
            var jsonDocument = System.Text.Json.JsonDocument.Parse(json);
            var contextWebInformation = jsonDocument.RootElement.GetProperty("d").GetProperty("GetContextWebInformation");
            
            value = contextWebInformation.GetProperty("FormDigestValue").GetString();
            timeoutSeconds = contextWebInformation.GetProperty("FormDigestTimeoutSeconds").GetInt32();
            created = DateTime.Now;            
        }
    }
}
