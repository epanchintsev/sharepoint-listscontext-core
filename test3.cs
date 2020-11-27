public T Convert<T>(object source) where T : new()
        {
            var sourceJson = source as string;
            JsonDocument jsonDocument = JsonDocument.Parse(sourceJson);
            JsonElement jsonItem = jsonDocument.RootElement.GetProperty("d");            

            var result = Create<T>(jsonItem);

            return result;
        }
