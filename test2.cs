public static void ConfigureHttpClient(HttpClient client, Options options)
        {
            client.BaseAddress = new Uri(options.SharePointSiteUrl);

            client.DefaultRequestHeaders.Clear();
            if (options.Credentials == null || options.Credentials is NetworkCredential)
            {
                client.DefaultRequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
            }
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json;odata=verbose");
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
        }
        
        
        public async Task<List<T>> GetItemsAsync(string query)
        {
            
            var digest = await formDigestStorage.GetFormDigestAsync();
            

            var path = $"_api/web/lists/GetByTitle('{listName}')/GetItems?$select={GetSelectParameter()}";
            var dataObj = new { query = new { __metadata = new { type = "SP.CamlQuery" }, ViewXml = query } };
            
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            var dataJson = System.Text.Json.JsonSerializer.Serialize(dataObj, options);
            var data = new StringContent(dataJson, Encoding.UTF8);
            data.Headers.Clear();
            data.Headers.Add("Content-Type", "application/json;odata=verbose;charset=utf-8");
            


            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, path))
            {
                requestMessage.Headers.Add("X-RequestDigest", digest);                
                requestMessage.Content = data;
                var response = await httpClient.SendAsync(requestMessage);

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var result = converter.ConvertItems<T>(json);

                return result;
            }
        }
