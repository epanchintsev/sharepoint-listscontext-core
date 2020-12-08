using System;

namespace AE.SharePoint.ListsContextCore
{

    /// <summary>
    /// The exception that is thrown when the specified list item or list not found while retrieving from SharePoint.
    /// </summary>
    public class ItemNotFoundException: ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the AE.SharePoint.ListsContextCore.ItemNotFoundException class.
        /// </summary>
        public ItemNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.ApplicationException class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public ItemNotFoundException(string message): base(message)
        {
        }
    }
}

