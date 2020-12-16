# sharepoint-listscontext-core

## Project Description


## Installation Instructions

Install latest version of NuGet package:
```
PM> Install-Package AE.SharePoint.ListsContextCore
```

Additionaly you can install NuGet package for registration in Microsoft DI container:
```
PM> Install-Package AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection
```

## Quick Start

### 1. Create model for SharePoint list.

Create class with properties.

```csharp
class ArticleListItem
{        
    public int Id { get; set; }

    public string Title { get; set; }
}
```

Supported types:
* int
* long
* double
* decimal
* bool
* DateTime
* string

Property mapped to SharePoint field by name.
if you want to use property name different from SharePoint field name, use **SharePointFieldNameAttribute**.
```csharp
class ArticleListItem
{        
    public int Id { get; set; }

    public string Title { get; set; }

    [SharePointFieldName("Created")]
    public DateTime PublicationDate  { get; set; }
}
```

Use **SharePointNotMappedAttribute** for properties that do not need to be initialized with values from a SharePoint list.

```csharp
class ArticleListItem
{        
    public int Id { get; set; }

    public string Title { get; set; }

    [SharePointFieldName("Created")]
    public DateTime PublicationDate  { get; set; }

    [SharePointNotMapped]
    public string Year  { get; set; }
}
```

### 2. Create class for SharePoint lists context.

The class must inherit from **SharePointListsContext**.
Add properties representing SharePointLists, use **SharePointList<T>** generic type for that.
Add to each property **SharePointListNameAttribute** with current name of SharePoint list.

```csharp
class ExampleContext: SharePointListsContext
    {
        public ExampleContext(HttpClient client): base(client)
        {            
        }

        [SharePointListName("ArticlesList")]
        public SharePointList<ArticleListItem> Articles { get; set; }
    }
```

### 3. Register context in DI container.

Register context by using **AddSharePointListsContext<T>** extension method from **AE.SharePoint.ListsContextCore.Extensions.Microsoft.DependencyInjection** namespace.
Set SharePointSiteUrl to SharePoint site address. Notise that trailing slash at the end of url.
You may use Network, or SharePoint credentials to access the SharePoint site, or leave Credentials parameter not set if default (for example application pool credentials) are used.

```csharp
serviceCollection.AddSharePointListsContext<ExampleContext>(options =>
    {
        options.SharePointSiteUrl = "http://sharepointsite.url/sites/test-site/";
        // options.Credentials = new NetworkCredential(userName, password);
    });
```

### 4. Get strongly typed items from SharePoint.

You can get all items.

```csharp
var context = serviceProvider.GetService<ExampleContext>();
List<ArticleListItem> items = await context.Articles.GetAllItemsAsync();
```

Or get particular item by Id.

```csharp
var context = serviceProvider.GetService<ExampleContext>();
ArticleListItem item = await context.Articles.GetItemAsync(1);
```

Or get items from the list based on the specified CAML query.

```csharp
string ViewXml = "<View>" +
                    "<Query>" +
                        "<Where><Eq>" +
                            "<FieldRef Name='Title' />" +
                            "<Value Type='Text'>Happy New Year</Value>" +
                        "</Eq></Where>" +
                    "</Query>" +
                "</View>";

List<ArticleListItem> selectedItems = await context.Articles.GetItemsAsync(ViewXml);
```
### 5. Add item to SharePoint list.
To add new item, create item class with specified properties and add it to list using AddItemAsync(T item) method.
Method returns created item from SharePoint list.

```csharp
var context = serviceProvider.GetService<ExampleContext>();
var newItem = new ArticleListItem
{
    Title = "Happy New 2019 Year"
};

ArticleListItem createdItem = await context.Articles.AddItemAsync(newItem);
```

### 6. Update item in SharePoint list.
If you want to update existing item use UpdateItemAsync(T item).
*Item class must implements IListItemBase interface*. And have initialized Id property with identifier of item witch you want to update.
You may use ListItemBase class as base class of your models.

```csharp
var context = serviceProvider.GetService<ExampleContext>();

class ArticleListItem: ListItemBase
{
    public string Title { get; set; }

    [SharePointFieldName("Created")]
    public DateTime PublicationDate  { get; set; }

    [SharePointNotMapped]
    public string Year  { get; set; }
}

createdItem.Title = "Happy New 2020 Year";
await context.Articles.UpdateItemAsync(newItem);
```

### 7. Delete item from SharePoint list.

To delete item from SharePoint list use DeleteItemAsync(int id) method.

```csharp
var context = serviceProvider.GetService<ExampleContext>();
await context.Articles.DeleteItemAsync(1);
```

### 8. Optimizing request.

You can limit quantity of returned elements by using Take method.
Or you can receive only nessesary fields by using IncludeFields and ExcludeFields methods.
In example below GetAllItemsAsync returns only 5 items, and not retrieve data for Content field, that field will be initialized with default value.

```csharp
List<ArticleListItem> selectedItems = await context.Articles
    .ExcludeFields(x => new { x.Content })
    .Take(5)
    .GetAllItemsAsync();
```



## Release Notes

### Version 1.0.0
- Created methods for getting intems from SharePoint List: Task<List<T>> GetAllItemsAsync(), Task<T> GetItemAsync(int id), Task<List<T>> GetItemsAsync(string query)

### Version 1.1.0
- Created method for adding item to SharePoint List: Task<T> AddItemAsync(T item)
- Created method for updating item in SharePoint List: Task UpdateItemAsync(T item)
- Created method for deleting item from SharePoint List: Task DeleteItemAsync(int id)

### Version 1.2.0
- Created method for limiting retrieved items SharePointList<T> Take(int count)
- Created methods for limiting retrieved fields SharePointList<T> IncludeFields(Expression<Func<T,object>> fields), SharePointList<T> ExcludeFields(Expression<Func<T, object>> fields)
