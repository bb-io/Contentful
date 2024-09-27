using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class EntryTaskIdentifier : EnvironmentIdentifier
{
    [Display("Entry ID"), DataSource(typeof(EntryWithTaskDataSourceHandler))]
    public string EntryId { get; set; }

    [Display("Entry task ID", Description = "The ID of the entry task to retrieve"),
     DataSource(typeof(EntryTaskDataSource))]
    public string EntryTaskId { get; set; } = string.Empty;
}