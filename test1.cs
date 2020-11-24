var properties = spListProperties
                .Select(property =>
                {
                    Type listItemType = property.PropertyType.GetGenericArguments()[0];

                    return new SharePointListCreationInfo
                    {
                        PropertyToSet = property,
                        ListName = GetListName(property),
                        PropertyInstanceConstructor = typeof(SharePointList<>).MakeGenericType(listItemType).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                    };
                })
                .ToList();
                
                
                public static void AddSharePointListsContext<T>(this IServiceCollection serviceCollection, Action<Options> options) where T : SharePointListsContext
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), @"Not null options parameter required.");
            }

            var opt = new Options();
            options.Invoke(opt);

            serviceCollection.AddScoped<T>();

            serviceCollection
                .AddHttpClient<T>(client => HttpClientHelper.ConfigureHttpClient(client, opt))
                .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.GetHttpClientHandler(opt.Credentials))
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            
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
            time = contextWebInformation.GetProperty("FormDigestTimeoutSeconds").GetInt32();
            created = DateTime.Now;            
        }
        
        
        "{\"query\":{\"__metadata\":{\"type\":\"SP.CamlQuery\"},\"ViewXml\":\"{ ViewXml = \\u003CView\\u003E\\u003CQuery\\u003E\\u003CWhere\\u003E\\u003CEq\\u003E\\u003CFieldRef Name=\\u0027ID\\u0027 /\\u003E\\u003CValue Type=\\u0027Number\\u0027\\u003E1\\u003C/Value\\u003E\\u003C/Eq\\u003E\\u003C/Where\\u003E\\u003C/Query\\u003E\\u003C/View\\u003E }\"}}"
