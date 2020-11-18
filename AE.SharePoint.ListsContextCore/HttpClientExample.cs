class Program
    {
        static void Main(string[] args)
        {
            var webUri = new Uri("http://content.ftc.ru/");
            const string userName = "epanchintsev";
            const string password = "";
            //var securePassword = new SecureString();
            //foreach (var c in password)
            //{
            //    securePassword.AppendChar(c);
            //}
            var credentials = new System.Net.NetworkCredential(userName, password);// SharePointOnlineCredentials(userName, securePassword);

            var list = GetList(webUri, credentials, "Приглашения на мероприятия");
            var listItems = GetListItems(webUri, credentials, "Приглашения на мероприятия");

            //print List title
            Console.WriteLine(list["Title"]);
        }



        public static JToken GetList(Uri webUri, ICredentials credentials, string listTitle)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                client.Credentials = credentials;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
                var endpointUri = new Uri(webUri, string.Format("/sites/cft/_api/web/lists/getbytitle('{0}')", listTitle));
                var result = client.DownloadString(endpointUri);
                var t = JToken.Parse(result);
                return t["d"];
            }
        }

        public static JToken GetListItems(Uri webUri, ICredentials credentials, string listTitle)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                client.Credentials = credentials;
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json;odata=verbose");
                client.Headers.Add(HttpRequestHeader.Accept, "application/json;odata=verbose");
                var endpointUri = new Uri(webUri, string.Format("/sites/cft/_api/web/lists/getbytitle('{0}')/items", listTitle));
                var result = client.DownloadString(endpointUri);
                var t = JToken.Parse(result);
                return t["d"];
            }
        }
    }
