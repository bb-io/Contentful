using Apps.Contentful.DataSourceHandlers;
using Apps.Contentful.DataSourceHandlers.Tags;
using Apps.Contentful.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Requests;

public class FindEntryRequest : EnvironmentIdentifier
{
    [Display("Content model")]
    [DataSource(typeof(ContentModelDataSourceHandler))]
    public string? ContentModelId { get; set; }
    public string Field { get; set; }
    public string Value { get; set; }
}