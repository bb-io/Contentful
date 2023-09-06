using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class EntryIdentifier
{
    [Display("Entry")]
    [DataSource(typeof(EntryDataSourceHandler))]
    public string Id { get; set; }
}