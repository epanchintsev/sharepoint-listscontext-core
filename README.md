# sharepoint-listscontext-core

## Project Description


## Installation Instructions


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


