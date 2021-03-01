using System;
using System.Collections.Generic;
using System.Text;

namespace AE.SharePoint.ListsContextCore
{
    /// <summary>
    /// Represents the data model of a SharePoint list field of type FieldURL (Hyperlink).
    /// </summary>
    public class SharePointUrlField
    {
        private string url;

        /// <summary>
        /// Creates an instance of the class, with default property values.
        /// </summary>
        public SharePointUrlField()
        {
        }

        /// <summary>
        /// Creates an instance of the class, with the specified property values.
        /// </summary>
        /// <param name="url">Url.</param>
        /// <param name="description">Description for hyperlink.</param>
        public SharePointUrlField(string url, string description)
        {
            Url = url;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the resource Url.
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
        /// Gets or sets the hyperlink description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Url of a resource without a host, or empty string if the Url is not in the correct format.
        /// </summary>
        public string Path { get; private set; }

        
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
}
