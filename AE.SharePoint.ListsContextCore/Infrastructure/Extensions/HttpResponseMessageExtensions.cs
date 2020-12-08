using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AE.SharePoint.ListsContextCore.Infrastructure.Extensions
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<HttpResponseMessage> EnsureNon404StatusCodeAsync(this HttpResponseMessage responseMessage)
        {
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                string errorJson = await responseMessage.Content.ReadAsStringAsync();
                JsonDocument jsonDocument = JsonDocument.Parse(errorJson);
                string message = jsonDocument.RootElement.GetProperty("error").GetProperty("message").GetProperty("value").GetString();
                throw new ItemNotFoundException(message);
            }

            return responseMessage;
        }
    }
}
