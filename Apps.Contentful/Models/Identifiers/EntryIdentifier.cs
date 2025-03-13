using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class EntryIdentifier : EnvironmentIdentifier
{
    [Display("Entry ID"), DataSource(typeof(EntryDataSourceHandler))]
    public string EntryId { get; set; } = string.Empty;
}