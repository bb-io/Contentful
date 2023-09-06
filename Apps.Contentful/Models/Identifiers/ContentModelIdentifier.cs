using Apps.Contentful.DataSourceHandlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.Contentful.Models.Identifiers;

public class ContentModelIdentifier
{
    [Display("Content model")]
    [DataSource(typeof(ContentModelDataSourceHandler))]
    public string Id { get; set; }
}