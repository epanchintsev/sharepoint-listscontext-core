/// <summary>
    /// Представляет модель данных поля списка SharePoint типа FieldURL.
    /// </summary>
    public class SharePointUrlField
    {
        private string url;

        /// <summary>
        /// Создает экземпляр класса, со значениями свойств по умолчанию.
        /// </summary>
        public SharePointUrlField()
        {
        }

        /// <summary>
        /// Создает экземпляр класса, с заданными значениями свойств.
        /// </summary>
        /// <param name="url">Url ресурса.</param>
        /// <param name="description">Описание для гиперссылки.</param>
        public SharePointUrlField(string url, string description)
        {
            Url = url;
            Description = description;
        }
        
        /// <summary>
        /// Получает, или задает Url ресурса.
        /// </summary>
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                Path = GetPath(url);
            }
        }

        /// <summary>
        /// Получает, или задает описание для гиперссылки.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url ресурса без хоста. Если Url имеет неверный формат - пустая строка.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Преобразует строку в uri и возвращает путь, отбрасывая имя хоста.
        /// В случае неудачного преобразования возвращает пустую строку.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetPath(string url)
        {
            string path = string.Empty;

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Uri uri = new Uri(url);
                    path = uri.AbsolutePath;
                }
                catch (UriFormatException) { }
            }
            
            return path;
        }
    }
